using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using UnityEngine;
namespace NeonBlocks.Popups
{
    /// <summary>Một popup trên mỗi scene; đóng trong lúc tải sẽ release kết quả muộn, không hiển thị lại.</summary>
    public sealed class PopupManager : IDisposable
    {
        private readonly Dictionary<string, PopupSlot> slots = new Dictionary<string, PopupSlot>();
        private bool disposed;
        public int OpenCount => slots.Count;
        /// <summary>Mở prefab theo scope; yêu cầu trùng chia sẻ task và không nhân instance.</summary>
        public Task<GameObject> OpenAsync(SceneAssetScope scope, string key, Transform parent, CancellationToken ct)
        {
            if (disposed) { throw new ObjectDisposedException(nameof(PopupManager)); }
            if (slots.TryGetValue(scope.SceneKey, out PopupSlot existing))
            {
                if (existing.Key == key) { return existing.Pending; }
                Close(scope.SceneKey);
            }
            var slot = new PopupSlot(scope, key); slots.Add(scope.SceneKey, slot);
            slot.Pending = LoadAsync(slot, parent, ct);
            return slot.Pending;
        }
        /// <summary>Chuẩn hóa task load và dọn slot khi lỗi; không swallow exception.</summary>
        private async Task<GameObject> LoadAsync(PopupSlot slot, Transform parent, CancellationToken ct)
        {
            try
            {
                GameObject instance = await slot.Scope.InstantiateAsync(slot.Key, parent, ct);
                if (slot.Closed || disposed || slot.Scope.IsReleased || ct.IsCancellationRequested)
                {
                    // Đóng/cancel trước khi tải xong: kết quả không được hiện lên scene mới.
                    slot.Scope.ReleaseInstance(instance);
                    ct.ThrowIfCancellationRequested();
                    throw new OperationCanceledException("Popup đã đóng.");
                }
                slot.Instance = instance;
                return instance;
            }
            catch
            {
                // Chỉ xóa slot này; task cũ không được xóa yêu cầu mới thay thế nó.
                if (slots.TryGetValue(slot.Scope.SceneKey, out PopupSlot current) && ReferenceEquals(current, slot))
                { slots.Remove(slot.Scope.SceneKey); }
                throw;
            }
        }
        /// <summary>Đóng idempotent và release qua scope; không gọi Addressables trực tiếp.</summary>
        public void Close(string sceneKey)
        {
            if (!slots.TryGetValue(sceneKey, out PopupSlot slot)) { return; }
            slots.Remove(sceneKey); slot.Closed = true;
            if (slot.Instance != null)
            {
                // Tắt raycast/render ngay, Destroy thật có thể chờ cuối frame.
                slot.Instance.SetActive(false);
                slot.Scope.ReleaseInstance(slot.Instance);
            }
        }
        /// <summary>Dọn mọi slot khi session kết thúc; pending task vẫn tự release kết quả muộn.</summary>
        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;
            foreach (string key in new List<string>(slots.Keys)) { Close(key); }
        }
    }
}