namespace NeonBlocks.Assets
{
    /// <summary>
    /// Tham chiếu tới một scene Addressables đã tải, dùng để unload đúng scene đó. Bất biến, chỉ set qua constructor.
    /// </summary>
    public sealed class LoadedScene
    {
        /// <summary>Address key của scene.</summary>
        public string Key { get; }

        /// <summary>Handle scene do backend cấp; AddressableManager dùng để tìm và unload.</summary>
        public object Handle { get; }

        /// <summary>
        /// Tạo tham chiếu scene đã tải.
        /// </summary>
        /// <param name="key">Address key của scene.</param>
        /// <param name="handle">Handle scene từ backend.</param>
        public LoadedScene(string key, object handle)
        {
            Key = key;
            Handle = handle;
        }
    }
}
