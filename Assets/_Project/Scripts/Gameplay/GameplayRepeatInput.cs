using System;
using System.Collections.Generic;

namespace NeonBlocks.Gameplay
{
    /// <summary>Điều phối lặp input thuần C# cho trái/phải/rơi mềm bằng DAS và ARR cấu hình.</summary>
    public sealed class GameplayRepeatInput
    {
        private readonly float dasSeconds;
        private readonly float arrSeconds;
        private bool leftHeld;
        private bool rightHeld;
        private bool softDropHeld;
        private GameplayHeldCommand activeHorizontal;
        private float horizontalElapsed;
        private float softDropElapsed;
        private bool horizontalRepeated;
        private bool softDropRepeated;

        /// <summary>Tạo repeat controller với DAS/ARR hợp lệ từ TimingRules.</summary>
        public GameplayRepeatInput(float dasSeconds, float arrSeconds)
        {
            if (dasSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(dasSeconds));
            if (arrSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(arrSeconds));
            this.dasSeconds = dasSeconds;
            this.arrSeconds = arrSeconds;
        }

        /// <summary>Đánh dấu một lệnh bắt đầu giữ và trả true khi presenter cần áp dụng lần đầu ngay lập tức.</summary>
        public bool Begin(GameplayHeldCommand command)
        {
            switch (command)
            {
                case GameplayHeldCommand.MoveLeft:
                    if (leftHeld) return false;
                    leftHeld = true;
                    ActivateHorizontal(GameplayHeldCommand.MoveLeft);
                    return true;
                case GameplayHeldCommand.MoveRight:
                    if (rightHeld) return false;
                    rightHeld = true;
                    ActivateHorizontal(GameplayHeldCommand.MoveRight);
                    return true;
                case GameplayHeldCommand.SoftDrop:
                    if (softDropHeld) return false;
                    softDropHeld = true;
                    softDropElapsed = 0f;
                    softDropRepeated = false;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Đánh dấu lệnh được thả; nếu còn giữ hướng kia thì chuyển active direction mà không tạo lệnh thừa.</summary>
        public void End(GameplayHeldCommand command)
        {
            if (command == GameplayHeldCommand.MoveLeft) leftHeld = false;
            if (command == GameplayHeldCommand.MoveRight) rightHeld = false;
            if (command == GameplayHeldCommand.SoftDrop) softDropHeld = false;

            if (command == GameplayHeldCommand.SoftDrop)
            {
                softDropElapsed = 0f;
                softDropRepeated = false;
                return;
            }

            if (activeHorizontal == command)
            {
                if (leftHeld) ActivateHorizontal(GameplayHeldCommand.MoveLeft);
                else if (rightHeld) ActivateHorizontal(GameplayHeldCommand.MoveRight);
                else ResetHorizontal();
            }
        }

        /// <summary>Thêm các lệnh repeat đến hạn vào output; caller chịu trách nhiệm kiểm tra game state trước khi gọi.</summary>
        public void CollectRepeats(float deltaSeconds, ICollection<GameplayCommand> output)
        {
            if (deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (leftHeld || rightHeld) Collect(ref horizontalElapsed, ref horizontalRepeated, activeHorizontal == GameplayHeldCommand.MoveLeft ? GameplayCommand.MoveLeft : GameplayCommand.MoveRight, deltaSeconds, output);
            if (softDropHeld) Collect(ref softDropElapsed, ref softDropRepeated, GameplayCommand.SoftDrop, deltaSeconds, output);
        }

        /// <summary>Hủy toàn bộ trạng thái giữ khi pause, game over, mất focus hoặc scene rời đi.</summary>
        public void Clear()
        {
            leftHeld = false;
            rightHeld = false;
            softDropHeld = false;
            ResetHorizontal();
            softDropElapsed = 0f;
            softDropRepeated = false;
        }

        /// <summary>Đổi hướng đang ưu tiên và bắt đầu lại DAS để hai phím/ngón không nhân lệnh.</summary>
        private void ActivateHorizontal(GameplayHeldCommand command)
        {
            activeHorizontal = command;
            horizontalElapsed = 0f;
            horizontalRepeated = false;
        }

        /// <summary>Reset timer ngang sau khi không còn hướng nào được giữ.</summary>
        private void ResetHorizontal()
        {
            horizontalElapsed = 0f;
            horizontalRepeated = false;
        }

        /// <summary>Áp dụng DAS cho repeat đầu và ARR cho các repeat sau; ARR bằng 0 chỉ sinh tối đa một lệnh mỗi frame.</summary>
        private void Collect(ref float elapsed, ref bool repeated, GameplayCommand command, float deltaSeconds, ICollection<GameplayCommand> output)
        {
            elapsed += deltaSeconds;
            if (!repeated)
            {
                if (elapsed < dasSeconds) return;
                output.Add(command);
                repeated = true;
                elapsed -= dasSeconds;
            }

            if (arrSeconds <= 0f)
            {
                return;
            }

            while (elapsed >= arrSeconds)
            {
                output.Add(command);
                elapsed -= arrSeconds;
            }
        }
    }
}
