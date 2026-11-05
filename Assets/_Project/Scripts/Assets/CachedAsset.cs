using System.Threading.Tasks;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Bản ghi runtime của một request asset dùng chung; chỉ AddressableManager được thay đổi trạng thái giải phóng.
    /// </summary>
    internal sealed class CachedAsset
    {
        private int waiterCount;

        /// <summary>Task dùng chung cho mọi yêu cầu cùng khóa trong scope.</summary>
        public Task<AddressableResult> Load { get; }

        /// <summary>True sau khi handle của kết quả thành công đã được giải phóng.</summary>
        public bool IsReleased { get; private set; }

        /// <summary>
        /// Tạo bản ghi cho một request backend vừa bắt đầu.
        /// </summary>
        /// <param name="load">Task trả asset và handle từ backend.</param>
        public CachedAsset(Task<AddressableResult> load)
        {
            Load = load;
        }

        /// <summary>
        /// Đánh dấu handle đã được giải phóng để chặn release trùng.
        /// </summary>
        /// <returns>True nếu caller này được quyền release handle.</returns>
        public bool TryMarkReleased()
        {
            if (IsReleased)
            {
                return false;
            }

            IsReleased = true;
            return true;
        }

        /// <summary>
        /// Ghi nhận một caller đang chờ request dùng chung để hủy một caller không hủy request của caller khác.
        /// </summary>
        public void AddWaiter()
        {
            waiterCount++;
        }

        /// <summary>
        /// Gỡ một caller khỏi request dùng chung.
        /// </summary>
        /// <returns>Số caller còn chờ sau khi gỡ.</returns>
        public int RemoveWaiter()
        {
            waiterCount--;
            return waiterCount;
        }
    }
}
