using NeonBlocks.Gameplay;
using NeonBlocks.Scene;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlocks.Views
{
    /// <summary>Input view desktop dùng Input System, chỉ phát GameplayCommandSignal và không chứa luật chơi.</summary>
    [DisallowMultipleComponent]
    public sealed class GameplayKeyboardInput : MonoBehaviour
    {
        private InputAction moveLeft;
        private InputAction moveRight;
        private InputAction softDrop;
        private InputAction rotateClockwise;
        private InputAction rotateCounterClockwise;
        private InputAction hardDrop;
        private InputAction togglePause;
        private InputAction restart;
        private bool isStopping;

        /// <summary>Tạo các binding keyboard một lần trước khi component nhận input.</summary>
        private void Awake()
        {
            moveLeft = CreateHeldAction("MoveLeft", GameplayHeldCommand.MoveLeft, "<Keyboard>/leftArrow", "<Keyboard>/a");
            moveRight = CreateHeldAction("MoveRight", GameplayHeldCommand.MoveRight, "<Keyboard>/rightArrow", "<Keyboard>/d");
            softDrop = CreateHeldAction("SoftDrop", GameplayHeldCommand.SoftDrop, "<Keyboard>/downArrow", "<Keyboard>/s");
            rotateClockwise = CreateAction("RotateClockwise", GameplayCommand.RotateClockwise, "<Keyboard>/upArrow", "<Keyboard>/x");
            rotateCounterClockwise = CreateAction("RotateCounterClockwise", GameplayCommand.RotateCounterClockwise, "<Keyboard>/z");
            hardDrop = CreateAction("HardDrop", GameplayCommand.HardDrop, "<Keyboard>/space");
            togglePause = CreateAction("TogglePause", GameplayCommand.TogglePause, "<Keyboard>/p", "<Keyboard>/escape");
            restart = CreateAction("Restart", GameplayCommand.Restart, "<Keyboard>/r");
        }

        /// <summary>Đăng ký signal scene và bật các action khi view thuộc scene đang hoạt động.</summary>
        private void OnEnable()
        {
            ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(OnSceneStopping);
            SetActionsEnabled(true);
        }

        /// <summary>Tắt action và gỡ listener để không gửi lệnh từ scene đã rời.</summary>
        private void OnDisable()
        {
            ReleaseHeldActions();
            SetActionsEnabled(false);
            ServiceLocator.Instance.Resolve<SceneStopSignal>().RemoveListener(OnSceneStopping);
        }

        /// <summary>Giải phóng InputAction do component tạo sau khi GameObject bị hủy.</summary>
        private void OnDestroy()
        {
            DisposeAction(moveLeft); DisposeAction(moveRight); DisposeAction(softDrop); DisposeAction(rotateClockwise);
            DisposeAction(rotateCounterClockwise); DisposeAction(hardDrop); DisposeAction(togglePause); DisposeAction(restart);
        }

        /// <summary>Tắt input ngay khi SceneStopSignal chỉ đến scene sở hữu component này.</summary>
        /// <param name="stoppingSceneKey">Address key của scene đang rời.</param>
        private void OnSceneStopping(string stoppingSceneKey)
        {
            if (gameObject.scene.name == stoppingSceneKey)
            {
                isStopping = true;
                SetActionsEnabled(false);
            }
        }

        /// <summary>Tạo action button với một hoặc hai binding và callback phát signal.</summary>
        private InputAction CreateAction(string actionName, GameplayCommand command, string primaryBinding, string secondaryBinding = null)
        {
            var action = new InputAction(actionName, InputActionType.Button, primaryBinding);
            if (!string.IsNullOrWhiteSpace(secondaryBinding))
            {
                action.AddBinding(secondaryBinding);
            }

            action.performed += _ => Dispatch(command);
            return action;
        }

        /// <summary>Tạo action giữ có press/release rõ ràng để presenter áp dụng DAS và ARR chung với touch input.</summary>
        private InputAction CreateHeldAction(string actionName, GameplayHeldCommand command, string primaryBinding, string secondaryBinding = null)
        {
            var action = new InputAction(actionName, InputActionType.Button, primaryBinding);
            if (!string.IsNullOrWhiteSpace(secondaryBinding)) action.AddBinding(secondaryBinding);
            action.started += _ => DispatchHeld(command, true);
            action.canceled += _ => DispatchHeld(command, false);
            return action;
        }

        /// <summary>Phát lệnh khi scene còn hoạt động; presenter quyết định lệnh nào hợp lệ theo state.</summary>
        private void Dispatch(GameplayCommand command)
        {
            if (!isStopping)
            {
                ServiceLocator.Instance.Resolve<GameplayCommandSignal>().Dispatch(command);
            }
        }

        /// <summary>Phát thay đổi trạng thái giữ cho presenter khi scene còn hoạt động.</summary>
        private void DispatchHeld(GameplayHeldCommand command, bool isPressed)
        {
            if (!isStopping) ServiceLocator.Instance.Resolve<GameplayHeldCommandSignal>().Dispatch(new GameplayHeldInput(command, isPressed));
        }

        /// <summary>Phát release cho mọi action giữ trước khi input bị disable để không giữ state qua pause hoặc scene khác.</summary>
        private void ReleaseHeldActions()
        {
            DispatchHeld(GameplayHeldCommand.MoveLeft, false);
            DispatchHeld(GameplayHeldCommand.MoveRight, false);
            DispatchHeld(GameplayHeldCommand.SoftDrop, false);
        }

        /// <summary>Bật hoặc tắt toàn bộ action đã tạo; Awake có thể chưa chạy trong một số EditMode test.</summary>
        private void SetActionsEnabled(bool enabled)
        {
            SetActionEnabled(moveLeft, enabled); SetActionEnabled(moveRight, enabled); SetActionEnabled(softDrop, enabled);
            SetActionEnabled(rotateClockwise, enabled); SetActionEnabled(rotateCounterClockwise, enabled); SetActionEnabled(hardDrop, enabled);
            SetActionEnabled(togglePause, enabled); SetActionEnabled(restart, enabled);
        }

        /// <summary>Đổi trạng thái một action không null mà không gọi Enable/Disable lặp vô ích.</summary>
        private static void SetActionEnabled(InputAction action, bool enabled)
        {
            if (action == null || action.enabled == enabled) return;
            if (enabled) action.Enable(); else action.Disable();
        }

        /// <summary>Giải phóng action nếu đã được Awake tạo.</summary>
        private static void DisposeAction(InputAction action)
        {
            if (action != null) action.Dispose();
        }
    }
}
