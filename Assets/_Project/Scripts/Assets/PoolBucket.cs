using System.Collections.Generic;
using UnityEngine;

namespace NeonBlocks.Assets
{
    /// <summary>Một nhóm instance của cùng prefab Addressable trong phạm vi một scene.</summary>
    public sealed class PoolBucket
    {
        /// <summary>Address key của prefab template mà bucket tái sử dụng.</summary>
        public string TemplateKey { get; }
        /// <summary>Instance đang được view sử dụng.</summary>
        public List<GameObject> ActiveInstances { get; } = new List<GameObject>();
        /// <summary>Instance nhàn rỗi, vẫn phải bị hủy trước khi scope release template.</summary>
        public Queue<GameObject> InactiveInstances { get; } = new Queue<GameObject>();

        /// <summary>Tạo bucket cho một key template không rỗng.</summary>
        /// <param name="templateKey">Address key của prefab.</param>
        public PoolBucket(string templateKey)
        {
            if (string.IsNullOrWhiteSpace(templateKey))
            {
                throw new System.ArgumentException("Template key không được rỗng.", nameof(templateKey));
            }

            TemplateKey = templateKey;
        }
    }
}
