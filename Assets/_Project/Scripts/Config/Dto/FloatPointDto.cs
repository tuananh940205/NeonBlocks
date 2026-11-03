using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO tọa độ thập phân trong JSON, dùng cho tâm xoay của khối. Trường chỉ đọc, gán qua constructor.
    /// </summary>
    public sealed class FloatPointDto
    {
        /// <summary>Hoành độ tâm xoay.</summary>
        public readonly float x;

        /// <summary>Tung độ tâm xoay.</summary>
        public readonly float y;

        /// <summary>
        /// Constructor để Newtonsoft gán tâm xoay từ JSON.
        /// </summary>
        /// <param name="x">Hoành độ tâm xoay.</param>
        /// <param name="y">Tung độ tâm xoay.</param>
        [JsonConstructor]
        public FloatPointDto(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
