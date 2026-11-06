using UnityEngine;
namespace NeonBlocks.Views
{
    /// <summary>Fit toàn bộ nội dung gameplay 1080x1920 vào safe area, giữ ô vuông và vị trí control.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeGameplayLayout : MonoBehaviour
    {
        private Rect lastSafeArea;
        private Vector2 lastParentSize;
        /// <summary>Áp dụng trước frame đầu tiên.</summary>
        private void OnEnable() => Fit();
        /// <summary>Chỉ tính lại khi màn hình hoặc safe area thay đổi.</summary>
        private void LateUpdate()
        {
            RectTransform parent = transform.parent as RectTransform;
            if (parent != null && (lastSafeArea != Screen.safeArea || lastParentSize != parent.rect.size)) { Fit(); }
        }
        /// <summary>Co đồng nhất nội dung theo vùng an toàn; background vẫn cover ngoài vùng này.</summary>
        private void Fit()
        {
            RectTransform rect = (RectTransform)transform, parent = transform.parent as RectTransform;
            if (parent == null || Screen.width <= 0 || Screen.height <= 0) { return; }
            lastSafeArea = Screen.safeArea; lastParentSize = parent.rect.size;
            Vector2 fraction = new Vector2(lastSafeArea.width / Screen.width, lastSafeArea.height / Screen.height);
            Vector2 available = Vector2.Scale(lastParentSize, fraction);
            Vector2 center = new Vector2(lastSafeArea.center.x / Screen.width, lastSafeArea.center.y / Screen.height);
            rect.anchorMin = rect.anchorMax = center; rect.pivot = Vector2.one * .5f; rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(1080, 1920);
            float scale = Mathf.Min(available.x / 1080, available.y / 1920);
            rect.localScale = Vector3.one * Mathf.Max(.01f, scale);
        }
    }
}