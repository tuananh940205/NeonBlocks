using System.Collections.Generic;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NUnit.Framework;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử xoay SRS thuần C#: bốn orientation, kick J/L/S/T/Z, kick I, kick sàn, O và trường hợp mọi kick thất bại.
    /// </summary>
    public class SrsRotationTests
    {
        /// <summary>
        /// Kiểm tra bốn lần CW đi qua đủ bốn orientation rồi trả shape về hướng spawn.
        /// </summary>
        [Test]
        public void ClockwiseRotation_CyclesAllOrientationsAndReturnsToSpawnShape()
        {
            BoardModel board = CreateBoard(10, 20);
            Assert.IsTrue(board.TrySpawn(CreateT(), new GridPoint(3, 5)));
            GridPoint firstCell = board.ActivePiece.GetCell(0);

            Assert.IsTrue(board.TryRotateClockwise());
            Assert.AreEqual(PieceOrientation.Right, board.ActivePiece.Orientation);
            Assert.IsTrue(board.TryRotateClockwise());
            Assert.AreEqual(PieceOrientation.Reverse, board.ActivePiece.Orientation);
            Assert.IsTrue(board.TryRotateClockwise());
            Assert.AreEqual(PieceOrientation.Left, board.ActivePiece.Orientation);
            Assert.IsTrue(board.TryRotateClockwise());
            Assert.AreEqual(PieceOrientation.Spawn, board.ActivePiece.Orientation);
            Assert.AreEqual(firstCell, board.ActivePiece.GetCell(0));
        }

        /// <summary>
        /// Kiểm tra kick J/L/S/T/Z sát tường trái: R về Spawn cần offset +1 theo X để fit.
        /// </summary>
        [Test]
        public void JlstzKick_AtLeftWall_UsesHorizontalOffset()
        {
            BoardModel board = CreateBoard(4, 8);
            Assert.IsTrue(board.TrySpawn(CreateT(), new GridPoint(0, 0)));
            Assert.IsTrue(board.TryRotateClockwise());
            Assert.IsTrue(board.TryMoveLeft());
            Assert.AreEqual(new GridPoint(-1, 0), board.ActivePiece.Origin);

            Assert.IsTrue(board.TryRotateCounterClockwise());
            Assert.AreEqual(PieceOrientation.Spawn, board.ActivePiece.Orientation);
            Assert.AreEqual(new GridPoint(0, 0), board.ActivePiece.Origin);
        }

        /// <summary>
        /// Kiểm tra kick sàn của T: Spawn tại origin y = -1 vẫn fit, quay CCW cần offset (+1, +1).
        /// </summary>
        [Test]
        public void JlstzKick_AtFloor_UsesUpwardOffset()
        {
            BoardModel board = CreateBoard(6, 8);
            Assert.IsTrue(board.TrySpawn(CreateT(), new GridPoint(0, -1)));

            Assert.IsTrue(board.TryRotateCounterClockwise());
            Assert.AreEqual(PieceOrientation.Left, board.ActivePiece.Orientation);
            Assert.AreEqual(new GridPoint(1, 0), board.ActivePiece.Origin);
        }

        /// <summary>
        /// Kiểm tra I dùng bảng kick riêng: R về Spawn sát tường trái dịch +2 theo X, không phải +1 như JLSTZ.
        /// </summary>
        [Test]
        public void IKick_AtLeftWall_UsesITableOffset()
        {
            BoardModel board = CreateBoard(5, 8);
            Assert.IsTrue(board.TrySpawn(CreateI(), new GridPoint(0, 0)));
            Assert.IsTrue(board.TryRotateClockwise());
            Assert.IsTrue(board.TryMoveLeft());
            Assert.AreEqual(new GridPoint(-1, 0), board.ActivePiece.Origin);

            Assert.IsTrue(board.TryRotateCounterClockwise());
            Assert.AreEqual(PieceOrientation.Spawn, board.ActivePiece.Orientation);
            Assert.AreEqual(new GridPoint(1, 0), board.ActivePiece.Origin);
        }

        /// <summary>
        /// Kiểm tra O chấp nhận lệnh xoay nhưng giữ orientation, origin và hình không đổi.
        /// </summary>
        [Test]
        public void OPiece_RotationKeepsState()
        {
            BoardModel board = CreateBoard(6, 8);
            Assert.IsTrue(board.TrySpawn(CreateO(), new GridPoint(1, 1)));
            GridPoint firstCell = board.ActivePiece.GetCell(0);

            Assert.IsTrue(board.TryRotateClockwise());
            Assert.IsTrue(board.TryRotateCounterClockwise());
            Assert.AreEqual(PieceOrientation.Spawn, board.ActivePiece.Orientation);
            Assert.AreEqual(new GridPoint(1, 1), board.ActivePiece.Origin);
            Assert.AreEqual(firstCell, board.ActivePiece.GetCell(0));
        }

        /// <summary>
        /// Kiểm tra khi năm candidate SRS đều bị đáy hoặc settled cells chặn thì orientation và origin giữ nguyên.
        /// </summary>
        [Test]
        public void AllKicksBlocked_KeepsActivePieceUnchanged()
        {
            BoardModel board = CreateBoard(4, 4);
            PieceDefinition square = CreateO();
            Assert.IsTrue(board.TrySpawn(square, new GridPoint(0, 0)));
            Assert.IsTrue(board.TryLockActivePiece());
            Assert.IsTrue(board.TrySpawn(square, new GridPoint(2, 0)));
            Assert.IsTrue(board.TryLockActivePiece());
            Assert.IsTrue(board.TrySpawn(CreateT(), new GridPoint(0, 1)));

            Assert.IsFalse(board.TryRotateClockwise());
            Assert.AreEqual(PieceOrientation.Spawn, board.ActivePiece.Orientation);
            Assert.AreEqual(new GridPoint(0, 1), board.ActivePiece.Origin);
        }

        /// <summary>
        /// Tạo board đủ rộng/cao cho test SRS, không có hàng ẩn để dễ đọc tọa độ.
        /// </summary>
        /// <param name="width">Số cột board.</param>
        /// <param name="height">Số hàng board.</param>
        /// <returns>BoardModel rỗng.</returns>
        private static BoardModel CreateBoard(int width, int height)
        {
            return new BoardModel(new BoardSettings(width, height, 0));
        }

        /// <summary>
        /// Tạo định nghĩa T tại orientation spawn với pivot SRS nguyên.
        /// </summary>
        /// <returns>PieceDefinition T.</returns>
        private static PieceDefinition CreateT()
        {
            return new PieceDefinition("T", Cells((0, 1), (1, 1), (2, 1), (1, 2)), 1f, 1f);
        }

        /// <summary>
        /// Tạo định nghĩa I tại orientation spawn với pivot SRS nửa ô.
        /// </summary>
        /// <returns>PieceDefinition I.</returns>
        private static PieceDefinition CreateI()
        {
            return new PieceDefinition("I", Cells((0, 2), (1, 2), (2, 2), (3, 2)), 1.5f, 1.5f);
        }

        /// <summary>
        /// Tạo định nghĩa O có hình bất biến khi quay quanh tâm của nó.
        /// </summary>
        /// <returns>PieceDefinition O.</returns>
        private static PieceDefinition CreateO()
        {
            return new PieceDefinition("O", Cells((0, 0), (1, 0), (0, 1), (1, 1)), 0.5f, 0.5f);
        }

        /// <summary>
        /// Chuyển các cặp tọa độ thành danh sách GridPoint cho PieceDefinition test.
        /// </summary>
        /// <param name="points">Bốn ô của piece.</param>
        /// <returns>Danh sách GridPoint.</returns>
        private static List<GridPoint> Cells(params (int x, int y)[] points)
        {
            var cells = new List<GridPoint>();
            foreach ((int x, int y) point in points)
            {
                cells.Add(new GridPoint(point.x, point.y));
            }

            return cells;
        }
    }
}
