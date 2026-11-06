using System;
using System.Collections.Generic;
using NeonBlocks.Config;

namespace NeonBlocks.Gameplay
{
    /// <summary>Snapshot bất biến chỉ phục vụ render UI, không cấp quyền sửa luật hay BoardModel.</summary>
    public sealed class GameplaySnapshot
    {
        /// <summary>Kích thước board dùng để quy đổi tọa độ sang UI.</summary>
        public BoardSettings Settings { get; }
        /// <summary>Ô đã khóa trên board.</summary>
        public IReadOnlyList<GridPoint> SettledCells { get; }
        public IReadOnlyList<SettledTile> SettledTiles { get; }
        /// <summary>Ô của piece đang rơi.</summary>
        public IReadOnlyList<GridPoint> ActiveCells { get; }
        /// <summary>Ô của vị trí hard drop dự đoán, không thay đổi model.</summary>
        public IReadOnlyList<GridPoint> GhostCells { get; }
        /// <summary>Mã piece đang rơi để view chọn màu placeholder.</summary>
        public string ActivePieceId { get; }
        /// <summary>Các mã piece kế tiếp để hiển thị Next.</summary>
        public IReadOnlyList<string> NextPieceIds { get; }
        /// <summary>Mã khối Hold, null nếu ô đang rỗng.</summary>
        public string HeldPieceId { get; }
        /// <summary>Cho UI biết người chơi còn quyền Hold ở snapshot này.</summary>
        public bool CanHold { get; }
        /// <summary>Trạng thái vòng chơi hiện tại.</summary>
        public GameSessionState State { get; }
        /// <summary>Điểm hiện tại.</summary>
        public int Score { get; }
        /// <summary>Số hàng đã xóa.</summary>
        public int Lines { get; }
        /// <summary>Level hiện tại.</summary>
        public int Level { get; }

        /// <summary>Tạo snapshot và sao chép các danh sách để view không thể sửa state của game.</summary>
        public GameplaySnapshot(BoardSettings settings, IEnumerable<GridPoint> settledCells, IEnumerable<GridPoint> activeCells, IEnumerable<GridPoint> ghostCells, string activePieceId, IEnumerable<string> nextPieceIds, GameSessionState state, int score, int lines, int level, string heldPieceId = null, bool canHold = false, IEnumerable<SettledTile> settledTiles = null)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SettledCells = Array.AsReadOnly(new List<GridPoint>(settledCells ?? throw new ArgumentNullException(nameof(settledCells))).ToArray());
            ActiveCells = Array.AsReadOnly(new List<GridPoint>(activeCells ?? throw new ArgumentNullException(nameof(activeCells))).ToArray());
            GhostCells = Array.AsReadOnly(new List<GridPoint>(ghostCells ?? throw new ArgumentNullException(nameof(ghostCells))).ToArray());
            ActivePieceId = activePieceId;
            NextPieceIds = Array.AsReadOnly(new List<string>(nextPieceIds ?? throw new ArgumentNullException(nameof(nextPieceIds))).ToArray());
            State = state;
            Score = score;
            Lines = lines;
            Level = level;
            HeldPieceId = heldPieceId;
            CanHold = canHold;
            SettledTiles = Array.AsReadOnly(new List<SettledTile>(settledTiles ?? new SettledTile[0]).ToArray());
        }
    }
}
