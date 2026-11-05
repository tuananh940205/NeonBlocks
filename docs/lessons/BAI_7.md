# Bài 7 — Game loop và gravity

Trạng thái: code và 4 EditMode tests đã thêm, chưa chạy Unity Test Runner.

`GameLoop` là class thuần C#; caller truyền delta time vào `Tick`. Nó quản lý `Ready/Playing/Paused/GameOver`, gravity theo giây/ô, soft drop, hard drop, lock delay và giới hạn reset. Khi lock, state về `Ready`; Bài 8 sẽ thêm clear/top-out/spawn tiếp theo theo đúng thứ tự luật.

Không cần tạo GameObject, component, prefab hoặc asset trong Unity cho bài này. Chạy `GameLoopTests` trong **Window → General → Test Runner → EditMode**; kỳ vọng 4 test pass.
