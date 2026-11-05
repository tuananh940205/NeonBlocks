using System.Collections.Generic;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NUnit.Framework;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử luật lưới thuần C# của BoardModel: biên, vùng ẩn, va chạm, lock và spawn bị chặn.
    /// </summary>
    public class BoardModelTests
    {
        /// <summary>
        /// Kiểm tra dịch trái hoặc phải qua mép bảng trả false và giữ nguyên origin của active piece.
        /// </summary>
        [Test]
        public void MoveAtHorizontalEdges_DoesNotMutateActivePiece()
        {
            BoardModel board = CreateBoard();
            Assert.IsTrue(board.TrySpawn(CreateSquare(), new GridPoint(0, 0)));

            Assert.IsFalse(board.TryMoveLeft());
            Assert.AreEqual(new GridPoint(0, 0), board.ActivePiece.Origin);

            Assert.IsTrue(board.TryMoveRight());
            Assert.IsTrue(board.TryMoveRight());
            Assert.IsFalse(board.TryMoveRight());
            Assert.AreEqual(new GridPoint(2, 0), board.ActivePiece.Origin);
        }

        /// <summary>
        /// Kiểm tra khối ở đáy không rơi qua y bằng âm và trạng thái không bị thay đổi.
        /// </summary>
        [Test]
        public void FallAtBottom_DoesNotMutateActivePiece()
        {
            BoardModel board = CreateBoard();
            Assert.IsTrue(board.TrySpawn(CreateSquare(), new GridPoint(0, 0)));

            Assert.IsFalse(board.TryFallOneCell());
            Assert.AreEqual(new GridPoint(0, 0), board.ActivePiece.Origin);
        }

        /// <summary>
        /// Kiểm tra hai hàng ẩn là một phần hợp lệ của board và có thể chứa cell đã lock.
        /// </summary>
        [Test]
        public void HiddenRows_AcceptAndKeepLockedCells()
        {
            BoardModel board = CreateBoard();
            Assert.IsTrue(board.TrySpawn(CreateSquare(), new GridPoint(1, 4)));

            Assert.IsTrue(board.TryLockActivePiece());
            Assert.IsTrue(board.IsSettledAt(new GridPoint(1, 4)));
            Assert.IsTrue(board.IsSettledAt(new GridPoint(2, 5)));
            Assert.AreEqual(4, board.GetSettledCells().Count);
        }

        /// <summary>
        /// Kiểm tra spawn vượt đỉnh vùng ẩn bị từ chối và không để lại active piece.
        /// </summary>
        [Test]
        public void SpawnOutsideHiddenRows_ReturnsFalseWithoutActivePiece()
        {
            BoardModel board = CreateBoard();

            Assert.IsFalse(board.TrySpawn(CreateSquare(), new GridPoint(1, 5)));
            Assert.IsFalse(board.HasActivePiece);
        }

        /// <summary>
        /// Kiểm tra spawn trùng các cell đã lock bị từ chối và board không có active piece mới.
        /// </summary>
        [Test]
        public void SpawnBlockedBySettledCell_ReturnsFalseWithoutMutation()
        {
            BoardModel board = CreateBoard();
            PieceDefinition square = CreateSquare();
            Assert.IsTrue(board.TrySpawn(square, new GridPoint(0, 0)));
            Assert.IsTrue(board.TryLockActivePiece());

            Assert.IsFalse(board.TrySpawn(square, new GridPoint(0, 0)));
            Assert.IsFalse(board.HasActivePiece);
            Assert.AreEqual(4, board.GetSettledCells().Count);
        }

        /// <summary>
        /// Kiểm tra lock ghi đúng bốn cell và tách active piece khỏi lưới settled.
        /// </summary>
        [Test]
        public void LockActivePiece_SettlesCellsAndClearsActivePiece()
        {
            BoardModel board = CreateBoard();
            Assert.IsTrue(board.TrySpawn(CreateSquare(), new GridPoint(1, 1)));

            Assert.IsTrue(board.TryLockActivePiece());
            Assert.IsFalse(board.HasActivePiece);
            Assert.IsTrue(board.IsSettledAt(new GridPoint(1, 1)));
            Assert.IsTrue(board.IsSettledAt(new GridPoint(2, 2)));
            Assert.IsFalse(board.TryLockActivePiece());
        }

        /// <summary>
        /// Kiểm tra không thể spawn khối thứ hai khi active piece đầu tiên chưa lock và state đầu tiên được giữ nguyên.
        /// </summary>
        [Test]
        public void SpawnWhileActive_ReturnsFalseAndKeepsCurrentPiece()
        {
            BoardModel board = CreateBoard();
            PieceDefinition square = CreateSquare();
            Assert.IsTrue(board.TrySpawn(square, new GridPoint(0, 0)));

            Assert.IsFalse(board.TrySpawn(square, new GridPoint(2, 0)));
            Assert.AreEqual(new GridPoint(0, 0), board.ActivePiece.Origin);
        }

        /// <summary>
        /// Tạo board nhỏ có bốn hàng hiển thị và hai hàng ẩn để test biên gọn.
        /// </summary>
        /// <returns>BoardModel rỗng.</returns>
        private static BoardModel CreateBoard()
        {
            return new BoardModel(new BoardSettings(4, 4, 2));
        }

        /// <summary>
        /// Tạo tetromino vuông 2×2 với ô tương đối bắt đầu tại góc đáy trái.
        /// </summary>
        /// <returns>Định nghĩa piece vuông hợp lệ.</returns>
        private static PieceDefinition CreateSquare()
        {
            return new PieceDefinition(
                "O",
                new List<GridPoint>
                {
                    new GridPoint(0, 0),
                    new GridPoint(1, 0),
                    new GridPoint(0, 1),
                    new GridPoint(1, 1)
                },
                0.5f,
                0.5f);
        }
    }
}
