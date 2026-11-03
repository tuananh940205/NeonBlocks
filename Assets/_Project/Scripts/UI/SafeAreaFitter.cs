using UnityEngine;

namespace NeonBlocks
{
    /// <summary>
    /// Đặt RectTransform của view theo Screen.safeArea (tai thỏ, thanh điều hướng); tự cập nhật khi xoay hoặc đổi kích thước.
    /// Chỉ xử lý bố cục, không chứa luật chơi.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect appliedSafeArea;
        private int appliedWidth;
        private int appliedHeight;

        /// <summary>
        /// Lấy RectTransform và áp dụng vùng an toàn lần đầu trước frame đầu tiên.
        /// </summary>
        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            ApplySafeArea();
        }

        /// <summary>
        /// Kiểm tra mỗi frame xem vùng an toàn hoặc kích thước màn hình có đổi không; chỉ áp dụng lại khi có thay đổi.
        /// </summary>
        private void Update()
        {
            if (Screen.safeArea != appliedSafeArea || Screen.width != appliedWidth || Screen.height != appliedHeight)
            {
                ApplySafeArea();
            }
        }

        /// <summary>
        /// Ghi nhận trạng thái màn hình hiện tại và đặt anchor theo SafeAreaCalculator; nếu vùng không hợp lệ thì giữ toàn màn hình.
        /// </summary>
        private void ApplySafeArea()
        {
            appliedSafeArea = Screen.safeArea;
            appliedWidth = Screen.width;
            appliedHeight = Screen.height;

            if (SafeAreaCalculator.TryComputeAnchors(appliedSafeArea, appliedWidth, appliedHeight, out Vector2 anchorMin, out Vector2 anchorMax))
            {
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
            }
            else
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
            }

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
