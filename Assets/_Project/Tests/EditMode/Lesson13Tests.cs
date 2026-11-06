using System.Collections.Generic;
using System.Linq;
using NeonBlocks.Audio;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NUnit.Framework;
namespace NeonBlocks.Tests
{
    /// <summary>Regression danh tính cell và feedback sau commit, không phụ thuộc frame Unity.</summary>
    public sealed class Lesson13Tests
    {
        /// <summary>Khóa giữ ID thay vì chỉ giữ occupied flag.</summary>
        [Test]
        public void Lock_PreservesPieceIdentity()
        {
            var loop = CreateLoop(); loop.BeginNextPiece(); loop.HardDrop();
            Assert.AreEqual(4, loop.Board.GetSettledTiles().Count);
            Assert.IsTrue(loop.Board.GetSettledTiles().All(c => c.PieceId == "I"));
        }
        /// <summary>Compact kéo cả ID hàng trên xuống sau clear.</summary>
        [Test]
        public void Clear_CompactsIdentityTogetherWithPosition()
        {
            GameConfig config = Config();
            var board = new BoardModel(new BoardSettings(4, 20, 4));
            Assert.IsTrue(board.TrySpawn(config.Pieces.Single(p => p.Id == "O"), new GridPoint(0, -1)));
            Assert.IsTrue(board.TryLockActivePiece());
            Assert.IsTrue(board.TrySpawn(config.Pieces.Single(p => p.Id == "I"), new GridPoint(0, -2)));
            Assert.IsTrue(board.TryLockActivePiece());
            Assert.AreEqual(1, board.ClearFullRows());
            Assert.AreEqual(4, board.GetSettledTiles().Count);
            Assert.IsTrue(board.GetSettledTiles().All(c => c.PieceId == "O"));
            Assert.AreEqual(0, board.GetSettledTiles().Min(c => c.Position.Y));
        }
        /// <summary>Drop phát một impact, không đồng thời phát Lock.</summary>
        [Test]
        public void HardDrop_EmitsOneImpactAndCannotReplay()
        {
            var loop = CreateLoop(); loop.BeginNextPiece(); loop.HardDrop();
            var events = Drain(loop);
            CollectionAssert.AreEqual(new[] { FeedbackKind.Drop }, events.Select(e => e.Kind));
            Assert.IsEmpty(Drain(loop));
        }
        /// <summary>Xoay hợp lệ có event; O giữ nguyên hình nên không có event.</summary>
        [Test]
        public void Rotate_EmitsOnlyWhenOrientationChanges()
        {
            var loop = CreateLoop(); loop.BeginNextPiece();
            Assert.IsTrue(loop.TryRotateClockwise());
            Assert.AreEqual(FeedbackKind.Rotate, Drain(loop).Single().Kind);
            loop.TryHold();
            Assert.AreEqual("O", loop.Board.ActivePiece.Definition.Id);
            Assert.IsTrue(loop.TryRotateClockwise()); Assert.IsEmpty(Drain(loop));
        }
        /// <summary>Pause chặn thao tác và feedback.</summary>
        [Test]
        public void Paused_DoesNotEmitRotateOrDrop()
        {
            var loop = CreateLoop(); loop.BeginNextPiece(); loop.Pause();
            Assert.IsFalse(loop.TryRotateClockwise()); Assert.IsFalse(loop.HardDrop());
            Assert.IsEmpty(Drain(loop));
        }
        /// <summary>Spawn thất bại chỉ kết thúc một lần dù người dùng gửi lại command.</summary>
        [Test]
        public void SpawnFailure_EmitsGameOverOnce()
        {
            var loop = CreateLoop();
            var config = Config();
            loop.Board.TrySpawn(config.Pieces[0], config.SpawnOrigin); loop.Board.TryLockActivePiece();
            Assert.IsFalse(loop.BeginNextPiece()); Assert.IsFalse(loop.BeginNextPiece());
            CollectionAssert.AreEqual(new[] { FeedbackKind.GameOver }, Drain(loop).Select(e => e.Kind));
        }
        /// <summary>Ván mới có ID riêng dù sequence bắt đầu lại.</summary>
        [Test]
        public void Restart_UsesDistinctFeedbackSession()
        {
            var first = CreateLoop(); first.BeginNextPiece(); first.HardDrop();
            var second = CreateLoop(); second.BeginNextPiece(); second.HardDrop();
            GameplayFeedback a = Drain(first).Single(), b = Drain(second).Single();
            Assert.AreEqual(1, a.Sequence); Assert.AreEqual(1, b.Sequence);
            Assert.AreNotEqual(a.SessionId, b.SessionId);
        }
        /// <summary>Mute dùng floor dB, gain 1 dùng 0dB.</summary>
        [Test]
        public void AudioGain_UsesSafeFloor()
        { Assert.AreEqual(-80, AudioHost.GainToDb(0)); Assert.AreEqual(0, AudioHost.GainToDb(1)); Assert.AreEqual(-6.0206f, AudioHost.GainToDb(.5f), .001f); }
        /// <summary>Config thiếu key audio phải fail sớm trước khi tải scene.</summary>
        [Test]
        public void Config_MissingAudioKeyIsRejected()
        {
            var json = ConfigTestFiles.LoadValidJson(); json.Remove("audioBank");
            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(json.ToString()));
        }
        /// <summary>Lấy event một lần giống Presenter runtime.</summary>
        private static List<GameplayFeedback> Drain(GameLoop loop)
        { var result = new List<GameplayFeedback>(); while (loop.TryTakeFeedback(out GameplayFeedback item)) { result.Add(item); } return result; }
        /// <summary>Dùng config thật và thứ tự bag xác định.</summary>
        private static GameLoop CreateLoop()
        {
            var config = Config();
            return new GameLoop(new BoardModel(config.Board), new SevenBagPieceSource(config.Pieces, new FixedPieceRandom(6,5,4,3,2,1), config.NextQueueSize),
                new GameScore(config.Scoring, config.StartLevel, config.LinesPerLevel), config.SpawnOrigin, 1, .5f, config.Timing.MaxLockResets);
        }
        /// <summary>Đọc cấu hình chuẩn của project.</summary>
        private static GameConfig Config() => GameConfigParser.Parse(ConfigTestFiles.ReadGameConfigJson());
    }
}