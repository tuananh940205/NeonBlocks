using System.Collections.Generic;
using NUnit.Framework;
using NeonBlocks.Config;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử ràng buộc shape của PieceDefinition trực tiếp qua constructor: số ô, ô trùng, ô ngoài khung, liền nhau và tâm xoay.
    /// </summary>
    public class PieceDefinitionTests
    {
        /// <summary>
        /// Kiểm tra khối I hợp lệ được tạo và giữ đúng bốn ô và tâm xoay.
        /// </summary>
        [Test]
        public void ValidIPiece_IsCreated()
        {
            var piece = new PieceDefinition("I", Cells((0, 2), (1, 2), (2, 2), (3, 2)), 1.5f, 1.5f);

            Assert.AreEqual("I", piece.Id);
            Assert.AreEqual(4, piece.Cells.Count);
            Assert.AreEqual(1.5f, piece.RotationCenterX);
        }

        /// <summary>
        /// Kiểm tra hai ô trùng nhau trong khối bị từ chối.
        /// </summary>
        [Test]
        public void DuplicateCell_Throws()
        {
            Assert.Throws<ConfigValidationException>(() =>
                new PieceDefinition("T", Cells((0, 1), (1, 1), (1, 1), (1, 2)), 1f, 1f));
        }

        /// <summary>
        /// Kiểm tra ô có x = 4 (ngoài khung 0..3) bị từ chối.
        /// </summary>
        [Test]
        public void CellOutsideBox_Throws()
        {
            Assert.Throws<ConfigValidationException>(() =>
                new PieceDefinition("I", Cells((0, 2), (1, 2), (2, 2), (4, 2)), 1.5f, 1.5f));
        }

        /// <summary>
        /// Kiểm tra ô có tọa độ âm (ngoài khung) bị từ chối.
        /// </summary>
        [Test]
        public void NegativeCell_Throws()
        {
            Assert.Throws<ConfigValidationException>(() =>
                new PieceDefinition("I", Cells((-1, 2), (1, 2), (2, 2), (3, 2)), 1.5f, 1.5f));
        }

        /// <summary>
        /// Kiểm tra khối có bốn ô nhưng không liền nhau bị từ chối.
        /// </summary>
        [Test]
        public void DisconnectedCells_Throws()
        {
            Assert.Throws<ConfigValidationException>(() =>
                new PieceDefinition("S", Cells((0, 0), (0, 1), (3, 3), (3, 2)), 1f, 1f));
        }

        /// <summary>
        /// Kiểm tra số ô khác bốn bị từ chối.
        /// </summary>
        [Test]
        public void WrongCellCount_Throws()
        {
            Assert.Throws<ConfigValidationException>(() =>
                new PieceDefinition("T", Cells((0, 1), (1, 1), (2, 1)), 1f, 1f));
        }

        /// <summary>
        /// Kiểm tra tâm xoay nằm ngoài khung bị từ chối.
        /// </summary>
        [Test]
        public void RotationCenterOutsideBox_Throws()
        {
            Assert.Throws<ConfigValidationException>(() =>
                new PieceDefinition("T", Cells((0, 1), (1, 1), (2, 1), (1, 2)), -0.5f, 1f));
        }

        /// <summary>
        /// Chuyển danh sách cặp (x, y) thành danh sách GridPoint cho test.
        /// </summary>
        /// <param name="points">Các cặp tọa độ.</param>
        /// <returns>Danh sách GridPoint tương ứng.</returns>
        private static List<GridPoint> Cells(params (int x, int y)[] points)
        {
            var list = new List<GridPoint>();
            foreach ((int x, int y) p in points)
            {
                list.Add(new GridPoint(p.x, p.y));
            }

            return list;
        }
    }
}
