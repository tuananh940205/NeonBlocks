# Bài 5 — Xoay khối và wall kick SRS

Trạng thái: code và 6 EditMode tests đã được thêm; chưa chạy riêng `SrsRotationTests` trong Unity Test Runner.

## Phạm vi đã triển khai

- `ActivePiece` có bốn orientation: `Spawn` (0), `Right` (R), `Reverse` (2), `Left` (L).
- `BoardModel.TryRotateClockwise()` và `TryRotateCounterClockwise()` chỉ commit candidate đầu tiên fit; không có API xoay 180°.
- J/L/S/T/Z dùng bảng kick SRS chung; I dùng bảng riêng; O chấp nhận lệnh xoay nhưng giữ nguyên orientation, origin và hình.
- Khi cả năm candidate kick bị chặn, `ActivePiece` giữ nguyên orientation/origin.

## Nguồn và quy ước tọa độ

Bảng kick lấy theo [Super Rotation System — TetrisWiki](https://tetris.wiki/SRS). Bảng nguồn sử dụng trục Y dương hướng lên, khớp trực tiếp với `GridPoint` của NeonBlocks: X tăng sang phải, Y tăng lên, y = 0 là đáy. Vì vậy các offset được ghi nguyên dấu trong `SrsKickTable`; không có bước đảo dấu Y.

Các orientation quay theo chu kỳ `0 → R → 2 → L → 0` với CW; CCW theo chiều ngược lại. Origin vẫn là góc đáy trái của khung 4×4. Tâm xoay lấy từ `PieceDefinition.RotationCenterX/Y` đã có trong JSON config.

## Unity Editor

Bài này không cần GameObject, prefab, Addressable, component Inspector hay asset mới. Không gắn `BoardModel`, `ActivePiece`, `PieceOrientation` hoặc `SrsKickTable` vào Hierarchy; game loop của bài 7 sẽ gọi API model.

## Kiểm tra trong Unity

1. Mở **Window → General → Test Runner → EditMode**.
2. Chạy `SrsRotationTests`.
3. Kỳ vọng 6 test pass: chu kỳ bốn orientation, wall kick JLSTZ, floor kick JLSTZ, kick riêng của I, O giữ state, và mọi kick thất bại giữ state.

Không có asset mới nên không có mục bàn giao asset.
