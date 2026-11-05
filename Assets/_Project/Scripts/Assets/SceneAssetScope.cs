using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Phạm vi tài nguyên của một scene: giữ scene Addressables và mọi asset/instance tải cho scene đó.
    /// Là object thuần C#, không phải singleton; SceneFlowManager tạo mỗi lần vào scene và gọi ReleaseAsync khi rời scene.
    /// Không gọi Addressables trực tiếp; mọi thao tác đi qua AddressableManager.
    /// </summary>
    public sealed class SceneAssetScope
    {
        private readonly AddressableManager assets;
        private LoadedScene loadedScene;

        /// <summary>Address key của scene thuộc phạm vi này.</summary>
        public string SceneKey { get; }

        /// <summary>Chủ sở hữu dùng để gom mọi handle của scene.</summary>
        public AssetOwnerToken Owner { get; }

        /// <summary>True khi phạm vi đã được giải phóng.</summary>
        public bool IsReleased => Owner.IsReleased;

        /// <summary>
        /// Tạo phạm vi cho một scene.
        /// </summary>
        /// <param name="sceneKey">Address key của scene, không được rỗng.</param>
        /// <param name="assets">AddressableManager dùng để tải và giải phóng.</param>
        public SceneAssetScope(string sceneKey, AddressableManager assets)
        {
            if (string.IsNullOrWhiteSpace(sceneKey))
            {
                throw new System.ArgumentException("Scene key không được rỗng.", nameof(sceneKey));
            }

            SceneKey = sceneKey;
            this.assets = assets ?? throw new System.ArgumentNullException(nameof(assets));
            Owner = new AssetOwnerToken($"Scene:{sceneKey}");
        }

        /// <summary>
        /// Tải scene Addressables của phạm vi này; chỉ gọi một lần cho mỗi phạm vi.
        /// </summary>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Task hoàn tất khi scene đã tải.</returns>
        public async Task LoadSceneAsync(CancellationToken ct)
        {
            if (loadedScene != null)
            {
                throw new System.InvalidOperationException($"Scene '{SceneKey}' đã được tải trong phạm vi này.");
            }

            loadedScene = await assets.LoadSceneAsync(SceneKey, Owner, ct);
        }

        /// <summary>
        /// Tải asset cho scene này; asset được giải phóng cùng phạm vi.
        /// </summary>
        /// <typeparam name="T">Kiểu asset.</typeparam>
        /// <param name="key">Address key của asset.</param>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Asset đã tải.</returns>
        public Task<T> LoadAssetAsync<T>(string key, CancellationToken ct) where T : class
        {
            return assets.LoadAssetAsync<T>(key, Owner, ct);
        }

        /// <summary>
        /// Tạo instance thuần túy từ prefab cached cho scene này; instance bị destroy khi phạm vi kết thúc hoặc khi gọi ReleaseInstance.
        /// </summary>
        /// <param name="key">Address key của prefab.</param>
        /// <param name="parent">Transform cha, có thể null.</param>
        /// <param name="ct">Token hủy.</param>
        /// <returns>GameObject instance vừa tạo.</returns>
        public Task<GameObject> InstantiateAsync(string key, Transform parent, CancellationToken ct)
        {
            return assets.InstantiateAsync(key, parent, Owner, ct);
        }

        /// <summary>
        /// Kết thúc phạm vi: giải phóng asset và instance, unload scene. Gọi nhiều lần an toàn.
        /// </summary>
        /// <returns>Task hoàn tất khi scene đã unload.</returns>
        public Task ReleaseAsync()
        {
            loadedScene = null;
            return assets.ReleaseOwnerAsync(Owner);
        }
    }
}
