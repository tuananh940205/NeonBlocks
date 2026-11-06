namespace NeonBlocks.Save
{
    /// <summary>Kết quả bất biến để UI thông báo thay vì nuốt lỗi lưu.</summary>
    public sealed class SaveResult
    {
        public SaveStatus Status { get; }
        public string Message { get; }
        public bool Success => Status == SaveStatus.Loaded || Status == SaveStatus.Missing || Status == SaveStatus.Saved;
        /// <summary>Đóng gói trạng thái và thông báo có thể hiển thị.</summary>
        public SaveResult(SaveStatus status, string message = "") { Status = status; Message = message; }
    }
}