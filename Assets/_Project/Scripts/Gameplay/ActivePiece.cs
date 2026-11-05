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

        /// <summary>
        /// Tạo khối đang rơi tại một origin xác định.
        /// </summary>
        /// <param name="definition">Định nghĩa piece, không được null.</param>
        /// <param name="origin">Góc đáy trái của khung piece.</param>
        public ActivePiece(PieceDefinition definition, GridPoint origin)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Origin = origin;
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

            GridPoint cell = Definition.Cells[index];
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
            return new ActivePiece(Definition, new GridPoint(Origin.X + deltaX, Origin.Y + deltaY));
        }
    }
}
