using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO một level trong JSON: số level và thời gian rơi một ô. Trường chỉ đọc, gán qua constructor.
    /// </summary>
    public sealed class LevelDto
    {
        /// <summary>Số level.</summary>
        public readonly int level;

        /// <summary>Thời gian rơi một ô, tính bằng giây.</summary>
        public readonly float gravitySeconds;

        /// <summary>
        /// Constructor để Newtonsoft gán dữ liệu level từ JSON.
        /// </summary>
        /// <param name="level">Số level.</param>
        /// <param name="gravitySeconds">Thời gian rơi một ô.</param>
        [JsonConstructor]
        public LevelDto(int level, float gravitySeconds)
        {
            this.level = level;
            this.gravitySeconds = gravitySeconds;
        }
    }
}
