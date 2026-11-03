# NeonBlocks — Tiến độ

Cập nhật: 2026-11-03. Bài 1 đã triển khai code, chờ xác minh trong Unity Editor; chưa triển khai gameplay.

## Trạng thái
Chưa bắt đầu / Đang làm / Đã triển khai, chờ xác minh / Hoàn thành / Bị chặn.
“Hoàn thành” yêu cầu đã đạt tiêu chí và có kết quả kiểm tra thật. Nếu chưa chạy Unity hoặc Android, ghi rõ và dùng trạng thái chờ xác minh.

| Bài | Nội dung | Trạng thái | Bằng chứng / còn thiếu |
|---|---|---|---|
| 1 | Nền tảng dự án và Entry scene | Đã triển khai, chờ xác minh | Code + asmdef + 17 EditMode test đã viết; Core/Runtime biên dịch OK bằng Roslyn của Unity (ngoài Editor). Addressables 2.10.3 đã cài trong project. Còn thiếu: gắn `SessionManager` và Canvas vào `Entry.unity`, chạy Test Runner và Play Mode (xem docs/lessons/BAI_1.md) |
| 2 | Dữ liệu JSON và cấu hình luật chơi | Đã triển khai, chờ xác minh | Code + 34 EditMode test đã viết (`GameConfigParserTests` 23, `GameConfigRulesTests` 4, `PieceDefinitionTests` 7); Runtime biên dịch OK bằng Roslyn của Unity. Còn thiếu: chạy Test Runner, đặt Addressables key `GameConfig` (xem docs/lessons/BAI_2.md) |
| 3 | Addressables và vòng đời scene | Chưa bắt đầu | Chưa có |
| 4 | BoardModel và khối đang rơi | Chưa bắt đầu | Chưa có |
| 5 | Xoay khối và wall kick | Chưa bắt đầu | Chưa có |
| 6 | Nguồn khối và preview | Chưa bắt đầu | Chưa có |
| 7 | Game loop và gravity | Chưa bắt đầu | Chưa có |
| 8 | Xóa hàng, tính điểm và kết thúc | Chưa bắt đầu | Chưa có |
| 9 | BoardView, pool và ghost piece | Chưa bắt đầu | Chưa có |
| 10 | Điều khiển bàn phím và cảm ứng | Chưa bắt đầu | Chưa có |
| 11 | Hold và hoàn thiện HUD | Chưa bắt đầu | Chưa có |
| 12 | Menu, popup và lưu dữ liệu | Chưa bắt đầu | Chưa có |
| 13 | Mỹ thuật neon và âm thanh | Chưa bắt đầu | Chưa có |
| 14 | Polish và hướng dẫn chơi | Chưa bắt đầu | Chưa có |
| 15 | Kiểm thử và bản Android | Chưa bắt đầu | Chưa có |
| 16 | Profiling và tối ưu có số đo | Chưa bắt đầu | Chưa có |
| 17 | Hoàn tất portfolio | Chưa bắt đầu | Chưa có |

## Nhật ký từng bài
Khi triển khai, thêm mục “Bài N” gồm:
- Ngày và phạm vi đã thực hiện.
- File/asset đã thêm hoặc sửa.
- Thiết lập Unity đã thực hiện và phần người dùng cần làm.
- Kiểm tra: tên test/thao tác, môi trường và kết quả thực tế.
- Hạn chế, lỗi còn tồn tại và điều kiện để đánh dấu hoàn thành.
- Đường dẫn hướng dẫn docs/lessons/BAI_N.md và evidence nếu có.

### Bài 1
- **Ngày và phạm vi:** 2026-11-03. Nền tảng: asmdef Core/Runtime/Tests, `SessionManager` (owner session, DontDestroyOnLoad, chỉ hủy khi Restart Game hoặc thoát Play Mode), safe area, test EditMode. Addressables chưa cài (người dùng thực hiện).
- **Thay đổi so với phiên bản đầu:** bỏ `AppLifecycle`/`AppLifecycleState`/`EntryBootstrap` theo yêu cầu; singleton sống xuyên suốt được đăng ký trực tiếp qua `SessionManager.RegisterSingleton` trong logic khởi động. Các script đã xóa: `AppLifecycle.cs`, `AppLifecycleState.cs`, `EntryBootstrap.cs`, `AppLifecycleTests.cs` (kèm .meta).
- **File đã thêm:**
  - `Assets/Scripts/Core/NeonBlocks.Core.asmdef` (+ .meta): đưa Core vào assembly riêng tại chỗ, không di chuyển file.
  - `Assets/_Project/Scripts/NeonBlocks.Runtime.asmdef` (+ .meta): tham chiếu `NeonBlocks.Core`, không tham chiếu Assembly-CSharp.
  - `Assets/_Project/Scripts/App/SessionManager.cs`.
  - `Assets/_Project/Scripts/UI/SafeAreaCalculator.cs`, `SafeAreaFitter.cs`.
  - `Assets/_Project/Tests/EditMode/NeonBlocks.Tests.EditMode.asmdef` (+ .meta), `SessionManagerTests.cs`, `ServiceLocatorTests.cs`, `SafeAreaCalculatorTests.cs`, `ProbeService.cs`.
  - Thư mục `Assets/_Project/{Scenes,Configs,Prefabs,Art,Audio}` (có `.gitkeep`).
  - `docs/lessons/BAI_1.md`.
- **File đã sửa:** `Assets/Scripts/Core/ServiceLocator.cs` — thêm `IsRegistered(Type)` và `UnregisterAll()`. Lý do: `ServiceLocator.Instance` là static, Domain Reload tắt sẽ giữ đăng ký cũ giữa các lần Play. Không đổi hành vi API cũ.
- **Phiên bản Addressables:** 2.10.3, đã cài trong NeonBlocks (manifest + lock; `unity: 6000.0`). Registry công khai có 4.1.x mới hơn nhưng PM của Editor chỉ hiện 2.10.3; chưa thử cài 4.x nên chưa xác minh.
- **Thiết lập Unity:** người dùng làm theo `docs/lessons/BAI_1.md`: cài Addressables, Player Settings portrait, hoàn thiện Entry scene (SessionManager, Canvas, SafeAreaRoot), Build Settings chỉ còn Entry.
- **Kiểm tra thật đã chạy:**
  - Biên dịch `NeonBlocks.Core` và `NeonBlocks.Runtime` bằng `csc.dll` của Unity 6000.3.25f1 với bộ reference netstandard 2.1: exit 0, không có cảnh báo (kiểm tra lại sau khi đổi sang SessionManager).
  - Biên dịch `NeonBlocks.Tests.EditMode`: chỉ lỗi CS0012 do reference NUnit (bản net40) không tương thích với bộ netstandard trong môi trường này; không có lỗi ngữ nghĩa nào khác. **Test chưa được chạy.**
- **Chưa xác minh:** Test Runner (17 test EditMode), Play Mode Entry, Play lặp/Domain Reload tắt, Restart Game (chưa có nút UI, gắn ở bài 12), Device Simulator safe area, Build Android. Unity Editor đang mở project nên không chạy batch mode trên `Library/`.
- **Hạn chế:** `Signal` chưa có API phát sự kiện (chưa cần ở bài 1). `ServiceLocator.UnregisterSingleton(object)` vẫn khóa theo kiểu; `SessionManager` chỉ đăng ký/hủy singleton do chính nó sở hữu.
- **Điều kiện đánh dấu hoàn thành:** 17 EditMode test pass trong Unity; Entry chạy không lỗi Console; Play lặp không có `Already registered`; Build Settings chỉ có Entry.
- **Asset:** bài 1 không cần asset mới, không tạo mục trong `docs/ASSET_HANDOFF.md`.

### Bài 2
- **Ngày và phạm vi:** 2026-11-03. Cấu hình chung trong một file JSON duy nhất; model bất biến constructor-only; DTO riêng cho Newtonsoft (readonly, constructor `[JsonConstructor]`); validate khi map. Chưa làm gameplay; tải qua Addressables thuộc bài 3.
- **File đã thêm:**
  - `Assets/_Project/Configs/GameConfig.json`: toàn bộ dữ liệu chung.
  - `Assets/_Project/Scripts/Config/`: `ConfigValidationException`, `GridPoint`, `BoardSettings`, `PieceDefinition`, `LevelData`, `ScoringRules`, `TimingRules`, `GameConfig`, `GameConfigParser` (mỗi type một file; `AddressKey = "GameConfig"`).
  - `Assets/_Project/Scripts/Config/Dto/`: `GameConfigDto`, `BoardDto`, `GridPointDto`, `FloatPointDto`, `PieceDto`, `LevelDto`, `ScoringDto`, `TimingDto`.
  - `Assets/_Project/Tests/EditMode/`: `GameConfigParserTests.cs`, `GameConfigRulesTests.cs`, `PieceDefinitionTests.cs`, `ConfigTestFiles.cs`.
  - `docs/lessons/BAI_2.md`.
- **Quyết định thiết kế:** giá trị luật lấy từ kế hoạch (10×20 + 4 ẩn, spawn (3, 20), Next 5, điểm 100/300/500/800, soft 1, hard 2, lock 0.5s, 15 reset, DAS 0.15s, ARR 0.05s). Tốc độ rơi 1.0 → 0.1 giây/ô qua 10 level là giá trị khởi tạo, chưa đánh giá khi chơi. Shape khối được kiểm tra: đúng 4 ô, trong khung 4×4, không trùng, liền nhau, tâm xoay trong khung.
- **Không có asset mới:** không tạo mục trong `docs/ASSET_HANDOFF.md`.
- **Kiểm tra thật đã chạy:**
  - `NeonBlocks.Core` và `NeonBlocks.Runtime` biên dịch bằng `csc.dll` của Unity 6000.3.25f1 (netstandard 2.1 + `UnityEngine.JSONSerializeModule`): exit 0, không cảnh báo.
  - Test EditMode: chỉ lỗi CS0012 do reference NUnit trong môi trường này, không có lỗi ngữ nghĩa khác. **Test chưa được chạy trong Unity.**
- **Chưa xác minh:** Test Runner (34 test); Newtonsoft 3.2.2 tải qua `precompiledReferences` trong asmdef; JSON sai cú pháp (test kỳ vọng `JsonException`); Addressables key `GameConfig`.
- **Điều kiện đánh dấu hoàn thành:** 34 test pass trong Unity; `GameConfig.json` có key Addressables `GameConfig`.

### Kết quả Test Runner lần đầu (2026-11-03)
- Nguồn: `TestResults.xml` (lần chạy gần nhất, 1 test, Failed) và Editor.log các lần chạy trước.
- **Đã xác định và sửa:**
  - `GameConfigRulesTests.ConfigTypes_HaveOnlyReadonlyInstanceFields` fail: reflection trả thêm field private `_message` của `System.Exception`. Đã thêm `BindingFlags.DeclaredOnly` cho các test reflection.
  - `SessionManagerTests.DuplicateSession_IsNotOwner_AndDestroyingItKeepsOwner`: log có `Type NeonBlocks.SessionManager not registered` tại dòng resolve, và không có cảnh báo `đã tồn tại` nào. Nguyên nhân: `Awake` không chạy khi `AddComponent` trong Edit Mode. Đã tách logic thành `TryBeginSession()`; test tạo GameObject không hoạt động rồi gọi method này tường minh. Các test SessionManager khác dùng cùng cách; suy luận từ mã, chưa thấy kết quả riêng của từng test.
- **Chưa xác minh:** chạy lại toàn bộ EditMode sau sửa. Test 1 và 2 của bài 2 (`Newtonsoft` qua `precompiledReferences`) chưa có kết quả.

## Ghi nhận ban đầu
Đã đọc AGENTS.md, ProjectVersion.txt, Packages/manifest.json, EditorBuildSettings.asset và danh sách Assets qua MCP neonblocks_filesystem. Chưa chạy Unity, compile, test, profiling hay build. Người dùng đã chọn thể loại xếp gạch rơi kiểu Tetris.
