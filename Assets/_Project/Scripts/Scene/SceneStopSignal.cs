namespace NeonBlocks.Scene
{
    /// <summary>
    /// Signal phát khi scene sắp rời: các view dừng input, timer và coroutine của mình. Kế thừa Signal có sẵn của bộ Core.
    /// </summary>
    public sealed class SceneStopSignal : Signal
    {
    }
}
