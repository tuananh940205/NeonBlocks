# Bài 12 — Kế hoạch triển khai (chưa code)

Ngày: 2026-11-06. Nguồn: mục Bài 12 của LESSON_PLAN, AGENTS và các module CORE, ASSETS_AND_EDITOR, UNITY_EDITOR, ADDRESSABLES_AND_LIFECYCLE, DATA_AND_TESTING; đã xem MainMenuCanvas và API SceneAssetScope hiện có.

## Phạm vi
Start/Settings; Pause/Resume/Restart/Home; GameOver/high score; local save có version cho best score, âm lượng/settings. Không thêm âm thanh/art polish của bài 13, leaderboard online hoặc lưu ván đang chơi.

## Asset
- NB-UI-009-PopupPanel-v1.png: nền chung Pause/Settings/GameOver, 1122x1402 RGBA.
- NB-UI-010-Settings-v1.png: icon Settings, 1254x1254 RGBA.
- NB-UI-011-HighScore-v1.png: cúp best score, 1312x1199 RGBA.
- Nguồn tại art-source/NeonBlocks, chờ duyệt, chưa import.
- Dùng lại nền NB-UI-001, button plate NB-UI-002 và Pause NB-UI-006 đang có trong bộ art, kiểm tra trạng thái duyệt trước khi dùng bản chưa tích hợp. Chữ Start/Settings/Resume/Restart/Home/GameOver/Best Score bằng TMP.
- Slider/toggle, vùng tối modal và nút đóng có label được dựng bằng Unity UI, không cần raster riêng.
- Tất cả panel/popup runtime là Addressable prefab; sprite là dependency, không tạo handle Addressable riêng không cần thiết.

## Thứ tự code dự kiến

### 1. Kiểm tra nền tảng và cấu hình
- Rà Entry, MainMenu, Gameplay, Presenter/Canvas, ConfigManager, AddressableManager/SceneAssetScope và kết quả bài 11.
- Giữ nguyên code thay đổi người dùng; xác minh hai test SessionManager cũ đang fail trước khi phân loại baseline.
- Thêm nhóm UI keys vào GameConfig JSON/DTO/domain/parser (một config chung). Key prefab bằng tên root, không chứa direct Unity reference.
- Không suy ra bài 11 đã nghiệm thu Android từ smoke Signal.

### 2. Local save có version
- SaveData/SaveDataDto readonly, schema version 1: bestScore, musicVolume, sfxVolume và setting MVP được xác định rõ (ví dụ mute). Không tự thêm setting gameplay chưa có consumer.
- LocalSaveManager thuần C#, đăng ký qua SessionManager; dependency lưu trữ có interface để test không ghi file thật.
- Newtonsoft.Json; validate score >= 0, volume trong [0,1], version được hỗ trợ.
- LoadAsync trả kết quả phân biệt Missing/Loaded/Corrupt/UnsupportedVersion/IoFailure; file thiếu dùng mặc định; lỗi báo trạng thái fallback rõ ràng cho UI.
- I/O exception chỉ xử lý trong Task service theo ràng buộc exception; không catch chung để nuốt lỗi.
- Ghi file tạm cùng thư mục rồi replace/rename khi hoàn tất, giữ bản hợp lệ trước nếu lỗi. Không ghi đè file version mới hơn hoặc file hỏng âm thầm; quyết định recovery và thông báo được nêu trong UI/hướng dẫn.
- Đường dẫn dưới Application.persistentDataPath; thử lưu khi settings được xác nhận và khi high score tăng. Bài này lưu/đọc volume, phần phát nhạc/SFX thực tế thuộc bài 13.

### 3. Popup lifecycle
- PopupManager thuần C#, đăng ký session; ownership instance gắn đúng scene/scope, cleanup khi SceneStop/SceneDispose.
- Mở async bằng SceneAssetScope.InstantiateAsync; đóng bằng ReleaseInstance qua scope/AddressableManager.
- Chặn double-click và yêu cầu mở trùng trong lúc load; popup đóng hoặc scene rời trước khi load xong không được xuất hiện muộn hoặc rò handle.
- Prefab riêng MainMenuPanel/PausePopup/SettingsPopup/GameOverPopup; root key đồng nhất tên asset.
- Modal backdrop chặn raycast; input gameplay chỉ bị chặn khi gameplay modal đang mở. Không spawn EventSystem/AudioListener riêng trong prefab.

### 4. UI và gameplay commands
- MainMenuCanvas quản lý Start/Settings và hiển thị best score.
- Các Canvas controller trên prefab quản lý Button listener AddListener/RemoveListener; view giao tiếp bằng Signal typed, không giữ serialized cross-view.
- Thêm yêu cầu UI mở/đóng popup, setting đổi, snapshot save/high-score, gameplay session outcome theo type riêng.
- Pause command: chuyển Playing sang Paused rồi mở PausePopup. Resume đóng/release và mở lại input; không tích lũy gravity/DAS trong lúc modal.
- Settings mở từ Menu hoặc Pause; đóng Settings trở lại đúng trạng thái/popup trước, không tự resume ván đã pause.
- Tách yêu cầu restart hợp lệ từ Pause/GameOver khỏi binding keyboard R chỉ dành GameOver nếu giữ tương thích bài 11. Tạo ván mới reset Hold/Next/score/repeat input đúng một lần.
- Home đóng popup, ngừng input và chuyển về Menu theo scene lifecycle hiện có.
- GameOver chỉ xử lý một lần mỗi ván: cập nhật best score bằng max, lưu, mở đúng một GameOverPopup hiển thị score/best/new record.

### 5. Unity setup sau khi asset được duyệt
- Import đúng paths trong ASSET_HANDOFF, giữ PNG gốc và meta asset đã tích hợp.
- Dựng prefab bằng Editor API, CanvasScaler 1080x1920, safe area và TMP. Gán child fields rõ ràng; không sửa YAML scene tay.
- Kiểm tra aspect panel 1122:1402; giữ tỷ lệ hoặc xác minh 9-slice trước khi stretch.
- Nối key Addressables và config; MainMenu/Gameplay chỉ giữ host cần thiết, popup không đặt sẵn trong scene.

### 6. Tests và nghiệm thu
- EditMode save: missing, round-trip, corruption, validation, unsupported version, I/O failure, ghi lỗi không mất save tốt, best score chỉ tăng, volume hợp lệ.
- Popup tests dùng backend giả: mở/đóng, mở trùng, cancel/load muộn, double-close, scene stop/dispose, release đúng một lần.
- GameOver xử lý một lần; Pause→Restart reset model và HUD; Settings đóng không resume sai; nhiều ván không tăng listener.
- PlayMode: Entry→Menu→Start→Pause→Settings→Back→Resume→GameOver→Restart→Home, lặp nhiều vòng; ghi số popup/listener/handle trước và sau.
- Kiểm thử qua Button.onClick/EventSystem và thao tác click/chạm thực tế; không chỉ dispatch Signal trực tiếp. C/Shift và Hold phải regression.
- Device Simulator/Android: safe area, slider/toggle, nút đóng, chạm nền modal không điều khiển khối.
- Bằng chứng build/TestRunner/PlayMode lưu docs/evidence; không đánh dấu pass khi chưa chạy.

## Workflow diagrams phải bàn giao khi triển khai
1. Bootstrap load save → menu/best score.
2. Open/close popup async, chống mở trùng và cancellation.
3. Pause/Settings/Resume/input gate.
4. Restart/Home và scene cleanup.
5. GameOver → cập nhật best → save → popup.
6. Load/save hợp lệ, save hỏng/version mới/I/O lỗi và fallback.
Tạo docs/lessons/BAI_12_WORKFLOWS.md, hướng dẫn BAI_12.md và cập nhật PROGRESS sau triển khai.

## Trạng thái hiện tại
Chỉ hoàn thành đọc tài liệu, tạo ba asset draft và lập kế hoạch. Chưa code runtime, chưa import Unity, chưa tạo prefab hoặc chạy test bài 12. Tên class/key trên là đề xuất, sẽ đối chiếu API hiện tại trước khi triển khai.
