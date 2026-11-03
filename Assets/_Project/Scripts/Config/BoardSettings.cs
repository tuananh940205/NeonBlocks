namespace NeonBlocks.Config
{
    /// <summary>
    /// Kích thước bảng: số cột, số hàng hiển thị và số hàng ẩn phía trên. Bất biến, chỉ set qua constructor.
    /// Hàng y = 0 là đáy bảng; hàng y từ VisibleHeight đến TotalHeight - 1 là vùng ẩn.
    /// </summary>
    public sealed class BoardSettings
    {
        /// <summary>Số cột của bảng.</summary>
        public int Width { get; }

        /// <summary>Số hàng hiển thị, tính từ đáy (y = 0) đến y = VisibleHeight - 1.</summary>
        public int VisibleHeight { get; }

        /// <summary>Số hàng ẩn nằm phía trên vùng hiển thị.</summary>
        public int HiddenRows { get; }

        /// <summary>Tổng số hàng gồm hàng hiển thị và hàng ẩn.</summary>
        public int TotalHeight => VisibleHeight + HiddenRows;

        /// <summary>
        /// Tạo cấu hình bảng và kiểm tra giá trị không hợp lệ.
        /// </summary>
        /// <param name="width">Số cột, phải ≥ 4 để chứa khối 4×4.</param>
        /// <param name="visibleHeight">Số hàng hiển thị, phải &gt; 0.</param>
        /// <param name="hiddenRows">Số hàng ẩn, không được âm.</param>
        public BoardSettings(int width, int visibleHeight, int hiddenRows)
        {
            if (width < 4)
            {
                throw new ConfigValidationException($"board.width phải ≥ 4 để chứa khối 4×4; giá trị hiện tại: {width}.");
            }

            if (visibleHeight <= 0)
            {
                throw new ConfigValidationException($"board.visibleHeight phải > 0; giá trị hiện tại: {visibleHeight}.");
            }

            if (hiddenRows < 0)
            {
                throw new ConfigValidationException($"board.hiddenRows không được âm; giá trị hiện tại: {hiddenRows}.");
            }

            Width = width;
            VisibleHeight = visibleHeight;
            HiddenRows = hiddenRows;
        }
    }
}
