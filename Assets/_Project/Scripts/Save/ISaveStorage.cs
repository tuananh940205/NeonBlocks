using System.Threading.Tasks;
namespace NeonBlocks.Save
{
    /// <summary>Backend lưu trữ inject được để test không dùng file người chơi.</summary>
    public interface ISaveStorage
    {
        /// <summary>Đọc save, null khi file chưa tồn tại.</summary>
        Task<string> ReadAsync();
        /// <summary>Ghi thay thế nguyên tử, giữ bản cũ nếu thao tác thất bại.</summary>
        Task WriteAtomicAsync(string content);
    }
}