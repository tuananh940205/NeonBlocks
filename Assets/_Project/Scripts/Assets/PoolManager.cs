using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Manager pool session cho instance Addressable, phân vùng theo SceneAssetScope và không tự release handle.
    /// </summary>
    public sealed class PoolManager : IDisposable
    {
        private readonly Dictionary<SceneAssetScope, Dictionary<string, PoolBucket>> bucketsByScope = new Dictionary<SceneAssetScope, Dictionary<string, PoolBucket>>();
        private bool isDisposed;

        /// <summary>Lấy instance nhàn rỗi hoặc instantiate mới qua scope, giữ template sống cho đến khi scope được dọn.</summary>
        public async Task<GameObject> AcquireAsync(SceneAssetScope scope, string templateKey, Transform parent, CancellationToken ct)
        {
            if (isDisposed) throw new InvalidOperationException("PoolManager đã dispose.");
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            if (scope.IsReleased) throw new InvalidOperationException("Không thể lấy instance từ scope đã release.");
            if (string.IsNullOrWhiteSpace(templateKey)) throw new ArgumentException("Template key không được rỗng.", nameof(templateKey));

            PoolBucket bucket = GetBucket(scope, templateKey);
            GameObject instance;
            if (bucket.InactiveInstances.Count > 0)
            {
                instance = bucket.InactiveInstances.Dequeue();
                instance.transform.SetParent(parent, false);
                instance.SetActive(true);
            }
            else
            {
                instance = await scope.InstantiateAsync(templateKey, parent, ct);
            }

            bucket.ActiveInstances.Add(instance);
            return instance;
        }

        /// <summary>
        /// Trả một instance về bucket cùng scope mà không release prefab/template.
        /// </summary>
        public bool Release(SceneAssetScope scope, GameObject instance)
        {
            if (scope == null || instance == null || !bucketsByScope.TryGetValue(scope, out Dictionary<string, PoolBucket> buckets)) return false;
            foreach (PoolBucket bucket in buckets.Values)
            {
                if (bucket.ActiveInstances.Remove(instance))
                {
                    instance.SetActive(false);
                    bucket.InactiveInstances.Enqueue(instance);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Hủy toàn bộ active và inactive instance của scope trước khi scope giải phóng handle template.
        /// </summary>
        public void ReleaseScope(SceneAssetScope scope)
        {
            if (scope == null || !bucketsByScope.TryGetValue(scope, out Dictionary<string, PoolBucket> buckets)) return;
            bucketsByScope.Remove(scope);
            foreach (PoolBucket bucket in buckets.Values)
            {
                ReleaseAll(scope, bucket.ActiveInstances);
                while (bucket.InactiveInstances.Count > 0)
                {
                    scope.ReleaseInstance(bucket.InactiveInstances.Dequeue());
                }
            }
        }

        /// <summary>Dọn toàn bộ bucket còn sống khi session kết thúc.</summary>
        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            SceneAssetScope[] scopes = new SceneAssetScope[bucketsByScope.Count];
            bucketsByScope.Keys.CopyTo(scopes, 0);
            foreach (SceneAssetScope scope in scopes) ReleaseScope(scope);
        }

        /// <summary>Lấy hoặc tạo bucket duy nhất theo scope và template key.</summary>
        private PoolBucket GetBucket(SceneAssetScope scope, string templateKey)
        {
            if (!bucketsByScope.TryGetValue(scope, out Dictionary<string, PoolBucket> buckets))
            {
                buckets = new Dictionary<string, PoolBucket>();
                bucketsByScope.Add(scope, buckets);
            }

            if (!buckets.TryGetValue(templateKey, out PoolBucket bucket))
            {
                bucket = new PoolBucket(templateKey);
                buckets.Add(templateKey, bucket);
            }

            return bucket;
        }

        /// <summary>Giải phóng lần lượt các instance active còn do scope sở hữu.</summary>
        private static void ReleaseAll(SceneAssetScope scope, List<GameObject> instances)
        {
            for (int index = instances.Count - 1; index >= 0; index--) scope.ReleaseInstance(instances[index]);
            instances.Clear();
        }
    }
}
