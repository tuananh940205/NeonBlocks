using System;
using NeonBlocks.Config;

namespace NeonBlocks.Gameplay
{
    /// <summary>
    /// Khối đang rơi bất biến: giữ định nghĩa shape ở hướng spawn và origin của khung 4×4 trong tọa độ BoardModel.
    /// Các ô đã khóa không nằm trong object này.
    /// </summary>
    public sealed class ActivePiece
    {
        /// <summary>Định nghĩa shape dùng để lấy bốn ô tương đối.</summary>
        public PieceDefinition Definition { get; }

        /// <summary>Góc đáy trái của khung piece 4×4 trong tọa độ bảng.</summary>
        public GridPoint Origin { get; }

        /// <summary>Orientation SRS hiện tại của piece.</summary>
        public PieceOrientation Orientation { get; }

        /// <summary>
        /// Tạo khối đang rơi tại một origin xác định.
        /// </summary>
        /// <param name="definition">Định nghĩa piece, không được null.</param>
        /// <param name="origin">Góc đáy trái của khung piece.</param>
        public ActivePiece(PieceDefinition definition, GridPoint origin)
            : this(definition, origin, PieceOrientation.Spawn)
        {
        }

        /// <summary>
        /// Tạo khối đang rơi tại origin và orientation xác định.
        /// </summary>
        /// <param name="definition">Định nghĩa piece, không được null.</param>
        /// <param name="origin">Góc đáy trái của khung piece.</param>
        /// <param name="orientation">Orientation SRS của piece.</param>
        public ActivePiece(PieceDefinition definition, GridPoint origin, PieceOrientation orientation)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Origin = origin;
            Orientation = orientation;
        }

        /// <summary>
        /// Lấy tọa độ bảng tuyệt đối của một ô thuộc piece.
        /// </summary>
        /// <param name="index">Chỉ số ô trong bốn ô của định nghĩa.</param>
        /// <returns>Tọa độ tuyệt đối của ô.</returns>
        public GridPoint GetCell(int index)
        {
            if (index < 0 || index >= Definition.Cells.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            GridPoint cell = RotateCell(Definition.Cells[index]);
            return new GridPoint(Origin.X + cell.X, Origin.Y + cell.Y);
        }

        /// <summary>
        /// Tạo trạng thái piece mới đã dịch một số ô; không thay đổi instance hiện tại.
        /// </summary>
        /// <param name="deltaX">Số ô dịch theo trục X.</param>
        /// <param name="deltaY">Số ô dịch theo trục Y.</param>
        /// <returns>Piece mới ở vị trí đã dịch.</returns>
        public ActivePiece Translate(int deltaX, int deltaY)
        {
            return new ActivePiece(Definition, new GridPoint(Origin.X + deltaX, Origin.Y + deltaY), Orientation);
        }

        /// <summary>
        /// Tạo candidate quay 90 độ theo chiều kim đồng hồ quanh tâm xoay của định nghĩa.
        /// </summary>
        /// <returns>Piece mới có orientation kế tiếp.</returns>
        public ActivePiece RotateClockwise()
        {
            return new ActivePiece(Definition, Origin, (PieceOrientation)(((int)Orientation + 1) % 4));
        }

        /// <summary>
        /// Tạo candidate quay 90 độ ngược chiều kim đồng hồ quanh tâm xoay của định nghĩa.
        /// </summary>
        /// <returns>Piece mới có orientation trước đó.</returns>
        public ActivePiece RotateCounterClockwise()
        {
            return new ActivePiece(Definition, Origin, (PieceOrientation)(((int)Orientation + 3) % 4));
        }

        /// <summary>
        /// Xoay một ô tương đối từ orientation spawn theo orientation hiện tại quanh tâm SRS của định nghĩa.
        /// </summary>
        /// <param name="cell">Ô ở orientation spawn.</param>
        /// <returns>Ô đã xoay, vẫn là tọa độ nguyên.</returns>
        private GridPoint RotateCell(GridPoint cell)
        {
            float relativeX = cell.X - Definition.RotationCenterX;
            float relativeY = cell.Y - Definition.RotationCenterY;
            float rotatedX = cell.X;
            float rotatedY = cell.Y;

            switch (Orientation)
            {
                case PieceOrientation.Right:
                    rotatedX = Definition.RotationCenterX + relativeY;
                    rotatedY = Definition.RotationCenterY - relativeX;
                    break;
                case PieceOrientation.Reverse:
                    rotatedX = Definition.RotationCenterX - relativeX;
                    rotatedY = Definition.RotationCenterY - relativeY;
                    break;
                case PieceOrientation.Left:
                    rotatedX = Definition.RotationCenterX - relativeY;
                    rotatedY = Definition.RotationCenterY + relativeX;
                    break;
            }

            return new GridPoint((int)Math.Round(rotatedX), (int)Math.Round(rotatedY));
        }
    }
}
