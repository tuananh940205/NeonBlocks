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

Tái sử dụng bộ ServiceLocator và Signal người dùng đã import tại Assets/Scripts/Core. Locator hiện là class với static Instance, không phải static class; đây là ngoại lệ đã được người dùng chọn so với quy tắc static class cũ. Không tạo locator/signal thứ hai. Các service được quản lý là class thường, không có static singleton riêng. Typed Signal register/dispatch cho giao tiếp giữa các MonoBehaviour; không serialized reference giữa các view để gọi nhau. UI button dùng AddListener/RemoveListener. Mọi method tạo/sửa trong Assets có XML comment tiếng Việt, đặt trước attribute.

Chỉ Entry nằm trong Build Settings. Entry là bootstrap tạm: sau khi GameConfig và scene đầu tiên đã tải thành công, scene đích được đặt active rồi Entry được unload. Chỉ GameObject riêng chứa SessionManager (DontDestroyOnLoad) và manager session sống tiếp; BootVisuals/EntryLauncher không được DontDestroyOnLoad. Nếu tải đầu tiên lỗi, giữ Entry để log/báo lỗi. Scene khác và popup/panel tải Addressables. Key bằng tên asset hoặc root GameObject. LevelData giữ key thay cho prefab/sprite reference. Chỉ AddressableManager/SceneAssetScope release handle, kể cả release instance thông qua owner. Khi thoát: dừng input/job → dispose native data nếu có → hủy pool instances active/inactive → release scope. Chưa được release template khi pool còn instance. Unload native Entry không phải release Addressables handle.

Tuân thủ các giới hạn try-catch trong AGENTS.md; không nuốt lỗi. Không sửa Library, Temp hoặc file Unity sinh tự động. Chỉ dùng Jobs/Burst nếu có baseline và benchmark phù hợp.

## Quy tắc thực hiện bài
Đọc AGENTS.md cùng cả ba file docs trước mỗi bài. Kiểm tra code/trạng thái thực tế, không suy ra bài hoàn tất từ sự tồn tại của file. Nếu dependency thiếu, báo rõ và chỉ sửa phần cần thiết trong phạm vi được giao; không âm thầm triển khai bài tiếp.

Mỗi bài phải kèm hướng dẫn Editor cụ thể: đường dẫn asset, vị trí Hierarchy, component, field Inspector, giá trị và thao tác kiểm tra. Nếu không có công cụ Unity thì tạo code/setup hướng dẫn khả dụng, ghi phần Editor chưa xác minh, không báo hoàn thành đầy đủ. Không bịa test, build, FPS hay memory.

## Cập nhật nền tảng — 2026-11-03
Người dùng đã import ServiceLocator.cs và Signals.cs vào Assets/Scripts/Core. Bài 1 được điều chỉnh để tích hợp và kiểm tra vòng đời bộ này, không viết lại. Chưa triển khai bài 1 hoặc chạy Unity. API đăng ký theo concrete type; Resolve chưa tự tạo signal khi type chưa được đăng ký. Signals hiện chưa có API phát sự kiện. GetPrefabs dùng Resources và không dùng cho runtime asset NeonBlocks.

## Session và singleton — 2026-11-03
- Entry có một `SessionManager` (DontDestroyOnLoad). Singleton sống xuyên suốt game được đăng ký qua `SessionManager.RegisterSingleton` trong logic khởi động của Entry; không dùng AppLifecycle hay framework lifecycle riêng.
- `SessionManager` chỉ bị hủy khi Restart Game (`RestartGame()`: kết thúc session → hủy GameObject → tải lại Entry) hoặc khi thoát Play Mode. Instance trùng bị hủy ngay trong Awake và không đụng vào session đang chạy.
- Sau khi khởi động thành công, Entry scene bị unload; điều này không hủy SessionManager vì nó đã ở GameObject riêng được DontDestroyOnLoad. EntryLauncher, BootVisuals và UI bootstrap bị hủy cùng scene. Không chỉ tắt BootVisuals làm trạng thái cuối.
- Singleton gắn với một scene (không phải session) do scene đó sở hữu và phải gỡ khi rời scene.
- Chi tiết và quy tắc tái sử dụng cho các game khác nằm trong `PuzzlePortfolio/AGENTS.md`, mục "Session và singleton".

## Manager logic xuyên suốt game — cập nhật theo yêu cầu người dùng

- Class quản lý/điều phối logic cho một chức năng đặt tên `{Function}Manager`: AddressableManager, SceneFlowManager, ConfigManager; PoolManager được triển khai ở bài 9. Đây là class thuần C#, không kế thừa MonoBehaviour, không static Instance riêng.
- Đăng ký một instance qua SessionManager.RegisterSingleton tại Entry và resolve qua ServiceLocator hiện có. Manager sống xuyên suốt game/session, giữ cùng instance khi đổi scene; cleanup/unregister khi Restart Game hoặc kết thúc game/Play Mode.
- SessionManager hiện có là ngoại lệ làm cầu nối MonoBehaviour sở hữu session. Không buộc Manager logic vào component, serialized field hoặc lifecycle của MonoBehaviour scene.
- Manager tồn tại lâu không đồng nghĩa giữ tài nguyên scene mãi: dọn listener/view/pool instances và handle của scene khi rời scene. Không dispose chính Manager khi đổi scene. Config dùng chung thuộc scope session; SceneAssetScope thuộc từng scene.
- Model, data, DTO, parser, signal và scope không phải class điều phối chức năng nên giữ tên theo trách nhiệm (BoardModel, GameConfigParser, SceneAssetScope...), không đổi tất cả thành Manager hoặc singleton.
- Tên chuẩn mới là AddressableManager thay AddressableService, PoolManager thay PoolService. Các quy tắc cũ về ownership/release vẫn giữ nguyên ý nghĩa với tên mới. Chỉ AddressableManager và SceneAssetScope release Addressables handle.
- Quy tắc này ưu tiên khi tài liệu cũ dùng tên Service hoặc mô tả singleton của Manager gắn với scene; object scene và scope vẫn được cleanup theo vòng đời riêng.
