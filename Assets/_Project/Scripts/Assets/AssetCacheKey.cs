using System;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Khóa bất biến xác định một asset cached theo Addressables key và kiểu yêu cầu trong cùng một chủ sở hữu.
    /// </summary>
    internal sealed class AssetCacheKey : IEquatable<AssetCacheKey>
    {
        /// <summary>Address key của asset.</summary>
        public string Key { get; }

        /// <summary>Kiểu asset người gọi yêu cầu.</summary>
        public Type AssetType { get; }

        /// <summary>
        /// Tạo khóa cache từ key và kiểu asset đã được kiểm tra hợp lệ.
        /// </summary>
        /// <param name="key">Address key.</param>
        /// <param name="assetType">Kiểu asset.</param>
        public AssetCacheKey(string key, Type assetType)
        {
            Key = key;
            AssetType = assetType;
        }

        /// <summary>
        /// So sánh hai khóa bằng key và cùng đối tượng kiểu.
        /// </summary>
        /// <param name="other">Khóa cần so sánh.</param>
        /// <returns>True nếu hai khóa biểu diễn cùng asset cùng kiểu.</returns>
        public bool Equals(AssetCacheKey other)
        {
            return other != null && Key == other.Key && AssetType == other.AssetType;
        }

        /// <summary>
        /// So sánh object với khóa cache hiện tại.
        /// </summary>
        /// <param name="obj">Object cần so sánh.</param>
        /// <returns>True nếu object là khóa tương đương.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AssetCacheKey);
        }

        /// <summary>
        /// Trả hash code ổn định theo key và kiểu asset.
        /// </summary>
        /// <returns>Hash code của khóa.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (AssetType != null ? AssetType.GetHashCode() : 0);
            }
        }
    }
}
