using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử SessionManager: quyền sở hữu session, đăng ký singleton, kết thúc session và chặn instance trùng.
    /// </summary>
    public class SessionManagerTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        /// <summary>
        /// Xóa đăng ký còn sót từ test trước để mỗi test bắt đầu với ServiceLocator sạch.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Instance.UnregisterAll();
        }

        /// <summary>
        /// Hủy mọi GameObject tạo trong test và xóa đăng ký còn lại để không ảnh hưởng test khác.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in createdObjects)
            {
                if (go != null)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }

            createdObjects.Clear();
            ServiceLocator.Instance.UnregisterAll();
        }

        /// <summary>
        /// Tạo một GameObject có SessionManager để Awake chạy như trong Entry scene.
        /// </summary>
        /// <param name="name">Tên GameObject, dùng để dễ đọc khi debug.</param>
        /// <returns>Component SessionManager vừa tạo.</returns>
        private SessionManager CreateSession(string name)
        {
            var go = new GameObject(name);
            createdObjects.Add(go);
            return go.AddComponent<SessionManager>();
        }

        /// <summary>
        /// Kiểm tra session đầu tiên trở thành owner và tự đăng ký vào ServiceLocator.
        /// </summary>
        [Test]
        public void FirstSession_BecomesOwnerAndRegistersSelf()
        {
            SessionManager session = CreateSession("Session");

            Assert.IsTrue(session.IsOwner);
            Assert.AreSame(session, ServiceLocator.Instance.Resolve<SessionManager>());
        }

        /// <summary>
        /// Kiểm tra ResetSession dispose singleton theo thứ tự ngược với đăng ký và gỡ hết đăng ký.
        /// </summary>
        [Test]
        public void ResetSession_DisposesSingletonsInReverseOrderAndUnregisters()
        {
            var disposeLog = new List<string>();
            SessionManager session = CreateSession("Session");
            ProbeService alpha = session.RegisterSingleton(new ProbeService("alpha", disposeLog));
            ProbeService beta = session.RegisterSingleton(new ProbeService("beta", disposeLog));

            session.ResetSession();

            CollectionAssert.AreEqual(new[] { "beta", "alpha" }, disposeLog);
            Assert.AreEqual(1, alpha.DisposeCount);
            Assert.AreEqual(1, beta.DisposeCount);
            Assert.IsFalse(ServiceLocator.Instance.IsRegistered(typeof(ProbeService)));
            Assert.IsFalse(ServiceLocator.Instance.IsRegistered(typeof(SessionManager)));
            Assert.IsFalse(session.IsOwner);
        }

        /// <summary>
        /// Kiểm tra đăng ký cùng kiểu runtime lần hai bị từ chối và singleton đầu tiên vẫn được giữ.
        /// </summary>
        [Test]
        public void RegisterSingleton_DuplicateType_Throws()
        {
            SessionManager session = CreateSession("Session");
            ProbeService first = session.RegisterSingleton(new ProbeService("first"));

            Assert.Throws<InvalidOperationException>(() => session.RegisterSingleton(new ProbeService("second")));
            Assert.AreSame(first, ServiceLocator.Instance.Resolve<ProbeService>());
        }

        /// <summary>
        /// Kiểm tra đăng ký instance null bị từ chối bằng ArgumentNullException.
        /// </summary>
        [Test]
        public void RegisterSingleton_Null_Throws()
        {
            SessionManager session = CreateSession("Session");

            Assert.Throws<ArgumentNullException>(() => session.RegisterSingleton<ProbeService>(null));
        }

        /// <summary>
        /// Kiểm tra ResetSession gọi nhiều lần chỉ dispose singleton một lần.
        /// </summary>
        [Test]
        public void ResetSession_CalledTwice_DisposesOnce()
        {
            SessionManager session = CreateSession("Session");
            ProbeService service = session.RegisterSingleton(new ProbeService("once"));

            session.ResetSession();
            session.ResetSession();

            Assert.AreEqual(1, service.DisposeCount);
        }

        /// <summary>
        /// Kiểm tra sau ResetSession, một session mới có thể nhận quyền sở hữu lại (tương đương Restart Game).
        /// </summary>
        [Test]
        public void AfterReset_NewSessionBecomesOwnerAgain()
        {
            SessionManager first = CreateSession("First");
            first.ResetSession();

            SessionManager second = CreateSession("Second");

            Assert.IsTrue(second.IsOwner);
            Assert.AreSame(second, ServiceLocator.Instance.Resolve<SessionManager>());
        }

        /// <summary>
        /// Kiểm tra SessionManager thứ hai bị từ chối quyền sở hữu, không đè đăng ký của session đang chạy,
        /// và việc hủy instance trùng không làm mất session hiện tại.
        /// </summary>
        [Test]
        public void DuplicateSession_IsNotOwner_AndDestroyingItKeepsOwner()
        {
            SessionManager owner = CreateSession("Owner");
            LogAssert.Expect(LogType.Warning, new Regex("đã tồn tại"));
            SessionManager duplicate = CreateSession("Duplicate");

            Assert.IsFalse(duplicate.IsOwner);
            Assert.AreSame(owner, ServiceLocator.Instance.Resolve<SessionManager>());

            UnityEngine.Object.DestroyImmediate(duplicate.gameObject);

            Assert.IsTrue(owner.IsOwner);
            Assert.AreSame(owner, ServiceLocator.Instance.Resolve<SessionManager>());
        }

        /// <summary>
        /// Kiểm tra instance không phải owner không được đăng ký singleton.
        /// </summary>
        [Test]
        public void RegisterSingleton_OnNonOwner_Throws()
        {
            CreateSession("Owner");
            LogAssert.Expect(LogType.Warning, new Regex("đã tồn tại"));
            SessionManager duplicate = CreateSession("Duplicate");

            Assert.Throws<InvalidOperationException>(() => duplicate.RegisterSingleton(new ProbeService("rejected")));
        }
    }
}
