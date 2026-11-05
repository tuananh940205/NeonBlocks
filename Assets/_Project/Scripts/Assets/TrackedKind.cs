namespace NeonBlocks.Assets
{
    /// <summary>
    /// Loại tài nguyên được AddressableManager theo dõi, quyết định cách giải phóng: Release, Destroy hoặc UnloadScene.
    /// </summary>
    public enum TrackedKind
    {
        /// <summary>Asset tải bằng LoadAssetAsync, giải phóng bằng Release.</summary>
        Asset,

        /// <summary>Instance tạo bằng Instantiate thuần túy, giải phóng bằng Destroy.</summary>
        Instance,

        /// <summary>Scene tải bằng LoadSceneAsync, giải phóng bằng UnloadSceneAsync.</summary>
        Scene
    }
}
