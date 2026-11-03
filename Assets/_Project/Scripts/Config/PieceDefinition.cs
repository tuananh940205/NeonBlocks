using System;
using System.Collections.Generic;
using System.Linq;

namespace NeonBlocks.Config
{
    /// <summary>
    /// Định nghĩa một tetromino ở hướng spawn: 4 ô trong khung 4×4 (x, y từ 0 đến 3, gốc ở đáy trái của khung),
    /// cùng tâm xoay (rotation pivot) tính trong cùng hệ tọa độ khung. Bất biến, chỉ set qua constructor.
    /// </summary>
    public sealed class PieceDefinition
    {
        /// <summary>Kích thước khung chứa khối.</summary>
        public const int BoxSize = 4;

        /// <summary>Số ô của một tetromino.</summary>
        public const int CellCount = 4;

        /// <summary>Mã khối, ví dụ "I", "O", "T", "J", "L", "S", "Z".</summary>
        public string Id { get; }

        /// <summary>Bốn ô của khối trong khung 4×4 ở hướng spawn; chỉ đọc.</summary>
        public IReadOnlyList<GridPoint> Cells { get; }

        /// <summary>Hoành độ tâm xoay trong khung, có thể là số thập phân (ví dụ 1.5 cho khối I).</summary>
        public float RotationCenterX { get; }

        /// <summary>Tung độ tâm xoay trong khung, có thể là số thập phân.</summary>
        public float RotationCenterY { get; }

        /// <summary>
        /// Tạo định nghĩa khối và kiểm tra: đúng 4 ô, mọi ô nằm trong khung 4×4, không trùng ô, các ô liền nhau,
        /// tâm xoay nằm trong khung.
        /// </summary>
        /// <param name="id">Mã khối, không được rỗng.</param>
        /// <param name="cells">Đúng 4 ô trong khung 4×4.</param>
        /// <param name="rotationCenterX">Hoành độ tâm xoay trong khung, nằm trong [0, 3].</param>
        /// <param name="rotationCenterY">Tung độ tâm xoay trong khung, nằm trong [0, 3].</param>
        public PieceDefinition(string id, IEnumerable<GridPoint> cells, float rotationCenterX, float rotationCenterY)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ConfigValidationException("piece.id không được rỗng.");
            }

            List<GridPoint> cellList = cells?.ToList() ?? throw new ConfigValidationException($"piece '{id}' thiếu danh sách cells.");

            if (cellList.Count != CellCount)
            {
                throw new ConfigValidationException($"piece '{id}' phải có đúng {CellCount} ô; hiện tại: {cellList.Count}.");
            }

            var seen = new HashSet<GridPoint>();
            foreach (GridPoint cell in cellList)
            {
                if (cell.X < 0 || cell.X >= BoxSize || cell.Y < 0 || cell.Y >= BoxSize)
                {
                    throw new ConfigValidationException($"piece '{id}' có ô {cell} nằm ngoài khung {BoxSize}×{BoxSize}.");
                }

                if (!seen.Add(cell))
                {
                    throw new ConfigValidationException($"piece '{id}' có ô trùng tại {cell}.");
                }
            }

            if (!AreCellsConnected(cellList))
            {
                throw new ConfigValidationException($"piece '{id}' có các ô không liền nhau.");
            }

            if (rotationCenterX < 0f || rotationCenterX > BoxSize - 1 || rotationCenterY < 0f || rotationCenterY > BoxSize - 1)
            {
                throw new ConfigValidationException($"piece '{id}' có tâm xoay ({rotationCenterX}, {rotationCenterY}) nằm ngoài khung.");
            }

            Id = id;
            Cells = Array.AsReadOnly(cellList.ToArray());
            RotationCenterX = rotationCenterX;
            RotationCenterY = rotationCenterY;
        }

        /// <summary>
        /// Kiểm tra các ô có tạo thành một khối liền nhau theo 4 hướng hay không, bằng tìm kiếm theo chiều rộng.
        /// </summary>
        /// <param name="cells">Danh sách ô đã kiểm tra là không trùng và nằm trong khung.</param>
        /// <returns>True nếu mọi ô có thể đi tới nhau qua các ô kề cạnh.</returns>
        private static bool AreCellsConnected(IReadOnlyList<GridPoint> cells)
        {
            var remaining = new HashSet<GridPoint>(cells);
            var queue = new Queue<GridPoint>();
            queue.Enqueue(cells[0]);
            remaining.Remove(cells[0]);

            while (queue.Count > 0)
            {
                GridPoint current = queue.Dequeue();
                GridPoint[] neighbours =
                {
                    new GridPoint(current.X + 1, current.Y),
                    new GridPoint(current.X - 1, current.Y),
                    new GridPoint(current.X, current.Y + 1),
                    new GridPoint(current.X, current.Y - 1)
                };

                foreach (GridPoint next in neighbours)
                {
                    if (remaining.Remove(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            return remaining.Count == 0;
        }
    }
}
