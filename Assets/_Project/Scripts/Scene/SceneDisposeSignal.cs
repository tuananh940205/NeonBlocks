namespace NeonBlocks.Scene
{
    /// <summary>
    /// Signal phát sau SceneStopSignal cho một scene cụ thể: các view thuộc scene đó dispose dữ liệu native
    /// và bỏ reference tới object sắp bị unload. Scene đích không xử lý signal của scene cũ.
    /// </summary>
    public sealed class SceneDisposeSignal : Signal<string>
    {
    }
}
