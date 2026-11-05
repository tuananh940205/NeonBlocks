using System;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Ném khi một yêu cầu Addressables thất bại: key rỗng, key không tồn tại, asset sai kiểu, tải lỗi hoặc không unload được scene.
    /// Đây là loại exception được tài liệu hóa cho mọi lỗi của AddressableManager và backend.
    /// </summary>
    public sealed class AddressableLoadException : Exception
    {
        /// <summary>
        /// Tạo exception với thông điệp mô tả lỗi bằng tiếng Việt và nguyên nhân gốc nếu có.
        /// </summary>
        /// <param name="message">Thông điệp mô tả key hoặc thao tác bị lỗi.</param>
        /// <param name="inner">Nguyên nhân gốc từ Addressables, có thể null.</param>
        public AddressableLoadException(string message, Exception inner = null) : base(message, inner)
        {
        }
    }
}
