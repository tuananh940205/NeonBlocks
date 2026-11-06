# Bài 12 — Menu, popup, save và checklist Unity

Ngày triển khai: 2026-11-06. Runtime và asset đã có trong project. **Prefab và scene wiring chưa chạy setup trong phiên này** để người dùng thao tác/đánh dấu. Tất cả ô dưới đây dành cho người dùng tự tick; không coi sự tồn tại của code là đã nghiệm thu UI.

## Đã triển khai trong project

- LocalSaveManager + storage inject được, JSON version 1, best score, music/sfx volume, mute. Ghi temp/replace giữ bản .bak.
- Save hỏng, version lạ hoặc lỗi đọc trả trạng thái rõ ràng, dùng mặc định và khóa ghi file gốc; update vẫn giữ trong session. Không tự xóa hoặc downgrade save.
- PopupManager: một slot cho mỗi scene, chống double-open, release instance qua SceneAssetScope, bỏ kết quả tải muộn; template cached được giữ theo scope và release khi scene rời.
- PopupHost/PopupCanvas: MainMenu, Pause, Settings, GameOver; Resume/Restart/Home; chặn input dưới modal.
- Settings mở từ Pause rồi Back/Apply quay lại Pause, không tự resume.
- GameOver chỉ ghi điểm ở lần chuyển state; high score không giảm. Âm lượng/mute được lưu, phần âm thanh thực tế thuộc bài 13.
- GameConfig.json thêm ui keys; Entry đăng ký LocalSaveManager/PopupManager. Không cần gán manager bằng Inspector.
- Menu setup và TestRunner trong NeonBlocks > Lesson 12.
- Workflow: [BAI_12_WORKFLOWS.md](BAI_12_WORKFLOWS.md).

## A. Chuẩn bị

- [x] Mở đúng project D:\Repositories\PuzzlePortfolio\NeonBlocks bằng Unity 6000.3.25f1.
- [x] Thoát Play Mode; lưu scene đang chỉnh bằng Ctrl+S.
- [x] Chờ Unity refresh/compile xong; Console không có lỗi compile đỏ.
- [x] Package Addressables, Input System, TextMeshPro/UGUI và Test Framework đã có; không cần cài lại nếu đang dùng bài 11.
- [x] TMP Essentials/font mặc định có sẵn; chữ TMP hiện bình thường.
- [x] Đã có Addressables Settings tại Assets/AddressableAssetsData.
- [x] CanvasScaler hiện có dùng Scale With Screen Size, reference 1080 x 1920; kiểm tra parent SafeArea hiện có.
- [x] Chỉ Entry nằm trong Build Settings; MainMenu/Gameplay dùng Addressables như các bài trước.

## B. Kiểm tra ba asset đã đưa vào project

- [x] Popup: Assets/_Project/Art/UI/Popups/NB-UI-009-PopupPanel.png (1122 x 1402).
- [x] Settings: Assets/_Project/Art/UI/Icons/NB-UI-010-Settings.png (1254 x 1254).
- [x] HighScore: Assets/_Project/Art/UI/Icons/NB-UI-011-HighScore.png (1312 x 1199).
- [ ] Chọn từng ảnh, Inspector: Texture Type Sprite (2D and UI), Sprite Mode Single, Mesh Type Full Rect.
- [x] Filter Bilinear, Wrap Clamp, sRGB bật, Alpha Is Transparency bật, Mip Maps tắt, Max Size 2048.
- [x] Giữ nguồn art-source; không di chuyển/xóa meta PNG đã import.
- [x] Panel Image dùng Simple + Preserve Aspect, không tự stretch hoặc 9-slice khi Border chưa được xác minh.

## C. Chạy setup bằng menu (đường nhanh đề xuất)

- [ ] Sau khi lưu scene và thoát Play, chọn **NeonBlocks > Lesson 12 > Create prefabs and configure scenes**.
- [ ] Console báo “Bài 12: đã dựng prefab, Addressables và scene host”.
- [ ] Có bốn prefab tại Assets/_Project/Prefabs/UI: MainMenuPanel, PausePopup, SettingsPopup, GameOverPopup.
- [ ] Menu setup không ghi đè prefab đã có: nếu đã tự tạo prefab cùng tên, kiểm tra/gán field theo mục D bên dưới.
- [ ] Mở MainMenu.unity và Gameplay.unity, xác nhận Canvas/PopupHost đã được tạo và scene đã lưu.
- [ ] MainMenuCanvas legacy đã disable, StartButton cũ đã inactive để không có hai nút Start.
- [ ] Gameplay BackButton và RestartButton legacy đã inactive; Hold và touch controls bài 10 vẫn giữ.
- [ ] Không chạy lại setup bài 11 sau bài 12 vì setup bài 11 có thể khôi phục UI legacy; nếu cần chạy lại, kiểm tra lại các mục trên.
- [ ] Lưu scene/prefab khi có chỉnh tay bổ sung.

## D. Chi tiết prefab và field Inspector (cũng dùng nếu dựng tay)

Mỗi prefab root có RectTransform stretch toàn parent, Canvas (Override Sorting bật, Order 200), GraphicRaycaster, Image backdrop và PopupCanvas. Root không có EventSystem hoặc AudioListener. Tên root bằng đúng key Addressable.

Child Panel giữa root: 800 x 1000, nền NB-UI-009, Preserve Aspect bật; Title/Details/SaveStatus là TMP child. Label và icon Raycast Target tắt. Backdrop Raycast Target bật. Button child có Image + Button, persistent OnClick để trống vì script tự AddListener.

- [ ] MainMenuPanel/PopupCanvas: Kind = MainMenu.
- [ ] MainMenuPanel: Start Button → Panel/START; Settings Button → Panel/SETTINGS.
- [ ] MainMenuPanel: Details Text → Panel/Details; Status Text → Panel/SaveStatus.
- [ ] PausePopup/PopupCanvas: Kind = Pause.
- [ ] PausePopup: Resume Button → Panel/RESUME; Restart Button → Panel/RESTART.
- [ ] PausePopup: Settings Button → Panel/SETTINGS; Home Button → Panel/HOME.
- [ ] PausePopup: Details Text/Status Text trỏ child TMP tương ứng.
- [ ] SettingsPopup/PopupCanvas: Kind = Settings.
- [ ] SettingsPopup: Music Slider → Panel/MusicSlider; Sfx Slider → Panel/SfxSlider.
- [ ] Hai Slider: Min=0, Max=1, Whole Numbers tắt, Handle Rect → child Handle, Target Graphic → Image của Handle.
- [ ] Slider track Image nhận raycast; handle có kích thước đủ chạm trên màn hình portrait.
- [ ] SettingsPopup: Mute Toggle → Panel/MuteToggle; Target Graphic → Image nền; Graphic → child Checkmark.
- [ ] SettingsPopup: Apply Button → Panel/APPLY; Back Button → Panel/BACK.
- [ ] SettingsPopup: Details Text/Status Text → child TMP tương ứng; các field nút không dùng để None.
- [ ] GameOverPopup/PopupCanvas: Kind = GameOver.
- [ ] GameOverPopup: Restart Button → Panel/RESTART; Home Button → Panel/HOME.
- [ ] GameOverPopup: Details Text → Panel/Details hiển thị SCORE/BEST; Status Text → Panel/SaveStatus.
- [ ] SettingsIcon dùng NB-UI-010; HighScoreIcon dùng NB-UI-011, Preserve Aspect bật, Raycast Target tắt.
- [ ] Các Button chỉ có một đường gửi lệnh: không gắn thêm GameplayTouchButton lên nút popup.
- [ ] Đảm bảo prefab không chứa reference tới MonoBehaviour ở scene khác.

## E. Scene host (kiểm tra sau menu hoặc gán tay)

- [ ] Mỗi scene MainMenu/Gameplay chỉ có đúng một Canvas/PopupHost.
- [ ] PopupHost root: RectTransform stretch toàn Canvas, Canvas + GraphicRaycaster + script PopupHost.
- [ ] PopupHost/Error Text → child LoadError (TMP); kích thước mặc định 900 x 150 tại y=600.
- [ ] PopupHost/Retry Button → child Retry (Image + Button), có TMP Label RETRY; mặc định inactive.
- [ ] Nếu popup host chưa nằm trong vùng safe area, chuyển nó vào parent SafeArea dưới Canvas rồi stretch, kiểm tra lại portrait.
- [ ] Không thả sẵn bốn prefab popup vào scene; runtime tải bằng InstantiateAsync.
- [ ] Gameplay vẫn có đúng một GameplayPresenter và GameplayKeyboardInput.
- [ ] Nút Pause hiện tại vẫn phát GameplayCommand.TogglePause (GameplayTouchButton Action TogglePause).
- [ ] HoldButton vẫn do GameplayCanvas quản lý; HoldPreview chỉ để hiển thị, không cần GameplayTouchButton.
- [ ] Scene đang chơi chỉ có một EventSystem/InputSystemUIInputModule; không tạo thêm trong prefab.
- [ ] Sau chuyển scene hoàn tất chỉ còn một AudioListener.

## F. Addressables và config

- [ ] Addressables Groups có MainMenuPanel.prefab, address chính xác **MainMenuPanel**.
- [ ] PausePopup.prefab có address **PausePopup**.
- [ ] SettingsPopup.prefab có address **SettingsPopup**.
- [ ] GameOverPopup.prefab có address **GameOverPopup**.
- [ ] Config GameConfig.json: ui.mainMenu=MainMenuPanel; ui.pause=PausePopup; ui.settings=SettingsPopup; ui.gameOver=GameOverPopup.
- [ ] GameConfig TextAsset vẫn address **GameConfig**; MainMenu/Gameplay scene key giữ đúng tên scene.
- [ ] Ba PNG là dependency của prefab, không cần đánh Addressable riêng.
- [ ] Để thử trong Editor, chọn Play Mode Script **Use Asset Database (fastest)**.
- [ ] Nếu dùng Existing Build hoặc chạy Android, build Addressables content lại sau khi thêm prefab/config; không dùng bundle cũ.

## G. Playtest bằng nút thật (chưa xác minh trong phiên code)

- [ ] Play từ Entry; chờ MainMenuPanel xuất hiện và BEST hiển thị.
- [ ] Click SETTINGS; kéo Music/Sfx, bật Mute, click APPLY; trở lại Menu.
- [ ] Mở Settings lại, giá trị đã lưu vẫn đúng; chỉnh rồi BACK thì bỏ thay đổi chưa apply.
- [ ] Click START vào Gameplay; Next/Hold/HUD hoạt động.
- [ ] Click/chạm Pause hoặc P/Esc: khối dừng, chỉ một PausePopup.
- [ ] Trong Pause, giữ phím trái/xuống hoặc chạm control bên dưới: khối không đổi.
- [ ] Mở Settings từ Pause, đóng bằng BACK/APPLY vẫn ở Pause; chỉ RESUME mới chơi tiếp.
- [ ] RESUME không lặp DAS/ARR đã giữ trước pause.
- [ ] RESTART từ Pause tạo ván mới: Score/Lines=0, Level đầu, Hold rỗng, Next reset, popup đóng.
- [ ] Stack đến GameOver: chỉ một GameOverPopup; SCORE/BEST đúng, high score không giảm.
- [ ] Click RESTART hoặc R ở GameOver: reset model và HUD, không còn overlay chặn click.
- [ ] Click HOME từ Pause/GameOver về Menu; BEST được cập nhật.
- [ ] Double-click Start/Settings/Home/Restart không tạo hai scene, hai popup hoặc hai hành động.
- [ ] Lặp Start→Pause→Settings→Resume→GameOver→Restart→Home ít nhất 10 vòng.
- [ ] Theo dõi Hierarchy: không tăng số PopupCanvas, EventSystem hoặc AudioListener sau mỗi vòng; popup đóng không còn instance active.
- [ ] Kiểm tra PopupManager.OpenCount: Menu/modal=1, gameplay thường=0, scene rời không giữ slot cũ. Template cached trong scope có thể còn tới lúc rời scene, đó không phải instance rò.
- [ ] Device Simulator/Android portrait: không bị notch che nút, đủ vùng chạm slider/button; chạm backdrop không tác động board.

## H. Lưu dữ liệu và xử lý lỗi

Save runtime: Path.Combine(Application.persistentDataPath, "neonblocks-save.json"). Khi replace thành công có thể có .bak và khi đang ghi có .tmp; không lưu trong Assets.

- [ ] Thoát/chạy lại Play: BEST, Music/Sfx/Mute giữ nguyên.
- [ ] Sao lưu file save thử nghiệm trước khi kiểm tra dữ liệu hỏng.
- [ ] Dùng bản save thử nghiệm sai JSON: UI báo save hỏng, game dùng mặc định và không ghi đè file đó.
- [ ] Dùng version=99: UI báo khác phiên bản, file gốc giữ nguyên.
- [ ] Để phục hồi: thoát Play, sao lưu/đổi tên file lỗi hoặc version lạ, rồi khởi động lại để tạo save mới; không xóa bản duy nhất cần giữ.
- [ ] Giả lỗi ghi bằng test: bản tốt vẫn còn, UI có thông báo thất bại, có thể thử APPLY lại khi backend hết lỗi.
- [ ] Volume/Mute hiện mới được lưu và khôi phục; chưa kỳ vọng nghe SFX/nhạc đổi trước phần audio bài 13.

## I. Test và kết quả thật

- [ ] Chạy NeonBlocks > Lesson 12 > Run EditMode tests hoặc Unity Test Runner EditMode.
- [ ] LocalSaveManagerTests: 9/9 pass.
- [ ] PopupManagerTests: 4/4 pass.
- [ ] Đọc docs/evidence/lesson12-editmode.xml sau khi chạy.
- [ ] Ghi kết quả PlayMode/device vào PROGRESS sau khi hoàn thành mục G/H.

Kết quả đã chạy trong phiên này: Unity EditMode **125/127 pass**, toàn bộ 13 test mới pass. Còn hai lỗi SessionManagerTests từ bài trước (DuplicateSession đọc object đã hủy; ResetSession đăng ký ProbeService cùng concrete type hai lần); không sửa test/SessionManager ngoài phạm vi. Build test project 0 error/12 warning; Editor project 0 error/17 warning tại lượt compile đã chạy. Prefab setup menu, vòng chơi qua nút thật, PlayMode bài 12 và Android **chưa chạy**; không đánh dấu bài 12 hoàn thành nghiệm thu.
