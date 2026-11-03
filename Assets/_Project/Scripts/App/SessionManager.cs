using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonBlocks
{
    /// <summary>
    /// Quản lý session toàn game, đặt trong Entry scene. Đăng ký các singleton sống xuyên suốt game
    /// vào ServiceLocator trong logic khởi động; object này được giữ qua mọi lần đổi scene.
    /// Chỉ bị hủy khi Restart Game hoặc khi thoát Play Mode/app.
    /// </summary>
    [DisallowMultipleComponent]
    public class SessionManager : MonoBehaviour
    {
        /// <summary>Tên scene Entry trong Build Settings; Restart Game tải lại scene này.</summary>
        private const string EntrySceneName = "Entry";

        private readonly List<IDisposable> ownedDisposables = new List<IDisposable>();
        private bool isOwner;

        /// <summary>True khi instance này là session đang hoạt động và đã đăng ký vào ServiceLocator.</summary>
        public bool IsOwner => isOwner;

        /// <summary>
        /// Unity gọi khi scene tải; chuyển toàn bộ việc nhận session cho TryBeginSession để có thể gọi tường minh trong test.
        /// </summary>
        private void Awake()
        {
            TryBeginSession();
        }

        /// <summary>
        /// Nhận quyền sở hữu session nếu chưa có session nào: đăng ký chính mình vào ServiceLocator và giữ qua đổi scene.
        /// Nếu đã có session khác đang chạy thì hủy instance này và trả false. Gọi lại khi đã là owner thì trả true, không đăng ký lại.
        /// </summary>
        /// <returns>True nếu instance này là owner của session sau lời gọi.</returns>
        public bool TryBeginSession()
        {
            if (isOwner)
            {
                return true;
            }

            if (ServiceLocator.Instance.IsRegistered(typeof(SessionManager)))
            {
                Debug.LogWarning("SessionManager đã tồn tại; instance mới bị hủy.");
                Destroy(gameObject);
                return false;
            }

            isOwner = true;
            ServiceLocator.Instance.RegisterSingleton(this);
            DontDestroyOnLoad(gameObject);
            return true;
        }

        /// <summary>
        /// Đăng ký một singleton sống theo session; nếu instance là IDisposable thì sẽ được dispose khi kết thúc session.
        /// Chỉ instance đang sở hữu session mới được đăng ký.
        /// </summary>
        /// <typeparam name="T">Kiểu runtime cụ thể của singleton.</typeparam>
        /// <param name="instance">Instance cần đăng ký, không được null.</param>
        /// <returns>Chính instance đã đăng ký.</returns>
        public T RegisterSingleton<T>(T instance) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!isOwner)
            {
                throw new InvalidOperationException("Chỉ SessionManager đang sở hữu session mới được đăng ký singleton.");
            }

            Type runtimeType = instance.GetType();
            if (ServiceLocator.Instance.IsRegistered(runtimeType))
            {
                throw new InvalidOperationException($"Singleton {runtimeType} đã được đăng ký.");
            }

            ServiceLocator.Instance.RegisterSingleton(instance);

            if (instance is IDisposable disposable)
            {
                ownedDisposables.Add(disposable);
            }

            return instance;
        }

        /// <summary>
        /// Kết thúc session: dispose singleton IDisposable theo thứ tự ngược, rồi xóa toàn bộ đăng ký trong ServiceLocator.
        /// Không hủy GameObject. Gọi nhiều lần an toàn; instance không phải owner thì không làm gì.
        /// </summary>
        public void ResetSession()
        {
            if (!isOwner)
            {
                return;
            }

            isOwner = false;

            for (int i = ownedDisposables.Count - 1; i >= 0; i--)
            {
                ownedDisposables[i].Dispose();
            }

            ownedDisposables.Clear();
            ServiceLocator.Instance.UnregisterAll();
        }

        /// <summary>
        /// Restart Game: kết thúc session, hủy SessionManager và tải lại Entry để tạo session mới.
        /// Đây là cách duy nhất hủy SessionManager trong lúc game đang chạy.
        /// </summary>
        public void RestartGame()
        {
            ResetSession();
            Destroy(gameObject);
            SceneManager.LoadScene(EntrySceneName);
        }

        /// <summary>
        /// Khi GameObject bị hủy (Restart Game hoặc thoát Play Mode), kết thúc session nếu instance này còn là owner.
        /// </summary>
        private void OnDestroy()
        {
            ResetSession();
        }
    }
}
