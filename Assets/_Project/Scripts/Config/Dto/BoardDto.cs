using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO kích thước bảng trong JSON, các trường chỉ đọc và gán qua constructor.
    /// </summary>
    public sealed class BoardDto
    {
        /// <summary>Số cột.</summary>
        public readonly int width;

        /// <summary>Số hàng hiển thị.</summary>
        public readonly int visibleHeight;

        /// <summary>Số hàng ẩn phía trên.</summary>
        public readonly int hiddenRows;

        /// <summary>
        /// Constructor để Newtonsoft gán kích thước bảng từ JSON.
        /// </summary>
        /// <param name="width">Số cột.</param>
        /// <param name="visibleHeight">Số hàng hiển thị.</param>
        /// <param name="hiddenRows">Số hàng ẩn.</param>
        [JsonConstructor]
        public BoardDto(int width, int visibleHeight, int hiddenRows)
        {
            this.width = width;
            this.visibleHeight = visibleHeight;
            this.hiddenRows = hiddenRows;
        }
    }
}
