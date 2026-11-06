namespace NeonBlocks.Gameplay
{
    /// <summary>Các lệnh điều khiển hợp lệ từ input view gửi tới GameplayPresenter.</summary>
    public enum GameplayCommand
    {
        MoveLeft,
        MoveRight,
        SoftDrop,
        RotateClockwise,
        RotateCounterClockwise,
        HardDrop,
        TogglePause,
        Restart,
        Hold
    }
}
