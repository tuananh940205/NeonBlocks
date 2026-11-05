# Bài 6 — Nguồn 7-bag và Next preview

Trạng thái: code và 4 EditMode tests đã được thêm; chưa chạy riêng `SevenBagPieceSourceTests` trong Unity Test Runner.

## Phạm vi đã triển khai

- `SevenBagPieceSource` là class thuần C# trả `PieceDefinition` theo 7-bag Fisher-Yates.
- Constructor nhận seed để tái lập chuỗi, hoặc nhận `IPieceRandom` để test/thay chiến lược RNG.
- `SeededPieceRandom` dùng `System.Random`; không có `UnityEngine.Random` trong gameplay source.
- Preview `NextPieces` luôn có đúng kích thước caller cấu hình. Bài MVP truyền `GameConfig.NextQueueSize`, hiện là 5; `TakeNext()` lấy phần tử đầu rồi bù lại ngay.

## Cách dùng ở game loop Bài 7

Sau khi `ConfigManager.Config` sẵn sàng, logic game tạo một nguồn:

```csharp
var source = new SevenBagPieceSource(config.Pieces, seed, config.NextQueueSize);
PieceDefinition next = source.TakeNext();
bool spawned = board.TrySpawn(next, config.SpawnOrigin);
```

`BoardModel` không tự lấy config hay random; nó chỉ nhận `PieceDefinition` và origin. Hold chưa được thêm, sẽ thuộc bài 11.

## Unity Editor

Bài này không cần GameObject, prefab, Addressable, component Inspector hay asset mới. Không gắn các class nguồn piece lên Hierarchy.

## Kiểm tra trong Unity

1. Mở **Window → General → Test Runner → EditMode**.
2. Chạy `SevenBagPieceSourceTests`.
3. Kỳ vọng 4 test pass: mỗi bag đủ bảy mã, cùng seed cùng chuỗi, preview dịch/bù đúng năm phần tử, và RNG injection quyết định thứ tự Fisher-Yates.

Không có asset mới nên không có mục bàn giao asset.
