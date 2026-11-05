using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Manager thuần C# duy nhất được phép giữ và giải phóng handle Addressables (cùng SceneAssetScope đi qua nó).
    /// Mỗi tài nguyên được gắn với một AssetOwnerToken; giải phóng theo chủ sở hữu, mỗi handle đúng một lần.
    /// Yêu cầu tải bị hủy hoặc chủ sở hữu kết thúc giữa chừng thì kết quả đến muộn cũng được giải phóng.
    /// Không kế thừa MonoBehaviour và không có static Instance; được đăng ký qua SessionManager.
    /// </summary>
    public sealed class AddressableManager : IDisposable
    {
        private readonly IAddressableBackend backend;
        private readonly Dictionary<AssetOwnerToken, List<TrackedAsset>> ownedAssets = new Dictionary<AssetOwnerToken, List<TrackedAsset>>();
        private readonly Dictionary<AssetOwnerToken, Dictionary<AssetCacheKey, CachedAsset>> cachedAssets = new Dictionary<AssetOwnerToken, Dictionary<AssetCacheKey, CachedAsset>>();
        private bool isDisposed;

        /// <summary>Số tài nguyên đang được giữ trên tất cả chủ sở hữu.</summary>
        public int TrackedCount
        {
            get
            {
                int count = 0;
                foreach (List<TrackedAsset> list in ownedAssets.Values)
                {
                    count += list.Count;
                }

                foreach (Dictionary<AssetCacheKey, CachedAsset> cache in cachedAssets.Values)
                {
                    count += cache.Count;
                }

                return count;
            }
        }

        /// <summary>
        /// Tạo manager với backend Addressables; backend được truyền vào để có thể thay bằng bản giả khi test.
        /// </summary>
        /// <param name="backend">Backend thực hiện thao tác Addressables, không được null.</param>
        public AddressableManager(IAddressableBackend backend)
        {
            this.backend = backend ?? throw new ArgumentNullException(nameof(backend));
        }

        /// <summary>
        /// Tải asset theo key và gắn handle vào chủ sở hữu. Ném AddressableLoadException khi key rỗng, tải lỗi hoặc sai kiểu;
        /// ném OperationCanceledException khi bị hủy hoặc chủ sở hữu kết thúc trong lúc tải.
        /// </summary>
        /// <typeparam name="T">Kiểu asset mong đợi.</typeparam>
        /// <param name="key">Address key của asset.</param>
        /// <param name="owner">Chủ sở hữu sẽ giữ handle.</param>
        /// <param name="ct">Token hủy yêu cầu.</param>
        /// <returns>Asset đã tải.</returns>
        public async Task<T> LoadAssetAsync<T>(string key, AssetOwnerToken owner, CancellationToken ct) where T : class
        {
            ValidateRequest(key, owner);
            var cacheKey = new AssetCacheKey(key, typeof(T));
            CachedAsset cached = GetOrCreateCachedAsset(owner, cacheKey);
            AddressableResult result;
            bool isCancelled = false;
            cached.AddWaiter();

            try
            {
                result = await AwaitCachedAssetAsync(cached, owner, ct);
            }
            catch (OperationCanceledException)
            {
                isCancelled = true;
                throw;
            }
            finally
            {
                if (isCancelled && cached.RemoveWaiter() == 0)
                {
                    RemoveCachedAsset(owner, cacheKey, cached);
                    ReleaseCachedAsset(cached);
                }
                else if (!isCancelled)
                {
                    cached.RemoveWaiter();
                }
            }

            if (!(result.Value is T value))
            {
                RemoveCachedAsset(owner, cacheKey, cached);
                ReleaseCachedAsset(cached);
                throw new AddressableLoadException($"Asset '{key}' không đúng kiểu {typeof(T).Name}.");
            }

            return value;
        }

        /// <summary>
        /// Tạo instance thuần túy từ prefab cached theo key và gắn vào chủ sở hữu; giải phóng bằng Destroy.
        /// </summary>
        /// <param name="key">Address key của prefab.</param>
        /// <param name="parent">Transform cha, có thể null.</param>
        /// <param name="owner">Chủ sở hữu instance.</param>
        /// <param name="ct">Token hủy yêu cầu.</param>
        /// <returns>GameObject instance vừa tạo.</returns>
        public async Task<GameObject> InstantiateAsync(string key, Transform parent, AssetOwnerToken owner, CancellationToken ct)
        {
            ValidateRequest(key, owner);
            GameObject prefab = await LoadAssetAsync<GameObject>(key, owner, ct);
            GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);

            Track(owner, new TrackedAsset(TrackedKind.Instance, key, null, instance));
            return instance;
        }

        /// <summary>
        /// Tải scene Addressables theo key (Additive) và gắn vào chủ sở hữu; unload bằng ReleaseSceneAsync hoặc ReleaseOwnerAsync.
        /// </summary>
        /// <param name="key">Address key của scene.</param>
        /// <param name="owner">Chủ sở hữu scene.</param>
        /// <param name="ct">Token hủy yêu cầu.</param>
        /// <returns>Tham chiếu tới scene đã tải.</returns>
        public async Task<LoadedScene> LoadSceneAsync(string key, AssetOwnerToken owner, CancellationToken ct)
        {
            ValidateRequest(key, owner);
            Task<AddressableResult> load = backend.LoadSceneAsync(key, ct);
            AddressableResult result = await AwaitOwnedAsync(load, owner, TrackedKind.Scene, ct);

            Track(owner, new TrackedAsset(TrackedKind.Scene, key, result.Handle, null));
            return new LoadedScene(key, result.Handle);
        }

        /// <summary>
        /// Hủy một instance thuần túy đã tạo bằng InstantiateAsync. Gọi lần thứ hai hoặc với instance không được quản lý thì trả false.
        /// </summary>
        /// <param name="instance">GameObject instance cần giải phóng.</param>
        /// <returns>True nếu instance được giải phóng trong lần gọi này.</returns>
        public bool ReleaseInstance(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            AssetOwnerToken foundOwner = null;
            int foundIndex = -1;

            foreach (KeyValuePair<AssetOwnerToken, List<TrackedAsset>> pair in ownedAssets)
            {
                foundIndex = pair.Value.FindIndex(t => t.Kind == TrackedKind.Instance && ReferenceEquals(t.Instance, instance));
                if (foundIndex >= 0)
                {
                    foundOwner = pair.Key;
                    break;
                }
            }

            if (foundOwner == null)
            {
                return false;
            }

            RemoveTracked(foundOwner, foundIndex);
            DestroyInstance(instance);
            return true;
        }

        /// <summary>
        /// Unload một scene cụ thể và gỡ khỏi chủ sở hữu. Gọi lần thứ hai thì trả false và không unload lại.
        /// </summary>
        /// <param name="scene">Scene cần unload.</param>
        /// <returns>True nếu scene được unload trong lần gọi này.</returns>
        public async Task<bool> ReleaseSceneAsync(LoadedScene scene)
        {
            if (scene == null)
            {
                return false;
            }

            AssetOwnerToken foundOwner = null;
            int foundIndex = -1;

            foreach (KeyValuePair<AssetOwnerToken, List<TrackedAsset>> pair in ownedAssets)
            {
                foundIndex = pair.Value.FindIndex(t => t.Kind == TrackedKind.Scene && ReferenceEquals(t.Handle, scene.Handle));
                if (foundIndex >= 0)
                {
                    foundOwner = pair.Key;
                    break;
                }
            }

            if (foundOwner == null)
            {
                return false;
            }

            RemoveTracked(foundOwner, foundIndex);
            await backend.UnloadSceneAsync(scene.Handle);
            return true;
        }

        /// <summary>
        /// Kết thúc chủ sở hữu: giải phóng asset và instance ngay, sau đó unload các scene của chủ sở hữu.
        /// Gọi nhiều lần an toàn; lần sau không làm gì vì danh sách đã được gỡ.
        /// </summary>
        /// <param name="owner">Chủ sở hữu cần kết thúc.</param>
        /// <returns>Task hoàn tất khi mọi scene của chủ sở hữu đã unload.</returns>
        public async Task ReleaseOwnerAsync(AssetOwnerToken owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            List<object> scenes = ReleaseOwnerNow(owner);

            foreach (object scene in scenes)
            {
                await backend.UnloadSceneAsync(scene);
            }
        }

        /// <summary>
        /// Giải phóng toàn bộ tài nguyên của mọi chủ sở hữu và từ chối yêu cầu mới. Việc unload scene được khởi động
        /// nhưng không chờ hoàn tất vì Dispose là đồng bộ; gọi nhiều lần an toàn.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            var owners = new HashSet<AssetOwnerToken>(ownedAssets.Keys);
            foreach (AssetOwnerToken owner in cachedAssets.Keys)
            {
                owners.Add(owner);
            }

            foreach (AssetOwnerToken owner in owners)
            {
                foreach (object scene in ReleaseOwnerNow(owner))
                {
                    _ = backend.UnloadSceneAsync(scene);
                }
            }
        }

        /// <summary>
        /// Kiểm tra yêu cầu mới trước khi gọi backend: manager chưa dispose, chủ sở hữu còn hiệu lực, key không rỗng.
        /// </summary>
        /// <param name="key">Address key.</param>
        /// <param name="owner">Chủ sở hữu.</param>
        private void ValidateRequest(string key, AssetOwnerToken owner)
        {
            if (isDisposed)
            {
                throw new InvalidOperationException("AddressableManager đã dispose; không nhận yêu cầu mới.");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (owner.IsReleased)
            {
                throw new InvalidOperationException($"Chủ sở hữu '{owner.Label}' đã kết thúc; không nhận yêu cầu mới.");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new AddressableLoadException("Address key không được rỗng.");
            }
        }

        /// <summary>
        /// Chờ kết quả tải hoặc hủy. Nếu bị hủy, kết quả đến sau sẽ được giải phóng ngay khi hoàn tất.
        /// Nếu chủ sở hữu đã kết thúc khi tải xong, giải phóng kết quả và ném OperationCanceledException.
        /// </summary>
        /// <param name="load">Task tải từ backend.</param>
        /// <param name="owner">Chủ sở hữu yêu cầu.</param>
        /// <param name="kind">Loại tài nguyên, dùng để giải phóng đúng cách.</param>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Kết quả tải còn hiệu lực.</returns>
        private async Task<AddressableResult> AwaitOwnedAsync(Task<AddressableResult> load, AssetOwnerToken owner, TrackedKind kind, CancellationToken ct)
        {
            var cancelled = new TaskCompletionSource<bool>();

            using (ct.Register(() => cancelled.TrySetResult(true)))
            {
                Task finished = await Task.WhenAny(load, cancelled.Task);

                if (finished != load)
                {
                    ReleaseWhenCompleted(load, kind);
                    throw new OperationCanceledException("Yêu cầu tải Addressables đã bị hủy.", ct);
                }
            }

            AddressableResult result = await load;

            if (owner.IsReleased || isDisposed)
            {
                ReleaseResult(result, kind);
                throw new OperationCanceledException($"Chủ sở hữu '{owner.Label}' đã kết thúc trong lúc tải.");
            }

            return result;
        }

        /// <summary>
        /// Khi yêu cầu đã hủy mà backend vẫn trả kết quả sau đó, giải phóng kết quả đó; lỗi chỉ được đọc để đánh dấu đã quan sát.
        /// </summary>
        /// <param name="load">Task tải đang chạy.</param>
        /// <param name="kind">Loại tài nguyên.</param>
        private void ReleaseWhenCompleted(Task<AddressableResult> load, TrackedKind kind)
        {
            load.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    ReleaseResult(t.Result, kind);
                }
                else
                {
                    _ = t.Exception;
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Giải phóng một kết quả chưa được ghi nhận theo đúng cách của loại tài nguyên.
        /// </summary>
        /// <param name="result">Kết quả cần giải phóng.</param>
        /// <param name="kind">Loại tài nguyên.</param>
        private void ReleaseResult(AddressableResult result, TrackedKind kind)
        {
            switch (kind)
            {
                case TrackedKind.Asset:
                    backend.ReleaseAsset(result.Handle);
                    break;
                case TrackedKind.Instance:
                    DestroyInstance((GameObject)result.Value);
                    break;
                case TrackedKind.Scene:
                    _ = backend.UnloadSceneAsync(result.Handle);
                    break;
            }
        }

        /// <summary>
        /// Ghi nhận một tài nguyên mới vào chủ sở hữu.
        /// </summary>
        /// <param name="owner">Chủ sở hữu.</param>
        /// <param name="tracked">Bản ghi tài nguyên.</param>
        private void Track(AssetOwnerToken owner, TrackedAsset tracked)
        {
            if (!ownedAssets.TryGetValue(owner, out List<TrackedAsset> list))
            {
                list = new List<TrackedAsset>();
                ownedAssets.Add(owner, list);
            }

            list.Add(tracked);
        }

        /// <summary>
        /// Gỡ một bản ghi khỏi chủ sở hữu và xóa chủ sở hữu khi không còn tài nguyên nào.
        /// </summary>
        /// <param name="owner">Chủ sở hữu.</param>
        /// <param name="index">Vị trí bản ghi trong danh sách.</param>
        private void RemoveTracked(AssetOwnerToken owner, int index)
        {
            List<TrackedAsset> list = ownedAssets[owner];
            list.RemoveAt(index);

            if (list.Count == 0)
            {
                ownedAssets.Remove(owner);
            }
        }

        /// <summary>
        /// Đánh dấu chủ sở hữu đã kết thúc, gỡ toàn bộ tài nguyên của họ và giải phóng asset/instance ngay.
        /// Trả về danh sách handle scene cần unload bất đồng bộ.
        /// </summary>
        /// <param name="owner">Chủ sở hữu cần kết thúc.</param>
        /// <returns>Danh sách handle scene chờ unload.</returns>
        private List<object> ReleaseOwnerNow(AssetOwnerToken owner)
        {
            owner.MarkReleased();
            var scenes = new List<object>();

            if (!ownedAssets.TryGetValue(owner, out List<TrackedAsset> list))
            {
                ReleaseCachedAssets(owner);
                return scenes;
            }

            ownedAssets.Remove(owner);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                TrackedAsset tracked = list[i];

                switch (tracked.Kind)
                {
                    case TrackedKind.Asset:
                        backend.ReleaseAsset(tracked.Handle);
                        break;
                    case TrackedKind.Instance:
                        DestroyInstance(tracked.Instance);
                        break;
                    case TrackedKind.Scene:
                        scenes.Add(tracked.Handle);
                        break;
                }
            }

            ReleaseCachedAssets(owner);

            return scenes;
        }

        /// <summary>
        /// Lấy request cached theo owner hoặc tạo một request backend mới duy nhất cho khóa đó.
        /// </summary>
        /// <param name="owner">Chủ sở hữu của cache.</param>
        /// <param name="key">Khóa asset và kiểu.</param>
        /// <returns>Bản ghi request dùng chung.</returns>
        private CachedAsset GetOrCreateCachedAsset(AssetOwnerToken owner, AssetCacheKey key)
        {
            if (!cachedAssets.TryGetValue(owner, out Dictionary<AssetCacheKey, CachedAsset> ownerCache))
            {
                ownerCache = new Dictionary<AssetCacheKey, CachedAsset>();
                cachedAssets.Add(owner, ownerCache);
            }

            if (ownerCache.TryGetValue(key, out CachedAsset cached))
            {
                return cached;
            }

            cached = new CachedAsset(backend.LoadAssetAsync(key.Key, key.AssetType, CancellationToken.None));
            ownerCache.Add(key, cached);
            RemoveFailedCachedAssetWhenComplete(owner, key, cached);
            return cached;
        }

        /// <summary>
        /// Chờ request cached mà không hủy request dùng chung khi chỉ một caller hủy chờ đợi.
        /// Nếu owner đã kết thúc trước khi kết quả đến, handle được giải phóng đúng một lần.
        /// </summary>
        /// <param name="cached">Request cached dùng chung.</param>
        /// <param name="owner">Chủ sở hữu của request.</param>
        /// <param name="ct">Token hủy của caller.</param>
        /// <returns>Kết quả asset còn hiệu lực.</returns>
        private async Task<AddressableResult> AwaitCachedAssetAsync(CachedAsset cached, AssetOwnerToken owner, CancellationToken ct)
        {
            var cancelled = new TaskCompletionSource<bool>();

            using (ct.Register(() => cancelled.TrySetResult(true)))
            {
                Task finished = await Task.WhenAny(cached.Load, cancelled.Task);
                if (finished != cached.Load)
                {
                    throw new OperationCanceledException("Yêu cầu tải Addressables đã bị hủy.", ct);
                }
            }

            AddressableResult result = await cached.Load;
            if (owner.IsReleased || isDisposed)
            {
                ReleaseCachedAsset(cached);
                throw new OperationCanceledException($"Chủ sở hữu '{owner.Label}' đã kết thúc trong lúc tải.");
            }

            return result;
        }

        /// <summary>
        /// Bỏ một entry cache chỉ khi nó vẫn là entry hiện tại của khóa đó.
        /// </summary>
        /// <param name="owner">Chủ sở hữu cache.</param>
        /// <param name="key">Khóa cần gỡ.</param>
        /// <param name="cached">Entry cần khớp trước khi gỡ.</param>
        private void RemoveCachedAsset(AssetOwnerToken owner, AssetCacheKey key, CachedAsset cached)
        {
            if (!cachedAssets.TryGetValue(owner, out Dictionary<AssetCacheKey, CachedAsset> ownerCache) ||
                !ownerCache.TryGetValue(key, out CachedAsset current) || !ReferenceEquals(current, cached))
            {
                return;
            }

            ownerCache.Remove(key);
            if (ownerCache.Count == 0)
            {
                cachedAssets.Remove(owner);
            }
        }

        /// <summary>
        /// Gỡ cache thất bại để lần tải sau có thể thử lại; không có handle thành công để giải phóng.
        /// </summary>
        /// <param name="owner">Chủ sở hữu cache.</param>
        /// <param name="key">Khóa cache.</param>
        /// <param name="cached">Request cần quan sát.</param>
        private void RemoveFailedCachedAssetWhenComplete(AssetOwnerToken owner, AssetCacheKey key, CachedAsset cached)
        {
            cached.Load.ContinueWith(task =>
            {
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    RemoveCachedAsset(owner, key, cached);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Giải phóng handle của một entry cached sau khi request hoàn tất, tối đa một lần.
        /// </summary>
        /// <param name="cached">Entry chứa handle cần giải phóng.</param>
        private void ReleaseCachedAsset(CachedAsset cached)
        {
            if (!cached.TryMarkReleased())
            {
                return;
            }

            cached.Load.ContinueWith(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    backend.ReleaseAsset(task.Result.Handle);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Gỡ toàn bộ cache của owner sau khi các instance thuần túy của owner đã bị hủy.
        /// </summary>
        /// <param name="owner">Chủ sở hữu cache cần giải phóng.</param>
        private void ReleaseCachedAssets(AssetOwnerToken owner)
        {
            if (!cachedAssets.TryGetValue(owner, out Dictionary<AssetCacheKey, CachedAsset> ownerCache))
            {
                return;
            }

            cachedAssets.Remove(owner);
            foreach (CachedAsset cached in ownerCache.Values)
            {
                ReleaseCachedAsset(cached);
            }
        }

        /// <summary>
        /// Hủy GameObject được Instantiate thuần túy; EditMode dùng DestroyImmediate để test không giữ object rác.
        /// </summary>
        /// <param name="instance">Instance cần hủy.</param>
        private static void DestroyInstance(GameObject instance)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(instance);
                return;
            }

            UnityEngine.Object.DestroyImmediate(instance);
        }
    }
}
