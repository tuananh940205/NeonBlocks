using System;
using System.Collections.Generic;
using NeonBlocks.Config;

namespace NeonBlocks.Gameplay
{
    /// <summary>
    /// Luật lưới thuần C#: lưu các ô đã khóa và một ActivePiece tách biệt, kiểm tra va chạm, spawn, di chuyển, rơi và khóa.
    /// Tọa độ X tăng sang phải, Y tăng lên; y từ 0 đến TotalHeight - 1, bao gồm các hàng ẩn.
    /// </summary>
    public sealed class BoardModel
    {
        private readonly bool[,] settledCells;
        private ActivePiece activePiece;

        /// <summary>Cấu hình kích thước bảng bất biến.</summary>
        public BoardSettings Settings { get; }

        /// <summary>Khối đang rơi, null khi bảng chưa spawn hoặc vừa lock.</summary>
        public ActivePiece ActivePiece => activePiece;

        /// <summary>True khi đang có một khối chưa khóa.</summary>
        public bool HasActivePiece => activePiece != null;

        /// <summary>
        /// Tạo một board rỗng theo kích thước cấu hình.
        /// </summary>
        /// <param name="settings">Kích thước bảng hợp lệ, không được null.</param>
        public BoardModel(BoardSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            settledCells = new bool[Settings.Width, Settings.TotalHeight];
        }

        /// <summary>
        /// Thử spawn piece tại origin. Nếu đã có active piece, piece ra ngoài bảng hoặc đụng ô đã khóa thì trả false và không mutate.
        /// </summary>
        /// <param name="definition">Định nghĩa shape để spawn.</param>
        /// <param name="origin">Góc đáy trái của khung 4×4 ở lúc spawn.</param>
        /// <returns>True nếu đã spawn thành công.</returns>
        public bool TrySpawn(PieceDefinition definition, GridPoint origin)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (activePiece != null)
            {
                return false;
            }

            var candidate = new ActivePiece(definition, origin);
            if (!CanFit(candidate))
            {
                return false;
            }

            activePiece = candidate;
            return true;
        }

        /// <summary>Thay active piece khi Hold; bỏ khối cũ mà không khóa ô hoặc cộng điểm, spawn lại ở hướng ban đầu.</summary>
        public bool TryReplaceActivePiece(PieceDefinition definition, GridPoint origin)
        {
            if (definition == null)
            {
                // Dữ liệu sai phải báo rõ trước khi thay đổi board.
                throw new ArgumentNullException(nameof(definition));
            }
            if (activePiece == null)
            {
                // Hold chỉ thay khối đang tồn tại.
                return false;
            }
            activePiece = null;
            return TrySpawn(definition, origin);
        }

        /// <summary>
        /// Thử dịch khối đang rơi sang trái một ô; khi chạm biên hoặc ô đã khóa thì giữ nguyên trạng thái.
        /// </summary>
        /// <returns>True nếu di chuyển thành công.</returns>
        public bool TryMoveLeft()
        {
            return TryMove(-1, 0);
        }

        /// <summary>
        /// Thử dịch khối đang rơi sang phải một ô; khi chạm biên hoặc ô đã khóa thì giữ nguyên trạng thái.
        /// </summary>
        /// <returns>True nếu di chuyển thành công.</returns>
        public bool TryMoveRight()
        {
            return TryMove(1, 0);
        }

        /// <summary>
        /// Thử cho khối đang rơi hạ xuống đúng một ô; khi chạm đáy hoặc ô đã khóa thì giữ nguyên trạng thái.
        /// </summary>
        /// <returns>True nếu khối đã rơi xuống một ô.</returns>
        public bool TryFallOneCell()
        {
            return TryMove(0, -1);
        }

        /// <summary>
        /// Thử xoay active piece 90 độ theo chiều kim đồng hồ bằng SRS; chỉ commit orientation và kick khi candidate fit.
        /// Khối O luôn trả true nhưng giữ nguyên state vì hình không đổi.
        /// </summary>
        /// <returns>True nếu phép xoay hợp lệ hoặc active piece là O.</returns>
        public bool TryRotateClockwise()
        {
            return TryRotate(true);
        }

        /// <summary>
        /// Thử xoay active piece 90 độ ngược chiều kim đồng hồ bằng SRS; chỉ commit orientation và kick khi candidate fit.
        /// Khối O luôn trả true nhưng giữ nguyên state vì hình không đổi.
        /// </summary>
        /// <returns>True nếu phép xoay hợp lệ hoặc active piece là O.</returns>
        public bool TryRotateCounterClockwise()
        {
            return TryRotate(false);
        }

        /// <summary>
        /// Khóa active piece vào lưới settled khi nó còn fit. Không có active piece hoặc trạng thái không hợp lệ thì trả false và không mutate.
        /// </summary>
        /// <returns>True nếu đã ghi cả bốn ô vào lưới và xóa active piece.</returns>
        public bool TryLockActivePiece()
        {
            if (activePiece == null || !CanFit(activePiece))
            {
                return false;
            }

            for (int i = 0; i < activePiece.Definition.Cells.Count; i++)
            {
                GridPoint cell = activePiece.GetCell(i);
                settledCells[cell.X, cell.Y] = true;
            }

            activePiece = null;
            return true;
        }

        /// <summary>
        /// Kiểm tra một piece có nằm trọn bảng và không đụng ô đã khóa hay không; không thay đổi model.
        /// </summary>
        /// <param name="piece">Piece cần kiểm tra, không được null.</param>
        /// <returns>True nếu mọi ô của piece fit trong bảng.</returns>
        public bool CanFit(ActivePiece piece)
        {
            if (piece == null)
            {
                throw new ArgumentNullException(nameof(piece));
            }

            for (int i = 0; i < piece.Definition.Cells.Count; i++)
            {
                GridPoint cell = piece.GetCell(i);
                if (!IsInsideBoard(cell) || settledCells[cell.X, cell.Y])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Đọc trạng thái đã khóa của một ô hợp lệ trong bảng.
        /// </summary>
        /// <param name="cell">Tọa độ ô cần đọc.</param>
        /// <returns>True nếu ô đã được khóa.</returns>
        public bool IsSettledAt(GridPoint cell)
        {
            if (!IsInsideBoard(cell))
            {
                throw new ArgumentOutOfRangeException(nameof(cell), $"Ô {cell} nằm ngoài bảng.");
            }

            return settledCells[cell.X, cell.Y];
        }

        /// <summary>
        /// Tạo snapshot tọa độ các ô đã khóa để view đọc mà không có quyền sửa lưới nội bộ.
        /// </summary>
        /// <returns>Danh sách chỉ đọc chứa mọi ô đã khóa.</returns>
        public IReadOnlyList<GridPoint> GetSettledCells()
        {
            var cells = new List<GridPoint>();
            for (int x = 0; x < Settings.Width; x++)
            {
                for (int y = 0; y < Settings.TotalHeight; y++)
                {
                    if (settledCells[x, y])
                    {
                        cells.Add(new GridPoint(x, y));
                    }
                }
            }

            return cells.AsReadOnly();
        }

        /// <summary>Xóa đồng thời mọi hàng đầy rồi compact các hàng còn lại xuống đáy.</summary>
        /// <returns>Số hàng đã xóa.</returns>
        public int ClearFullRows()
        {
            int writeY = 0;
            int cleared = 0;
            for (int readY = 0; readY < Settings.TotalHeight; readY++)
            {
                bool full = true;
                for (int x = 0; x < Settings.Width; x++) if (!settledCells[x, readY]) { full = false; break; }
                if (full) { cleared++; continue; }
                if (writeY != readY) for (int x = 0; x < Settings.Width; x++) settledCells[x, writeY] = settledCells[x, readY];
                writeY++;
            }
            for (int y = writeY; y < Settings.TotalHeight; y++) for (int x = 0; x < Settings.Width; x++) settledCells[x, y] = false;
            return cleared;
        }

        /// <summary>Kiểm tra sau clear còn ô đã khóa nào nằm trong vùng hàng ẩn hay không.</summary>
        /// <returns>True nếu còn ô settled trong vùng ẩn.</returns>
        public bool HasSettledCellsInHiddenRows()
        {
            for (int y = Settings.VisibleHeight; y < Settings.TotalHeight; y++) for (int x = 0; x < Settings.Width; x++) if (settledCells[x, y]) return true;
            return false;
        }

        /// <summary>
        /// Thử dịch active piece theo vector nguyên; chỉ commit khi candidate fit hoàn toàn.
        /// </summary>
        /// <param name="deltaX">Dịch theo X.</param>
        /// <param name="deltaY">Dịch theo Y.</param>
        /// <returns>True nếu đã commit candidate.</returns>
        private bool TryMove(int deltaX, int deltaY)
        {
            if (activePiece == null)
            {
                return false;
            }

            ActivePiece candidate = activePiece.Translate(deltaX, deltaY);
            if (!CanFit(candidate))
            {
                return false;
            }

            activePiece = candidate;
            return true;
        }

        /// <summary>
        /// Tạo candidate quay theo chiều yêu cầu và thử từng offset SRS cho đến khi tìm được candidate fit.
        /// </summary>
        /// <param name="clockwise">True cho CW, false cho CCW.</param>
        /// <returns>True nếu phép xoay được commit.</returns>
        private bool TryRotate(bool clockwise)
        {
            if (activePiece == null)
            {
                return false;
            }

            if (activePiece.Definition.Id == "O")
            {
                return true;
            }

            ActivePiece rotated = clockwise ? activePiece.RotateClockwise() : activePiece.RotateCounterClockwise();
            IReadOnlyList<GridPoint> kicks = SrsKickTable.GetKicks(activePiece.Definition.Id, activePiece.Orientation, rotated.Orientation);

            foreach (GridPoint kick in kicks)
            {
                ActivePiece candidate = rotated.Translate(kick.X, kick.Y);
                if (CanFit(candidate))
                {
                    activePiece = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra một tọa độ có thuộc cả vùng hiển thị lẫn vùng ẩn của bảng hay không.
        /// </summary>
        /// <param name="cell">Tọa độ cần kiểm tra.</param>
        /// <returns>True nếu tọa độ nằm trong bảng.</returns>
        private bool IsInsideBoard(GridPoint cell)
        {
            return cell.X >= 0 && cell.X < Settings.Width && cell.Y >= 0 && cell.Y < Settings.TotalHeight;
        }
    }
}
