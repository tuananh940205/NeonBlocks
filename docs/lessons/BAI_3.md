# Bài 3 — Hướng dẫn Unity Editor và kiến trúc Manager

Trạng thái: code và test đã viết và đã biên dịch kiểm tra cú pháp/tham chiếu ngoài Editor; chưa chạy Test Runner hay Play Mode trong Unity. Các bước dưới đây cần người dùng thực hiện.

## 1. Bảng Manager, dependency, owner và vòng đời

| Object | Loại | Dependency (truyền qua constructor) | Owner | Vòng đời |
|---|---|---|---|---|
| `AddressableManager` | Class thuần C#, `IDisposable` | `IAddressableBackend` (`UnityAddressableBackend` trong Entry) | Session | Từ Entry đến Restart Game hoặc thoát Play Mode |
| `ConfigManager` | Class thuần C#, `IDisposable` | `AddressableManager` | Session (token `Session:Config`) | Như trên; giữ TextAsset đến khi session kết thúc |
| `SceneFlowManager` | Class thuần C#, `IDisposable` | `AddressableManager` (Signal được resolve từ ServiceLocator khi phát/gỡ listener, không giữ tham chiếu) | Session | Như trên; mỗi lần đổi scene tạo `SceneAssetScope` mới |
| `SceneAssetScope` | Object thuần C#, không singleton | `AddressableManager` | `SceneFlowManager` (một scope cho mỗi scene) | Từ lúc tải scene đến khi rời scene |
| `EntryLauncher` | MonoBehaviour (glue) | `SessionManager` lấy qua ServiceLocator; GameObject riêng | Entry scene | Chỉ chạy trong scene Entry (build index 0); bị hủy khi Entry unload |
| `SessionManager` | MonoBehaviour (bài 1) | — | Entry scene | `DontDestroyOnLoad`; chỉ hủy khi Restart Game hoặc thoát Play Mode |

Thứ tự đăng ký tại `EntryLauncher.Start`: `AddressableManager` → `ConfigManager` → `SceneFlowManager` (Signal không đăng ký thủ công; được tạo lazy khi resolve). Khi session kết thúc, `SessionManager` dispose theo thứ tự ngược: `SceneFlowManager` → `ConfigManager` → `AddressableManager`.

Thứ tự khởi động hiện tại: tải `GameConfig` và parse xong mới chuyển sang `MainMenu`. Từ bài 9, sau khi MainMenu đã tải thành công và được đặt active, Entry được unload; chỉ SessionManager/manager session đã DontDestroyOnLoad còn sống.

Thứ tự rời scene (do `SceneFlowManager` điều phối): `SceneStopSignal` → `SceneDisposeSignal` → giải phóng `SceneAssetScope` (unload scene và handle). Bước pool sẽ thêm ở bài 9.

## 2. Key Addressables
| Key | Asset | Ghi chú |
|---|---|---|
| `GameConfig` | `Assets/_Project/Configs/GameConfig.json` | Đã đặt từ bài 2 |
| `MainMenu` | `Assets/_Project/Scenes/MainMenu.unity` | Scene mới, tạo ở bước 4. Giá trị lấy từ `scenes.mainMenu` trong GameConfig |
| `Gameplay` | `Assets/_Project/Scenes/Gameplay.unity` | Scene mới, tạo ở bước 4. Giá trị lấy từ `scenes.gameplay` trong GameConfig |
| `Entry` | Build Settings | Tên scene Entry, lấy từ `scenes.entry` trong GameConfig (dùng khi Restart Game) |

Key bằng tên asset, duy nhất. Tên scene không được viết cứng trong class: đọc từ `ConfigManager.Config.Scenes`. Riêng key `GameConfig` là hằng số của parser vì phải biết trước khi config được tải.

## 3. Cài đặt Addressables Settings
1. **Window → Asset Management → Addressables → Groups**. Nếu hộp thoại yêu cầu, bấm **Create Addressables Settings**.
2. **Window → Asset Management → Addressables → Settings** (hoặc tab Settings trong Groups). Chọn **Play Mode Script** = `Use Asset Database (fastest)` để chạy Play Mode không cần build.
3. Đảm bảo `GameConfig.json` đã có Address `GameConfig` (xem bài 2).

## 4. Tạo scene MainMenu và Gameplay
Tạo hai scene từ template **2D (URP)**, lưu tại `Assets/_Project/Scenes/`. Nút và script điều khiển nút nằm cùng trên **Canvas**: mọi tương tác với nút đi qua script gắn trên Canvas, script khác gửi Signal tới Canvas.

### 4.1 MainMenu.unity
| Object | Tạo ở đâu | Component / cấu hình |
|---|---|---|
| Canvas | Hierarchy → **UI → Canvas** (EventSystem tự sinh) | Canvas Scaler: Scale With Screen Size, 1080×1920, Match 0.5. Add Component **MainMenuCanvas** (`NeonBlocks.Views.MainMenuCanvas`) |
| StartButton | Chuột phải Canvas → **UI → Button - TextMeshPro** (hoặc Legacy) đổi tên `StartButton` | Không gắn On Click trong Inspector |
| — | Inspector của **Canvas** → MainMenuCanvas | Field **Start Button** = kéo `StartButton` vào |

Nếu scene đã có object `MainMenuView` (từ phiên bản trước): chuyển component sang Canvas, nối lại **Start Button**, rồi xóa object `MainMenuView`.

### 4.2 Gameplay.unity
| Object | Tạo ở đâu | Component / cấu hình |
|---|---|---|
| Canvas | Hierarchy → **UI → Canvas** | Như MainMenu. Add Component **GameplayCanvas** (`NeonBlocks.Views.GameplayCanvas`) |
| BackButton | Chuột phải Canvas → **UI → Button** đổi tên `BackButton` | Không gắn On Click |
| — | Inspector của **Canvas** → GameplayCanvas | Field **Back Button** = kéo `BackButton` vào |

Nếu scene đã có object `GameplayView` (từ phiên bản trước): chuyển component sang Canvas, nối lại **Back Button**, rồi xóa object `GameplayView`.

Không đặt sẵn gameplay trong scene; bài 9 trở đi mới có nội dung thật.

## 5. Cấu hình Entry.unity
Scene `Entry.unity` có `BootVisuals` (camera, light, canvas khởi động), `Session` và `EntryLauncher`. Sau khi tải scene đầu thành công, bài 9 sẽ unload toàn bộ Entry; không coi việc chỉ ẩn BootVisuals là trạng thái cuối.

1. Chọn object **Session**: chỉ giữ **SessionManager**; không thêm component nào khác và không đặt nó dưới BootVisuals.
2. Tạo object root riêng **EntryLauncher**, add component **EntryLauncher** (`NeonBlocks.EntryLauncher`). Không cần gán field nào.
3. Chọn object **BootVisuals** (gốc chứa Main Camera, Global Light 2D, Canvas và EventSystem nếu có). `BootVisualsView` chỉ là tương thích tạm với bản bài 3; bài 9 sẽ bỏ/không dùng nó vì unload Entry sẽ tự hủy toàn bộ object này.
4. Không đặt MainMenu hay Gameplay vào Entry; hai scene đó tải bằng Addressables.

Thứ tự khởi động: `SessionManager.Awake` nhận owner trước, sau đó `EntryLauncher.Start` đăng ký manager và tải config/scene đầu. Bài 9 thêm bước đặt MainMenu active rồi unload Entry đúng một lần.

## 6. Build Settings
1. **File → Build Profiles** → **Scene List**.
2. Chỉ giữ `Entry` (index 0). Nếu còn `MainMenu` hoặc `Gameplay` trong danh sách, bỏ chúng; chúng phải là Addressables.

## 7. Kiểm tra

### 7.1 Edit Mode tests
**Window → General → Test Runner → EditMode → Run All**. Có 73 test tổng cộng; các test mới của bài 3:
- `AddressableManagerTests` — 12 test: tải và giải phóng đúng một lần, lỗi tải, key rỗng, sai kiểu, hủy khi đang tải, chủ sở hữu kết thúc khi đang tải, instance, scene, Dispose.
- `SceneFlowManagerTests` — 5 test: scene đầu tiên, thứ tự stop → dispose → unload → load, tải lỗi không để lại scene, yêu cầu mới hủy yêu cầu cũ, Dispose.
- `ConfigManagerTests` — 5 test: JSON hợp lệ, truy cập trước khi tải, JSON sai, thiếu asset, Dispose.

Số lượng này đếm từ mã nguồn; chưa chạy trong Unity.

### 7.2 Play Mode
1. Mở `Entry.unity`, bấm **Play**.
2. Trước bài 9: BootVisuals có thể chỉ bị tắt theo implementation hiện có. Sau bài 9: Hierarchy không còn scene `Entry` sau khi MainMenu tải; chỉ GameObject DontDestroyOnLoad chứa SessionManager còn sống, Console không có lỗi đỏ.
3. Bấm **Start** trong MainMenu → Gameplay hiện lên, MainMenu biến mất.
4. Bấm **Back** trong Gameplay → MainMenu hiện lại.
5. Bấm **Stop**, rồi **Play** lần hai: không có `Already registered`, `not registered` hoặc `SessionManager đã tồn tại`.
6. Nếu config thiếu (ví dụ đổi Address của GameConfig), Console báo `Khởi động Entry thất bại: AddressableLoadException` và không chuyển scene.

## 8. Ghi chú
- Restart Game chưa có nút UI; `SessionManager.RestartGame()` đã có và sẽ gắn ở bài 12.
- Signal được gửi từ view bằng `Dispatch`; `Signals.cs` đã được bổ sung `Dispatch()` tối thiểu cho `Signal` và `Signal<T>`.
- Không có asset ảnh mới; không tạo mục trong `docs/ASSET_HANDOFF.md`.
