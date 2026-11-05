using System.Text.RegularExpressions;
using NUnit.Framework;
using NeonBlocks.Scene;
using UnityEngine;
using UnityEngine.TestTools;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử vòng đời đăng ký/resolve/unregister của ServiceLocator và cơ chế lazy của Signal.
    /// </summary>
    public class ServiceLocatorTests
    {
        /// <summary>
        /// Xóa toàn bộ singleton trước mỗi test để không rò trạng thái giữa các test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Instance.UnregisterAll();
        }

        /// <summary>
        /// Xóa toàn bộ singleton sau mỗi test để không ảnh hưởng test khác.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Instance.UnregisterAll();
        }

        /// <summary>
        /// Kiểm tra đăng ký rồi resolve trả về đúng cùng instance và IsRegistered báo đã đăng ký.
        /// </summary>
        [Test]
        public void RegisterThenResolve_ReturnsSameInstance()
        {
            var service = new ProbeService("alpha");

            ServiceLocator.Instance.RegisterSingleton(service);

            Assert.IsTrue(ServiceLocator.Instance.IsRegistered(typeof(ProbeService)));
            Assert.AreSame(service, ServiceLocator.Instance.Resolve<ProbeService>());
        }

        /// <summary>
        /// Kiểm tra unregister gỡ đúng đăng ký và không còn kiểu đó sau khi gỡ.
        /// </summary>
        [Test]
        public void UnregisterSingleton_RemovesRegistration()
        {
            var service = new ProbeService("alpha");
            ServiceLocator.Instance.RegisterSingleton(service);

            ServiceLocator.Instance.UnregisterSingleton(service);

            Assert.IsFalse(ServiceLocator.Instance.IsRegistered(typeof(ProbeService)));
        }

        /// <summary>
        /// Kiểm tra đăng ký trùng kiểu bị từ chối: giữ instance đầu tiên và báo lỗi đã đăng ký.
        /// </summary>
        [Test]
        public void RegisterDuplicateType_KeepsFirstInstance()
        {
            var first = new ProbeService("first");
            var second = new ProbeService("second");
            ServiceLocator.Instance.RegisterSingleton(first);

            LogAssert.Expect(LogType.Error, new Regex("Already registered"));
            ServiceLocator.Instance.RegisterSingleton(second);

            Assert.AreSame(first, ServiceLocator.Instance.Resolve<ProbeService>());
        }

        /// <summary>
        /// Kiểm tra UnregisterAll xóa mọi đăng ký để Entry chạy lại không gặp service cũ.
        /// </summary>
        [Test]
        public void UnregisterAll_ClearsAllRegistrations()
        {
            ServiceLocator.Instance.RegisterSingleton(new ProbeService("alpha"));

            ServiceLocator.Instance.UnregisterAll();

            Assert.IsFalse(ServiceLocator.Instance.IsRegistered(typeof(ProbeService)));
        }

        /// <summary>
        /// Kiểm tra signal được tạo lazy ở lần Resolve đầu tiên và các lần sau trả về cùng instance.
        /// </summary>
        [Test]
        public void Resolve_SignalType_CreatesInstanceLazilyAndReturnsSameInstance()
        {
            SceneStopSignal first = ServiceLocator.Instance.Resolve<SceneStopSignal>();
            SceneStopSignal second = ServiceLocator.Instance.Resolve<SceneStopSignal>();

            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
            Assert.IsTrue(ServiceLocator.Instance.IsRegistered(typeof(SceneStopSignal)));
        }

        /// <summary>
        /// Kiểm tra Resolve kiểu không phải signal khi chưa đăng ký: ghi log lỗi và trả null.
        /// </summary>
        [Test]
        public void Resolve_UnregisteredNonSignal_LogsErrorAndReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("not registered"));

            Assert.IsNull(ServiceLocator.Instance.Resolve<ProbeService>());
        }
    }
}
