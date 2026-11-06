# NeonBlocks — Tiến độ

Cập nhật: 2026-11-05. Bài 1–7 đã triển khai code; Bài 4 đã có bằng chứng Test Runner, Bài 5–7 chờ xác minh Unity.

## Trạng thái
Chưa bắt đầu / Đang làm / Đã triển khai, chờ xác minh / Hoàn thành / Bị chặn.
“Hoàn thành” yêu cầu đã đạt tiêu chí và có kết quả kiểm tra thật. Nếu chưa chạy Unity hoặc Android, ghi rõ và dùng trạng thái chờ xác minh.

| Bài | Nội dung | Trạng thái | Bằng chứng / còn thiếu |
|---|---|---|---|
| 1 | Nền tảng dự án và Entry scene | Đã triển khai, chờ xác minh | Code + asmdef + 17 EditMode test đã viết; Core/Runtime biên dịch OK bằng Roslyn của Unity (ngoài Editor). Addressables 2.10.3 đã cài trong project. Còn thiếu: gắn `SessionManager` và Canvas vào `Entry.unity`, chạy Test Runner và Play Mode (xem docs/lessons/BAI_1.md) |
| 2 | Dữ liệu JSON và cấu hình luật chơi | Đã triển khai, chờ xác minh | Code + 34 EditMode test đã viết (`GameConfigParserTests` 23, `GameConfigRulesTests` 4, `PieceDefinitionTests` 7); Runtime biên dịch OK bằng Roslyn của Unity. Còn thiếu: chạy Test Runner, đặt Addressables key `GameConfig` (xem docs/lessons/BAI_2.md) |
| 3 | Manager xuyên suốt game, Addressables và vòng đời scene | Đã triển khai, chờ xác minh | Code + 22 EditMode test mới (tổng 73). Runtime và test biên dịch OK bằng Roslyn của Unity; Addressables biên dịch từ source 2.10.3. Còn thiếu: Test Runner, Play Mode Entry→MainMenu→Gameplay→MainMenu, tạo MainMenu/Gameplay scene và Addressables Settings (xem docs/lessons/BAI_3.md) |
| 4 | BoardModel và khối đang rơi | Hoàn thành | `BoardModelTests` (7/7) pass trong Unity EditMode Test Runner; build test assembly thành công. |
| 5 | Xoay khối và wall kick | Đã triển khai, chờ xác minh | SRS CW/CCW, bảng kick JLSTZ/I, O giữ state và 6 EditMode tests; `dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công (0 lỗi). Còn thiếu: chạy `SrsRotationTests` trong Unity Test Runner (xem docs/lessons/BAI_5.md). |
| 6 | Nguồn khối và preview | Đã triển khai, chờ xác minh | 7-bag có seed, RNG injection, preview 5 và 4 EditMode tests; `dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công (0 lỗi). Còn thiếu: chạy `SevenBagPieceSourceTests` trong Unity Test Runner (xem docs/lessons/BAI_6.md). |
| 7 | Game loop và gravity | Đã triển khai, chờ xác minh | State/tick/gravity/drop/lock delay/reset cap và 4 EditMode tests; build thành công (0 lỗi). Còn thiếu: chạy `GameLoopTests` trong Unity Test Runner (xem docs/lessons/BAI_7.md). |
| 8 | Xóa hàng, tính điểm và kết thúc | Đã triển khai, chờ xác minh | Người dùng xác nhận đã hoàn thành phần triển khai; chưa có playtest. Mốc playtest placeholder desktop được chuyển lên bài 9; còn cần ghi kết quả Test Runner/Play Mode thực tế. |
| 9 | BoardView, pool và ghost piece | Chưa bắt đầu | Chưa có |
| 10 | Điều khiển bàn phím và cảm ứng | Đã triển khai, chờ xác minh | Touch button/Signal, DAS/ARR, 4 EditMode tests và asset handoff đã có; còn Unity Test Runner, Play Mode và Android device. |
| 11 | Hold và hoàn thiện HUD | Đã triển khai, chờ xác minh | HoldTests 9/9 pass trong Unity; PlayMode smoke Hold/HUD/restart pass; 2 asset đã duyệt/import/gắn Gameplay; sáu workflow diagrams. Toàn bộ EditMode 112/114 pass, còn 2 lỗi SessionManagerTests cũ và kiểm tra touch Android. Xem docs/lessons/BAI_11.md. |
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

### Bài 3
- **Ngày và phạm vi:** 2026-11-03. Manager thuần C# (`AddressableManager`, `ConfigManager`, `SceneFlowManager`) đăng ký qua `SessionManager`; `SceneAssetScope` theo từng scene; luồng Entry→MainMenu→Gameplay→MainMenu; Signal `Dispatch` tối thiểu.
- **File đã thêm:**
  - `Scripts/Assets/`: `AssetOwnerToken`, `AddressableLoadException`, `AddressableResult`, `IAddressableBackend`, `UnityAddressableBackend`, `AddressableManager`, `TrackedKind`, `TrackedAsset`, `LoadedScene`, `SceneAssetScope`.
  - `Scripts/Scene/`: `SceneFlowManager`, `SceneStopSignal`, `SceneDisposeSignal`, `SceneRequestSignal`.
  - `Scripts/Config/ConfigManager.cs`; `Scripts/App/EntryLauncher.cs`; `Scripts/Views/MainMenuCanvas.cs`, `GameplayCanvas.cs` (script gắn trên Canvas, giữ nút làm serialized field; đổi tên từ `MainMenuView`/`GameplayView` và giữ GUID để scene không đứt tham chiếu).
  - `Tests/EditMode/`: `FakeAddressableBackend`, `AddressableManagerTests` (12), `SceneFlowManagerTests` (5), `ConfigManagerTests` (5).
  - `docs/lessons/BAI_3.md`.
- **File đã sửa:** `Scripts/Core/Signals.cs` (thêm `Dispatch()` và `Dispatch(T)`); `Scripts/NeonBlocks.Runtime.asmdef` (thêm `Unity.Addressables`, `Unity.ResourceManager`, `Unity.UI`).
- **Quyết định thiết kế:**
  - Chỉ `AddressableManager` gọi Addressables release, qua `UnityAddressableBackend` là phần hạ tầng của nó. `SceneAssetScope` chỉ gọi `AddressableManager`.
  - Scene tải Additive; Entry giữ session và `BootVisuals` (camera/canvas khởi động) bị tắt sau khi MainMenu tải.
  - Rời scene: dispatch stop, dispatch dispose, rồi giải phóng scope; load scene đích sau đó. Hệ quả: có một khoảng không có scene khi chuyển; nếu tải scene đích lỗi, không có scene hoạt động cho đến lần chuyển thành công tiếp theo.
  - `ConfigManager.LoadAsync` không bọc lỗi parse; `JsonException` và `ConfigValidationException` là loại được tài liệu hóa, khác với quy tắc "bọc mọi lỗi" trong AGENTS. Cần xác nhận hoặc đổi.
- **Kiểm tra thật đã chạy (ngoài Editor):**
  - `Addressables` (2.10.3, từ source trong Library) biên dịch sạch bằng `csc.dll` của Unity, cùng `UnityEngine.UI.dll` và module UnityWebRequest.
  - `NeonBlocks.Core`, `NeonBlocks.Runtime` biên dịch sạch.
  - Test EditMode biên dịch sạch với NUnit netstandard 3.14.0 (từ NuGet, chỉ dùng cho kiểm tra biên dịch; project vẫn dùng NUnit của Unity). **Test chưa được chạy**, vì các test dùng GameObject/TextAsset cần Unity runtime.
- **Chưa xác minh:** Test Runner (73 test tổng cộng); Play Mode toàn luồng; cancel và unload trong Editor thật; Addressables Settings và các scene mới; Restart Game (chưa có UI).
- **Điều kiện đánh dấu hoàn thành:** 73 EditMode test pass; Entry→MainMenu→Gameplay→MainMenu chạy không lỗi; Play lặp không đăng ký trùng; load lỗi báo rõ trên Console.

- **Sửa 2026-11-05 (theo yêu cầu người dùng):** BootVisuals không còn được tham chiếu qua `[SerializeField]`. Thêm `BootVisualsHideSignal` (Signal kế thừa, `Scripts/App/`) và `BootVisualsView` gắn trên gốc `BootVisuals`: view resolve signal trong Start và tự tắt khi nhận lệnh. `EntryLauncher` đăng ký signal trước mọi `await`, có `DefaultExecutionOrder(-100)` để Start chạy trước view, và phát signal sau khi scene đầu đã tải. Đã biên dịch lại Runtime và test (không đổi số test). Chưa chạy Unity.
- **Sửa 2026-11-05 (nút):** nút Start/Back và listener của chúng chuyển sang script gắn trên Canvas (`MainMenuCanvas`, `GameplayCanvas`), giữ `Button` làm serialized field trên chính Canvas. View khác không chạm tới nút; muốn tác động tới nút phải gửi Signal tới script trên Canvas.
- **Hệ quả cho Editor:** `BootVisuals` cần Add Component `BootVisualsView`; `Session` không còn field Boot Visuals. Xem `docs/lessons/BAI_3.md` mục 5.

## Ghi nhận ban đầu
Đã đọc AGENTS.md, ProjectVersion.txt, Packages/manifest.json, EditorBuildSettings.asset và danh sách Assets qua MCP neonblocks_filesystem. Chưa chạy Unity, compile, test, profiling hay build. Người dùng đã chọn thể loại xếp gạch rơi kiểu Tetris.

## Cập nhật kế hoạch Manager
- Theo yêu cầu người dùng: bài 3 dùng AddressableManager, SceneFlowManager và ConfigManager thuần C#, singleton sống xuyên suốt game do SessionManager đăng ký/sở hữu. Đổi scene chỉ cleanup tài nguyên scene; không hủy Manager.
- Bài 9 dùng tên PoolManager; SceneAssetScope vẫn là scope theo scene. Đã đồng bộ LESSON_PLAN.md, PROJECT_CONTEXT.md, AGENTS.md và ghi chú hướng dẫn bài 1/2.
- Đây là thay đổi tài liệu; bài 3 vẫn chưa bắt đầu. Không sửa code hoặc chạy thêm test/build trong lần cập nhật này; giữ nguyên các kết quả kiểm tra trước đó.
- **Sửa 2026-11-05 (signal):** không class nào giữ tham chiếu tới Signal. Signal được resolve tại thời điểm dùng. Bộ Core: `ServiceLocator.Resolve<T>()` tạo lazy lớp signal ở lần đầu (trước đây nhánh này là code chết); đã bỏ `TryResolve`. `EntryLauncher` không đăng ký signal thủ công. Đã biên dịch lại Runtime và test.
- **Sửa 2026-11-05 (scene keys):** tên scene (`Entry`, `MainMenu`, `Gameplay`) chuyển khỏi hằng số trong class vào mục `scenes` của GameConfig, qua `SceneKeysDto` và `SceneKeys`. `EntryLauncher`, `MainMenuCanvas`, `GameplayCanvas`, `SessionManager.RestartGame` đọc từ `ConfigManager.Config.Scenes`. `RestartGame` đọc tên Entry trước `ResetSession`. Hằng số `GameConfigParser.AddressKey` được giữ vì cần trước khi config tải. Thêm 2 test parser (scene rỗng, scene trùng). Đã biên dịch lại Runtime và test.
- **Sửa 2026-11-05 (gọi signal trực tiếp):** bỏ kiểm tra null cho signal (luôn có instance nhờ lazy `Resolve`) trong `MainMenuCanvas`, `GameplayCanvas`, `BootVisualsView` và constructor `SceneFlowManager`; gọi `Resolve<T>().AddListener/RemoveListener/Dispatch` trực tiếp. Chỉ giữ kiểm tra null cho nút Inspector. Đã biên dịch lại Runtime và test.
- **Sửa 2026-11-05 (Entry unload):** `EntryLauncher` unload native scene Entry sau khi MainMenu Addressables tải và active thành công. Scene Entry không thuộc `SceneAssetScope`, nên cần dọn bằng `SceneManager.UnloadSceneAsync`; SessionManager nằm riêng ở DontDestroyOnLoad nên còn sống. Mục tiêu là dọn EventSystem/AudioListener bootstrap để không bị trùng ở MainMenu.

### Bài 4
- **Ngày và phạm vi:** 2026-11-05. Thêm gameplay thuần C# gồm `BoardModel` và `ActivePiece`; không thêm view, input, xoay, random bag, gravity, clear hàng hay scoring thuộc bài sau.
- **File đã thêm:** `Assets/_Project/Scripts/Gameplay/ActivePiece.cs`, `BoardModel.cs`; `Assets/_Project/Tests/EditMode/BoardModelTests.cs` (7 test); `docs/lessons/BAI_4.md`.
- **Quyết định thiết kế:** `BoardModel` sở hữu `bool[,]` settled cells, còn `ActivePiece` bất biến giữ `PieceDefinition` cùng origin. Mọi candidate phải `CanFit` trước khi commit. Bảng bao gồm y = 0 đến `TotalHeight - 1`, do đó lock trong hàng ẩn là hợp lệ. Origin spawn được truyền vào API để game loop sau này lấy từ `GameConfig.SpawnOrigin` mà model không phụ thuộc ConfigManager/Unity.
- **Kiểm tra thật đã chạy:** `dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công, 0 lỗi; có 10 warning conflict `System.Memory/System.Buffers` từ reference Unity hiện có.
- **Chưa xác minh:** 7 test `BoardModelTests` trong Unity Test Runner. Không cần setup Unity Editor, component, asset hay Addressables trong bài này.
- **Điều kiện đánh dấu hoàn thành:** toàn bộ `BoardModelTests` pass trong Unity EditMode Test Runner.

### Sửa sau Test Runner (2026-11-05)
- **Bằng chứng nhận được:** Unity EditMode Test Runner có 15 test fail: `GameConfigRulesTests` (1), `SceneFlowManagerTests` (5), `ServiceLocatorTests` (1) và `SessionManagerTests` (8). `AddressableManagerTests`, `BoardModelTests`, `ConfigManagerTests`, `GameConfigParserTests`, `PieceDefinitionTests` và `SafeAreaCalculatorTests` hiển thị pass.
- **Đã sửa:** `SessionManager.TryBeginSession()` chỉ gọi `DontDestroyOnLoad` khi đang Play Mode, tránh `InvalidOperationException` trong EditMode; `ServiceLocator.Resolve<T>()` chỉ đăng ký lazy Signal một lần, tránh log `Already registered`; `GameConfigRulesTests` loại các lớp hậu tố `Manager` có state runtime khỏi kiểm tra readonly của data/domain/DTO.
- **Kiểm tra sau sửa:** `dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công, 0 lỗi; chưa chạy lại Unity Test Runner nên không xác nhận các test này đã pass.

### Bài 5
- **Ngày và phạm vi:** 2026-11-05. Thêm xoay SRS CW/CCW cho gameplay thuần C#, gồm orientation, bảng wall kick riêng I/JLSTZ và O giữ nguyên state. Không thêm 180°, game loop, input, random bag, hold, clear hàng hay view.
- **File đã thêm:** `Assets/_Project/Scripts/Gameplay/PieceOrientation.cs`, `SrsKickTable.cs`; `Assets/_Project/Tests/EditMode/SrsRotationTests.cs` (6 test); `docs/lessons/BAI_5.md`.
- **File đã sửa:** `ActivePiece.cs` tính cell theo orientation quanh pivot config; `BoardModel.cs` thêm `TryRotateClockwise` và `TryRotateCounterClockwise`.
- **Quy ước:** nguồn bảng kick là TetrisWiki SRS; X sang phải/Y hướng lên nên giữ nguyên dấu offset SRS. Candidate được thử theo đúng thứ tự năm offset, chỉ commit candidate `CanFit` đầu tiên.
- **Kiểm tra thật đã chạy:** `dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công, 0 lỗi; có 10 warning conflict `System.Memory/System.Buffers` từ reference Unity hiện có.
- **Chưa xác minh:** 6 test `SrsRotationTests` trong Unity EditMode Test Runner. Không cần setup Unity Editor, component, asset hay Addressables trong bài này.
- **Điều kiện đánh dấu hoàn thành:** toàn bộ `SrsRotationTests` pass trong Unity EditMode Test Runner.

### Bài 6
- **Ngày và phạm vi:** 2026-11-05. Thêm nguồn tetromino 7-bag có seed và hàng đợi Next; không thêm game loop, gravity, input, hold, scoring, clear hàng hay view.
- **File đã thêm:** `Assets/_Project/Scripts/Gameplay/IPieceRandom.cs`, `SeededPieceRandom.cs`, `SevenBagPieceSource.cs`; `Assets/_Project/Tests/EditMode/FixedPieceRandom.cs`, `SevenBagPieceSourceTests.cs` (4 test); `docs/lessons/BAI_6.md`.
- **Quyết định thiết kế:** nguồn trả `PieceDefinition` từ GameConfig. Bag đầy đủ được giữ nội bộ; preview chỉ chứa đúng `NextQueueSize` (MVP là 5) và được bù ngay sau `TakeNext`. `IPieceRandom` cho phép test không phụ thuộc Unity Random.
- **Kiểm tra thật đã chạy:** `dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công, 0 lỗi; có 10 warning conflict `System.Memory/System.Buffers` từ reference Unity hiện có.
- **Chưa xác minh:** 4 test `SevenBagPieceSourceTests` trong Unity EditMode Test Runner. Không cần setup Unity Editor, component, asset hay Addressables trong bài này.
- **Điều kiện đánh dấu hoàn thành:** toàn bộ `SevenBagPieceSourceTests` pass trong Unity EditMode Test Runner.

### Bài 7
- **Ngày và phạm vi:** 2026-11-05. Thêm `GameLoop` thuần C# với Ready/Playing/Paused/GameOver, tick deterministic, gravity, soft/hard drop, lock delay và cap reset. Lock để state Ready chờ quy trình clear/spawn ở Bài 8.
- **File:** `Scripts/Gameplay/GameSessionState.cs`, `GameLoop.cs`; `Tests/EditMode/GameLoopTests.cs` (4); `docs/lessons/BAI_7.md`.
- **Chưa xác minh:** chạy `GameLoopTests` trong Unity EditMode Test Runner.
- **Sửa 2026-11-05 (EntryLauncher/SessionManager):** `EntryLauncher` bỏ `RequireComponent` và `GetComponent`, lấy session qua `ServiceLocator.Resolve<SessionManager>()`; chỉ chạy khi `gameObject.scene.buildIndex == 0` và cùng GameObject với session đang sở hữu. Đã biên dịch lại Runtime và test.

### Bài 8
- **Trạng thái:** logic core đã nối `lock → clear → score → top-out → spawn`; có `BoardModel.ClearFullRows()`, `BoardModel.HasSettledCellsInHiddenRows()`, `GameScore` và `GameplayPlaytestController` (IMGUI placeholder). Tài liệu ở `docs/lessons/BAI_8.md`.
- **Playtest sớm:** controller cần được gắn thủ công vào root object mới trong `Gameplay.unity`; automation đã mở Unity Test Runner nhưng không thể thao tác tiếp các control Editor do accessibility state của Unity không ổn định. Hướng dẫn thao tác tay và phím chơi ghi trong BAI_8.md.
- **Chưa xác minh:** EditMode tests riêng của clear/scoring/top-out chưa được thêm; Unity Test Runner/Play Mode chưa chạy thành công. Do đó không đánh dấu Bài 8 hoàn thành.

### Điều chỉnh lộ trình playtest — 2026-11-05
- Theo yêu cầu người dùng sau khi hoàn thành bài 8: đưa renderer placeholder, bridge gameplay và điều khiển bàn phím vào bài 9 để playtest desktop bắt đầu ngay sau bài này. Bài 10 chuyển trọng tâm sang cảm ứng Android/Device Simulator và polish input; keyboard bài 9 vẫn là regression bắt buộc.
- Entry đổi thành bootstrap tạm. Sau khi config và MainMenu tải thành công, MainMenu được active rồi Entry unload; SessionManager/manager session đã DontDestroyOnLoad sống tiếp. Nếu tải đầu tiên lỗi thì Entry không unload. Tài liệu đã đồng bộ ở LESSON_PLAN, PROJECT_CONTEXT, BAI_3 và AGENTS.
- Đây là thay đổi lộ trình/tài liệu; chưa sửa runtime hiện có để unload Entry và chưa có kết quả Play Mode mới.

### Rà soát asset scene và bản nháp — 2026-11-05
- Đã rà Entry, MainMenu và Gameplay: chưa có artwork dự án trong `Assets/_Project/Art`; MainMenu chỉ có StartButton mặc định, Gameplay chỉ có BackButton mặc định, Entry là bootstrap.
- Đã tạo ba bản nháp tại `art-source/NeonBlocks/`: nền portrait `NB-UI-001`, sprite cell `NB-GAME-001` và button plate `NB-UI-002`. Chúng chưa được đưa vào Assets hoặc gắn vào scene; xem `docs/ASSET_HANDOFF.md` để duyệt/crop/import.

### Sửa chuyển scene Gameplay — 2026-11-05
- Từ ảnh Play Mode: Console báo hai EventSystem/hai AudioListener trong lúc chuyển Additive và BackButton `MENU` không nhận click.
- Nguyên nhân button: `SceneFlowManager` tải scene đích trước, khiến `GameplayCanvas` đã đăng ký `SceneStopSignal`; signal không tham số sau đó khóa cả button của scene mới lẫn scene cũ.
- Đã đổi `SceneStopSignal` và `SceneDisposeSignal` thành signal có `sceneKey`; SceneFlowManager phát key scope cũ, MainMenuCanvas/GameplayCanvas chỉ khóa button khi `gameObject.scene.name` trùng key. Cập nhật test SceneFlowManager để xác nhận `stop:MainMenu` → `dispose:MainMenu`; bỏ `Debug.LogError` giả trong `GameplayCanvas.OnBackClicked`.
- Chưa chạy Unity Test Runner/Play Mode sau sửa. Cảnh báo EventSystem/AudioListener được phát trong thời điểm hai scene Additive cùng tồn tại; sau khi MainMenu unload, Hierarchy phải chỉ còn Gameplay và một EventSystem/AudioListener hoạt động. Nếu vẫn còn hai sau một frame, kiểm tra MainMenu chưa được unload hoặc Entry chưa unload.

### Bài 9
- **Ngày và phạm vi:** 2026-11-05. Hoàn thành code playtest placeholder desktop: `GameplayPresenter`, snapshot Signal, `BoardView`, Input System keyboard và `PoolManager` theo SceneAssetScope.
- **Asset trước code:** tạo `art-source/NeonBlocks/NB-UI-003-BoardFrame-v1.png`, trạng thái chờ duyệt; đã ghi yêu cầu nhập tại `docs/ASSET_HANDOFF.md`. Code hiện dùng RawImage placeholder, không dùng asset chưa duyệt.
- **File chính:** `Views/GameplayPresenter.cs`, `GameplayKeyboardInput.cs`, `BoardView.cs`; `Gameplay/GameplaySnapshot*`, `GameplayCommand*`; `Assets/PoolManager.cs`, `PoolBucket.cs`; `docs/lessons/BAI_9.md`.
- **Lifecycle:** `EntryLauncher` đăng ký PoolManager vào SessionManager. `SceneFlowManager` gọi PoolManager dọn mọi active/inactive instance của scope sau SceneStop/SceneDispose và trước `SceneAssetScope.ReleaseAsync`; chỉ AddressableManager vẫn release handle.
- **Kiểm tra thật đã chạy:** `dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công, 0 error; còn 12 warning reference conflict Unity hiện hữu.
- **Cần người dùng xác minh trong Unity:** thực hiện setup component theo `docs/lessons/BAI_9.md`, rồi Play từ Entry và kiểm tra input, ghost khớp hard drop, pause, restart GameOver, Back và Hierarchy sau chuyển scene. Unity Test Runner/Play Mode chưa được chạy trong lần này.

### Tái cấu trúc hướng dẫn agent — 2026-11-05
- Theo ủy quyền của người dùng, nội dung `AGENTS.md` cũ đã được chuyển thành các module ở `docs/agent-rules/`: `CORE`, `ADDRESSABLES_AND_LIFECYCLE`, `ASSETS_AND_EDITOR`, `DATA_AND_TESTING`, `UNITY_EDITOR`, `QUALITY`.
- `AGENTS.md` nay là router ngắn, có bảng chọn module theo loại công việc và theo Bài. Mục tiêu là agent chỉ nạp rule trực tiếp liên quan thay vì luôn đọc toàn bộ hướng dẫn.

### Sửa BoardView — tile khớp playfield (2026-11-05)
- `BoardView` không còn chia tile theo toàn bộ `BoardRoot` (bao gồm transparent margin và viền của khung PNG). Tile được tính vuông từ phần playfield ngang bên trong, sau đó căn giữa theo chiều dọc.
- Inspector có `Horizontal Inset Fraction` (mặc định `0.115`) và `Cell Gap Pixels` (mặc định `2`); giá trị mặc định khớp `NB-UI-003-BoardFrame-v1`.
- Sửa tiếp: BoardView dùng `Vertical Bottom Inset Fraction = 0.07` để hàng logic `y=0` bám hàng lưới dưới thay vì căn giữa. Handoff đã yêu cầu `NB-UI-003 v2` có lưới 10x20 pixel-perfect; cần thay asset này để bỏ calibration khi chốt art.

### Bài 10
- **Ngày và phạm vi:** 2026-11-05. Bổ sung touch input và hoàn thiện input giữ, không thay đổi binding keyboard Bài 9.
- **Asset trước code:** tạo `NB-UI-004-TouchDirectional-v1.png` và `NB-UI-005-TouchActions-v1.png` tại `art-source/NeonBlocks/`; đã ghi handoff, trạng thái chờ duyệt. Code tiếp tục dùng Image placeholder.
- **Code:** thêm `GameplayRepeatInput` (DAS/ARR), `GameplayHeldInput`/Signal và `GameplayTouchButton`; `GameplayKeyboardInput` phát press/release cho input giữ, Presenter áp dụng repeat khi Playing và clear khi pause/game over/scene stop.
- **Test:** thêm 4 `GameplayRepeatInputTests`; `dotnet build NeonBlocks.Tests.EditMode.csproj --no-restore` thành công, 0 error, 12 warning dependency Unity hiện hữu.
- **Cần người dùng xác minh:** setup `TouchControls` theo `docs/lessons/BAI_10.md`; chạy Unity Test Runner và Play Mode desktop/touch hoặc Android device. Chưa có kết quả device Android.

### Bài 11 — Hold và HUD
- **Ngày/phạm vi:** 2026-11-05 theo ngày phiên làm việc. Triển khai đúng Hold/HUD; không làm bài 12.
- **Logic/input:** GameLoop.TryHold, HeldPiece/CanHold; BoardModel.TryReplaceActivePiece; thêm Hold cuối enum command/touch để giữ giá trị cũ; keyboard C/Left Shift; snapshot có HeldPieceId/CanHold và Signal riêng yêu cầu phát lại snapshot.
- **View:** GameplayHudView dùng TMP và preview shape thật; GameplayCanvas quản lý listener nút Hold/Restart, khóa tương tác theo snapshot và lifecycle; Presenter phát state đồng bộ sau input/restart.
- **Asset/setup:** người dùng duyệt NB-UI-007/008; đã import vào đúng Art/UI/Controls/Touch và Art/UI/Gameplay, giữ nguồn art-source. Chạy Lesson11Setup trong Unity, gán field child UI và lưu Gameplay bằng Editor API; thêm reference Unity.TextMeshPro.
- **Kiểm tra thật:** Unity 6000.3.25f1 EditMode 114 tests: 112 pass, 2 fail; HoldTests mới 9/9 pass. PlayMode smoke Entry→MainMenu→Gameplay, Hold/second hold/lock/swap/pause/top-out/restart/HUD và một EventSystem/AudioListener pass. Build cuối tests 0 error/12 warning; editor 0 error/13 warning.
- **Hai test chưa pass:** SessionManagerTests.DuplicateSession_IsNotOwner_AndDestroyingItKeepsOwner đọc duplicate đã DestroyImmediate; SessionManagerTests.ResetSession_DisposesSingletonsInReverseOrderAndUnregisters đăng ký ProbeService cùng type hai lần. Không sửa SessionManager hoặc test cũ trong phạm vi bài 11.
- **Tài liệu/diagram:** docs/lessons/BAI_11.md và BAI_11_WORKFLOWS.md gồm sáu sơ đồ Mermaid cho Hold, input/Signal, HUD, restart/lifecycle, import, validation.
- **Evidence:** docs/evidence/lesson11-editmode.xml, lesson11-playmode.txt, lesson11-setup.txt.
- **Còn cần:** chạm thật Android/Device Simulator và xử lý hai lỗi test cũ để chốt toàn bộ regression. Không khẳng định cả suite pass. Board frame v1 và calibration giữ theo bài 9.
