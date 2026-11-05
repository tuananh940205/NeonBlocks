# Bài 8 — Xóa hàng, tính điểm và kết thúc

Trạng thái: đang triển khai, chưa hoàn tất và chưa đủ điều kiện đánh dấu hoàn thành.

## Phần đã có

- `BoardModel.ClearFullRows()` xóa đồng thời mọi hàng đầy, sau đó compact các hàng còn lại xuống đáy.
- `BoardModel.HasSettledCellsInHiddenRows()` kiểm tra top-out sau bước clear.
- `GameScore` lưu `Score`, `Lines`, `Level`; tính điểm 1–4 hàng theo level trước clear, tăng level sau mỗi số hàng cấu hình, đồng thời hỗ trợ điểm soft drop/hard drop.

Các file liên quan:

- `Assets/_Project/Scripts/Gameplay/BoardModel.cs`
- `Assets/_Project/Scripts/Gameplay/GameScore.cs`

## Phần còn thiếu trong phạm vi bài

- Nối quy trình sau lock vào `GameLoop`: lock → clear → cộng điểm → kiểm tra hàng ẩn → spawn khối tiếp theo.
- Chuyển sang `GameOver` khi còn cell ở vùng ẩn hoặc spawn không fit.
- Cộng điểm theo đúng số ô soft/hard drop thực tế từ `GameLoop`.
- Viết EditMode tests cho clear 1–4 hàng, hàng không liền nhau, boundary level, thứ tự lock/clear/top-out/spawn và điểm drop.

## Playtest Unity tạm thời

Theo yêu cầu playtest sớm, đã thêm `GameplayPlaytestController` dùng IMGUI placeholder để render board và nhận bàn phím. Đây là bridge tạm thời của Bài 8; BoardView/pool/presentation chính thức vẫn thuộc Bài 9.

Automation đã mở **Window → General → Test Runner**, nhưng Unity Editor không expose được control của Test Runner/hierarchy ổn định cho phiên automation hiện tại. Thực hiện các bước còn lại bằng tay:

1. Trong Project, mở `Assets/_Project/Scenes/Gameplay.unity`.
2. Trong Hierarchy, tạo **Create Empty** dưới root, đặt tên `GameplayPlaytest`.
3. Chọn object đó, **Add Component → Gameplay Playtest Controller**. Không cần gán field Inspector.
4. Lưu scene. Unity sẽ tự tạo file `.meta` cho script nếu Editor chưa refresh ngay.
5. Play từ `Entry` để `ConfigManager` tải config rồi vào MainMenu, sau đó nhấn Start để tới Gameplay. Không chạy trực tiếp Gameplay vì playtest controller cần config session đã đăng ký.

Điều khiển: `←`/`→` di chuyển, `↑` hoặc `X` xoay CW, `Z` xoay CCW, `↓` soft drop, `Space` hard drop, `P` pause/resume. Góc trên trái hiển thị score, lines, level và state.

Để chạy test, trong **Window → General → Test Runner → EditMode**, chọn **Run All** sau khi Unity biên dịch xong. Hiện chưa có kết quả Unity Test Runner cho Bài 8.

## Kiểm tra đã thực hiện

`dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` đã biên dịch thành công, 0 lỗi. Lệnh vẫn có cảnh báo conflict `System.Memory`/`System.Buffers` từ reference Unity hiện có.
