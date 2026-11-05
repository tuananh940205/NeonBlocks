using System.Collections.Generic;
using System.Linq;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NUnit.Framework;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử 7-bag có seed, RNG thay thế được và hàng đợi preview của SevenBagPieceSource.
    /// </summary>
    public class SevenBagPieceSourceTests
    {
        /// <summary>
        /// Kiểm tra mỗi bag bảy phần tử có đủ bảy mã piece, không lặp hoặc thiếu mã.
        /// </summary>
        [Test]
        public void EachBag_ContainsEveryConfiguredPieceExactlyOnce()
        {
            var source = new SevenBagPieceSource(CreatePieces(), 12345, 5);

            List<string> firstBag = TakeIds(source, 7);
            CollectionAssert.AreEquivalent(new[] { "I", "O", "T", "J", "L", "S", "Z" }, firstBag);
            Assert.AreEqual(7, firstBag.Distinct().Count());
        }

        /// <summary>
        /// Kiểm tra hai nguồn cùng seed sinh chuỗi piece giống hệt nhau qua nhiều bag.
        /// </summary>
        [Test]
        public void SameSeed_ProducesSamePieceSequence()
        {
            var first = new SevenBagPieceSource(CreatePieces(), 42, 5);
            var second = new SevenBagPieceSource(CreatePieces(), 42, 5);

            CollectionAssert.AreEqual(TakeIds(first, 28), TakeIds(second, 28));
        }

        /// <summary>
        /// Kiểm tra preview có năm phần tử và sau khi lấy một piece thì các phần tử còn lại dịch lên, rồi được bù đủ năm.
        /// </summary>
        [Test]
        public void TakeNext_ShiftsAndRefillsPreview()
        {
            var source = new SevenBagPieceSource(CreatePieces(), 7, 5);
            List<string> previewBefore = source.NextPieces.Select(piece => piece.Id).ToList();

            PieceDefinition taken = source.TakeNext();
            List<string> previewAfter = source.NextPieces.Select(piece => piece.Id).ToList();

            Assert.AreEqual(5, previewBefore.Count);
            Assert.AreEqual(previewBefore[0], taken.Id);
            CollectionAssert.AreEqual(previewBefore.Skip(1).ToArray(), previewAfter.Take(4).ToArray());
            Assert.AreEqual(5, previewAfter.Count);
        }

        /// <summary>
        /// Kiểm tra nguồn thực sự dùng IPieceRandom được truyền vào thay vì Unity Random hoặc RNG ẩn.
        /// </summary>
        [Test]
        public void InjectedRandom_DeterminesFisherYatesOrder()
        {
            var source = new SevenBagPieceSource(CreatePieces(), new FixedPieceRandom(0), 5);

            CollectionAssert.AreEqual(new[] { "O", "T", "J", "L", "S", "Z", "I" }, TakeIds(source, 7));
        }

        /// <summary>
        /// Tạo bảy PieceDefinition hợp lệ có đúng mã config; shape giống nhau là đủ cho test nguồn, không tham gia gameplay board.
        /// </summary>
        /// <returns>Danh sách bảy piece hợp lệ.</returns>
        private static IReadOnlyList<PieceDefinition> CreatePieces()
        {
            string[] ids = { "I", "O", "T", "J", "L", "S", "Z" };
            var pieces = new List<PieceDefinition>();
            foreach (string id in ids)
            {
                pieces.Add(new PieceDefinition(id, Cells(), 0.5f, 0.5f));
            }

            return pieces.AsReadOnly();
        }

        /// <summary>
        /// Tạo shape vuông bốn ô hợp lệ để phục vụ dữ liệu test của mọi mã piece.
        /// </summary>
        /// <returns>Bốn GridPoint liền nhau.</returns>
        private static IReadOnlyList<GridPoint> Cells()
        {
            return new[]
            {
                new GridPoint(0, 0),
                new GridPoint(1, 0),
                new GridPoint(0, 1),
                new GridPoint(1, 1)
            };
        }

        /// <summary>
        /// Lấy số lượng mã piece theo thứ tự từ nguồn cần kiểm tra.
        /// </summary>
        /// <param name="source">Nguồn piece.</param>
        /// <param name="count">Số piece cần lấy.</param>
        /// <returns>Danh sách mã đã lấy.</returns>
        private static List<string> TakeIds(SevenBagPieceSource source, int count)
        {
            var ids = new List<string>();
            for (int i = 0; i < count; i++)
            {
                ids.Add(source.TakeNext().Id);
            }

            return ids;
        }
    }
}
