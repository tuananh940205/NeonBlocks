# Bài 1 — Hướng dẫn Unity Editor

Trạng thái: code đã viết; các bước dưới đây cần người dùng thực hiện trong Unity Editor 6000.3.25f1. Chưa chạy được Test Runner/Play Mode trong phiên làm việc này.

## 1. Cài Addressables (chỉ cài package, chưa cấu hình group)
1. Menu **Window → Package Manager**.
2. Chọn dropdown **Packages: Unity Registry** (góc trên trái).
3. Tìm **Addressables**, chọn version **2.10.3** (đang cài trong project), bấm **Install** nếu chưa có.
   - Version 2.10.3 khai báo `unity: 6000.0` trên registry, tương thích Unity 6.3. Đã xác nhận trong `Packages/manifest.json` và `packages-lock.json` của NeonBlocks.
   - Registry công khai còn có bản mới hơn (4.1.x, cũng khai báo `6000.0`), nhưng Package Manager trong Editor chỉ hiển thị 2.10.3. Chưa rõ nguyên nhân; muốn dùng 4.x thì thử **+ → Add package by name** với `com.unity.addressables@4.1.1` rồi kiểm tra Console. Chưa thử nên không coi là đã xác minh.
   - Package kéo theo `com.unity.scriptablebuildpipeline` (2.6.2 trong lock file); để Unity tự cài.
4. Nếu Unity hiện hộp thoại tạo Addressables Settings, có thể bỏ qua; cấu hình Addressables thuộc bài 3.

## 2. Player Settings cho Android portrait
1. Menu **Edit → Project Settings → Player**, chọn tab **Android** (icon robot).
2. Mục **Resolution and Presentation**:
   - **Default Orientation** = `Portrait`.
   - **Allowed Orientations for Auto Rotation**: chỉ bật `Portrait`; tắt `Portrait Upside Down`, `Landscape Right`, `Landscape Left`.
3. Không đổi Package Name ở bước này (Bundle ID hiện là `com.DefaultCompany.urp_2d`); sẽ chỉnh ở bài 15.

## 3. Entry scene
Scene `Assets/_Project/Scenes/Entry.unity` đã tồn tại (Main Camera, Global Light 2D). Cần bổ sung:

| Object | Tạo ở đâu | Component / cấu hình | Giá trị |
|---|---|---|---|
| Main Camera | Có sẵn | Camera: Clear Flags = `Solid Color`, Background | `#0B0B14` (nền tối) |
| SessionManager | Hierarchy → chuột phải (root) → **Create Empty**, đổi tên `SessionManager` | Add Component | `SessionManager` (script `NeonBlocks.SessionManager`) |
| Canvas | Hierarchy → chuột phải → **UI → Canvas** | Canvas Scaler: UI Scale Mode | `Scale With Screen Size` |
| | | Reference Resolution | `1080 × 1920` |
| | | Match | `0.5` |
| | | Render Mode | `Screen Space - Overlay` (mặc định) |
| SafeAreaRoot | Chuột phải **Canvas → Create Empty**, đổi tên `SafeAreaRoot` | Rect Transform: anchor preset | Stretch (giữ Shift+Alt khi chọn preset góc dưới phải) |
| | | Left / Right / Top / Bottom | `0 / 0 / 0 / 0` |
| | | Add Component | `SafeAreaFitter` (script `NeonBlocks.SafeAreaFitter`) |
| EventSystem | Tự sinh cùng Canvas | Giữ nguyên | Input System UI Input Module |

- Không gán field nào thủ công: `SessionManager` và `SafeAreaFitter` không có field tham chiếu trong Inspector.
- Không đặt UI con khác dưới `SafeAreaRoot`; nội dung menu/gameplay tạo bằng Addressables ở bài 3 trở đi.
- Nếu scene đã có object `EntryBootstrap` (từ phiên bản trước), xóa object đó và dùng `SessionManager` thay thế.

## 4. Build Settings
1. Menu **File → Build Profiles** → mục **Scene List** (hoặc **File → Build Settings** nếu hiển thị kiểu cũ).
2. Bấm **Add Open Scenes** khi `Entry` đang mở.
3. Đảm bảo `Entry` là dòng đầu tiên (index 0) và **chỉ còn Entry**. Nếu còn dòng `Assets/Scenes/SampleScene.unity` (đường dẫn cũ, trước khi đổi tên thành Entry) thì bỏ khỏi danh sách.

## 5. Kiểm tra
### 5.1 Edit Mode tests
1. Menu **Window → General → Test Runner**, chọn tab **EditMode**.
2. Bấm **Run All**.
3. Kỳ vọng: **17 test**, tất cả pass:
   - `ServiceLocatorTests` — 4 test.
   - `SessionManagerTests` — 8 test.
   - `SafeAreaCalculatorTests` — 5 test.
   (Ghi chú: số test đã đếm từ mã nguồn, chưa được chạy trong Unity.)
4. Nếu lỗi biên dịch, mở Console và gửi lỗi đầu tiên.

### 5.2 Play Mode (Entry)
1. Mở `Entry.unity`, bấm **Play**.
2. Console: không có lỗi đỏ.
3. Chọn `SessionManager` trong Hierarchy khi đang Play: Inspector hiển thị component; chuyển sang Scene view không làm mất object.
4. Bấm **Stop**, rồi **Play** lần hai: Console không được báo `Already registered` hoặc `SessionManager đã tồn tại`. Điều này xác nhận `OnDestroy` đã kết thúc session khi thoát Play Mode.
5. Tùy chọn kiểm tra Domain Reload tắt: **Edit → Project Settings → Editor → Enter Play Mode Settings**, bật `Enter Play Mode Options`, bỏ `Reload Domain`; lặp bước 4.
6. **Restart Game** chưa có nút trên UI (sẽ gắn ở bài 12). Hàm `SessionManager.RestartGame()` đã có; kiểm tra khi có UI.

### 5.3 Safe area
1. Menu **Window → General → Device Simulator**, chọn thiết bị có tai thỏ/đục lỗ.
2. Bấm **Play**, chọn `SafeAreaRoot` và kiểm tra Rect Transform đã thu theo vùng an toàn. Không có nội dung hiển thị ở bài này nên kiểm tra qua Rect Transform.

## 6. Ghi chú cho bài sau
- `Assets/Scripts/Core/` đã được đưa vào asmdef `NeonBlocks.Core` tại chỗ; file không được di chuyển.
- Singleton sống xuyên suốt game đăng ký qua `SessionManager.RegisterSingleton(...)` trong logic khởi động; xem mục "Session và singleton" trong `PuzzlePortfolio/AGENTS.md`.
- Signal hiện chưa có API phát sự kiện (chưa cần ở bài 1).
- Không có asset mới cho bài 1; không tạo mục trong `docs/ASSET_HANDOFF.md`.
