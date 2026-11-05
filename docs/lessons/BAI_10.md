# Bài 10 — Điều khiển cảm ứng và hoàn thiện input

## Code đã triển khai

- `GameplayRepeatInput` là logic thuần C# dùng DAS/ARR từ `GameConfig.Timing` cho Left, Right và Soft Drop.
- `GameplayKeyboardInput` phát press/release cho các phím giữ; keyboard Bài 9 vẫn giữ các binding cũ.
- `GameplayTouchButton` xử lý PointerDown/PointerUp/PointerExit, chống lệnh lặp từ nhiều pointer cùng chạm một button và phát Signal thay vì gọi Presenter trực tiếp.
- `GameplayPresenter` chặn input/repeat khi Paused, GameOver hoặc scene đang rời; release repeat khi pause/scene stop.

## Unity Editor setup

1. Trong `Assets/_Project/Scenes/Gameplay.unity`, dưới `GameplayCanvas/Playtest`, tạo UI empty `TouchControls` neo đáy màn hình, stretch ngang, cao khoảng `260` Canvas units.
2. Tạo bảy child `Image` có raycast target bật: `MoveLeft`, `SoftDrop`, `MoveRight`, `RotateCW`, `RotateCCW`, `HardDrop`, `Pause`. Mỗi object thêm `GameplayTouchButton`; không cần giữ serialized reference tới Presenter.
3. Chọn enum **Action** tương ứng: MoveLeft, SoftDrop, MoveRight, RotateClockwise, RotateCounterClockwise, HardDrop, TogglePause.
4. Bố cục khuyến nghị: cụm MoveLeft/SoftDrop/MoveRight ở dưới-trái; RotateCW/RotateCCW/HardDrop ở dưới-phải; Pause ở góc trên-phải. Mỗi vùng chạm tối thiểu 96×96 Canvas units.
5. Đảm bảo scene có đúng một `EventSystem`. Nếu project đang dùng Input System UI, EventSystem dùng `InputSystemUIInputModule`; không để đồng thời `StandaloneInputModule`.
6. Chỉ sau khi duyệt asset, import các PNG riêng `NB-UI-004A/B/C` và `NB-UI-005A/B` theo `docs/ASSET_HANDOFF.md`, rồi đặt sprite tương ứng cho từng Image. Bản code hoạt động với Image placeholder trước.

## Kiểm tra thủ công

- Desktop: A/←, D/→ và S/↓ có bước đầu ngay, lặp sau DAS 0.15 giây rồi theo ARR 0.05 giây; Z/X/↑/Space/P/Esc/R vẫn hoạt động.
- Touch: giữ Left/Right/SoftDrop, nhấn RotateCW/RotateCCW/HardDrop/Pause; kéo ngón ra ngoài button phải release; chạm hai ngón cùng một button không được nhân lệnh.
- Khi Pause/GameOver hoặc rời Gameplay, lệnh giữ không tiếp tục; sau resume cần chạm/nhấn lại.

## Kiểm tra đã thực hiện

`dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công, 0 error; 12 warning dependency Unity hiện hữu. Chưa chạy Unity Test Runner hoặc device Android trong lần này.
