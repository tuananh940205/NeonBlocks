using System;
using NeonBlocks.Gameplay;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// RNG giả trả một dãy giá trị cố định để kiểm thử thứ tự Fisher-Yates của SevenBagPieceSource.
    /// </summary>
    public sealed class FixedPieceRandom : IPieceRandom
    {
        private readonly int[] values;
        private int index;

        /// <summary>
        /// Tạo RNG giả từ dãy giá trị tuần tự.
        /// </summary>
        /// <param name="values">Các giá trị sẽ trả theo thứ tự, không được rỗng.</param>
        public FixedPieceRandom(params int[] values)
        {
            this.values = values ?? throw new ArgumentNullException(nameof(values));
            if (values.Length == 0)
            {
                throw new ArgumentException("RNG giả cần ít nhất một giá trị.", nameof(values));
            }
        }

        /// <summary>
        /// Trả giá trị tiếp theo và kiểm tra nó nằm trong cận Fisher-Yates được yêu cầu.
        /// </summary>
        /// <param name="maxExclusive">Cận trên độc quyền của lần gọi hiện tại.</param>
        /// <returns>Giá trị giả tiếp theo.</returns>
        public int Next(int maxExclusive)
        {
            int value = values[index % values.Length];
            index++;
            if (value < 0 || value >= maxExclusive)
            {
                throw new InvalidOperationException("Giá trị RNG giả nằm ngoài cận yêu cầu.");
            }

            return value;
        }
    }
}
