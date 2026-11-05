using System;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using UnityEngine;

namespace NeonBlocks.Config
{
    /// <summary>
    /// Manager cấu hình dùng chung của session: tải TextAsset GameConfig qua AddressableManager, parse bằng GameConfigParser
    /// và giữ GameConfig bất biến cho đến khi session kết thúc. Không kế thừa MonoBehaviour, không có static Instance.
    /// Lỗi tải ném AddressableLoadException; lỗi dữ liệu ném JsonException hoặc ConfigValidationException (theo GameConfigParser).
    /// </summary>
    public sealed class ConfigManager : IDisposable
    {
        private readonly AddressableManager assets;
        private readonly AssetOwnerToken sessionOwner = new AssetOwnerToken("Session:Config");
        private GameConfig config;

        /// <summary>True khi cấu hình đã tải và parse thành công.</summary>
        public bool IsReady => config != null;

        /// <summary>
        /// Cấu hình đã tải; ném InvalidOperationException nếu chưa gọi LoadAsync thành công.
        /// </summary>
        public GameConfig Config => config ?? throw new InvalidOperationException("Cấu hình chưa sẵn sàng; cần gọi LoadAsync trước.");

        /// <summary>
        /// Tạo manager với AddressableManager đã được khởi tạo trong session.
        /// </summary>
        /// <param name="assets">AddressableManager dùng để tải TextAsset.</param>
        public ConfigManager(AddressableManager assets)
        {
            this.assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        /// <summary>
        /// Tải TextAsset có key GameConfigParser.AddressKey, parse và kiểm tra dữ liệu. Gọi lại khi đã sẵn sàng thì không làm gì.
        /// TextAsset được giữ đến khi session kết thúc; GameConfig không chứa tham chiếu Unity nên không cần giữ handle thêm.
        /// </summary>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Task hoàn tất khi cấu hình đã sẵn sàng.</returns>
        public async Task LoadAsync(CancellationToken ct)
        {
            if (config != null)
            {
                return;
            }

            TextAsset text = await assets.LoadAssetAsync<TextAsset>(GameConfigParser.AddressKey, sessionOwner, ct);
            config = GameConfigParser.Parse(text.text);
        }

        /// <summary>
        /// Giải phóng TextAsset của session và xóa cấu hình. Gọi nhiều lần an toàn.
        /// </summary>
        public void Dispose()
        {
            config = null;
            _ = assets.ReleaseOwnerAsync(sessionOwner);
        }
    }
}
