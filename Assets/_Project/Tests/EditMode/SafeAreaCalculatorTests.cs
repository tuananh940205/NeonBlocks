using NUnit.Framework;
using UnityEngine;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử chuyển Screen.safeArea sang anchor chuẩn hóa, gồm trường hợp tai thỏ và vùng không hợp lệ.
    /// </summary>
    public class SafeAreaCalculatorTests
    {
        private const float Epsilon = 0.00001f;

        /// <summary>
        /// Kiểm tra vùng an toàn bằng toàn màn hình cho anchor 0–1.
        /// </summary>
        [Test]
        public void FullScreenSafeArea_ReturnsFullAnchors()
        {
            bool ok = SafeAreaCalculator.TryComputeAnchors(new Rect(0, 0, 1080, 1920), 1080, 1920, out Vector2 min, out Vector2 max);

            Assert.IsTrue(ok);
            Assert.AreEqual(0f, min.x, Epsilon);
            Assert.AreEqual(0f, min.y, Epsilon);
            Assert.AreEqual(1f, max.x, Epsilon);
            Assert.AreEqual(1f, max.y, Epsilon);
        }

        /// <summary>
        /// Kiểm tra khoảng tai thỏ ở đỉnh màn hình làm giảm anchorMax.y đúng theo tỷ lệ pixel.
        /// </summary>
        [Test]
        public void NotchAtTop_ReducesAnchorMaxY()
        {
            bool ok = SafeAreaCalculator.TryComputeAnchors(new Rect(0, 0, 1080, 1820), 1080, 1920, out Vector2 min, out Vector2 max);

            Assert.IsTrue(ok);
            Assert.AreEqual(0f, min.y, Epsilon);
            Assert.AreEqual(1820f / 1920f, max.y, Epsilon);
        }

        /// <summary>
        /// Kiểm tra vùng an toàn lớn hơn màn hình được kẹp về khoảng 0–1.
        /// </summary>
        [Test]
        public void SafeAreaLargerThanScreen_ClampsToUnitRange()
        {
            bool ok = SafeAreaCalculator.TryComputeAnchors(new Rect(-100, -100, 1280, 2120), 1080, 1920, out Vector2 min, out Vector2 max);

            Assert.IsTrue(ok);
            Assert.AreEqual(0f, min.x, Epsilon);
            Assert.AreEqual(0f, min.y, Epsilon);
            Assert.AreEqual(1f, max.x, Epsilon);
            Assert.AreEqual(1f, max.y, Epsilon);
        }

        /// <summary>
        /// Kiểm tra kích thước màn hình bằng 0 trả false và đặt anchor về toàn màn hình.
        /// </summary>
        [Test]
        public void ZeroScreenSize_ReturnsFalseWithFullAnchors()
        {
            bool ok = SafeAreaCalculator.TryComputeAnchors(new Rect(0, 0, 1080, 1920), 0, 1920, out Vector2 min, out Vector2 max);

            Assert.IsFalse(ok);
            Assert.AreEqual(Vector2.zero, min);
            Assert.AreEqual(Vector2.one, max);
        }

        /// <summary>
        /// Kiểm tra vùng an toàn rỗng hoặc nằm ngoài màn hình trả false thay vì tạo anchor suy biến.
        /// </summary>
        [Test]
        public void EmptyOrOutOfScreenSafeArea_ReturnsFalse()
        {
            bool emptyOk = SafeAreaCalculator.TryComputeAnchors(new Rect(0, 0, 0, 1920), 1080, 1920, out _, out _);
            bool outsideOk = SafeAreaCalculator.TryComputeAnchors(new Rect(2000, 0, 100, 100), 1080, 1920, out _, out _);

            Assert.IsFalse(emptyOk);
            Assert.IsFalse(outsideOk);
        }
    }
}
