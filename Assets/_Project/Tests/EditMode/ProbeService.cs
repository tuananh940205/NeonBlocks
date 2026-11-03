using System;
using System.Collections.Generic;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Service giả dùng cho test vòng đời: ghi lại thứ tự dispose và số lần dispose để kiểm tra ownership.
    /// </summary>
    public sealed class ProbeService : IDisposable
    {
        private readonly List<string> disposeLog;

        /// <summary>Tên dùng để nhận diện service trong log dispose.</summary>
        public string Name { get; }

        /// <summary>Số lần Dispose đã được gọi; phải bằng 1 sau mỗi lần shutdown hợp lệ.</summary>
        public int DisposeCount { get; private set; }

        /// <summary>Callback tùy chọn chạy khi Dispose, dùng để kiểm tra trạng thái trong lúc shutdown.</summary>
        public Action OnDispose { get; set; }

        /// <summary>
        /// Tạo service giả với tên và danh sách log dùng chung để ghi thứ tự dispose.
        /// </summary>
        /// <param name="name">Tên định danh của service.</param>
        /// <param name="disposeLog">Danh sách nhận tên service mỗi khi Dispose; có thể null.</param>
        public ProbeService(string name, List<string> disposeLog = null)
        {
            Name = name;
            this.disposeLog = disposeLog;
        }

        /// <summary>
        /// Ghi nhận một lần dispose, thêm tên vào log dùng chung và chạy callback nếu có.
        /// </summary>
        public void Dispose()
        {
            DisposeCount++;
            disposeLog?.Add(Name);
            OnDispose?.Invoke();
        }
    }
}
