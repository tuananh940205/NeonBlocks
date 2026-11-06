using System.Linq;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NUnit.Framework;

namespace NeonBlocks.Tests
{
    /// <summary>Kiểm thử Hold bằng config thật, không phụ thuộc thời gian Unity hoặc input.</summary>
    public sealed class HoldTests
    {
        /// <summary>Hold lần đầu lấy đúng Next, lưu khối cũ, không khóa ô hoặc cộng điểm.</summary>
        [Test]
        public void FirstHold_StoresActiveAndConsumesOneNext()
        {
            GameLoop loop = CreateLoop(); loop.BeginNextPiece();
            string outgoing = loop.Board.ActivePiece.Definition.Id;
            string incoming = loop.NextPieces[0].Id;
            string second = loop.NextPieces[1].Id;
            Assert.IsTrue(loop.TryHold());
            Assert.AreEqual(outgoing, loop.HeldPiece.Id);
            Assert.AreEqual(incoming, loop.Board.ActivePiece.Definition.Id);
            Assert.AreEqual(second, loop.NextPieces[0].Id);
            Assert.AreEqual(5, loop.NextPieces.Count);
            Assert.IsEmpty(loop.Board.GetSettledCells());
            Assert.AreEqual(0, loop.Score.Score);
        }

        /// <summary>Hold lần hai bị chặn và không thay đổi object active hoặc queue.</summary>
        [Test]
        public void SecondHold_DoesNotMutate()
        {
            GameLoop loop = CreateLoop(); loop.BeginNextPiece(); loop.TryHold();
            ActivePiece active = loop.Board.ActivePiece;
            string[] next = loop.NextPieces.Select(p => p.Id).ToArray();
            Assert.IsFalse(loop.CanHold); Assert.IsFalse(loop.TryHold());
            Assert.AreSame(active, loop.Board.ActivePiece);
            CollectionAssert.AreEqual(next, loop.NextPieces.Select(p => p.Id));
        }

        /// <summary>Khóa/spawn trả quyền Hold; swap không tiêu thụ hàng đợi.</summary>
        [Test]
        public void LockThenSwap_PreservesQueueAndResetsSpawn()
        {
            GameConfig config = Config();
            GameLoop loop = CreateLoop(); loop.BeginNextPiece();
            string first = loop.Board.ActivePiece.Definition.Id;
            loop.TryRotateClockwise(); loop.TryMoveLeft(); loop.SoftDrop(); loop.TryHold();
            loop.HardDrop();
            Assert.IsTrue(loop.CanHold);
            string outgoing = loop.Board.ActivePiece.Definition.Id;
            string[] next = loop.NextPieces.Select(p => p.Id).ToArray();
            loop.TryMoveRight(); loop.TryRotateClockwise(); loop.SoftDrop();
            Assert.IsTrue(loop.TryHold());
            Assert.AreEqual(first, loop.Board.ActivePiece.Definition.Id);
            Assert.AreEqual(outgoing, loop.HeldPiece.Id);
            Assert.AreEqual(config.SpawnOrigin, loop.Board.ActivePiece.Origin);
            Assert.AreEqual(PieceOrientation.Spawn, loop.Board.ActivePiece.Orientation);
            CollectionAssert.AreEqual(next, loop.NextPieces.Select(p => p.Id));
            Assert.IsFalse(loop.CanHold);
        }

        /// <summary>Ready, Paused và GameOver không cho Hold.</summary>
        [Test]
        public void NonPlaying_HoldIsRejected()
        {
            GameLoop loop = CreateLoop();
            Assert.IsFalse(loop.TryHold());
            loop.BeginNextPiece(); loop.Pause();
            Assert.IsFalse(loop.CanHold); Assert.IsFalse(loop.TryHold()); Assert.IsNull(loop.HeldPiece);
            loop.Resume(); Assert.IsTrue(loop.CanHold);
            while (loop.State == GameSessionState.Playing) { loop.HardDrop(); }
            Assert.IsFalse(loop.TryHold());
        }

        /// <summary>Hold reset gravity để khối thay vào nhận đủ chu kỳ rơi.</summary>
        [Test]
        public void Hold_ResetsGravityTimer()
        {
            GameLoop loop = CreateLoop(); loop.BeginNextPiece();
            loop.Tick(0.8f); loop.TryHold();
            int y = loop.Board.ActivePiece.Origin.Y;
            loop.Tick(0.3f);
            Assert.AreEqual(y, loop.Board.ActivePiece.Origin.Y);
            loop.Tick(0.7f);
            Assert.AreEqual(y - 1, loop.Board.ActivePiece.Origin.Y);
            Assert.AreEqual(0, loop.LockResetCount);
        }

        /// <summary>Hold vào O bị chặn ở hàng 23 thì GameOver, không để active cũ hoặc điểm giả.</summary>
        [Test]
        public void FirstHold_BlockedSpawn_EndsGame()
        {
            GameConfig config = Config();
            var board = new BoardModel(config.Board);
            var blocker = new PieceDefinition("blocker", new[] { new GridPoint(0,0), new GridPoint(1,0), new GridPoint(2,0), new GridPoint(3,0) }, 1f, 0f);
            Assert.IsTrue(board.TrySpawn(blocker, new GridPoint(3, 23)));
            Assert.IsTrue(board.TryLockActivePiece());
            GameLoop loop = CreateLoop(board);
            Assert.IsTrue(loop.BeginNextPiece()); // I ở hàng 22, không đụng blocker hàng 23.
            Assert.AreEqual("I", loop.Board.ActivePiece.Definition.Id);
            Assert.IsTrue(loop.TryHold()); // Next O chiếm hàng 22/23.
            Assert.AreEqual(GameSessionState.GameOver, loop.State);
            Assert.IsNull(board.ActivePiece); Assert.IsFalse(loop.CanHold);
            Assert.AreEqual(4, board.GetSettledCells().Count);
            Assert.AreEqual(0, loop.Score.Score);
        }

        /// <summary>Swap từ Hold cũng kết thúc khi khối trả về không fit spawn.</summary>
        [Test]
        public void Swap_BlockedSpawn_EndsGame()
        {
            GameConfig config = Config();
            var board = new BoardModel(config.Board);
            GameLoop loop = CreateLoop(board);
            loop.BeginNextPiece(); loop.TryHold(); loop.HardDrop(); // Giữ I, active T.
            while (board.TryFallOneCell()) { } // Đưa active ra khỏi spawn.
            PieceDefinition outgoing = board.ActivePiece.Definition;
            Assert.IsTrue(board.TryReplaceActivePiece(config.Pieces.First(p => p.Id == "I"), config.SpawnOrigin));
            Assert.IsTrue(board.TryLockActivePiece()); // Chặn hàng spawn của I bằng ô settled.
            Assert.IsTrue(board.TrySpawn(outgoing, new GridPoint(3, 5)));
            Assert.IsTrue(loop.TryHold());
            Assert.AreEqual(GameSessionState.GameOver, loop.State);
            Assert.IsNull(board.ActivePiece);
        }

        /// <summary>Tạo ván mới không giữ Hold, score hoặc queue đã tiêu thụ của ván cũ.</summary>
        [Test]
        public void NewGame_ResetsHoldScoreAndQueue()
        {
            GameLoop old = CreateLoop(); old.BeginNextPiece(); old.TryHold(); old.HardDrop();
            Assert.IsNotNull(old.HeldPiece); Assert.Greater(old.Score.Score, 0);
            GameLoop fresh = CreateLoop(); fresh.BeginNextPiece();
            Assert.IsNull(fresh.HeldPiece); Assert.IsTrue(fresh.CanHold);
            Assert.AreEqual(0, fresh.Score.Score); Assert.AreEqual(0, fresh.Score.Lines); Assert.AreEqual(1, fresh.Score.Level);
            Assert.AreEqual("I", fresh.Board.ActivePiece.Definition.Id);
            Assert.AreEqual("O", fresh.NextPieces[0].Id);
        }

        /// <summary>Snapshot sao chép queue và giữ đúng dữ liệu Hold/permission khi queue nguồn đổi.</summary>
        [Test]
        public void Snapshot_PreservesHoldAndCopiesNext()
        {
            GameLoop loop = CreateLoop(); loop.BeginNextPiece(); loop.TryHold();
            var next = loop.NextPieces.Select(p => p.Id).ToList();
            var snapshot = new GameplaySnapshot(loop.Board.Settings, loop.Board.GetSettledCells(),
                new GridPoint[0], new GridPoint[0], loop.Board.ActivePiece.Definition.Id, next,
                loop.State, loop.Score.Score, loop.Score.Lines, loop.Score.Level, loop.HeldPiece.Id, loop.CanHold);
            next.Clear(); loop.HardDrop();
            Assert.AreEqual("I", snapshot.HeldPieceId); Assert.IsFalse(snapshot.CanHold);
            Assert.AreEqual(5, snapshot.NextPieceIds.Count);
        }

        /// <summary>Đọc config thật để mọi test dùng đúng spawn và shape của game.</summary>
        private static GameConfig Config() => GameConfigParser.Parse(ConfigTestFiles.ReadGameConfigJson());

        /// <summary>Tạo nguồn giữ nguyên thứ tự I/O/T/J/L/S/Z bằng RNG identity Fisher-Yates.</summary>
        private static GameLoop CreateLoop(BoardModel board = null)
        {
            GameConfig config = Config();
            return new GameLoop(board ?? new BoardModel(config.Board),
                new SevenBagPieceSource(config.Pieces, new FixedPieceRandom(6,5,4,3,2,1), config.NextQueueSize),
                new GameScore(config.Scoring, config.StartLevel, config.LinesPerLevel),
                config.SpawnOrigin, 1f, 0.5f, config.Timing.MaxLockResets);
        }
    }
}
