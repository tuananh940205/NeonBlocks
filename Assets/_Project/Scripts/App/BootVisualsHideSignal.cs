using NeonBlocks.Scene;

namespace NeonBlocks.App
{
    /// <summary>
    /// Signal yêu cầu ẩn các object chỉ dùng lúc khởi động (BootVisuals) sau khi scene đầu tiên đã tải.
    /// EntryLauncher phát signal; BootVisualsView lắng nghe. Không có tham chiếu serialized giữa hai bên.
    /// </summary>
    public sealed class BootVisualsHideSignal : Signal
    {
    }
}
