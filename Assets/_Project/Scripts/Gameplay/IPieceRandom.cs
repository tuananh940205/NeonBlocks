namespace NeonBlocks.Gameplay
{
    /// <summary>
    /// Nguồn số ngẫu nhiên tối thiểu cho SevenBagPieceSource để có thể thay bằng bản xác định trong test.
    /// </summary>
    public interface IPieceRandom
    {
        /// <summary>
        /// Trả một số nguyên trong khoảng từ 0 đến nhỏ hơn maxExclusive.
        /// </summary>
        /// <param name="maxExclusive">Cận trên độc quyền, phải lớn hơn 0.</param>
        /// <returns>Giá trị ngẫu nhiên hợp lệ.</returns>
        int Next(int maxExclusive);
    }
}
