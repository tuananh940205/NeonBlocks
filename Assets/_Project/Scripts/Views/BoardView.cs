using System.Collections.Generic;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlocks.Views
{
    /// <summary>View UI của board: chỉ nghe snapshot và vẽ các ô placeholder theo hệ tọa độ X phải, Y lên.</summary>
    [DisallowMultipleComponent]
    public sealed class BoardView : MonoBehaviour
    {
        /// <summary>RectTransform chứa các RawImage ô placeholder, gán từ child của chính BoardView.</summary>
        [SerializeField] private RectTransform boardRoot;
        /// <summary>Text HUD thuộc BoardView, gán từ child của chính BoardView.</summary>
        [SerializeField] private Text hudText;
        /// <summary>Tỷ lệ inset hai cạnh ngang của playfield bên trong sprite khung board.</summary>
        [SerializeField, Range(0f, 0.25f)] private float horizontalInsetFraction = 0.115f;
        /// <summary>Khoảng hở pixel giữa hai tile placeholder sau khi tính tile vuông.</summary>
        [SerializeField, Min(0f)] private float cellGapPixels = 2f;
        /// <summary>Tỷ lệ inset đáy của playfield trong artwork, dùng để khóa hàng y=0 vào đúng hàng lưới dưới cùng.</summary>
        [SerializeField, Range(0f, 0.25f)] private float verticalBottomInsetFraction = 0.07f;

        private readonly List<RawImage> cellViews = new List<RawImage>();

        /// <summary>Đăng ký nhận snapshot khi view được bật.</summary>
        private void OnEnable()
        {
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(Render);
        }

        /// <summary>Gỡ listener snapshot đối xứng khi view bị tắt.</summary>
        private void OnDisable()
        {
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(Render);
        }

        /// <summary>Vẽ settled, ghost và active theo thứ tự layer; hàng ẩn bị bỏ qua.</summary>
        /// <param name="snapshot">State bất biến do GameplayPresenter phát.</param>
        private void Render(GameplaySnapshot snapshot)
        {
            if (boardRoot == null)
            {
                Debug.LogError("BoardView chưa gán Board Root trong Inspector.");
                return;
            }

            int required = snapshot.SettledCells.Count + snapshot.GhostCells.Count + snapshot.ActiveCells.Count;
            EnsureCellCount(required);
            float cellSize = CalculateCellSize(snapshot.Settings);
            float boardLeft = -boardRoot.rect.width * boardRoot.pivot.x + boardRoot.rect.width * horizontalInsetFraction;
            float boardBottom = -boardRoot.rect.height * boardRoot.pivot.y + boardRoot.rect.height * verticalBottomInsetFraction;
            int index = 0;
            index = RenderCells(snapshot.SettledCells, snapshot.Settings, new Color(0.15f, 0.9f, 1f, 1f), cellSize, boardLeft, boardBottom, index);
            index = RenderCells(snapshot.GhostCells, snapshot.Settings, GetPieceColor(snapshot.ActivePieceId, 0.22f), cellSize, boardLeft, boardBottom, index);
            index = RenderCells(snapshot.ActiveCells, snapshot.Settings, GetPieceColor(snapshot.ActivePieceId, 1f), cellSize, boardLeft, boardBottom, index);

            for (; index < cellViews.Count; index++)
            {
                cellViews[index].gameObject.SetActive(false);
            }

            if (hudText != null)
            {
                string next = string.Join(" ", snapshot.NextPieceIds);
                hudText.text = $"Score {snapshot.Score}  Lines {snapshot.Lines}  Level {snapshot.Level}\n{snapshot.State}  Next: {next}\n←/A  →/D  ↓/S  Z/X/↑  Space  P/Esc  R (GameOver)";
            }
        }

        /// <summary>Đảm bảo pool nội bộ có đủ RawImage cho snapshot lớn nhất hiện tại.</summary>
        /// <param name="required">Số ô sẽ render trong frame.</param>
        private void EnsureCellCount(int required)
        {
            while (cellViews.Count < required)
            {
                var cell = new GameObject("PlaceholderCell", typeof(RectTransform), typeof(RawImage));
                cell.transform.SetParent(boardRoot, false);
                RawImage image = cell.GetComponent<RawImage>();
                image.raycastTarget = false;
                cellViews.Add(image);
            }
        }

        /// <summary>Tính cạnh tile vuông từ bề rộng playfield, để tỷ lệ 10x20 không kéo dãn ô theo texture khung.</summary>
        /// <param name="settings">Cấu hình số cột của board.</param>
        /// <returns>Cạnh tile theo pixel Canvas.</returns>
        private float CalculateCellSize(BoardSettings settings)
        {
            float playableWidth = boardRoot.rect.width * (1f - horizontalInsetFraction * 2f);
            return playableWidth / settings.Width;
        }

        /// <summary>Đặt từng ô hợp lệ vào playfield nội bộ và trả về chỉ số phần tử tiếp theo.</summary>
        private int RenderCells(IReadOnlyList<GridPoint> cells, BoardSettings settings, Color color, float cellSize, float boardLeft, float boardBottom, int startIndex)
        {
            int index = startIndex;
            for (int sourceIndex = 0; sourceIndex < cells.Count; sourceIndex++)
            {
                GridPoint cell = cells[sourceIndex];
                if (cell.X < 0 || cell.X >= settings.Width || cell.Y < 0 || cell.Y >= settings.VisibleHeight)
                {
                    continue;
                }

                RawImage image = cellViews[index++];
                image.gameObject.SetActive(true);
                image.color = color;
                RectTransform rect = image.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(cellSize - cellGapPixels, cellSize - cellGapPixels);
                rect.anchoredPosition = new Vector2(
                    boardLeft + (cell.X + 0.5f) * cellSize,
                    boardBottom + (cell.Y + 0.5f) * cellSize);
            }

            return index;
        }

        /// <summary>Chọn màu placeholder theo mã tetromino; alpha dùng để phân biệt ghost với active.</summary>
        private static Color GetPieceColor(string pieceId, float alpha)
        {
            Color color = pieceId switch
            {
                "I" => new Color(0.1f, 0.9f, 1f),
                "O" => new Color(1f, 0.85f, 0.2f),
                "T" => new Color(0.75f, 0.3f, 1f),
                "J" => new Color(0.2f, 0.45f, 1f),
                "L" => new Color(1f, 0.55f, 0.15f),
                "S" => new Color(0.25f, 1f, 0.45f),
                "Z" => new Color(1f, 0.25f, 0.4f),
                _ => Color.white
            };
            color.a = alpha;
            return color;
        }
    }
}
