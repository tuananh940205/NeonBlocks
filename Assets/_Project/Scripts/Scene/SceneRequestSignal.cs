namespace NeonBlocks.Scene
{
    /// <summary>
    /// Signal yêu cầu đổi scene: view gửi address key của scene đích, SceneFlowManager lắng nghe và thực hiện chuyển scene.
    /// </summary>
    public sealed class SceneRequestSignal : Signal<string>
    {
    }
}
