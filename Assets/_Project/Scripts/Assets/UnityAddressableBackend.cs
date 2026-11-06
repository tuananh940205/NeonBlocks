using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Triển khai IAddressableBackend bằng Addressables thật. Đây là phần hạ tầng bên trong AddressableManager;
    /// không gọi trực tiếp từ gameplay hay view. Mọi handle được đổi thành Task và lỗi được bọc AddressableLoadException.
    /// </summary>
    public sealed class UnityAddressableBackend : IAddressableBackend
    {
        /// <summary>
        /// Tải asset bằng Addressables.LoadAssetAsync và kiểm tra kiểu của kết quả.
        /// </summary>
        /// <param name="key">Address key.</param>
        /// <param name="assetType">Kiểu asset mong đợi.</param>
        /// <param name="ct">Token hủy, được AddressableManager xử lý.</param>
        /// <returns>Kết quả chứa asset và handle.</returns>
        public Task<AddressableResult> LoadAssetAsync(string key, Type assetType, CancellationToken ct)
        {
            AsyncOperationHandle<UnityEngine.Object> handle = Addressables.LoadAssetAsync<UnityEngine.Object>(key);
            return ToResultTask(handle, key, obj => obj);
        }

        /// <summary>
        /// Tải scene Addressables ở chế độ Additive, giữ handle để unload sau.
        /// </summary>
        /// <param name="key">Address key của scene.</param>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Kết quả chứa SceneInstance dạng boxed và handle scene.</returns>
        public Task<AddressableResult> LoadSceneAsync(string key, CancellationToken ct)
        {
            // Single sẽ hủy Entry/scene cũ trước khi SceneFlowManager nhận scope, gây cancel bootstrap và kẹt PopupHost.
            // SceneFlowManager tự active scene mới rồi stop/dispose/release scope cũ; backend chỉ tải Additive.
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive, true);
            return ToResultTask(handle, key, scene => scene);
        }

        /// <summary>
        /// Đặt SceneInstance Addressables đã tải làm active scene của Unity.
        /// </summary>
        /// <param name="sceneValue">SceneInstance boxed từ LoadSceneAsync.</param>
        /// <param name="ct">Token hủy.</param>
        /// <returns>Task đã hoàn tất khi Unity nhận scene active mới.</returns>
        public Task SetActiveSceneAsync(object sceneValue, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (!(sceneValue is SceneInstance sceneInstance) || !sceneInstance.Scene.IsValid())
            {
                return Task.FromException(new AddressableLoadException("Scene Addressables không hợp lệ để đặt active."));
            }

            if (!SceneManager.SetActiveScene(sceneInstance.Scene))
            {
                return Task.FromException(new AddressableLoadException($"Không thể đặt scene '{sceneInstance.Scene.name}' làm active."));
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Giải phóng handle asset bằng Addressables.Release. Chỉ được gọi từ AddressableManager.
        /// </summary>
        /// <param name="handle">Handle nhận từ LoadAssetAsync, kiểu AsyncOperationHandle.</param>
        public void ReleaseAsset(object handle)
        {
            if (handle is AsyncOperationHandle asyncOperationHandle)
            {
                Addressables.Release(asyncOperationHandle);
                return;
            }
            
            Debug.LogError($"ReleaseAsset invalid handle: {handle.GetType()}");
        }

        /// <summary>
        /// Unload scene bằng Addressables.UnloadSceneAsync với autoReleaseHandle = true, nên handle không được giải phóng lần nữa.
        /// </summary>
        /// <param name="handle">Handle scene, kiểu AsyncOperationHandle.</param>
        /// <returns>Task hoàn tất khi unload xong; lỗi ném AddressableLoadException.</returns>
        public Task UnloadSceneAsync(object handle)
        {
            AsyncOperationHandle op = Addressables.UnloadSceneAsync((AsyncOperationHandle)handle, true);
            var tcs = new TaskCompletionSource<bool>();

            if (op.IsDone)
            {
                CompleteUnload(op, tcs);
            }
            else
            {
                op.Completed += _ => CompleteUnload(op, tcs);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Chuyển kết quả unload thành Task: thành công thì hoàn tất, thất bại thì ném AddressableLoadException.
        /// </summary>
        /// <param name="op">Thao tác unload đã hoàn tất.</param>
        /// <param name="tcs">Nguồn Task cần hoàn tất.</param>
        private static void CompleteUnload(AsyncOperationHandle op, TaskCompletionSource<bool> tcs)
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                tcs.TrySetResult(true);
            }
            else
            {
                tcs.TrySetException(new AddressableLoadException("Không unload được scene Addressables.", op.OperationException));
            }
        }

        /// <summary>
        /// Chuyển AsyncOperationHandle thành Task&lt;AddressableResult&gt;. Lỗi thao tác được bọc AddressableLoadException.
        /// </summary>
        /// <typeparam name="T">Kiểu kết quả của handle.</typeparam>
        /// <param name="handle">Handle Addressables đang chạy.</param>
        /// <param name="key">Key dùng trong thông điệp lỗi.</param>
        /// <param name="select">Hàm chọn giá trị từ kết quả; không được ném exception.</param>
        /// <returns>Task kết quả tải.</returns>
        private static Task<AddressableResult> ToResultTask<T>(AsyncOperationHandle<T> handle, string key, Func<T, object> select)
        {
            var tcs = new TaskCompletionSource<AddressableResult>();

            void Complete(AsyncOperationHandle<T> completeAsyncOperationHandle)
            {
                if (completeAsyncOperationHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    tcs.TrySetException(new AddressableLoadException($"Không tải được '{key}'.", completeAsyncOperationHandle.OperationException));
                    return;
                }

                object value = select(completeAsyncOperationHandle.Result);
                tcs.TrySetResult(new AddressableResult(value, (AsyncOperationHandle)completeAsyncOperationHandle));
            }

            if (handle.IsDone)
            {
                Complete(handle);
            }
            else
            {
                handle.Completed += Complete;
            }

            return tcs.Task;
        }
    }
}
