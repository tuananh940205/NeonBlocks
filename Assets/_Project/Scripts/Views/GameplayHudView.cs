using System.Collections.Generic;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using NeonBlocks.Scene;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeonBlocks.Views
{
    /// <summary>HUD chỉ render snapshot: số liệu TMP và preview hình khối ở orientation spawn.</summary>
    [DisallowMultipleComponent]
    public sealed class GameplayHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text linesText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private RectTransform holdRoot;
        [SerializeField] private RectTransform[] nextRoots;
        [SerializeField] private Sprite cellSprite;
        private readonly List<Image[]> previewCells = new List<Image[]>();
        private bool isStopping;

        /// <summary>Nghe snapshot/lifecycle rồi yêu cầu state hiện tại qua Signal riêng.</summary>
        private void OnEnable()
        {
            isStopping = false;
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(Render);
            ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(OnSceneStopping);
            ServiceLocator.Instance.Resolve<GameplaySnapshotRequestSignal>().Dispatch();
        }

        /// <summary>Gỡ mọi listener khi HUD bị tắt hoặc scene unload.</summary>
        private void OnDisable()
        {
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(Render);
            ServiceLocator.Instance.Resolve<SceneStopSignal>().RemoveListener(OnSceneStopping);
        }

        /// <summary>Chặn render sau khi scene sở hữu HUD bắt đầu dọn tài nguyên.</summary>
        private void OnSceneStopping(string sceneKey)
        {
            if (gameObject.scene.name == sceneKey)
            {
                // Scene mới load additive không bị dừng bởi scene cũ.
                isStopping = true;
            }
        }

        /// <summary>Đồng bộ toàn bộ số liệu và preview từ cùng một snapshot, kể cả khi restart.</summary>
        private void Render(GameplaySnapshot snapshot)
        {
            if (isStopping)
            {
                // Không tạo cell mới trong quá trình unload.
                return;
            }
            if (scoreText != null) { scoreText.text = $"SCORE\n{snapshot.Score}"; }
            if (levelText != null) { levelText.text = $"LEVEL\n{snapshot.Level}"; }
            if (linesText != null) { linesText.text = $"LINES\n{snapshot.Lines}"; }
            if (stateText != null) { stateText.text = snapshot.State == GameSessionState.Playing ? "" : snapshot.State.ToString(); }
            RenderPreview(0, holdRoot, snapshot.HeldPieceId, snapshot.CanHold ? 1f : 0.45f);
            if (nextRoots == null)
            {
                // HUD chưa cấu hình Next vẫn hiển thị được Hold và số liệu.
                return;
            }
            for (int i = 0; i < nextRoots.Length; i++)
            {
                RenderPreview(i + 1, nextRoots[i], i < snapshot.NextPieceIds.Count ? snapshot.NextPieceIds[i] : null, 1f);
            }
        }

        /// <summary>Tái sử dụng bốn Image cho mỗi preview; căn theo bounds shape thật để I/O/T đều nằm giữa.</summary>
        private void RenderPreview(int slot, RectTransform root, string pieceId, float alpha)
        {
            if (root == null)
            {
                // Slot không tồn tại được bỏ qua, không tìm view khác qua hierarchy.
                return;
            }
            while (previewCells.Count <= slot) { previewCells.Add(null); }
            if (previewCells[slot] == null)
            {
                // Tạo đúng bốn cell một lần cho mỗi slot; các snapshot sau chỉ cập nhật transform/màu.
                var cells = new Image[PieceDefinition.CellCount];
                for (int i = 0; i < cells.Length; i++)
                {
                    var cell = new GameObject("PreviewCell", typeof(RectTransform), typeof(Image));
                    cell.transform.SetParent(root, false);
                    cells[i] = cell.GetComponent<Image>();
                    cells[i].sprite = cellSprite;
                    cells[i].raycastTarget = false;
                }
                previewCells[slot] = cells;
            }
            PieceDefinition definition = null;
            foreach (PieceDefinition piece in ServiceLocator.Instance.Resolve<ConfigManager>().Config.Pieces)
            {
                if (piece.Id == pieceId) { definition = piece; break; }
            }
            Image[] images = previewCells[slot];
            if (definition == null)
            {
                // Hold rỗng hoặc queue ngắn phải tắt cell cũ, tránh hình sót sau restart.
                foreach (Image image in images) { image.gameObject.SetActive(false); }
                return;
            }
            int minX = 4, maxX = 0, minY = 4, maxY = 0;
            foreach (GridPoint cell in definition.Cells)
            {
                minX = Mathf.Min(minX, cell.X); maxX = Mathf.Max(maxX, cell.X);
                minY = Mathf.Min(minY, cell.Y); maxY = Mathf.Max(maxY, cell.Y);
            }
            float unit = Mathf.Min(root.rect.width, root.rect.height) * 0.8f / PieceDefinition.BoxSize;
            float centerX = (minX + maxX) * 0.5f, centerY = (minY + maxY) * 0.5f;
            Color color = PieceColor(pieceId); color.a = alpha;
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                image.gameObject.SetActive(true);
                image.color = color;
                RectTransform rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = Vector2.one * (unit * 0.94f);
                rect.anchoredPosition = new Vector2((definition.Cells[i].X - centerX) * unit, (definition.Cells[i].Y - centerY) * unit);
            }
        }

        /// <summary>Chọn màu theo cùng quy ước bảy tetromino của board placeholder.</summary>
        private static Color PieceColor(string id)
        {
            return id switch
            {
                "I" => new Color(0.1f, 0.9f, 1f), "O" => new Color(1f, 0.85f, 0.2f),
                "T" => new Color(0.75f, 0.3f, 1f), "J" => new Color(0.2f, 0.45f, 1f),
                "L" => new Color(1f, 0.55f, 0.15f), "S" => new Color(0.25f, 1f, 0.45f),
                "Z" => new Color(1f, 0.25f, 0.4f), _ => Color.white
            };
        }
    }
}
