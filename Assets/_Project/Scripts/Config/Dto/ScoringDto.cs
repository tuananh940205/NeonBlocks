using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO luật điểm trong JSON. Trường chỉ đọc, gán qua constructor.
    /// </summary>
    public sealed class ScoringDto
    {
        /// <summary>Điểm cho 1, 2, 3, 4 hàng xóa cùng lúc.</summary>
        public readonly int[] lineClear;

        /// <summary>Điểm mỗi ô soft drop.</summary>
        public readonly int softDropPerCell;

        /// <summary>Điểm mỗi ô hard drop.</summary>
        public readonly int hardDropPerCell;

        /// <summary>
        /// Constructor để Newtonsoft gán luật điểm từ JSON.
        /// </summary>
        /// <param name="lineClear">Điểm xóa 1..4 hàng.</param>
        /// <param name="softDropPerCell">Điểm soft drop mỗi ô.</param>
        /// <param name="hardDropPerCell">Điểm hard drop mỗi ô.</param>
        [JsonConstructor]
        public ScoringDto(int[] lineClear, int softDropPerCell, int hardDropPerCell)
        {
            this.lineClear = lineClear;
            this.softDropPerCell = softDropPerCell;
            this.hardDropPerCell = hardDropPerCell;
        }
    }
}
