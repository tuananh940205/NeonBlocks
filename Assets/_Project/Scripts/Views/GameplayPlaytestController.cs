using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using UnityEngine;

namespace NeonBlocks.Views
{
    /// <summary>
    /// Bridge tạm thời cho playtest desktop: tạo GameLoop từ GameConfig, nhận bàn phím và vẽ board bằng IMGUI.
    /// Script chỉ hiển thị/đọc input; toàn bộ luật clear, score, top-out và spawn nằm trong class thuần C#.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayPlaytestController : MonoBehaviour
    {
        private const float CellPixels = 24f;
        private const float BoardLeft = 24f;
        private const float BoardTop = 70f;

        private GameLoop loop;

        /// <summary>
        /// Tạo lượt playtest từ config session đã được tải trước khi scene Gameplay mở.
        /// </summary>
        private void Start()
        {
            GameConfig config = ServiceLocator.Instance.Resolve<ConfigManager>().Config;
            loop = new GameLoop(
                new BoardModel(config.Board),
                new SevenBagPieceSource(config.Pieces, 1, config.NextQueueSize),
                new GameScore(config.Scoring, config.StartLevel, config.LinesPerLevel),
                config.SpawnOrigin,
                config.GetLevel(config.StartLevel).GravitySeconds,
                config.Timing.LockDelaySeconds,
                config.Timing.MaxLockResets);
            loop.BeginNextPiece();
        }

        /// <summary>
        /// Chuyển delta time và từng phím thao tác vào GameLoop trong khi playtest.
        /// </summary>
        private void Update()
        {
            if (loop == null)
            {
                return;
            }

            loop.Tick(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.LeftArrow)) loop.TryMoveLeft();
            if (Input.GetKeyDown(KeyCode.RightArrow)) loop.TryMoveRight();
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.X)) loop.TryRotateClockwise();
            if (Input.GetKeyDown(KeyCode.Z)) loop.TryRotateCounterClockwise();
            if (Input.GetKeyDown(KeyCode.DownArrow)) loop.SoftDrop();
            if (Input.GetKeyDown(KeyCode.Space)) loop.HardDrop();
            if (Input.GetKeyDown(KeyCode.P)) TogglePause();
        }

        /// <summary>
        /// Vẽ board placeholder, active piece và thông tin điểm để kiểm tra gameplay mà không cần asset art.
        /// </summary>
        private void OnGUI()
        {
            if (loop == null)
            {
                return;
            }

            GUI.Label(new Rect(BoardLeft, 20f, 420f, 28f), "NeonBlocks Playtest — ← → / ↑ X / Z / ↓ / Space / P");
            GUI.Label(new Rect(BoardLeft, 46f, 420f, 24f), $"Score {loop.Score.Score}   Lines {loop.Score.Lines}   Level {loop.Score.Level}   {loop.State}");
            DrawBoard();
        }

        /// <summary>
        /// Chuyển giữa Playing và Paused khi người chơi nhấn P.
        /// </summary>
        private void TogglePause()
        {
            if (loop.State == GameSessionState.Playing)
            {
                loop.Pause();
                return;
            }

            if (loop.State == GameSessionState.Paused)
            {
                loop.Resume();
            }
        }

        /// <summary>
        /// Vẽ settled cells và active cells trong vùng board hiển thị; hàng ẩn không được render.
        /// </summary>
        private void DrawBoard()
        {
            BoardSettings settings = loop.Board.Settings;
            GUI.Box(new Rect(BoardLeft - 2f, BoardTop - 2f, settings.Width * CellPixels + 4f, settings.VisibleHeight * CellPixels + 4f), GUIContent.none);

            foreach (GridPoint cell in loop.Board.GetSettledCells())
            {
                DrawCell(cell, settings, new Color(0.2f, 0.9f, 1f));
            }

            ActivePiece active = loop.Board.ActivePiece;
            if (active == null)
            {
                return;
            }

            for (int index = 0; index < active.Definition.Cells.Count; index++)
            {
                DrawCell(active.GetCell(index), settings, new Color(1f, 0.35f, 0.85f));
            }
        }

        /// <summary>
        /// Vẽ một cell nếu nó thuộc vùng hiển thị của board.
        /// </summary>
        /// <param name="cell">Tọa độ board của cell.</param>
        /// <param name="settings">Kích thước board dùng để đổi trục y.</param>
        /// <param name="color">Màu placeholder của cell.</param>
        private static void DrawCell(GridPoint cell, BoardSettings settings, Color color)
        {
            if (cell.X < 0 || cell.X >= settings.Width || cell.Y < 0 || cell.Y >= settings.VisibleHeight)
            {
                return;
            }

            Color previous = GUI.color;
            GUI.color = color;
            float x = BoardLeft + cell.X * CellPixels;
            float y = BoardTop + (settings.VisibleHeight - 1 - cell.Y) * CellPixels;
            GUI.Box(new Rect(x, y, CellPixels - 1f, CellPixels - 1f), GUIContent.none);
            GUI.color = previous;
        }
    }
}
