using System.Collections.Generic;
using NeonBlocks.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NeonBlocks.Views
{
    /// <summary>
    /// Vùng chạm UI phát Signal cho một action; lệnh giữ theo dõi pointer để đa chạm không nhân command.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        /// <summary>
        /// Action của vùng chạm, chọn tại Inspector của chính button.
        /// </summary>
        [SerializeField] private TouchControlAction action;

        private readonly HashSet<int> pointerIds = new HashSet<int>();

        /// <summary>
        /// Phát press một lần cho pointer đầu tiên chạm vào vùng UI.
        /// </summary>
        /// <param name="eventData">Dữ liệu pointer EventSystem.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!pointerIds.Add(eventData.pointerId) || pointerIds.Count != 1) return;
            if (TryGetHeldCommand(out GameplayHeldCommand held))
            {
                ServiceLocator.Instance.Resolve<GameplayHeldCommandSignal>().Dispatch(new GameplayHeldInput(held, true));
                return;
            }

            ServiceLocator.Instance.Resolve<GameplayCommandSignal>().Dispatch(ToCommand());
        }

        /// <summary>
        /// Phát release khi pointer cuối cùng rời action giữ.
        /// </summary>
        /// <param name="eventData">Dữ liệu pointer EventSystem.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            ReleasePointer(eventData.pointerId);
        }

        /// <summary>
        /// Hủy trạng thái giữ nếu pointer kéo ra khỏi button.
        /// </summary>
        /// <param name="eventData">Dữ liệu pointer EventSystem.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            ReleasePointer(eventData.pointerId);
        }

        /// <summary>
        /// Đảm bảo không còn lệnh giữ khi UI bị tắt do pause, popup hoặc scene lifecycle.
        /// </summary>
        private void OnDisable()
        {
            if (pointerIds.Count == 0 || !TryGetHeldCommand(out GameplayHeldCommand held)) return;
            pointerIds.Clear();
            ServiceLocator.Instance.Resolve<GameplayHeldCommandSignal>().Dispatch(new GameplayHeldInput(held, false));
        }

        /// <summary>
        /// Gỡ pointer và chỉ release command sau khi không còn ngón nào trong cùng button.
        /// </summary>
        private void ReleasePointer(int pointerId)
        {
            if (!pointerIds.Remove(pointerId) || pointerIds.Count != 0 || !TryGetHeldCommand(out GameplayHeldCommand held)) return;
            ServiceLocator.Instance.Resolve<GameplayHeldCommandSignal>().Dispatch(new GameplayHeldInput(held, false));
        }

        /// <summary>
        /// Ánh xạ các action lặp sang lệnh giữ tương ứng.
        /// </summary>
        private bool TryGetHeldCommand(out GameplayHeldCommand held)
        {
            switch (action)
            {
                case TouchControlAction.MoveLeft: held = GameplayHeldCommand.MoveLeft; return true;
                case TouchControlAction.MoveRight: held = GameplayHeldCommand.MoveRight; return true;
                case TouchControlAction.SoftDrop: held = GameplayHeldCommand.SoftDrop; return true;
                default: held = default; return false;
            }
        }

        /// <summary>
        /// Ánh xạ action chạm một lần sang GameplayCommand.
        /// </summary>
        private GameplayCommand ToCommand()
        {
            return action switch
            {
                TouchControlAction.RotateClockwise => GameplayCommand.RotateClockwise,
                TouchControlAction.RotateCounterClockwise => GameplayCommand.RotateCounterClockwise,
                TouchControlAction.HardDrop => GameplayCommand.HardDrop,
                TouchControlAction.TogglePause => GameplayCommand.TogglePause,
                _ => throw new System.InvalidOperationException($"Action '{action}' phải dùng held command.")
            };
        }
    }
}
