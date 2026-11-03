using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO tọa độ nguyên trong JSON; mỗi ô là một object {x, y}. Trường chỉ đọc, gán qua constructor.
    /// </summary>
    public sealed class GridPointDto
    {
        /// <summary>Hoành độ, tăng sang phải.</summary>
        public readonly int x;

        /// <summary>Tung độ, tăng lên trên.</summary>
        public readonly int y;

        /// <summary>
        /// Constructor để Newtonsoft gán tọa độ từ JSON.
        /// </summary>
        /// <param name="x">Hoành độ.</param>
        /// <param name="y">Tung độ.</param>
        [JsonConstructor]
        public GridPointDto(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
