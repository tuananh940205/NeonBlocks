namespace NeonBlocks.Scene
{
    /// <summary>
    /// Signal phát khi một scene cụ thể sắp rời: các view thuộc đúng scene đó dừng input, timer và coroutine của mình.
    /// Scene đích đã tải Additive không được dừng chỉ vì scene trước đang được release.
    /// </summary>
    public sealed class SceneStopSignal : Signal<string>
    {
    }
}
