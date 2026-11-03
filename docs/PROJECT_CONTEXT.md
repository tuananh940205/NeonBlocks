# NeonBlocks — Project context

## Mục tiêu
Game xếp gạch rơi kiểu Tetris, được người dùng xác nhận ngày 2026-11-03. Làm theo từng bài, người dùng chỉ cần nói “Triển khai bài N”. Mục tiêu Android portrait 9:16, Unity 6.3 LTS, URP 2D, dùng mỹ thuật neon riêng.

## Hiện trạng đã đọc
- Editor: 6000.3.25f1.
- Assets hiện có Scenes và Settings; chưa thấy thư mục gameplay riêng.
- Build Settings hiện có SampleScene.
- manifest có URP, Input System, Test Framework; chưa có Addressables. Không coi Jobs/Burst đã sẵn sàng chỉ vì có file csproj.
- AGENTS.md được copy từ Color Factory. Giữ nguyên quy tắc kiến trúc/coding của file đó, cập nhật tên game.
- Chưa triển khai bài nào; chưa chạy test hoặc build.

## Luật MVP dự kiến
Ngoài thể loại được xác nhận, các lựa chọn dưới đây là mặc định thiết kế để triển khai, có thể đổi khi người dùng yêu cầu; nếu đổi, cập nhật kế hoạch và ảnh hưởng bài phụ thuộc.
- Bảng 10 cột × 20 hàng hiển thị, thêm 4 hàng ẩn phía trên; tọa độ y tăng lên.
- 7 tetromino, nguồn 7-bag có seed; next preview 5 khối.
- Xoay SRS CW/CCW, không 180; hold một lần mỗi khối.
- Gravity, soft drop, hard drop, ghost, lock delay 0.5 giây, tối đa 15 reset hợp lệ.
- Xóa hàng đồng thời, không gravity cho từng ô sau clear; chỉ compact hàng.
- Điểm 100/300/500/800 × level cho 1/2/3/4 hàng, soft drop 1/ô, hard drop 2/ô. Level đầu 1, mỗi 10 hàng tăng 1; dùng level trước clear để tính điểm.
- Sau lock: clear → kiểm tra cell còn ở vùng ẩn → spawn; spawn bị chặn cũng game over.
- Endless single-player offline; chưa thêm T-spin scoring, combo, B2B, multiplayer, quảng cáo, IAP hoặc leaderboard online.
- Game dùng tên, hình ảnh, âm thanh riêng; mô tả “kiểu Tetris” là tham chiếu thể loại.

## Kiến trúc cần giữ
BoardModel và gameplay thuần C#, Unity views xử lý render/input. Config JSON TextAsset tải qua Addressables key; deserialize DTO rồi map sang data readonly/constructor-only, không yêu cầu Unity serializer tự gán readonly field. Một type mỗi file.

Tái sử dụng bộ ServiceLocator và Signal người dùng đã import tại Assets/Scripts/Core. Locator hiện là class với static Instance, không phải static class; đây là ngoại lệ đã được người dùng chọn so với quy tắc static class cũ. Không tạo locator/signal thứ hai. Các service được quản lý là class thường, không có static singleton riêng. Typed Signal cho sự kiện, callback/action cho yêu cầu view; không serialized reference giữa các view để gọi nhau. UI button dùng AddListener/RemoveListener. Mọi method tạo/sửa trong Assets có XML comment tiếng Việt, đặt trước attribute.

Chỉ Entry nằm trong Build Settings. Scene khác và popup/panel tải Addressables. Key bằng tên asset hoặc root GameObject. LevelData giữ key thay cho prefab/sprite reference. Chỉ AddressableService/SceneAssetScope release handle, kể cả release instance thông qua owner. Khi thoát: dừng input/job → dispose native data nếu có → hủy pool instances active/inactive → release scope. Chưa được release template khi pool còn instance.

Tuân thủ các giới hạn try-catch trong AGENTS.md; không nuốt lỗi. Không sửa Library, Temp hoặc file Unity sinh tự động. Chỉ dùng Jobs/Burst nếu có baseline và benchmark phù hợp.

## Quy tắc thực hiện bài
Đọc AGENTS.md cùng cả ba file docs trước mỗi bài. Kiểm tra code/trạng thái thực tế, không suy ra bài hoàn tất từ sự tồn tại của file. Nếu dependency thiếu, báo rõ và chỉ sửa phần cần thiết trong phạm vi được giao; không âm thầm triển khai bài tiếp.

Mỗi bài phải kèm hướng dẫn Editor cụ thể: đường dẫn asset, vị trí Hierarchy, component, field Inspector, giá trị và thao tác kiểm tra. Nếu không có công cụ Unity thì tạo code/setup hướng dẫn khả dụng, ghi phần Editor chưa xác minh, không báo hoàn thành đầy đủ. Không bịa test, build, FPS hay memory.

## Cập nhật nền tảng — 2026-11-03
Người dùng đã import ServiceLocator.cs và Signals.cs vào Assets/Scripts/Core. Bài 1 được điều chỉnh để tích hợp và kiểm tra vòng đời bộ này, không viết lại. Chưa triển khai bài 1 hoặc chạy Unity. API đăng ký theo concrete type; Resolve chưa tự tạo signal khi type chưa được đăng ký. Signals hiện chưa có API phát sự kiện. GetPrefabs dùng Resources và không dùng cho runtime asset NeonBlocks.

## Session và singleton — 2026-11-03
- Entry có một `SessionManager` (DontDestroyOnLoad). Singleton sống xuyên suốt game được đăng ký qua `SessionManager.RegisterSingleton` trong logic khởi động của Entry; không dùng AppLifecycle hay framework lifecycle riêng.
- `SessionManager` chỉ bị hủy khi Restart Game (`RestartGame()`: kết thúc session → hủy GameObject → tải lại Entry) hoặc khi thoát Play Mode. Instance trùng bị hủy ngay trong Awake và không đụng vào session đang chạy.
- Singleton gắn với một scene (không phải session) do scene đó sở hữu và phải gỡ khi rời scene.
- Chi tiết và quy tắc tái sử dụng cho các game khác nằm trong `PuzzlePortfolio/AGENTS.md`, mục "Session và singleton".
