namespace NeonBlocks.Assets
{
    /// <summary>
    /// Định danh chủ sở hữu handle Addressables (session, scene hoặc config). Mỗi token là một đối tượng riêng biệt
    /// dùng để gom và giải phóng handle theo đúng chủ; sau khi được giải phóng, token không nhận yêu cầu mới.
    /// </summary>
    public sealed class AssetOwnerToken
    {
        /// <summary>Nhãn dùng cho log và thông báo lỗi, ví dụ "Session", "Scene:MainMenu".</summary>
        public string Label { get; }

        /// <summary>True sau khi AddressableManager đã giải phóng mọi handle của chủ sở hữu này.</summary>
        public bool IsReleased { get; private set; }

        /// <summary>
        /// Tạo token với nhãn mô tả chủ sở hữu.
        /// </summary>
        /// <param name="label">Nhãn mô tả, không được rỗng.</param>
        public AssetOwnerToken(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new System.ArgumentException("Nhãn chủ sở hữu không được rỗng.", nameof(label));
            }

            Label = label;
        }

        /// <summary>
        /// Đánh dấu token đã được giải phóng; chỉ AddressableManager gọi khi giải phóng handle của chủ sở hữu.
        /// </summary>
        internal void MarkReleased()
        {
            IsReleased = true;
        }
    }
}
