using System.Collections.Generic;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NeonBlocks.Scene;
using UnityEngine;

namespace NeonBlocks.Views
{
    /// <summary>Driver mỏng của scene Gameplay: tạo GameLoop thuần C#, nhận lệnh Signal và phát snapshot cho view.</summary>
    [DisallowMultipleComponent]
    public sealed class GameplayPresenter : MonoBehaviour
    {
        private GameLoop loop;
        private GameplayRepeatInput repeatInput;
        private readonly List<GameplayCommand> repeatedCommands = new List<GameplayCommand>();
        private bool isStopping;

        /// <summary>Đăng ký nhận lệnh input và thông báo dừng scene để chặn tick trước khi scope được giải phóng.</summary>
        private void OnEnable()
        {
            ServiceLocator.Instance.Resolve<GameplayCommandSignal>().AddListener(OnCommand);
            ServiceLocator.Instance.Resolve<GameplayHeldCommandSignal>().AddListener(OnHeldInput);
            ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(OnSceneStopping);
        }

        /// <summary>Gỡ listener đối xứng với OnEnable.</summary>
        private void OnDisable()
        {
            ServiceLocator.Instance.Resolve<GameplayCommandSignal>().RemoveListener(OnCommand);
            ServiceLocator.Instance.Resolve<GameplayHeldCommandSignal>().RemoveListener(OnHeldInput);
            ServiceLocator.Instance.Resolve<SceneStopSignal>().RemoveListener(OnSceneStopping);
        }

        /// <summary>Khởi tạo một ván mới từ config session sau khi scene Gameplay được tạo.</summary>
        private void Start()
        {
            StartNewGame();
        }

        /// <summary>Tick luật thuần C# bằng Unity delta time rồi phát state mới cho view.</summary>
        private void Update()
        {
            if (loop == null || isStopping)
            {
                return;
            }

            loop.Tick(Time.deltaTime);
            if (loop.State == GameSessionState.Playing)
            {
                repeatedCommands.Clear();
                repeatInput.CollectRepeats(Time.deltaTime, repeatedCommands);
                foreach (GameplayCommand command in repeatedCommands) ExecutePlayingCommand(command);
            }
            else
            {
                repeatInput.Clear();
            }
            PublishSnapshot();
        }

        /// <summary>Chặn presenter khi chỉ scene của nó đang rời đi.</summary>
        /// <param name="stoppingSceneKey">Address key của scene đang rời.</param>
        private void OnSceneStopping(string stoppingSceneKey)
        {
            if (gameObject.scene.name == stoppingSceneKey)
            {
                isStopping = true;
                repeatInput?.Clear();
            }
        }

        /// <summary>Áp dụng lệnh người chơi nếu trạng thái hiện tại cho phép.</summary>
        /// <param name="command">Lệnh được gửi từ GameplayKeyboardInput.</param>
        private void OnCommand(GameplayCommand command)
        {
            if (loop == null || isStopping)
            {
                return;
            }

            if (command == GameplayCommand.Restart)
            {
                if (loop.State == GameSessionState.GameOver)
                {
                    StartNewGame();
                }

                return;
            }

            if (command == GameplayCommand.TogglePause)
            {
                TogglePause();
                repeatInput.Clear();
                PublishSnapshot();
                return;
            }

            if (loop.State != GameSessionState.Playing)
            {
                return;
            }

            ExecutePlayingCommand(command);

            PublishSnapshot();
        }

        /// <summary>Nhận press/release của lệnh giữ; chỉ lệnh press ở Playing mới thực hiện hành động tức thời.</summary>
        /// <param name="input">Sự kiện giữ từ keyboard hoặc touch button.</param>
        private void OnHeldInput(GameplayHeldInput input)
        {
            if (loop == null || repeatInput == null || isStopping || input == null) return;
            if (!input.IsPressed)
            {
                repeatInput.End(input.Command);
                return;
            }

            if (loop.State != GameSessionState.Playing || !repeatInput.Begin(input.Command)) return;
            ExecutePlayingCommand(ToGameplayCommand(input.Command));
            PublishSnapshot();
        }

        /// <summary>Thực hiện một lệnh đã được lọc ở trạng thái Playing.</summary>
        private void ExecutePlayingCommand(GameplayCommand command)
        {
            switch (command)
            {
                case GameplayCommand.MoveLeft: loop.TryMoveLeft(); break;
                case GameplayCommand.MoveRight: loop.TryMoveRight(); break;
                case GameplayCommand.SoftDrop: loop.SoftDrop(); break;
                case GameplayCommand.RotateClockwise: loop.TryRotateClockwise(); break;
                case GameplayCommand.RotateCounterClockwise: loop.TryRotateCounterClockwise(); break;
                case GameplayCommand.HardDrop: loop.HardDrop(); break;
            }
        }

        /// <summary>Chuyển enum lệnh giữ sang enum gameplay dùng chung.</summary>
        private static GameplayCommand ToGameplayCommand(GameplayHeldCommand command)
        {
            return command switch
            {
                GameplayHeldCommand.MoveLeft => GameplayCommand.MoveLeft,
                GameplayHeldCommand.MoveRight => GameplayCommand.MoveRight,
                GameplayHeldCommand.SoftDrop => GameplayCommand.SoftDrop,
                _ => throw new System.ArgumentOutOfRangeException(nameof(command))
            };
        }

        /// <summary>Tạo loop mới bằng config session và spawn piece đầu tiên.</summary>
        private void StartNewGame()
        {
            GameConfig config = ServiceLocator.Instance.Resolve<ConfigManager>().Config;
            repeatInput = new GameplayRepeatInput(config.Timing.DasSeconds, config.Timing.ArrSeconds);
            loop = new GameLoop(new BoardModel(config.Board), new SevenBagPieceSource(config.Pieces, 1, config.NextQueueSize), new GameScore(config.Scoring, config.StartLevel, config.LinesPerLevel), config.SpawnOrigin, config.GetLevel(config.StartLevel).GravitySeconds, config.Timing.LockDelaySeconds, config.Timing.MaxLockResets);
            loop.BeginNextPiece();
            PublishSnapshot();
        }

        /// <summary>Chuyển đổi model thành snapshot render-only, bao gồm ghost tính từ candidate không commit vào model.</summary>
        private void PublishSnapshot()
        {
            ActivePiece active = loop.Board.ActivePiece;
            var activeCells = new List<GridPoint>();
            var ghostCells = new List<GridPoint>();
            string activePieceId = null;

            if (active != null)
            {
                activePieceId = active.Definition.Id;
                AddPieceCells(active, activeCells);
                ActivePiece ghost = active;
                while (loop.Board.CanFit(ghost.Translate(0, -1)))
                {
                    ghost = ghost.Translate(0, -1);
                }

                AddPieceCells(ghost, ghostCells);
            }

            var nextPieceIds = new List<string>();
            foreach (PieceDefinition piece in loop.NextPieces)
            {
                nextPieceIds.Add(piece.Id);
            }

            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().Dispatch(new GameplaySnapshot(loop.Board.Settings, loop.Board.GetSettledCells(), activeCells, ghostCells, activePieceId, nextPieceIds, loop.State, loop.Score.Score, loop.Score.Lines, loop.Score.Level));
        }

        /// <summary>Sao chép bốn ô của một active piece vào danh sách snapshot.</summary>
        /// <param name="piece">Piece cần sao chép.</param>
        /// <param name="cells">Đích nhận các tọa độ.</param>
        private static void AddPieceCells(ActivePiece piece, ICollection<GridPoint> cells)
        {
            for (int index = 0; index < piece.Definition.Cells.Count; index++)
            {
                cells.Add(piece.GetCell(index));
            }
        }

        /// <summary>Đổi Playing sang Paused và ngược lại, bỏ qua Ready/GameOver.</summary>
        private void TogglePause()
        {
            if (loop.State == GameSessionState.Playing)
            {
                loop.Pause();
            }
            else if (loop.State == GameSessionState.Paused)
            {
                loop.Resume();
            }
        }
    }
}
