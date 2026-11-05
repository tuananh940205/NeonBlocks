using UnityEngine;

namespace NeonBlocks.Assets
{
    /// <summary>
    /// Một tài nguyên đang được AddressableManager giữ cho một chủ sở hữu. Bất biến, chỉ set qua constructor.
    /// Với Scene, Handle là handle cần unload; với Instance, Instance là GameObject Instantiate thuần túy cần destroy.
    /// </summary>
    public sealed class TrackedAsset
    {
        /// <summary>Loại tài nguyên, quyết định cách giải phóng.</summary>
        public TrackedKind Kind { get; }

        /// <summary>Address key đã dùng để tải, dùng cho log.</summary>
        public string Key { get; }

        /// <summary>Handle Addressables cho Asset hoặc Scene; null với Instance.</summary>
        public object Handle { get; }

        /// <summary>GameObject instance cho Instance; null với Asset và Scene.</summary>
        public GameObject Instance { get; }

        /// <summary>
        /// Tạo bản ghi tài nguyên đang giữ.
        /// </summary>
        /// <param name="kind">Loại tài nguyên.</param>
        /// <param name="key">Address key đã dùng.</param>
        /// <param name="handle">Handle cho Asset hoặc Scene.</param>
        /// <param name="instance">GameObject cho Instance.</param>
        public TrackedAsset(TrackedKind kind, string key, object handle, GameObject instance)
        {
            Kind = kind;
            Key = key;
            Handle = handle;
            Instance = instance;
        }
    }
}
