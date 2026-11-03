using System;

namespace NeonBlocks.Config
{
    /// <summary>
    /// Ném khi dữ liệu cấu hình không hợp lệ (thiếu trường, giá trị âm, shape sai, trùng id...).
    /// </summary>
    public sealed class ConfigValidationException : Exception
    {
        /// <summary>
        /// Tạo ngoại lệ cấu hình với thông điệp mô tả lỗi bằng tiếng Việt.
        /// </summary>
        /// <param name="message">Mô tả trường hoặc quy tắc bị vi phạm.</param>
        public ConfigValidationException(string message) : base(message)
        {
        }
    }
}
