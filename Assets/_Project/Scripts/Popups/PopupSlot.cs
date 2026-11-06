using System.Threading.Tasks;
using UnityEngine;
using NeonBlocks.Assets;
namespace NeonBlocks.Popups
{
    /// <summary>Slot mutable runtime do PopupManager sở hữu.</summary>
    internal sealed class PopupSlot
    {
        internal readonly SceneAssetScope Scope;
        internal readonly string Key;
        internal GameObject Instance;
        internal Task<GameObject> Pending;
        internal bool Closed;
        /// <summary>Tạo slot cho đúng owner scene.</summary>
        internal PopupSlot(SceneAssetScope scope, string key) { Scope = scope; Key = key; }
    }
}