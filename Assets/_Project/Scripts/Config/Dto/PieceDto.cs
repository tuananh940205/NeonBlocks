using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO một khối tetromino trong JSON: mã khối, bốn ô trong khung 4×4 và tâm xoay. Trường chỉ đọc, gán qua constructor.
    /// </summary>
    public sealed class PieceDto
    {
        /// <summary>Mã khối, ví dụ "I".</summary>
        public readonly string id;

        /// <summary>Bốn ô của khối trong khung 4×4.</summary>
        public readonly GridPointDto[] cells;

        /// <summary>Tâm xoay trong khung 4×4.</summary>
        public readonly FloatPointDto rotationCenter;

        /// <summary>
        /// Constructor để Newtonsoft gán dữ liệu một khối từ JSON.
        /// </summary>
        /// <param name="id">Mã khối.</param>
        /// <param name="cells">Các ô của khối.</param>
        /// <param name="rotationCenter">Tâm xoay.</param>
        [JsonConstructor]
        public PieceDto(string id, GridPointDto[] cells, FloatPointDto rotationCenter)
        {
            this.id = id;
            this.cells = cells;
            this.rotationCenter = rotationCenter;
        }
    }
}
