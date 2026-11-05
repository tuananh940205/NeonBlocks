using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO gốc của file GameConfig.json, dùng riêng cho Newtonsoft deserialize. Mọi trường chỉ đọc,
    /// giá trị được gán qua constructor; không dùng trực tiếp trong gameplay.
    /// </summary>
    public sealed class GameConfigDto
    {
        /// <summary>Số khối trong hàng đợi Next.</summary>
        public readonly int nextQueueSize;

        /// <summary>Level bắt đầu.</summary>
        public readonly int startLevel;

        /// <summary>Số hàng để tăng một level.</summary>
        public readonly int linesPerLevel;

        /// <summary>Tên scene dùng trong game.</summary>
        public readonly SceneKeysDto scenes;

        /// <summary>Kích thước bảng.</summary>
        public readonly BoardDto board;

        /// <summary>Góc đáy trái khung spawn.</summary>
        public readonly GridPointDto spawn;

        /// <summary>Bảng tốc độ rơi theo level.</summary>
        public readonly LevelDto[] levels;

        /// <summary>Luật điểm.</summary>
        public readonly ScoringDto scoring;

        /// <summary>Luật thời gian.</summary>
        public readonly TimingDto timing;

        /// <summary>Bảy khối tetromino.</summary>
        public readonly PieceDto[] pieces;

        /// <summary>
        /// Constructor để Newtonsoft gán toàn bộ trường từ JSON theo tên tham số.
        /// </summary>
        /// <param name="nextQueueSize">Số khối trong hàng đợi Next.</param>
        /// <param name="startLevel">Level bắt đầu.</param>
        /// <param name="linesPerLevel">Số hàng để tăng level.</param>
        /// <param name="scenes">Tên scene.</param>
        /// <param name="board">Kích thước bảng.</param>
        /// <param name="spawn">Góc đáy trái khung spawn.</param>
        /// <param name="levels">Bảng level.</param>
        /// <param name="scoring">Luật điểm.</param>
        /// <param name="timing">Luật thời gian.</param>
        /// <param name="pieces">Các khối tetromino.</param>
        [JsonConstructor]
        public GameConfigDto(
            int nextQueueSize,
            int startLevel,
            int linesPerLevel,
            SceneKeysDto scenes,
            BoardDto board,
            GridPointDto spawn,
            LevelDto[] levels,
            ScoringDto scoring,
            TimingDto timing,
            PieceDto[] pieces)
        {
            this.nextQueueSize = nextQueueSize;
            this.startLevel = startLevel;
            this.linesPerLevel = linesPerLevel;
            this.scenes = scenes;
            this.board = board;
            this.spawn = spawn;
            this.levels = levels;
            this.scoring = scoring;
            this.timing = timing;
            this.pieces = pieces;
        }
    }
}
