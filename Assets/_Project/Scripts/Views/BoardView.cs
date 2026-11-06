using System.Collections.Generic;
using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using UnityEngine;
using UnityEngine.UI;
namespace NeonBlocks.Views
{
    /// <summary>Board render sprite theo ID; grid được dựng chính xác từ kích thước logic.</summary>
    [DisallowMultipleComponent]
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private RectTransform boardRoot;
        [SerializeField] private Text hudText;
        [SerializeField] private CellTheme theme;
        [SerializeField, Min(0f)] private float cellGapPixels = 1f;
        private readonly List<Image> cells = new List<Image>();
        private readonly List<Image> lines = new List<Image>();
        private readonly HashSet<GridPoint> active = new HashSet<GridPoint>();
        private GameplaySnapshot latest;
        private float clearElapsed = 1f;
        /// <summary>Nghe render snapshot và clear feedback độc lập.</summary>
        private void OnEnable()
        {
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(Render);
            ServiceLocator.Instance.Resolve<GameplayFeedbackSignal>().AddListener(OnFeedback);
        }
        /// <summary>Gỡ mọi listener trước scene unload.</summary>
        private void OnDisable()
        {
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(Render);
            ServiceLocator.Instance.Resolve<GameplayFeedbackSignal>().RemoveListener(OnFeedback);
        }
        /// <summary>Clear chỉ làm viền sáng nhẹ, không flash board hoặc delay model.</summary>
        private void OnFeedback(GameplayFeedback value)
        {
            if (value.Kind == FeedbackKind.Clear) { clearElapsed = 0f; }
        }
        /// <summary>Fade viền trong 160ms bằng thời gian không scale.</summary>
        private void Update()
        {
            if (clearElapsed >= .16f) { return; }
            clearElapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(.5f, .18f, Mathf.Clamp01(clearElapsed / .16f));
            foreach (Image line in lines) { line.color = new Color(.2f, .7f, .85f, alpha); }
        }
        /// <summary>Vẽ settled/ghost/active; ghost không đè ô active khi chạm đáy.</summary>
        private void Render(GameplaySnapshot snapshot)
        {
            if (boardRoot == null) { return; }
            latest = snapshot;
            float size = Mathf.Min(boardRoot.rect.width / snapshot.Settings.Width, boardRoot.rect.height / snapshot.Settings.VisibleHeight);
            float left = -size * snapshot.Settings.Width * .5f, bottom = -size * snapshot.Settings.VisibleHeight * .5f;
            DrawGrid(snapshot.Settings, size, left, bottom);
            active.Clear(); foreach (GridPoint point in snapshot.ActiveCells) { active.Add(point); }
            int count = 0;
            foreach (SettledTile tile in snapshot.SettledTiles) { Draw(tile.Position, tile.PieceId, false, .72f, snapshot.Settings, size, left, bottom, ref count); }
            if (snapshot.SettledTiles.Count == 0)
            {
                // Snapshot cũ vẫn render được trong giai đoạn chuyển đổi API.
                foreach (GridPoint point in snapshot.SettledCells) { Draw(point, null, false, .72f, snapshot.Settings, size, left, bottom, ref count); }
            }
            foreach (GridPoint point in snapshot.GhostCells)
            { if (!active.Contains(point)) { Draw(point, snapshot.ActivePieceId, true, .45f, snapshot.Settings, size, left, bottom, ref count); } }
            foreach (GridPoint point in snapshot.ActiveCells) { Draw(point, snapshot.ActivePieceId, false, 1f, snapshot.Settings, size, left, bottom, ref count); }
            for (int i = count; i < cells.Count; i++) { cells[i].gameObject.SetActive(false); }
        }
        /// <summary>Tạo Image một lần dưới chính root thuộc view.</summary>
        private Image CreateImage(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(boardRoot, false);
            Image image = go.GetComponent<Image>(); image.raycastTarget = false; return image;
        }
        /// <summary>Vẽ ô vuông theo bounds chuẩn, không theo pixel của artwork frame.</summary>
        private void Draw(GridPoint point, string id, bool ghost, float brightness, BoardSettings settings, float size, float left, float bottom, ref int index)
        {
            if (point.Y < 0 || point.Y >= settings.VisibleHeight || point.X < 0 || point.X >= settings.Width) { return; }
            if (index >= cells.Count) { cells.Add(CreateImage("NeonCell")); }
            Image image = cells[index++]; image.gameObject.SetActive(true);
            image.sprite = theme == null ? null : ghost ? theme.Ghost : theme.GetCell(id);
            image.color = ghost ? new Color(.65f, .9f, 1f, brightness) : new Color(brightness, brightness, brightness, 1f);
            Place(image.rectTransform, new Vector2(left + (point.X + .5f) * size, bottom + (point.Y + .5f) * size), Vector2.one * Mathf.Max(1f, size - cellGapPixels));
            image.transform.SetAsLastSibling();
        }
        /// <summary>Đúng W+1 đường dọc và H+1 đường ngang, cập nhật khi layout thay đổi.</summary>
        private void DrawGrid(BoardSettings settings, float size, float left, float bottom)
        {
            int required = settings.Width + settings.VisibleHeight + 2;
            while (lines.Count < required) { lines.Add(CreateImage("GridLine")); }
            int index = 0;
            for (int x = 0; x <= settings.Width; x++)
            {
                Image image = lines[index++]; image.gameObject.SetActive(true); image.color = new Color(.2f, .7f, .85f, Mathf.Lerp(.5f, .18f, Mathf.Clamp01(clearElapsed / .16f)));
                Place(image.rectTransform, new Vector2(left + x * size, 0), new Vector2(1f, settings.VisibleHeight * size));
                image.transform.SetAsFirstSibling();
            }
            for (int y = 0; y <= settings.VisibleHeight; y++)
            {
                Image image = lines[index++]; image.gameObject.SetActive(true); image.color = new Color(.2f, .7f, .85f, .18f);
                Place(image.rectTransform, new Vector2(0, bottom + y * size), new Vector2(settings.Width * size, 1f));
                image.transform.SetAsFirstSibling();
            }
            for (; index < lines.Count; index++) { lines[index].gameObject.SetActive(false); }
        }
        /// <summary>Đặt rect quanh tâm root.</summary>
        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        { rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one * .5f; rect.anchoredPosition = position; rect.sizeDelta = size; }
        /// <summary>Render lại sau đổi kích thước màn hình.</summary>
        private void OnRectTransformDimensionsChange() { if (latest != null && isActiveAndEnabled) { Render(latest); } }
    }
}