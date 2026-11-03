using System;

namespace NeonBlocks.Config
{
    /// <summary>
    /// Tọa độ lưới nguyên, bất biến. X tăng sang phải, Y tăng lên trên.
    /// </summary>
    public readonly struct GridPoint : IEquatable<GridPoint>
    {
        /// <summary>Hoành độ, tăng sang phải.</summary>
        public int X { get; }

        /// <summary>Tung độ, tăng lên trên.</summary>
        public int Y { get; }

        /// <summary>
        /// Tạo tọa độ lưới từ hai thành phần nguyên.
        /// </summary>
        /// <param name="x">Hoành độ.</param>
        /// <param name="y">Tung độ.</param>
        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// So sánh hai tọa độ theo giá trị X và Y.
        /// </summary>
        /// <param name="other">Tọa độ cần so sánh.</param>
        /// <returns>True nếu X và Y bằng nhau.</returns>
        public bool Equals(GridPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// So sánh với object khác, chỉ đúng khi cùng kiểu GridPoint và cùng giá trị.
        /// </summary>
        /// <param name="obj">Object cần so sánh.</param>
        /// <returns>True nếu obj là GridPoint cùng giá trị.</returns>
        public override bool Equals(object obj)
        {
            return obj is GridPoint other && Equals(other);
        }

        /// <summary>
        /// Tính mã băm từ X và Y để dùng được trong HashSet.
        /// </summary>
        /// <returns>Mã băm của tọa độ.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        /// <summary>
        /// Biểu diễn chuỗi dạng (x, y) để đọc log và thông báo lỗi.
        /// </summary>
        /// <returns>Chuỗi dạng "(x, y)".</returns>
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
