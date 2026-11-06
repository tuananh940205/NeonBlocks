# Bài 11 — Hold và hoàn thiện HUD

## Kết quả triển khai

- Hold một lần mỗi lượt khối, mở lại sau lock và spawn thành công.
- Hold lần đầu tiêu thụ một khối Next; swap với Hold đã có không tiêu thụ Next.
- Khối quay về spawn position/orientation; reset gravity, lock timer, reset count; không khóa khối bỏ ra và không cộng điểm Hold.
- Spawn fail khi Hold hoặc swap chuyển GameOver, không còn active piece.
- Keyboard: C hoặc **Left Shift** để Hold. Nút Hold thật thuộc GameplayCanvas; chỉ Canvas đăng ký/gỡ Button listener.
- HUD TMP hiển thị Score/Level/Lines/State; preview hình khối Hold và 5 Next ở hướng spawn, căn theo bounds của từng shape.
- Snapshot typed có HeldPieceId và CanHold; request snapshot qua Signal riêng khi HUD bật. Không serialized cross-view.
- R hoặc Restart sau GameOver tạo ván mới, reset toàn bộ model, queue, Hold và HUD.
- Canvas/HUD dừng phản hồi khi SceneStop chỉ đúng scene; gỡ listener trong OnDisable.

## Asset đã được duyệt và import

| Asset | Đường dẫn Unity | Import |
| --- | --- | --- |
| Nút Hold | Assets/_Project/Art/UI/Controls/Touch/NB-UI-007-TouchHold.png | Sprite, Single, Full Rect, Bilinear, Clamp, sRGB, alpha |
| Khung preview | Assets/_Project/Art/UI/Gameplay/NB-UI-008-PreviewPanel.png | Như trên |

Hai PNG RGBA gốc 1254 x 1254; max texture 2048, compression Normal, pivot Center, PPU 100, không thêm Addressable riêng vì là dependency của Gameplay scene. Nguồn ở art-source được giữ nguyên; meta do Unity tạo.

## Setup đã thực hiện trong Unity

Menu **NeonBlocks > Lesson 11 > Import and configure** đã được chạy trong Editor 6000.3.25f1. Công cụ dùng TextureImporter, SerializedObject và EditorSceneManager, không chỉnh YAML scene bằng tay. Chạy lại không nhân object hoặc ghi đè PNG/meta đã có; dừng nếu đang Play hoặc còn scene chưa lưu.

Hierarchy thực tế:
```text
Gameplay
  Canvas [GameplayCanvas]
    ... parent hiện có chứa BoardRoot và TouchControls
      BoardRoot [Image] 700 x 1400, centered (0,20)
      Lesson11HUD [GameplayHudView]
        Score [TextMeshProUGUI]
        Level [TextMeshProUGUI]
        Lines [TextMeshProUGUI]
        State [TextMeshProUGUI]
        HoldLabel, NextLabel, HoldHint [TextMeshProUGUI]
        HoldPreview [Image]
        Next0 ... Next4 [Image]
        HoldButton [Image, Button]
        RestartButton [Image, Button]
          Label [TextMeshProUGUI]
```

- CanvasScaler hiện có giữ reference resolution 1080 x 1920.
- GameplayCanvas: Hold Button = child HoldButton; Restart Button = child RestartButton; Back Button giữ tham chiếu cũ.
- GameplayHudView: Score/Level/Lines/State Text trỏ đúng child TMP; Hold Root = HoldPreview; Next Roots có 5 phần tử Next0..Next4 theo thứ tự.
- Cell Sprite trên HUD để trống: preview dùng Image vuông tint màu theo bảy tetromino, đồng bộ board placeholder hiện tại. Không tự tích hợp sprite cell còn chờ duyệt.
- HoldPreview/Next mỗi slot 160 x 160; giữ Preserve Aspect cho panel. Hold x=-440, y=410; Next x=440, y=410-185*i.
- HoldButton 160 x 160 tại (-440,210), Preserve Aspect bật; không thêm GameplayTouchButton lên nút này để tránh phát hai lệnh.
- RestartButton 300 x 65 tại (0,-760), chỉ interactable khi GameOver; Hold chỉ interactable khi CanHold.
- Score/Level/Lines ở phần trên, chữ/số TMP; text debug cũ của BoardView đã tắt và bỏ gán.
- BoardRoot Image không nhận raycast; layout thu nhỏ để chừa hai cột preview; giữ calibration lưới NB-UI-003 v1 như bài 9.
- Nút và input cảm ứng bài 10 được giữ; TouchControlAction.Hold được bổ sung cuối enum cho các scene muốn dùng vùng chạm riêng.

## Workflow diagrams

Xem [BAI_11_WORKFLOWS.md](BAI_11_WORKFLOWS.md): sáu sơ đồ Mermaid cho Hold, input/Signal, HUD, restart/lifecycle, import và validation.

## Kiểm tra thật đã chạy

1. **Unity EditMode TestRunner:** 114 tests, **112 pass / 2 fail**. HoldTests mới **9/9 pass**: lần đầu, lần hai, lock/swap/queue/spawn orientation, ngoài Playing, gravity timer, spawn fail lần đầu và swap, ván mới, snapshot copy.
2. Hai lỗi thuộc SessionManagerTests cũ, ngoài phần thay đổi bài 11:
   - DuplicateSession_IsNotOwner_AndDestroyingItKeepsOwner: test đọc gameObject của duplicate đã bị DestroyImmediate.
   - ResetSession_DisposesSingletonsInReverseOrderAndUnregisters: test đăng ký hai ProbeService cùng concrete type, trái quy tắc singleton hiện có.
   Chưa sửa các test hoặc SessionManager trong bài này.
3. **Unity PlayMode smoke thực tế:** Entry → MainMenu → Gameplay qua SceneRequestSignal; Hold/second Hold/lock/swap/pause/GameOver/restart pass; preview Hold 4 cell rồi rỗng sau restart; score TMP reset; chỉ một EventSystem và AudioListener sau transition. Đây là kiểm tra Signal/runtime thật, chưa mô phỏng pointer vật lý Android.
4. **Build cuối:** NeonBlocks.Tests.EditMode.csproj 0 error / 12 warning; NeonBlocks.Editor.csproj 0 error / 13 warning, các warning dependency Unity hiện có.
5. Đã xem layout trong Unity, chỉnh Restart khỏi cụm touch phía dưới và lưu scene.

Evidence:
- [lesson11-editmode.xml](../evidence/lesson11-editmode.xml)
- [lesson11-playmode.txt](../evidence/lesson11-playmode.txt)
- [lesson11-setup.txt](../evidence/lesson11-setup.txt)

## Cách chạy lại và kiểm tra tay

1. Mở Entry scene, Play, bấm Start vào Gameplay.
2. Bấm C/Left Shift hoặc Hold: khối cũ xuất hiện trong Hold, active lấy từ Next; nhấn Hold lần hai không đổi.
3. Space để lock; Hold mở lại. Di chuyển/xoay khối rồi swap: khối được trả về hướng/vị trí spawn.
4. Pause chặn Hold. Khi GameOver, R hoặc Restart: Score=0, Lines=0, Level=startLevel, Hold trống, Next được tạo mới.
5. Mở **NeonBlocks > Lesson 11 > Run EditMode tests** để lưu XML; **Run PlayMode smoke** để chạy các bước tự động và lưu text evidence. Thoát Play và lưu scene trước khi chạy menu.
6. Kiểm tra Device Simulator/Android riêng để xác minh safe area và chạm thật; chưa có bằng chứng thiết bị trong bài này.

## Giới hạn

Bài 11 đã triển khai và được kiểm tra phần Hold/HUD trong Unity; trạng thái nghiệm thu toàn dự án vẫn chờ xử lý hai test SessionManager cũ và kiểm tra touch thiết bị. Không triển khai menu/popup/save của bài 12. Khung board v1 vẫn có sai lệch lưới raster đã ghi trong ASSET_HANDOFF, không sửa asset đó trong bài 11.
