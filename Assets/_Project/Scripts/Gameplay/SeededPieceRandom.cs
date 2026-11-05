using System;

namespace NeonBlocks.Gameplay
{
    /// <summary>
    /// Nguồn số ngẫu nhiên thuần C# có seed cố định, dùng cho thứ tự 7-bag tái lập được.
    /// </summary>
    public sealed class SeededPieceRandom : IPieceRandom
    {
        private readonly Random random;

        /// <summary>
        /// Tạo nguồn ngẫu nhiên bằng seed do caller chỉ định.
        /// </summary>
        /// <param name="seed">Seed quyết định chuỗi số trả về.</param>
        public SeededPieceRandom(int seed)
        {
            random = new Random(seed);
        }

        /// <summary>
        /// Trả số ngẫu nhiên trong khoảng hợp lệ bằng System.Random.
        /// </summary>
        /// <param name="maxExclusive">Cận trên độc quyền.</param>
        /// <returns>Số nguyên từ 0 đến nhỏ hơn maxExclusive.</returns>
        public int Next(int maxExclusive)
        {
            return random.Next(maxExclusive);
        }
    }
}
