# Core architecture rules

## Unity baseline

- Unity 6.3 LTS, Universal 2D (URP), Android portrait 9:16.

- Một file chỉ định nghĩa một class/object cùng tên file.
- Class logic thuần không kế thừa `MonoBehaviour`. Gameplay rule ở pure C# (`BoardModel`); Unity view chỉ render/input.

## Giao tiếp và ServiceLocator

- Tái sử dụng `Assets/Scripts/Core/ServiceLocator.cs` và `Assets/Scripts/Core/Signals.cs`; không tạo framework thứ hai.
- Đọc API thực tế trước khi dùng; chỉ bổ sung tối thiểu và giữ tương thích lifecycle. Không dùng `GetPrefabs`/`Resources` cho runtime asset NeonBlocks.
- Dùng `ServiceLocator.Instance`; không tạo static instance riêng cho singleton khác.
- Mọi singleton do `SessionManager.RegisterSingleton` sở hữu; manager logic đặt tên `{Function}Manager`, thuần C#, không gắn lifecycle component scene.
- `SessionManager` là MonoBehaviour cầu nối session. Manager sống xuyên scene nhưng cleanup tài nguyên theo scene; unregister/cleanup khi Restart Game hoặc kết thúc session.
- Model, data, DTO, parser, signal và scope giữ tên theo trách nhiệm (ví dụ `BoardModel`, `GameConfigParser`, `SceneAssetScope`), không đổi tất cả thành Manager/singleton.
- Tên chuẩn là `AddressableManager` và `PoolManager`; tài liệu cũ nhắc `AddressableService`/`PoolService` phải hiểu theo tên chuẩn này.
- Không cache singleton hay Signal trong field. Resolve trực tiếp đúng thời điểm dùng; local variable chỉ trong một thao tác nguyên tử.
- Signal kế thừa loại `Signal` phù hợp, được resolve qua ServiceLocator; không tạo signal instance riêng. Add/Remove listener đối xứng theo lifecycle.
- MonoBehaviour gọi MonoBehaviour khác bằng Signal, không bằng serialized reference hay callback trực tiếp. Callback/action chỉ dùng khi phù hợp bên trong cùng view hoặc cho class thuần logic.
- `SerializeField` chỉ tham chiếu child/UI thuộc chính view. Script Canvas là nơi duy nhất add/remove listener của Button đó.
- Signal resolve lazy, không null; không null-check Signal. Singleton không phải Signal chỉ resolve sau khi Entry đã đăng ký.

## Entry và scene key

- `SessionManager` ở GameObject riêng trong Entry và là object duy nhất DontDestroyOnLoad; không chung GameObject với `EntryLauncher`.
- `EntryLauncher` chỉ ở Entry, build index 0; không `RequireComponent` hay `GetComponent<SessionManager>()`.
- Thứ tự: `SessionManager.Awake` nhận session → `EntryLauncher.Start` (`DefaultExecutionOrder(-100)`) đăng ký manager.
- Entry là bootstrap: sau Config và scene đầu tiên tải/active thành công, unload native Entry đúng một lần. Không unload nếu bootstrap lỗi.
- BootVisuals và coordinator Entry không giữ serialized reference tới nhau: dùng Signal kế thừa (ví dụ `BootVisualsHideSignal`). Coordinator đăng ký trước mọi `await`; view resolve trong `Start` và gỡ listener ở `OnDestroy`.
- Tên scene và runtime Addressables key nằm trong `GameConfig.Scenes`, không nằm trong `const string`; ngoại lệ duy nhất là `GameConfigParser.AddressKey` (`GameConfig`).
- Cấu trúc gameplay bất biến như khung tetromino 4x4 vẫn ở code/validator.

## Viết comments bằng tiếng Việt
- Mọi method tạo hoặc sửa trong `Assets/` có XML comment mô tả trách nhiệm; XML nằm trên attribute của member nếu member có attribute.
- Giải thích luồng xử lí trong các `if` block