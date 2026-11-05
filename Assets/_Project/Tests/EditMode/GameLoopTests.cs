using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NUnit.Framework;

namespace NeonBlocks.Tests
{
    /// <summary>Kiểm thử tick deterministic, pause, hard drop, lock delay và reset cap của GameLoop.</summary>
    public class GameLoopTests
    {
        /// <summary>Kiểm tra delta lớn thực hiện đúng nhiều bước gravity.</summary>
        [Test]
        public void Tick_LargeDelta_AppliesMultipleGravitySteps()
        {
            GameLoop loop = CreateLoop(0.1f, 1f, 15);
            Assert.IsTrue(loop.BeginNextPiece());
            int startY = loop.Board.ActivePiece.Origin.Y;
            loop.Tick(0.35f);
            Assert.AreEqual(startY - 3, loop.Board.ActivePiece.Origin.Y);
        }

        /// <summary>Kiểm tra Pause không làm gravity tích lũy và Resume cho tick hoạt động lại.</summary>
        [Test]
        public void Pause_StopsTickUntilResume()
        {
            GameLoop loop = CreateLoop(0.1f, 1f, 15);
            loop.BeginNextPiece(); int startY = loop.Board.ActivePiece.Origin.Y;
            Assert.IsTrue(loop.Pause()); loop.Tick(1f); Assert.AreEqual(startY, loop.Board.ActivePiece.Origin.Y);
            Assert.IsTrue(loop.Resume()); loop.Tick(0.1f); Assert.AreEqual(startY - 1, loop.Board.ActivePiece.Origin.Y);
        }

        /// <summary>Kiểm tra HardDrop khóa một lần và đưa loop về Ready, không spawn tự động.</summary>
        [Test]
        public void HardDrop_LocksOnceAndReturnsReady()
        {
            GameLoop loop = CreateLoop(1f, 1f, 15); loop.BeginNextPiece();
            Assert.IsTrue(loop.HardDrop()); Assert.AreEqual(GameSessionState.Ready, loop.State);
            Assert.IsFalse(loop.HardDrop()); Assert.IsFalse(loop.Board.HasActivePiece);
        }

        /// <summary>Kiểm tra move hợp lệ khi chạm đất chỉ reset lock delay đến giới hạn cấu hình.</summary>
        [Test]
        public void GroundedMove_ResetsLockDelayOnlyToCap()
        {
            GameLoop loop = CreateLoop(0.1f, 100f, 1); loop.BeginNextPiece(); loop.Tick(5f);
            Assert.IsTrue(loop.TryMoveLeft() || loop.TryMoveRight()); Assert.AreEqual(1, loop.LockResetCount);
            Assert.IsTrue(loop.TryMoveLeft() || loop.TryMoveRight()); Assert.AreEqual(1, loop.LockResetCount);
        }

        /// <summary>Tạo loop dùng config thật và nguồn seed để test không phụ thuộc Unity.</summary>
        private static GameLoop CreateLoop(float gravity, float delay, int cap)
        {
            GameConfig config = GameConfigParser.Parse(ConfigTestFiles.ReadGameConfigJson());
            return new GameLoop(new BoardModel(config.Board), new SevenBagPieceSource(config.Pieces, 1, config.NextQueueSize), config.SpawnOrigin, gravity, delay, cap);
        }

    }
}
