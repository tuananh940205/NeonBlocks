# Bài 9 — Playtest placeholder desktop

## Thành phần đã triển khai

- `GameplayPresenter` tạo `GameLoop` từ `ConfigManager`, tick logic và phát `GameplaySnapshotSignal`.
- `GameplayKeyboardInput` dùng Input System: Left/A, Right/D, Down/S, Z, X/Up, Space, P/Escape và R khi GameOver.
- `BoardView` chỉ render snapshot: settled, active, ghost hard-drop và HUD/Next bằng `RawImage` placeholder.
- `PoolManager` là singleton session; scene flow gọi `ReleaseScope` trước `SceneAssetScope.ReleaseAsync`, vì thế active/inactive instance được hủy trước template Addressables.

## Unity Editor setup bắt buộc

1. Mở `Assets/_Project/Scenes/Gameplay.unity`. Trong `GameplayCanvas`, tạo empty UI object tên `Playtest` (RectTransform stretch toàn Canvas).
2. Thêm `GameplayPresenter` và `GameplayKeyboardInput` vào `Playtest`.
3. Dưới `Playtest`, tạo `BoardRoot` (UI > Empty). Đặt anchors ở giữa, width `600`, height `1200`; thêm `Image` nền tối nếu cần. Đây là nơi `BoardView` sinh các `RawImage` ô placeholder.
4. Dưới `Playtest`, tạo `HudText` (UI > Legacy > Text), neo phía trên board; font Arial mặc định, màu trắng, kích thước 28, horizontal overflow `Overflow`.
5. Thêm `BoardView` vào `Playtest`. Kéo `BoardRoot` vào **Board Root** và `HudText` vào **Hud Text**. Không gán Presenter hay Input vào BoardView. Với khung `NB-UI-003`, giữ **Horizontal Inset Fraction = 0.115**, **Vertical Bottom Inset Fraction = 0.07** và **Cell Gap Pixels = 2**: BoardView tính tile vuông từ playfield nội bộ, lấy hàng `y=0` bám đáy lưới thay vì căn giữa vùng texture.
6. Xóa component `GameplayPlaytestController` cũ khỏi scene nếu Unity hiện Missing Script sau khi script được thay thế.
7. Play từ Entry, bấm Start. Đảm bảo Console không có exception; test các phím trong HUD. Khi Back về MainMenu, Hierarchy phải không còn scene Gameplay và pool được dọn trước scope.

## Asset chờ duyệt

`art-source/NeonBlocks/NB-UI-003-BoardFrame-v1.png` là khung 10x20 mới tạo. Chưa import vào `Assets/`; khi được duyệt, làm theo `docs/ASSET_HANDOFF.md` và dùng nó làm Image nền của `BoardRoot`, không tạo lưới UI thứ hai.

## Kiểm tra đã thực hiện

`dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công: 0 error. Có 12 warning conflict `System.Memory/System.Buffers` từ reference Unity hiện hữu. Chưa chạy Unity Test Runner hoặc Play Mode sau setup scene thủ công.
