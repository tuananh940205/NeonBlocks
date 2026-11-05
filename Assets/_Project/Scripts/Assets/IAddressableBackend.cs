using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Lớp trừu tượng hóa các thao tác Addressables để AddressableManager có thể test bằng bản giả.
    /// Mọi lỗi tải ném AddressableLoadException. Chỉ AddressableManager được gọi giải phóng qua interface này.
    /// </summary>
    public interface IAddressableBackend
    {
        /// <summary>
        /// Tải một asset theo key, không giải phóng handle.
        /// </summary>
        /// <param name="key">Address key của asset.</param>
        /// <param name="assetType">Kiểu asset mong đợi, dùng để kiểm tra kết quả.</param>
        /// <param name="ct">Token hủy; backend có thể bỏ qua và để AddressableManager xử lý.</param>
        /// <returns>Giá trị và handle đã tải.</returns>
        Task<AddressableResult> LoadAssetAsync(string key, Type assetType, CancellationToken ct);

        /// <summary>
        /// Tải scene Addressables theo key bằng LoadSceneMode.Additive.
        /// </summary>
        /// <param name="key">Address key của scene.</param>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Kết quả chứa handle của scene.</returns>
        Task<AddressableResult> LoadSceneAsync(string key, CancellationToken ct);

        /// <summary>
        /// Giải phóng handle của asset đã tải bằng LoadAssetAsync.
        /// </summary>
        /// <param name="handle">Handle nhận từ LoadAssetAsync.</param>
        void ReleaseAsset(object handle);

        /// <summary>
        /// Unload scene đã tải bằng LoadSceneAsync và giải phóng handle của nó.
        /// </summary>
        /// <param name="handle">Handle scene nhận từ LoadSceneAsync.</param>
        /// <returns>Task hoàn tất khi scene đã unload.</returns>
        Task UnloadSceneAsync(object handle);
    }
}
