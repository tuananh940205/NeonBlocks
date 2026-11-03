namespace NeonBlocks.Config
{
    /// <summary>
    /// Tốc độ rơi của một level: thời gian (giây) để khối rơi một ô. Bất biến, chỉ set qua constructor.
    /// Chỉ chứa số liệu luật chơi; khi có asset theo level sẽ lưu address string, không lưu tham chiếu asset.
    /// </summary>
    public sealed class LevelData
    {
        /// <summary>Số thứ tự level, bắt đầu từ startLevel của cấu hình.</summary>
        public int Level { get; }

        /// <summary>Thời gian rơi một ô ở level này, tính bằng giây; luôn &gt; 0.</summary>
        public float GravitySeconds { get; }

        /// <summary>
        /// Tạo dữ liệu level và kiểm tra số level và tốc độ rơi hợp lệ.
        /// </summary>
        /// <param name="level">Số level, phải ≥ 1.</param>
        /// <param name="gravitySeconds">Thời gian rơi một ô, phải &gt; 0.</param>
        public LevelData(int level, float gravitySeconds)
        {
            if (level < 1)
            {
                throw new ConfigValidationException($"levels.level phải ≥ 1; giá trị hiện tại: {level}.");
            }

            if (gravitySeconds <= 0f)
            {
                throw new ConfigValidationException($"levels[{level}].gravitySeconds phải > 0; giá trị hiện tại: {gravitySeconds}.");
            }

            Level = level;
            GravitySeconds = gravitySeconds;
        }
    }
}
