# NeonBlocks — Agent Instructions

AI Agents do not remove anything in this file

## Khi người dùng nói “Triển khai bài N”

1. Đọc toàn bộ `docs/PROJECT_CONTEXT.md`, `docs/LESSON_PLAN.md` và `docs/PROGRESS.md`.
2. Tìm đúng heading `Bài N` trong `docs/LESSON_PLAN.md`.
3. Thực hiện trọn vẹn phạm vi bài đó: code, asset nếu được yêu cầu, Unity Editor setup, test và cập nhật progress.
4. Không thực hiện nội dung của bài tiếp theo khi chưa được người dùng yêu cầu rõ ràng.
5. Không nói bài đã pass nếu chưa chạy được validation khả dụng; nêu rõ phần nào cần người dùng kiểm tra trong Unity Editor.
6. Tóm tắt những thay đổi có liên quan trực tiếp đến trò chơi sau khi công việc hoàn tất
7. Nếu cần asset mới, ghi yêu cầu cụ thể vào docs/ASSET_HANDOFF.md; dùng placeholder để tiếp tục phần code.

## Khi người dùng nói "Tích hợp các asset đã sẵn sàng trong docs/ASSET_HANDOFF.md, kiểm tra và cập nhật tiến độ."

1. Generate asset theo yêu cầu, và đặt trong đúng thư mục
2. Cập nhật vào checklist file

## Unity và kiến trúc bắt buộc

- Unity 6.3 LTS, Universal 2D (URP), Android portrait 9:16.
- Mọi method tạo hoặc sửa trong `Assets/` phải có XML comment tiếng Việt giải thích trách nhiệm.
- Giữ gameplay rule trong pure C# (`BoardModel`), Unity views chỉ render/input.
- Runtime asset dùng Addressables string key. Key phải chính xác bằng tên asset hoặc root GameObject.
- `LevelData` chỉ lưu string key cho runtime asset, không lưu direct prefab/sprite reference.
- Chỉ `AddressableService` và `SceneAssetScope` được release Addressables handles.
- Khi rời scene: dừng input/job -> dispose native data -> destroy active/inactive pool instances -> release SceneAssetScope.
- Không release prefab template khi `PoolService` còn instance.
- Không gọi `Addressables.Release` trực tiếp trong gameplay/view/UI code.
- All popups/panels view should be packed by addressable prefab, not placed in scene at initial, shown by Instantiate Async and release when closed
- Only entry scene set in scene list. The another scene will be loaded by addressables

## Asset và UI

- Dùng SpriteAtlas cho sprite gameplay và UI đã ổn định.
- Việc generate/edit asset hình ảnh do chat Asset đảm nhiệm; Codex phụ trách code và tích hợp Unity. Codex không tự generate ảnh hoặc ghi đè ảnh đã duyệt.
- Ảnh đang tạo/chỉnh hoặc chờ duyệt phải nằm trong `art-source/` tại gốc dự án, ngoài thư mục `Assets/`. Chat Asset dùng Image Generation cho raster mới/chỉnh sửa, lưu PNG và kiểm tra alpha nếu yêu cầu nền trong suốt.
- Chỉ đưa ảnh đã được người dùng duyệt vào `Assets/_Project/Art/`. Sự tồn tại của file ảnh không đồng nghĩa với việc đã được duyệt; không tự duyệt thay người dùng.
- Theo dõi yêu cầu và bàn giao trong `docs/ASSET_HANDOFF.md`; nếu chưa có thì tạo khi phát sinh yêu cầu asset đầu tiên. Mỗi mục ghi mã asset, mục đích, đường dẫn nguồn/đích, kích thước, yêu cầu alpha, bố cục sprite sheet nếu có, thông số import đề xuất và trạng thái duyệt. Khi chỉnh ảnh đã duyệt, phiên bản mới cần được duyệt lại trước khi tích hợp.
- Khi cần asset mới, Codex ghi yêu cầu cụ thể vào file bàn giao và dùng placeholder cho phần có thể tiếp tục; ghi rõ phần tích hợp còn chờ asset.
- Codex phụ trách đưa ảnh đã duyệt vào Unity, cấu hình import, `.meta`, SpriteAtlas, prefab, scene và Addressables; kiểm tra kết quả và cập nhật tiến độ.
- Khi cập nhật ảnh đã tích hợp, giữ nguyên đường dẫn và `.meta` để bảo toàn tham chiếu. Nếu kích thước hoặc bố cục sprite sheet thay đổi, kiểm tra và cập nhật slicing cùng các thành phần sử dụng ảnh.
- Quy trình bàn giao asset này áp dụng cho mọi bài, kể cả khi tài liệu bài học yêu cầu tạo ảnh; không cần người dùng nhắc lại trong mỗi prompt.
- Không tạo asset Addressable riêng nếu nó chỉ là dependency của một Addressable prefab, trừ khi lesson ghi rõ.
- Mọi hướng dẫn Unity Editor phải nêu: object tạo ở đâu trong Hierarchy/Project, component thêm vào, field Inspector cần gán, giá trị cần đặt và cách test.

## Chất lượng và portfolio

- Dùng Jobs/Burst chỉ cho bài toán có baseline và benchmark trên workload đủ lớn.
- Không bịa số benchmark, profiler hoặc memory result.
- Các file evidence cuối dự án nằm ở `docs/profiling/`.
- Không sửa `Library/`, `Temp/` hay Unity-generated files.

## Coding instruction
Use action/callback when call from this view to that view, remove cross dependencies serialized fields
Use button listener by addlistener/removelistener instead of Button component
Every generated function should write explanation comments by Vietnamese
Game config and level config should be load text asset by json, then serialize to serialiable class
Text asset can be loaded by addressable by key
Data class has readonly field, set data from constructor only
1 file should define only class/object with the same name as file name

Every singleton instance managed by ServiceLocator
Use Signal instead of placing serialized monobehaviour script object to call methods
When need to use new signal, define a new class inherit from the needed signal
XML comments should be placed above the atrribute tags of the member definition

Only use try-catch in functions when:
- has CancellationTokenSource
- Test method that expects an exception: wrap the whole call (start the Task and await it) in try-catch, catch only the predicted exception type, keep a timeout so the test cannot hang
- Service methods that return Task must wrap every failure in the documented exception type, including synchronous errors from Addressables
- Test that triggers an exception logged by Unity or a library: try-catch cannot intercept a log, so declare it with LogAssert.Expect(<LogType as shown in the test failure output, e.g. Error>, <exact message regex>) and do not ignore all failing messages


Use one static class object as service locator to store all singletons
Do not use static class or static instance for singleton items

## Bộ ServiceLocator do người dùng import

- Tái sử dụng `Assets/Scripts/Core/ServiceLocator.cs` và `Assets/Scripts/Core/Signals.cs`; không tạo bộ ServiceLocator/Signal thứ hai hoặc thay framework.
- Theo lựa chọn của người dùng, giữ mô hình `ServiceLocator.Instance` của bộ đã import. Đây là ngoại lệ cho yêu cầu static class ServiceLocator ở trên; service khác vẫn không có static singleton riêng.
- Đọc API thực tế trước khi dùng; chỉ bổ sung tối thiểu nếu bài cần, giữ tương thích và xác minh lifecycle. Không dùng GetPrefabs/Resources cho runtime asset NeonBlocks; dùng Addressables theo quy tắc hiện có.


## Giao tiếp giữa MonoBehaviour và class thuần logic

- Signal là nơi quản lý listener và gọi các listener khi dispatch.
- Khi một MonoBehaviour cần gọi logic của MonoBehaviour khác, định nghĩa một class signal riêng kế thừa loại Signal phù hợp trong bộ đã import. Bên nhận register listener vào signal, bên gửi dispatch signal; không kéo serialized field tham chiếu MonoBehaviour bên kia để gọi method.
- Quy tắc signal này được ưu tiên hơn hướng dẫn action/callback giữa các view ở trên khi cả hai bên là MonoBehaviour. Không dùng callback/action trực tiếp giữa hai MonoBehaviour để thay thế signal.
- Lấy signal dùng chung qua ServiceLocator hiện có; đăng ký instance trước khi resolve theo API thực tế. Không tạo các instance signal riêng khiến bên gửi và bên nhận dùng hai kênh khác nhau.
- Register/unregister listener theo vòng đời phù hợp và đối xứng để tránh gọi trùng hoặc gọi object đã bị hủy. Dispatch chỉ gọi các listener đang đăng ký.
- Class thuần logic không kế thừa MonoBehaviour. Nếu cần được gọi từ nhiều nơi, đăng ký một instance singleton qua ServiceLocator hiện có và resolve để sử dụng; không tạo static Instance riêng cho class đó.
- Singleton không có nghĩa là tồn tại mãi: xác định rõ owner và vòng đời, unregister/cleanup khi kết thúc scope tương ứng.

## Serialization và dữ liệu bất biến (cập nhật 2026-11-03)

Áp dụng cho mọi lần làm việc sau trong NeonBlocks.

- Mọi serialize/deserialize dùng Newtonsoft.Json (`com.unity.nuget.newtonsoft-json` 3.2.2, khai báo trong `Packages/manifest.json`). Không dùng `JsonUtility` cho dữ liệu mới.
- Asmdef dùng JSON khai báo DLL qua `precompiledReferences: ["Newtonsoft.Json.dll"]`. Asmdef nào có code JSON, kể cả test, đều phải khai báo như vậy.
- **Mọi field phải read-only.** Áp dụng cho cả domain type và DTO. Field là `readonly` (hoặc property chỉ có getter với backing field readonly), gán duy nhất trong constructor.
- DTO nằm trong namespace `*.Dto`, là class sealed với `public readonly` field và một constructor có `[JsonConstructor]`. Tên tham số constructor trùng đúng tên key trong JSON. Không dùng setter, không dùng field mutable để nhận dữ liệu.
- Domain type validate trong constructor và ném exception đúng loại (`ConfigValidationException` cho dữ liệu sai). Mapping DTO sang domain do một hàm thuần thực hiện, không để DTO đi vào gameplay.
- Config dùng một file JSON duy nhất cho dữ liệu chung. Key Addressables trùng tên asset (ví dụ `GameConfig`), không chứa tham chiếu asset trực tiếp.
- Test cần dữ liệu biến thể: đọc JSON thành `JObject`, sửa một trường, rồi parse lại chuỗi. Không sửa DTO hoặc domain object bằng reflection.
- Test JSON sai cú pháp kỳ vọng `JsonException` (Newtonsoft); test dữ liệu sai kỳ vọng `ConfigValidationException`. Test cấu trúc readonly và không tham chiếu `UnityEngine.Object` nằm trong `GameConfigRulesTests`; chạy lại khi thêm kiểu cấu hình mới.
- Khi test reflection field, dùng `BindingFlags.DeclaredOnly`. Nếu không, Mono trả cả field private của lớp cha (ví dụ `Exception._message`) và test báo lỗi giả.
- Test EditMode cho MonoBehaviour không dựa vào `Awake` khi `AddComponent`: Unity không đảm bảo gọi Awake trong Edit Mode. Đưa logic khởi tạo vào method tường minh (ví dụ `TryBeginSession()`), `Awake` chỉ gọi method đó, và test gọi method trực tiếp sau khi tạo GameObject không hoạt động (`SetActive(false)`).

## Manager logic xuyên suốt game — cập nhật theo yêu cầu người dùng

- Class quản lý/điều phối logic cho một chức năng đặt tên `{Function}Manager`: AddressableManager, SceneFlowManager, ConfigManager; PoolManager được triển khai ở bài 9. Đây là class thuần C#, không kế thừa MonoBehaviour, không static Instance riêng.
- Đăng ký một instance qua SessionManager.RegisterSingleton tại Entry và resolve qua ServiceLocator hiện có. Manager sống xuyên suốt game/session, giữ cùng instance khi đổi scene; cleanup/unregister khi Restart Game hoặc kết thúc game/Play Mode.
- SessionManager hiện có là ngoại lệ làm cầu nối MonoBehaviour sở hữu session. Không buộc Manager logic vào component, serialized field hoặc lifecycle của MonoBehaviour scene.
- Manager tồn tại lâu không đồng nghĩa giữ tài nguyên scene mãi: dọn listener/view/pool instances và handle của scene khi rời scene. Không dispose chính Manager khi đổi scene. Config dùng chung thuộc scope session; SceneAssetScope thuộc từng scene.
- Model, data, DTO, parser, signal và scope không phải class điều phối chức năng nên giữ tên theo trách nhiệm (BoardModel, GameConfigParser, SceneAssetScope...), không đổi tất cả thành Manager hoặc singleton.
- Tên chuẩn mới là AddressableManager thay AddressableService, PoolManager thay PoolService. Các quy tắc cũ về ownership/release vẫn giữ nguyên ý nghĩa với tên mới. Chỉ AddressableManager và SceneAssetScope release Addressables handle.
- Quy tắc này ưu tiên khi tài liệu cũ dùng tên Service hoặc mô tả singleton của Manager gắn với scene; object scene và scope vẫn được cleanup theo vòng đời riêng.

## Addressables, Manager và vòng đời scene (cập nhật 2026-11-03)

Áp dụng từ bài 3 trở đi trong NeonBlocks.

- Class điều phối chức năng đặt tên `{Function}Manager`, là class thuần C#, không kế thừa MonoBehaviour, không có static Instance. Manager được đăng ký qua `SessionManager.RegisterSingleton` và resolve qua ServiceLocator.
- Chỉ `AddressableManager` được giải phóng Addressables handle, kể cả khi gọi qua `SceneAssetScope`. Backend Addressables (`UnityAddressableBackend`) là hạ tầng bên trong AddressableManager; gameplay, view và UI không gọi `Addressables.Release`, `ReleaseInstance` hay `UnloadSceneAsync`.
- Mỗi tài nguyên gắn với một `AssetOwnerToken`. Giải phóng theo chủ sở hữu, mỗi handle đúng một lần. Kết quả đến sau khi chủ sở hữu đã kết thúc hoặc yêu cầu đã bị hủy phải được giải phóng ngay.
- Test Addressables dùng `IAddressableBackend` giả (`FakeAddressableBackend`), không gọi Addressables thật trong EditMode.
- Lỗi của AddressableManager và backend ném `AddressableLoadException`. Yêu cầu sai trạng thái (đã dispose, chủ sở hữu đã kết thúc) ném `InvalidOperationException`; hủy ném `OperationCanceledException`.
- Rời scene theo đúng thứ tự: `SceneStopSignal` → `SceneDisposeSignal` → giải phóng `SceneAssetScope`. Signal là cách duy nhất để view nhận sự kiện này; không gọi trực tiếp giữa MonoBehaviour.
- Config dùng chung thuộc phạm vi session; `SceneAssetScope` thuộc từng scene.
- Field readonly áp dụng cho data/config và DTO. Service/manager giữ trạng thái runtime (ví dụ scope hiện tại, token hủy) có thể có field mutable; các field đó không được public và phải được gán ở đúng một nơi.

## Giao tiếp giữa Entry/BootVisuals và bộ điều phối (cập nhật 2026-11-05)

- Object khởi động trong Entry (BootVisuals) và bộ điều phối (EntryLauncher, SessionManager) không kéo `[SerializeField]` tham chiếu tới nhau. Sự kiện từ bộ điều phối tới view đi qua Signal kế thừa, ví dụ `BootVisualsHideSignal`.
- Bộ điều phối đăng ký signal trong `Start` trước mọi `await`, và có `[DefaultExecutionOrder]` thấp hơn view. View resolve signal trong `Start`, gỡ listener trong `OnDestroy`.
- Thuật ngữ: `AddressableService` trong các mục cũ nay là `AddressableManager`. Quy tắc ownership không đổi.
- Không dùng `SerializeField` để nối các view trong cùng scene với bộ điều phối; `SerializeField` chỉ dùng cho phần tử con cùng view (ví dụ Button của chính view), và phải được gán trong scene.
- Nút UI (Button) được giữ bởi script gắn trên Canvas của scene (ví dụ `MainMenuCanvas`, `GameplayCanvas`) dưới dạng serialized field trên chính Canvas. Chỉ script đó thêm/gỡ listener của nút; script khác muốn tác động tới nút phải gửi Signal tới script trên Canvas.
- Chỉ dùng `ServiceLocator.Instance.Resolve<T>()`; không có TryResolve. Signal (lớp kế thừa `BaseSignal`) không được lưu làm field trong class (manager, view, canvas script): resolve tại thời điểm phát hoặc gỡ listener. Signal được tạo lazy ở lần Resolve đầu tiên và luôn trả instance, nên không đăng ký thủ công. Singleton khác signal chỉ được Resolve sau khi đã Register ở đầu session (Entry qua SessionManager); Resolve kiểu chưa đăng ký là lỗi và sẽ ghi log.
- Test EditMode đăng ký signal vào ServiceLocator trong SetUp và gọi `UnregisterAll()` trong TearDown.

## Hằng số và tên scene (cập nhật 2026-11-05)

- Không khai báo hằng số tên scene hoặc key Addressables của scene/asset gameplay trong class (`const string`). Các giá trị này nằm trong mục `scenes` của GameConfig, đi qua `SceneKeysDto` và `SceneKeys`.
- Class đọc tên scene từ `ConfigManager.Config.Scenes` sau khi cấu hình đã được tải. Không đọc config trước khi session đăng ký `ConfigManager`.
- Ngoại lệ duy nhất: `GameConfigParser.AddressKey` ("GameConfig"), vì key này cần để tải chính config.
- Hằng số cấu trúc không đổi theo game (ví dụ kích thước khung tetromino 4×4, số ô 4, danh sách 7 mã khối) được giữ trong code và kiểm tra bằng validator; không đưa vào config.

## Gọi signal trực tiếp (cập nhật 2026-11-05)

- Signal lấy từ `ServiceLocator.Instance.Resolve<T>()` không bao giờ null, vì lớp signal được tạo lazy. Gọi trực tiếp, không kiểm tra null và không lưu vào biến:
  `ServiceLocator.Instance.Resolve<SceneStopSignal>().AddListener(OnSceneStopping);`
  `ServiceLocator.Instance.Resolve<SceneRequestSignal>().Dispatch(sceneKey);`
- Chỉ kiểm tra null cho những tham chiếu có thể thiếu do cấu hình Inspector (ví dụ `startButton`, `backButton`) hoặc do chính class tạo ra.
- Áp dụng cho mọi thao tác với signal: `AddListener`, `RemoveListener`, `Dispatch`.
- Singleton không phải signal (manager đã đăng ký ở đầu session) cũng được resolve trực tiếp và chỉ sau khi đã đăng ký; không kiểm tra null cho chúng.

## EntryLauncher và SessionManager (cập nhật 2026-11-05)

- `EntryLauncher` chỉ được đặt trong scene Entry (scene đầu tiên trong Build Settings, build index 0). Không đặt ở MainMenu, Gameplay hay scene khác. Script kiểm tra build index và ghi lỗi nếu đặt sai.
- `EntryLauncher` không dùng `RequireComponent` và không dùng `GetComponent<SessionManager>()`. Nó lấy session bằng `ServiceLocator.Instance.Resolve<SessionManager>()`.
- `SessionManager` gọi `DontDestroyOnLoad` phải nằm một mình trên GameObject riêng trong Entry. Không đặt chung GameObject đó với bất kỳ MonoBehaviour nào khác, gồm cả `EntryLauncher`; `EntryLauncher` là GameObject riêng trong Entry và chỉ khởi động khi `Resolve<SessionManager>().IsOwner` là true.
- `SessionManager` được truy cập như singleton qua ServiceLocator (`Resolve<SessionManager>()`) ở mọi nơi. Không giữ tham chiếu tới SessionManager trong class khác, không kéo serialized field tới nó.
- Singleton MonoBehaviour đã được giữ `DontDestroyOnLoad` và singleton pure C# đã được `SessionManager.RegisterSingleton` tại Entry được coi là luôn tồn tại sau khi resolve. Không kiểm tra null cho các singleton này.
- Không cache singleton trong field của class. Resolve trực tiếp qua `ServiceLocator.Instance.Resolve<T>()` đúng tại nơi cần dùng; biến cục bộ chỉ được dùng trong phạm vi một method khi giúp thực hiện một thao tác nguyên tử.
- Thứ tự: `SessionManager.Awake` nhận session → `EntryLauncher.Start` (DefaultExecutionOrder -100) đăng ký manager và khởi động.

## Cache Addressables theo scene (cập nhật 2026-11-05)

- Trong một `SceneAssetScope`, mỗi cặp address key và kiểu asset chỉ có một success handle được giữ bởi `AddressableManager`. Các lần tải sau trả về cùng asset từ cache, không gọi `Addressables.LoadAssetAsync` thêm và không tăng số handle.
- Prefab được tải qua cache asset nói trên; view tạo bản sao bằng `UnityEngine.Object.Instantiate` thuần túy. Không dùng `Addressables.InstantiateAsync` cho prefab đã có cached success handle trong scope.
- Khi `SceneAssetScope` kết thúc, `AddressableManager` giải phóng đúng một cached handle của từng asset thuộc scope, sau khi toàn bộ instance thuần túy do scope tạo đã bị destroy. Cache phải tách theo scope/owner, không chia sẻ handle scene này sang scene khác.
- Nếu load thất bại hoặc bị hủy thì không cache kết quả. Các yêu cầu đồng thời cho cùng key/kiểu trong cùng scope phải cùng chờ một request; chỉ cache sau khi request thành công và owner còn hiệu lực.
