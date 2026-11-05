# Bài 4 — BoardModel và khối đang rơi

Trạng thái: code và EditMode tests đã được thêm; cần chạy Test Runner trong Unity trước khi xác nhận hoàn thành.

## Phạm vi đã triển khai

- `BoardModel` là class thuần C#, không kế thừa `MonoBehaviour` và không dùng Unity API.
- Lưới chỉ lưu ô đã khóa trong `bool[,]`; `ActivePiece` bất biến được tách hoàn toàn khỏi lưới này.
- `TrySpawn`, di chuyển trái/phải, rơi một ô và lock chỉ commit state khi candidate hợp lệ. Thao tác không hợp lệ trả `false` và không mutate model.
- Cả vùng hiển thị lẫn `HiddenRows` là phần của lưới: tọa độ hợp lệ có `0 <= y < TotalHeight`.

## API dùng ở bài sau

| API | Trách nhiệm |
|---|---|
| `new BoardModel(BoardSettings)` | Tạo lưới settled rỗng. |
| `TrySpawn(PieceDefinition, GridPoint)` | Spawn khi không có active piece và bốn ô đều fit. |
| `TryMoveLeft`, `TryMoveRight`, `TryFallOneCell` | Dịch active piece một ô nếu còn fit. |
| `TryLockActivePiece` | Ghi bốn ô active vào settled grid rồi xóa active piece. |
| `CanFit`, `IsSettledAt`, `GetSettledCells` | Đọc/kiểm tra state mà không mở quyền sửa grid. |

`GridPoint` dùng hệ tọa độ đã chốt: X sang phải, Y hướng lên; y = 0 là đáy. Origin spawn truyền từ `GameConfig.SpawnOrigin` khi game loop được thêm ở bài 7, không để `BoardModel` resolve config hoặc ServiceLocator.

## Unity Editor

Bài này không cần GameObject, prefab, Addressable, component Inspector hay asset mới. Không gắn `BoardModel` hoặc `ActivePiece` lên Hierarchy; chúng được tạo từ logic game thuần ở bài 7.

## Kiểm tra trong Unity

1. Mở **Window → General → Test Runner → EditMode**.
2. Chạy `BoardModelTests`.
3. Kỳ vọng bảy test pass: mép trái/phải, đáy, đỉnh vùng ẩn, hàng ẩn, spawn bị chặn, lock và spawn khi đang có active piece.

Các tests chưa được chạy trong Unity ở lần triển khai này. Không có asset mới nên không có mục bàn giao asset.
