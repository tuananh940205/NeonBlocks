using System;
using System.Collections.Generic;
using NeonBlocks.Config;

namespace NeonBlocks.Gameplay
{
    /// <summary>Điều phối tick deterministic, gravity, lock delay, Hold và GameScore cho BoardModel; không phụ thuộc Unity time hoặc input.</summary>
    public sealed class GameLoop
    {
        private readonly BoardModel board;
        private readonly SevenBagPieceSource source;
        private readonly GridPoint spawnOrigin;
        private readonly float gravitySeconds;
        private readonly float lockDelaySeconds;
        private readonly int maxLockResets;
        private readonly GameScore score;
        private float gravityElapsed;
        private float lockElapsed;
        private int lockResetCount;
        private PieceDefinition heldPiece;
        private bool holdUsed;

        /// <summary>Trạng thái hiện tại của lượt game.</summary>
        public GameSessionState State { get; private set; }
        /// <summary>Board logic do loop điều phối, chỉ để view/test đọc state.</summary>
        public BoardModel Board => board;
        /// <summary>Số lần reset lock delay hợp lệ của piece đang rơi.</summary>
        public int LockResetCount => lockResetCount;
        /// <summary>Điểm, số hàng và level của lượt chơi hiện tại.</summary>
        public GameScore Score => score;
        /// <summary>Snapshot chỉ đọc của hàng đợi các khối sẽ xuất hiện tiếp theo.</summary>
        public IReadOnlyList<PieceDefinition> NextPieces => source.NextPieces;

        /// <summary>Định nghĩa khối đang được giữ, null khi chưa dùng Hold.</summary>
        public PieceDefinition HeldPiece => heldPiece;
        /// <summary>Quyền Hold chỉ mở khi Playing và chưa Hold trong lượt hiện tại.</summary>
        public bool CanHold => State == GameSessionState.Playing && board.HasActivePiece && !holdUsed;

        /// <summary>Tạo game loop với dependency thuần C# và timing của level hiện tại.</summary>
        public GameLoop(BoardModel board, SevenBagPieceSource source, GameScore score, GridPoint spawnOrigin, float gravitySeconds, float lockDelaySeconds, int maxLockResets)
        {
            this.board = board ?? throw new ArgumentNullException(nameof(board));
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.score = score ?? throw new ArgumentNullException(nameof(score));
            if (gravitySeconds <= 0f) throw new ArgumentOutOfRangeException(nameof(gravitySeconds));
            if (lockDelaySeconds < 0f) throw new ArgumentOutOfRangeException(nameof(lockDelaySeconds));
            if (maxLockResets < 0) throw new ArgumentOutOfRangeException(nameof(maxLockResets));
            this.spawnOrigin = spawnOrigin;
            this.gravitySeconds = gravitySeconds;
            this.lockDelaySeconds = lockDelaySeconds;
            this.maxLockResets = maxLockResets;
            State = GameSessionState.Ready;
        }

        /// <summary>Spawn piece kế tiếp khi Ready; spawn bị chặn chuyển GameOver.</summary>
        public bool BeginNextPiece()
        {
            if (State != GameSessionState.Ready) return false;
            if (!board.TrySpawn(source.TakeNext(), spawnOrigin)) { State = GameSessionState.GameOver; return false; }
            gravityElapsed = 0f; lockElapsed = 0f; lockResetCount = 0; holdUsed = false; State = GameSessionState.Playing; return true;
        }

        /// <summary>Giữ hoặc đổi khối đúng một lần mỗi lượt; spawn fail kết thúc ván, không khóa hay ghi điểm khối bỏ ra.</summary>
        public bool TryHold()
        {
            if (!CanHold)
            {
                // Không thay đổi board, queue hoặc timer khi hết quyền Hold hay đang pause.
                return false;
            }
            PieceDefinition outgoing = board.ActivePiece.Definition;
            PieceDefinition incoming = heldPiece ?? source.TakeNext();
            heldPiece = outgoing;
            holdUsed = true;
            gravityElapsed = 0f;
            lockElapsed = 0f;
            lockResetCount = 0;
            if (!board.TryReplaceActivePiece(incoming, spawnOrigin))
            {
                // Spawn không fit là top-out, không giữ lại active piece cũ để tiếp tục chơi.
                State = GameSessionState.GameOver;
            }
            return true;
        }

        /// <summary>Tiến thời gian deterministic; delta lớn được xử lý qua nhiều gravity step, Pause không tích lũy thời gian.</summary>
        public void Tick(float deltaSeconds)
        {
            if (deltaSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            if (State != GameSessionState.Playing) return;
            gravityElapsed += deltaSeconds;
            while (gravityElapsed >= gravitySeconds && State == GameSessionState.Playing)
            {
                gravityElapsed -= gravitySeconds;
                board.TryFallOneCell();
            }
            if (IsGrounded())
            {
                lockElapsed += deltaSeconds;
                if (lockElapsed >= lockDelaySeconds) LockCurrentPiece();
            }
            else lockElapsed = 0f;
        }

        /// <summary>Tạm dừng lượt đang chơi; không tác động timer hoặc board.</summary>
        public bool Pause() { if (State != GameSessionState.Playing) return false; State = GameSessionState.Paused; return true; }
        /// <summary>Tiếp tục lượt đã pause.</summary>
        public bool Resume() { if (State != GameSessionState.Paused) return false; State = GameSessionState.Playing; return true; }
        /// <summary>Thử dịch trái và reset lock delay nếu đây là thao tác hợp lệ khi đang chạm đất.</summary>
        public bool TryMoveLeft() { return TryGroundedAction(board.TryMoveLeft); }
        /// <summary>Thử dịch phải và reset lock delay nếu đây là thao tác hợp lệ khi đang chạm đất.</summary>
        public bool TryMoveRight() { return TryGroundedAction(board.TryMoveRight); }
        /// <summary>Thử xoay CW và reset lock delay nếu đây là thao tác hợp lệ khi đang chạm đất.</summary>
        public bool TryRotateClockwise() { return TryGroundedAction(board.TryRotateClockwise); }
        /// <summary>Thử xoay CCW và reset lock delay nếu đây là thao tác hợp lệ khi đang chạm đất.</summary>
        public bool TryRotateCounterClockwise() { return TryGroundedAction(board.TryRotateCounterClockwise); }
        /// <summary>Soft drop một ô nếu có thể; cộng điểm chỉ khi khối thực sự rơi.</summary>
        public bool SoftDrop()
        {
            if (State != GameSessionState.Playing || !board.TryFallOneCell())
            {
                return false;
            }

            score.AddSoftDrop(1);
            return true;
        }

        /// <summary>Hard drop đến ô thấp nhất, cộng điểm theo quãng rơi, rồi khóa và spawn khối kế tiếp.</summary>
        public bool HardDrop()
        {
            if (State != GameSessionState.Playing)
            {
                return false;
            }

            int cells = 0;
            while (board.TryFallOneCell())
            {
                cells++;
            }

            score.AddHardDrop(cells);
            return LockCurrentPiece();
        }

        /// <summary>Kiểm tra active piece có chạm đáy hoặc settled cell ngay dưới hay không.</summary>
        private bool IsGrounded() { return board.ActivePiece != null && !board.CanFit(board.ActivePiece.Translate(0, -1)); }
        /// <summary>Thực hiện action hợp lệ và reset lock delay tối đa số lần cho phép nếu piece đang chạm đất.</summary>
        private bool TryGroundedAction(Func<bool> action)
        {
            if (State != GameSessionState.Playing) return false;
            bool wasGrounded = IsGrounded();
            if (!action()) return false;
            if (wasGrounded && lockResetCount < maxLockResets) { lockElapsed = 0f; lockResetCount++; }
            return true;
        }
        /// <summary>Khóa active piece, clear hàng, tính điểm, kiểm tra top-out rồi spawn khối tiếp theo theo đúng thứ tự luật.</summary>
        private bool LockCurrentPiece()
        {
            if (State != GameSessionState.Playing || !board.TryLockActivePiece())
            {
                return false;
            }

            int cleared = board.ClearFullRows();
            score.ApplyLineClear(cleared);
            gravityElapsed = 0f;
            lockElapsed = 0f;
            lockResetCount = 0;

            if (board.HasSettledCellsInHiddenRows())
            {
                State = GameSessionState.GameOver;
                return true;
            }

            State = GameSessionState.Ready;
            BeginNextPiece();
            return true;
        }
    }
}
