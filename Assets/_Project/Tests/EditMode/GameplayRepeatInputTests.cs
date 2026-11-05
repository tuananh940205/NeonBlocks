using System.Collections.Generic;
using NeonBlocks.Gameplay;
using NUnit.Framework;

namespace NeonBlocks.Tests
{
    /// <summary>Kiểm thử DAS/ARR thuần C# cho keyboard và touch dùng chung repeat controller.</summary>
    public class GameplayRepeatInputTests
    {
        /// <summary>Kiểm tra press đầu tiên chỉ được chấp nhận một lần trước DAS.</summary>
        [Test]
        public void Begin_SameHeldCommandTwice_OnlyFirstPressIsAccepted()
        {
            var input = new GameplayRepeatInput(0.15f, 0.05f);
            Assert.IsTrue(input.Begin(GameplayHeldCommand.MoveLeft));
            Assert.IsFalse(input.Begin(GameplayHeldCommand.MoveLeft));
        }

        /// <summary>Kiểm tra repeat đầu xảy ra đúng khi DAS hết hạn.</summary>
        [Test]
        public void CollectRepeats_AfterDas_AddsOneHorizontalCommand()
        {
            var input = new GameplayRepeatInput(0.15f, 0.05f);
            var commands = new List<GameplayCommand>();
            input.Begin(GameplayHeldCommand.MoveLeft);
            input.CollectRepeats(0.14f, commands);
            Assert.IsEmpty(commands);
            input.CollectRepeats(0.01f, commands);
            CollectionAssert.AreEqual(new[] { GameplayCommand.MoveLeft }, commands);
        }

        /// <summary>Kiểm tra repeat tiếp theo tuân theo ARR sau lần lặp DAS đầu tiên.</summary>
        [Test]
        public void CollectRepeats_AfterArr_AddsNextHorizontalCommand()
        {
            var input = new GameplayRepeatInput(0.15f, 0.05f);
            var commands = new List<GameplayCommand>();
            input.Begin(GameplayHeldCommand.MoveRight);
            input.CollectRepeats(0.15f, commands);
            input.CollectRepeats(0.05f, commands);
            CollectionAssert.AreEqual(new[] { GameplayCommand.MoveRight, GameplayCommand.MoveRight }, commands);
        }

        /// <summary>Kiểm tra release hủy hoàn toàn lệnh lặp còn chờ.</summary>
        [Test]
        public void End_CancelsPendingRepeat()
        {
            var input = new GameplayRepeatInput(0.15f, 0.05f);
            var commands = new List<GameplayCommand>();
            input.Begin(GameplayHeldCommand.SoftDrop);
            input.End(GameplayHeldCommand.SoftDrop);
            input.CollectRepeats(1f, commands);
            Assert.IsEmpty(commands);
        }
    }
}
