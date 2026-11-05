using System;
using System.Collections.Generic;
using NeonBlocks.Config;

namespace NeonBlocks.Gameplay
{
    /// <summary>
    /// Bảng wall kick SRS trong hệ X sang phải, Y hướng lên. I dùng bảng riêng; J/L/S/T/Z dùng bảng chung.
    /// </summary>
    internal static class SrsKickTable
    {
        /// <summary>
        /// Lấy thứ tự năm offset kick cho một lần xoay SRS, bao gồm offset gốc (0, 0) ở đầu.
        /// </summary>
        /// <param name="pieceId">Mã piece.</param>
        /// <param name="from">Orientation trước khi xoay.</param>
        /// <param name="to">Orientation sau khi xoay.</param>
        /// <returns>Danh sách offset thử theo thứ tự SRS.</returns>
        public static IReadOnlyList<GridPoint> GetKicks(string pieceId, PieceOrientation from, PieceOrientation to)
        {
            if (pieceId == "I")
            {
                return GetIKicks(from, to);
            }

            return GetJlstzKicks(from, to);
        }

        /// <summary>
        /// Trả kick cho J/L/S/T/Z theo bảng SRS quy đổi sang trục Y hướng lên.
        /// </summary>
        /// <param name="from">Orientation trước khi xoay.</param>
        /// <param name="to">Orientation sau khi xoay.</param>
        /// <returns>Năm offset kick tương ứng.</returns>
        private static IReadOnlyList<GridPoint> GetJlstzKicks(PieceOrientation from, PieceOrientation to)
        {
            if (from == PieceOrientation.Spawn && to == PieceOrientation.Right ||
                from == PieceOrientation.Reverse && to == PieceOrientation.Right)
            {
                return Points((0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2));
            }

            if (from == PieceOrientation.Right && to == PieceOrientation.Spawn ||
                from == PieceOrientation.Right && to == PieceOrientation.Reverse)
            {
                return Points((0, 0), (1, 0), (1, -1), (0, 2), (1, 2));
            }

            if (from == PieceOrientation.Reverse && to == PieceOrientation.Left ||
                from == PieceOrientation.Spawn && to == PieceOrientation.Left)
            {
                return Points((0, 0), (1, 0), (1, 1), (0, -2), (1, -2));
            }

            if (from == PieceOrientation.Left && to == PieceOrientation.Reverse ||
                from == PieceOrientation.Left && to == PieceOrientation.Spawn)
            {
                return Points((0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2));
            }

            throw new ArgumentOutOfRangeException(nameof(to), $"Chuyển orientation SRS không hợp lệ: {from} → {to}.");
        }

        /// <summary>
        /// Trả kick cho I theo bảng SRS riêng quy đổi sang trục Y hướng lên.
        /// </summary>
        /// <param name="from">Orientation trước khi xoay.</param>
        /// <param name="to">Orientation sau khi xoay.</param>
        /// <returns>Năm offset kick tương ứng.</returns>
        private static IReadOnlyList<GridPoint> GetIKicks(PieceOrientation from, PieceOrientation to)
        {
            if (from == PieceOrientation.Spawn && to == PieceOrientation.Right)
            {
                return Points((0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2));
            }

            if (from == PieceOrientation.Right && to == PieceOrientation.Spawn)
            {
                return Points((0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2));
            }

            if (from == PieceOrientation.Right && to == PieceOrientation.Reverse)
            {
                return Points((0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1));
            }

            if (from == PieceOrientation.Reverse && to == PieceOrientation.Right)
            {
                return Points((0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1));
            }

            if (from == PieceOrientation.Reverse && to == PieceOrientation.Left)
            {
                return Points((0, 0), (2, 0), (-1, 0), (2, 1), (-1, -2));
            }

            if (from == PieceOrientation.Left && to == PieceOrientation.Reverse)
            {
                return Points((0, 0), (-2, 0), (1, 0), (-2, -1), (1, 2));
            }

            if (from == PieceOrientation.Left && to == PieceOrientation.Spawn)
            {
                return Points((0, 0), (1, 0), (-2, 0), (1, -2), (-2, 1));
            }

            if (from == PieceOrientation.Spawn && to == PieceOrientation.Left)
            {
                return Points((0, 0), (-1, 0), (2, 0), (-1, 2), (2, -1));
            }

            throw new ArgumentOutOfRangeException(nameof(to), $"Chuyển orientation SRS I không hợp lệ: {from} → {to}.");
        }

        /// <summary>
        /// Chuyển các cặp số nguyên thành danh sách offset bất biến về mặt API.
        /// </summary>
        /// <param name="points">Các cặp X, Y theo thứ tự kick.</param>
        /// <returns>Danh sách GridPoint chỉ đọc.</returns>
        private static IReadOnlyList<GridPoint> Points(params (int x, int y)[] points)
        {
            var result = new GridPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = new GridPoint(points[i].x, points[i].y);
            }

            return Array.AsReadOnly(result);
        }
    }
}
