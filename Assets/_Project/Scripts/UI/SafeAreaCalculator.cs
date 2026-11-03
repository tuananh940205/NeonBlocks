using UnityEngine;

namespace NeonBlocks
{
    /// <summary>
    /// Tính anchor chuẩn hóa (0–1) từ vùng an toàn của màn hình; thuần tính toán để có thể test trong Edit Mode.
    /// </summary>
    public static class SafeAreaCalculator
    {
        /// <summary>
        /// Chuyển Screen.safeArea (gốc dưới-trái, đơn vị pixel) thành anchorMin/anchorMax cho RectTransform.
        /// Trả false khi kích thước màn hình hoặc vùng an toàn không hợp lệ; khi đó anchor được đặt về toàn màn hình.
        /// </summary>
        /// <param name="safeArea">Vùng an toàn theo pixel.</param>
        /// <param name="screenWidth">Chiều rộng màn hình theo pixel.</param>
        /// <param name="screenHeight">Chiều cao màn hình theo pixel.</param>
        /// <param name="anchorMin">Anchor min đã kẹp trong khoảng 0–1.</param>
        /// <param name="anchorMax">Anchor max đã kẹp trong khoảng 0–1.</param>
        /// <returns>true nếu anchor hợp lệ và có diện tích dương.</returns>
        public static bool TryComputeAnchors(Rect safeArea, float screenWidth, float screenHeight, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            anchorMin = Vector2.zero;
            anchorMax = Vector2.one;

            if (screenWidth <= 0f || screenHeight <= 0f || safeArea.width <= 0f || safeArea.height <= 0f)
            {
                return false;
            }

            float minX = Mathf.Clamp01(safeArea.xMin / screenWidth);
            float minY = Mathf.Clamp01(safeArea.yMin / screenHeight);
            float maxX = Mathf.Clamp01(safeArea.xMax / screenWidth);
            float maxY = Mathf.Clamp01(safeArea.yMax / screenHeight);

            if (maxX <= minX || maxY <= minY)
            {
                return false;
            }

            anchorMin = new Vector2(minX, minY);
            anchorMax = new Vector2(maxX, maxY);
            return true;
        }
    }
}
