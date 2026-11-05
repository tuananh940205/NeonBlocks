using UnityEngine;

namespace NeonBlocks.Gameplay
{
    /// <summary>Sự kiện press hoặc release bất biến từ keyboard/touch view cho một lệnh giữ.</summary>
    public sealed class GameplayHeldInput
    {
        /// <summary>Lệnh đang thay đổi trạng thái giữ.</summary>
        public GameplayHeldCommand Command { get; }
        /// <summary>True khi press, false khi release.</summary>
        public bool IsPressed { get; }

        /// <summary>Tạo sự kiện giữ cho lệnh xác định.</summary>
        public GameplayHeldInput(GameplayHeldCommand command, bool isPressed)
        {
            Debug.Log($"Press input {command}");
            Command = command;
            IsPressed = isPressed;
        }
    }
}
