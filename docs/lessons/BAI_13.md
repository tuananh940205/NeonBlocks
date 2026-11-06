# Bài 13 — Mỹ thuật neon và âm thanh

Ngày triển khai: 2026-11-06. Asset đã được người dùng duyệt trong chat trước khi import.

## Đã triển khai
- 7 cell có màu và ký hiệu riêng, ghost viền rỗng, nền tối. Board giữ PieceId sau lock/clear để settled không mất màu. Hold/Next dùng cùng CellTheme.
- Grid dựng theo BoardSettings, không dùng raster frame/calibration cũ. Pool Image tái sử dụng; active sáng đầy đủ, settled 72%, ghost alpha45% và bỏ phần trùng active. Clear fade grid160ms, không flash toàn màn.
- SafeGameplayLayout fit thiết kế1080x1920 vào Screen.safeArea bằng scale đồng nhất; background cover riêng.
- AudioManager thuần C#, AudioHost adapter, hai music source crossfade400ms unscaled, tám SFX voices. Pause duck35%, GameOver15%. Rotate throttle40ms và voice priority.
- Feedback queue có session/sequence, drain một lần sau commit; Clear thay Drop/Lock trong một impact, GameOver one-shot. Snapshot request không phát lại cue.
- Settings preview khi kéo; Back rollback; Apply tiếp tục lưu qua LocalSaveManager của bài12. Mute giữ giá trị slider.
- AudioBank Addressable key từ GameConfig.audioBank, parent dưới SessionManager. Clip/theme là prefab/scene dependency, không đánh Addressable riêng.
- Source volume fallback đã hoạt động. AudioMixer chưa được tạo tự động; có field và hỗ trợ exposed parameter nếu gán theo phần tùy chọn bên dưới.

## Menu setup
`NeonBlocks > Lesson 13 > Import and configure`

Đã chạy trong Unity 6000.3.25f1. Bạn không cần kéo thả lại các field đã setup.

Menu từ chối chạy khi đang Play hoặc scene chưa lưu. Chạy lại cập nhật importer/theme/clip/scene, giữ GUID asset và mixer references; có thể ghi lại thay đổi thủ công tại các field do menu quản lý. Menu khôi phục danh sách scene đang mở sau khi hoàn tất.

| Artifact | Đường dẫn |
| --- | --- |
| Cell theme | Assets/_Project/Art/Gameplay/CellTheme.asset |
| Cell sprites | Assets/_Project/Art/Gameplay/Cells/ |
| Atlas | Assets/_Project/Art/Gameplay/GameplayCells.spriteatlas |
| Background | Assets/_Project/Art/UI/Backgrounds/NB-UI-012-QuietBackground.png |
| Bank prefab | Assets/_Project/Prefabs/Audio/AudioBank.prefab |
| Music / SFX | Assets/_Project/Audio/Music và Audio/SFX |
| Scene | Assets/_Project/Scenes/MainMenu.unity và Gameplay.unity |
| Config | Assets/_Project/Configs/GameConfig.json |

Sprite importer Single/FullRect, mipmapoff, sRGB, Bilinear, Clamp; cellmax256, background2048. Atlas gồm7cell+ghost, padding4, khôngrotate/tightpacking. SFX monoPCM/DecompressOnLoad; music stereoVorbis/Streaming. Không tạo AudioListener trong bank.

## Checklist thao tác còn lại trên Editor
- [ ] Mở Entry rồi Play; kiểm tra MainMenu vào Gameplay, Hold/Next, active/ghost/settled.
- [ ] Nghe5 SFX và 2 loop trên loa/tai nghe; chỉnh mức cân bằng cảm nhận nếu cần. Kiểm thử tự động chỉ xác nhận wiring/state, không thay thế việc nghe.
- [ ] Vào Settings, kéo Music/SFX, thử Mute; Back phục hồi giá trị cũ, Apply rồi thoát/vào Play để kiểm tra persist.
- [ ] Trong Game view, thử9:16,9:19.5,9:20 hoặc Device Simulator có notch. Kiểm tra chữ/control và khoảng trống board, không chỉ nhìn nền.
- [ ] Kiểm tra cell ở24/32/48px, ký hiệu dễ nhận ra và ghost không nhầm settled.
- [ ] Kiểm tra trên Android thật khi có thiết bị; chưa có build/device nghiệm thu trong bài này.

### AudioMixer tùy chọn
Game có âm thanh ngay cả khi bỏ qua mục này. Thực hiện khi cần bus mixer riêng:
- [ ] Thoát Play. Tạo Audio Mixer tại Assets/_Project/Audio/NeonBlocksMixer.mixer.
- [ ] Trong Mixer tạo hai group con dưới Master: Music và SFX.
- [ ] Expose Volume của Music, đổi exposed parameter thành MusicVolumeDb; SFX thành SfxVolumeDb (đúng chữ hoa/thường).
- [ ] Mở prefab AudioBank; trên AudioHost gán Mixer, Music Group và Sfx Group. Save prefab.
- [ ] Play lại từ Entry, kéo từng slider kiểm tra bus đúng. Code chuyển linear gain sang20log10, mute floor-80dB.
Không cần gán WAV thành Addressable riêng hoặc thêm bank vào từng scene.

## Kiểm tra và bằng chứng
- Unity EditMode136 tests: 134 pass, 2 fail hiện hữu ở SessionManagerTests;9 Lesson13Tests đều pass.
- Hai lỗi baseline: đọc duplicate đã DestroyImmediate; đăng ký ProbeService cùng concrete type hai lần. Không sửa ngoài phạm vi.
- Build tests và Editor thành công0 error;12/17 warning dependency Unity hiện hữu.
- Evidence: docs/evidence/lesson13-setup.txt, lesson13-editmode.xml. Smoke đã pass Entry → Menu → Gameplay → Pause → Settings preview → Back → Home; bank, sprite, gain và ownership đúng. Evidence: lesson13-playmode.txt.
- Menu chạy lại: NeonBlocks > Lesson13 > Run EditMode tests; Run PlayMode smoke.

Nguồn/quyền asset và thông số WAV xem ASSET_HANDOFF.md. Diagram sáu luồng xem BAI_13_WORKFLOWS.md. Trạng thái: đã tích hợp, chờ nghiệm thu nghe/thiết bị và các mục checklist.

Cảnh báo còn biết: EventSystem/AudioListener trùng tạm khi scene Additive overlap từ bài 12; sau chuyển cảnh chỉ còn mỗi loại một instance. Editor cũng có warning serialized m_Image reference mismatch hiện hữu; chưa xử lý ngoài phạm vi bài 13.
