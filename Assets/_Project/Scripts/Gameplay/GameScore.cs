using System;
using NeonBlocks.Config;

namespace NeonBlocks.Gameplay
{
    /// <summary>Điểm, số hàng và level MVP; không có combo, back-to-back hoặc T-spin.</summary>
    public sealed class GameScore
    {
        private readonly ScoringRules rules;
        private readonly int startLevel;
        private readonly int linesPerLevel;

        /// <summary>Điểm hiện tại.</summary>
        public int Score { get; private set; }

        /// <summary>Tổng hàng đã clear.</summary>
        public int Lines { get; private set; }

        /// <summary>Level hiện tại.</summary>
        public int Level { get; private set; }

        /// <summary>Tạo state điểm từ luật config.</summary>
        public GameScore(ScoringRules rules, int startLevel, int linesPerLevel)
        {
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
            if (startLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startLevel));
            }

            if (linesPerLevel <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(linesPerLevel));
            }

            this.startLevel = startLevel;
            this.linesPerLevel = linesPerLevel;
            Level = startLevel;
        }

        /// <summary>Áp dụng điểm clear dùng level trước clear rồi cập nhật số hàng/level.</summary>
        public void ApplyLineClear(int cleared)
        {
            if (cleared < 0 || cleared > ScoringRules.LineClearEntries)
            {
                throw new ArgumentOutOfRangeException(nameof(cleared));
            }

            if (cleared == 0)
            {
                return;
            }

            Score += rules.LineClearPoints[cleared - 1] * Level;
            Lines += cleared;
            Level = startLevel + Lines / linesPerLevel;
        }

        /// <summary>Cộng điểm soft drop theo số ô đã rơi.</summary>
        /// <param name="cells">Số ô đã rơi, không âm.</param>
        public void AddSoftDrop(int cells)
        {
            if (cells < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cells));
            }

            Score += cells * rules.SoftDropPointsPerCell;
        }

        /// <summary>Cộng điểm hard drop theo số ô đã rơi.</summary>
        /// <param name="cells">Số ô đã rơi, không âm.</param>
        public void AddHardDrop(int cells)
        {
            if (cells < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cells));
            }

            Score += cells * rules.HardDropPointsPerCell;
        }
    }
}
