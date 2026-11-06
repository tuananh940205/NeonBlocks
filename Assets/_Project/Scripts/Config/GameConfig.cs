using System;
using System.Collections.Generic;
using System.Linq;

namespace NeonBlocks.Config
{
    /// <summary>
    /// Toàn bộ cấu hình chung của game, đọc từ một file JSON duy nhất. Bất biến, chỉ set qua constructor;
    /// constructor kiểm tra toàn bộ quan hệ giữa các phần (spawn nằm trong bảng, đủ 7 khối, level liên tiếp).
    /// Không chứa tham chiếu tới Unity Object; chỉ lưu số liệu luật chơi.
    /// </summary>
    public sealed class GameConfig
    {
        /// <summary>Tập mã khối hợp lệ; cấu hình phải có đúng bảy khối, mỗi mã một lần.</summary>
        private static readonly HashSet<string> AllowedPieceIds = new HashSet<string> { "I", "O", "T", "J", "L", "S", "Z" };

        /// <summary>Kích thước bảng gồm hàng hiển thị và hàng ẩn.</summary>
        public BoardSettings Board { get; }
        public PopupKeys Ui { get; }
        public string AudioBankKey { get; }

        /// <summary>Ô góc đáy trái của khung 4×4 khi khối spawn, theo hệ tọa độ bảng (y lên trên).</summary>
        public GridPoint SpawnOrigin { get; }

        /// <summary>Số khối hiển thị trong hàng đợi Next.</summary>
        public int NextQueueSize { get; }

        /// <summary>Level đầu tiên của ván chơi.</summary>
        public int StartLevel { get; }

        /// <summary>Số hàng cần xóa để tăng một level.</summary>
        public int LinesPerLevel { get; }

        /// <summary>Tên scene Entry, MainMenu và Gameplay.</summary>
        public SceneKeys Scenes { get; }

        /// <summary>Bảng tốc độ rơi theo level, liên tiếp từ StartLevel; chỉ đọc.</summary>
        public IReadOnlyList<LevelData> Levels { get; }

        /// <summary>Luật tính điểm.</summary>
        public ScoringRules Scoring { get; }

        /// <summary>Thời gian lock delay, reset và DAS/ARR.</summary>
        public TimingRules Timing { get; }

        /// <summary>Bảy định nghĩa tetromino; chỉ đọc.</summary>
        public IReadOnlyList<PieceDefinition> Pieces { get; }

        /// <summary>
        /// Tạo cấu hình chung và kiểm tra quan hệ giữa các thành phần.
        /// </summary>
        /// <param name="board">Kích thước bảng.</param>
        /// <param name="spawnOrigin">Góc đáy trái khung spawn; khung 4×4 phải nằm trọn trong bảng.</param>
        /// <param name="nextQueueSize">Số khối trong hàng đợi Next, phải &gt; 0.</param>
        /// <param name="startLevel">Level bắt đầu, phải ≥ 1.</param>
        /// <param name="linesPerLevel">Số hàng để tăng level, phải &gt; 0.</param>
        /// <param name="scenes">Tên scene, không được null.</param>
        /// <param name="levels">Bảng level, phải bắt đầu đúng từ startLevel và liên tiếp.</param>
        /// <param name="scoring">Luật điểm.</param>
        /// <param name="timing">Luật thời gian.</param>
        /// <param name="pieces">Đúng bảy khối I, O, T, J, L, S, Z, mỗi khối một lần.</param>
        public GameConfig(
            BoardSettings board,
            GridPoint spawnOrigin,
            int nextQueueSize,
            int startLevel,
            int linesPerLevel,
            SceneKeys scenes,
            IEnumerable<LevelData> levels,
            ScoringRules scoring,
            TimingRules timing,
            IEnumerable<PieceDefinition> pieces, PopupKeys ui, string audioBank = "AudioBank")
        {
            if (string.IsNullOrWhiteSpace(audioBank)) { throw new ConfigValidationException("audioBank bị thiếu."); }
            AudioBankKey = audioBank;
            Ui = ui ?? throw new ConfigValidationException("ui bị thiếu.");
            Board = board ?? throw new ConfigValidationException("board bị thiếu.");
            Scoring = scoring ?? throw new ConfigValidationException("scoring bị thiếu.");
            Timing = timing ?? throw new ConfigValidationException("timing bị thiếu.");

            if (nextQueueSize <= 0)
            {
                throw new ConfigValidationException($"nextQueueSize phải > 0; giá trị hiện tại: {nextQueueSize}.");
            }

            if (startLevel < 1)
            {
                throw new ConfigValidationException($"startLevel phải ≥ 1; giá trị hiện tại: {startLevel}.");
            }

            if (linesPerLevel <= 0)
            {
                throw new ConfigValidationException($"linesPerLevel phải > 0; giá trị hiện tại: {linesPerLevel}.");
            }

            List<LevelData> levelList = levels?.ToList() ?? throw new ConfigValidationException("levels bị thiếu.");

            if (levelList.Count == 0)
            {
                throw new ConfigValidationException("levels phải có ít nhất một level.");
            }

            for (int i = 0; i < levelList.Count; i++)
            {
                if (levelList[i].Level != startLevel + i)
                {
                    throw new ConfigValidationException($"levels phải liên tiếp từ startLevel={startLevel}; vị trí {i} có level {levelList[i].Level}.");
                }
            }

            List<PieceDefinition> pieceList = pieces?.ToList() ?? throw new ConfigValidationException("pieces bị thiếu.");

            if (pieceList.Count != AllowedPieceIds.Count)
            {
                throw new ConfigValidationException($"pieces phải có đúng {AllowedPieceIds.Count} khối; hiện tại: {pieceList.Count}.");
            }

            var seenIds = new HashSet<string>();
            foreach (PieceDefinition piece in pieceList)
            {
                if (!AllowedPieceIds.Contains(piece.Id))
                {
                    throw new ConfigValidationException($"piece '{piece.Id}' không thuộc tập I, O, T, J, L, S, Z.");
                }

                if (!seenIds.Add(piece.Id))
                {
                    throw new ConfigValidationException($"piece '{piece.Id}' bị khai báo trùng.");
                }
            }

            bool spawnFits = spawnOrigin.X >= 0
                && spawnOrigin.Y >= 0
                && spawnOrigin.X + PieceDefinition.BoxSize <= board.Width
                && spawnOrigin.Y + PieceDefinition.BoxSize <= board.TotalHeight;

            if (!spawnFits)
            {
                throw new ConfigValidationException(
                    $"spawn {spawnOrigin} với khung {PieceDefinition.BoxSize}×{PieceDefinition.BoxSize} nằm ngoài bảng {board.Width}×{board.TotalHeight}.");
            }

            SpawnOrigin = spawnOrigin;
            NextQueueSize = nextQueueSize;
            StartLevel = startLevel;
            LinesPerLevel = linesPerLevel;
            Scenes = scenes ?? throw new ConfigValidationException("scenes bị thiếu.");
            Levels = Array.AsReadOnly(levelList.ToArray());
            Pieces = Array.AsReadOnly(pieceList.ToArray());
        }

        /// <summary>
        /// Lấy dữ liệu level; level vượt quá bảng thì dùng level cuối cùng của bảng.
        /// </summary>
        /// <param name="level">Số level cần tra, không được nhỏ hơn StartLevel.</param>
        /// <returns>Dữ liệu tốc độ rơi của level đó.</returns>
        public LevelData GetLevel(int level)
        {
            if (level < StartLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(level), $"level {level} nhỏ hơn startLevel {StartLevel}.");
            }

            int index = Math.Min(level - StartLevel, Levels.Count - 1);
            return Levels[index];
        }
    }
}
