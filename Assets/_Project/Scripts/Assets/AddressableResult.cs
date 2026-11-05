namespace NeonBlocks.Assets
{
    /// <summary>
    /// Kết quả thô của một lần tải từ backend: giá trị đã tải và handle cần giải phóng sau này. Bất biến, chỉ set qua constructor.
    /// </summary>
    public sealed class AddressableResult
    {
        /// <summary>Giá trị đã tải: asset, GameObject instance, hoặc SceneInstance dạng boxed.</summary>
        public object Value { get; }

        /// <summary>Handle Addressables cần giải phóng sau khi asset hoặc scene kết thúc scope.</summary>
        public object Handle { get; }

        /// <summary>
        /// Tạo kết quả tải từ backend.
        /// </summary>
        /// <param name="value">Giá trị đã tải.</param>
        /// <param name="handle">Handle để giải phóng, có thể null.</param>
        public AddressableResult(object value, object handle)
        {
            Value = value;
            Handle = handle;
        }
    }
}
