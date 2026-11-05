using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using UnityEngine;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Backend giả cho test: tải asset từ bảng dữ liệu, có thể giữ yêu cầu chưa hoàn tất để kiểm tra hủy và giao kết quả muộn,
    /// và ghi lại mọi lần giải phóng/unload để kiểm tra số lần đúng một.
    /// </summary>
    public sealed class FakeAddressableBackend : IAddressableBackend
    {
        private readonly List<(TaskCompletionSource<AddressableResult> source, AddressableResult result)> held =
            new List<(TaskCompletionSource<AddressableResult>, AddressableResult)>();
        private readonly List<GameObject> createdObjects = new List<GameObject>();
        private readonly Dictionary<object, string> sceneKeyByHandle = new Dictionary<object, string>();

        /// <summary>Asset có sẵn theo key.</summary>
        public Dictionary<string, UnityEngine.Object> Assets { get; } = new Dictionary<string, UnityEngine.Object>();

        /// <summary>Các key luôn tải lỗi (AddressableLoadException).</summary>
        public HashSet<string> FailingKeys { get; } = new HashSet<string>();

        /// <summary>Khi true, yêu cầu tải được giữ lại và chỉ hoàn tất khi test gọi CompleteHeld.</summary>
        public bool HoldLoads { get; set; }

        /// <summary>Handle asset đã được giải phóng, theo đúng thứ tự gọi (một handle có thể xuất hiện nhiều lần nếu release lặp).</summary>
        public List<object> ReleasedAssetHandles { get; } = new List<object>();

        /// <summary>Instance đã được giải phóng.</summary>
        public List<GameObject> ReleasedInstances { get; } = new List<GameObject>();

        /// <summary>Nhật ký thao tác scene theo thứ tự: "load:Key" và "unload:Key".</summary>
        public List<string> SceneLog { get; } = new List<string>();

        /// <summary>Số yêu cầu tải đang được giữ.</summary>
        public int HeldCount => held.Count;

        /// <summary>Số lần backend nhận yêu cầu LoadAssetAsync.</summary>
        public int LoadAssetRequestCount { get; private set; }

        /// <summary>
        /// Tải asset theo key: lỗi nếu key nằm trong FailingKeys, ngược lại trả asset đã cấu hình (hoặc giữ lại nếu HoldLoads).
        /// </summary>
        /// <param name="key">Address key.</param>
        /// <param name="assetType">Kiểu mong đợi; backend giả không kiểm tra.</param>
        /// <param name="ct">Token hủy, không dùng trong bản giả.</param>
        /// <returns>Kết quả tải.</returns>
        public Task<AddressableResult> LoadAssetAsync(string key, Type assetType, CancellationToken ct)
        {
            LoadAssetRequestCount++;

            if (FailingKeys.Contains(key) || !Assets.TryGetValue(key, out UnityEngine.Object asset))
            {
                return Task.FromException<AddressableResult>(new AddressableLoadException($"Không có asset '{key}'."));
            }

            return Deliver(new AddressableResult(asset, new object()));
        }

        /// <summary>
        /// Tạo GameObject giả cho prefab theo key.
        /// </summary>
        /// <param name="key">Address key của prefab.</param>
        /// <param name="parent">Transform cha, không dùng trong bản giả.</param>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Kết quả chứa GameObject giả.</returns>
        public Task<AddressableResult> InstantiateAsync(string key, Transform parent, CancellationToken ct)
        {
            if (FailingKeys.Contains(key))
            {
                return Task.FromException<AddressableResult>(new AddressableLoadException($"Không có prefab '{key}'."));
            }

            var go = new GameObject(key);
            createdObjects.Add(go);
            return Deliver(new AddressableResult(go, null));
        }

        /// <summary>
        /// Tải scene giả theo key và ghi "load:Key" vào nhật ký.
        /// </summary>
        /// <param name="key">Address key của scene.</param>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Kết quả chứa handle scene giả.</returns>
        public Task<AddressableResult> LoadSceneAsync(string key, CancellationToken ct)
        {
            if (FailingKeys.Contains(key))
            {
                return Task.FromException<AddressableResult>(new AddressableLoadException($"Không có scene '{key}'."));
            }

            object handle = new object();
            sceneKeyByHandle[handle] = key;
            SceneLog.Add($"load:{key}");
            return Deliver(new AddressableResult(key, handle));
        }

        /// <summary>
        /// Ghi nhận giải phóng asset.
        /// </summary>
        /// <param name="handle">Handle cần giải phóng.</param>
        public void ReleaseAsset(object handle)
        {
            ReleasedAssetHandles.Add(handle);
        }

        /// <summary>
        /// Ghi nhận giải phóng instance.
        /// </summary>
        /// <param name="instance">Instance cần giải phóng.</param>
        public void ReleaseInstance(GameObject instance)
        {
            ReleasedInstances.Add(instance);
        }

        /// <summary>
        /// Ghi nhận unload scene và hoàn tất ngay.
        /// </summary>
        /// <param name="handle">Handle scene.</param>
        /// <returns>Task đã hoàn tất.</returns>
        public Task UnloadSceneAsync(object handle)
        {
            string key = sceneKeyByHandle.TryGetValue(handle, out string value) ? value : "?";
            SceneLog.Add($"unload:{key}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hoàn tất một yêu cầu đang được giữ, theo thứ tự nhận được.
        /// </summary>
        /// <param name="index">Chỉ số yêu cầu giữ (0 là yêu cầu đầu tiên).</param>
        /// <param name="success">True để trả kết quả, false để trả AddressableLoadException.</param>
        public void CompleteHeld(int index, bool success)
        {
            (TaskCompletionSource<AddressableResult> source, AddressableResult result) entry = held[index];

            if (success)
            {
                entry.source.TrySetResult(entry.result);
            }
            else
            {
                entry.source.TrySetException(new AddressableLoadException("Tải giả thất bại."));
            }
        }

        /// <summary>
        /// Hủy các GameObject giả đã tạo trong test; gọi trong TearDown.
        /// </summary>
        public void DestroyCreatedObjects()
        {
            foreach (GameObject go in createdObjects)
            {
                if (go != null)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }

            createdObjects.Clear();

            foreach (UnityEngine.Object asset in Assets.Values)
            {
                if (asset != null)
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                }
            }

            Assets.Clear();
        }

        /// <summary>
        /// Trả kết quả ngay, hoặc giữ lại nếu HoldLoads đang bật.
        /// </summary>
        /// <param name="result">Kết quả sẽ trả.</param>
        /// <returns>Task kết quả.</returns>
        private Task<AddressableResult> Deliver(AddressableResult result)
        {
            if (!HoldLoads)
            {
                return Task.FromResult(result);
            }

            var source = new TaskCompletionSource<AddressableResult>();
            held.Add((source, result));
            return source.Task;
        }
    }
}
