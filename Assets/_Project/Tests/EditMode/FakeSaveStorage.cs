using System.IO;
using System.Threading.Tasks;
using NeonBlocks.Save;
namespace NeonBlocks.Tests
{
    /// <summary>Storage giả giữ nội dung khi write thất bại.</summary>
    public sealed class FakeSaveStorage : ISaveStorage
    {
        public string Content;
        public bool FailRead, FailWrite;
        public int Writes;
        /// <summary>Giả lỗi I/O hoặc trả nội dung hiện có.</summary>
        public Task<string> ReadAsync() => FailRead ? Task.FromException<string>(new IOException("read failed")) : Task.FromResult(Content);
        /// <summary>Chỉ thay Content khi ghi thành công.</summary>
        public Task WriteAtomicAsync(string value)
        {
            if (FailWrite) { return Task.FromException(new IOException("write failed")); }
            Content = value; Writes++; return Task.CompletedTask;
        }
    }
}