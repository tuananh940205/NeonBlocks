# NeonBlocks — Asset handoff

Asset được tạo hoặc nhận phải nằm tại `art-source/` cho đến khi người dùng duyệt. Chỉ sau khi duyệt mới đưa bản đã duyệt vào `Assets/_Project/Art/`, giữ nguyên đường dẫn và `.meta` khi đã được tích hợp.

## NB-UI-001 — Neon background

- **Mục đích:** nền portrait chung cho MainMenu và Gameplay placeholder; vùng giữa tối để TextMeshPro, board và HUD dễ đọc.
- **Nguồn:** `art-source/NeonBlocks/NB-UI-001-NeonBackground-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Backgrounds/NB-UI-001-NeonBackground.png`
- **Kích thước / alpha:** 1024 × 1536 px, RGB không alpha.
- **Import đề xuất:** Texture Type `Sprite (2D and UI)`; Mesh Type `Full Rect`; Filter Mode `Bilinear`; Compression `Normal Quality`; sRGB bật. Canvas Image stretch toàn màn hình, Preserve Aspect bật.
- **Trạng thái:** Chờ duyệt. Không phải Addressable riêng nếu chỉ là dependency của MainMenu/Gameplay scene.

## NB-GAME-001 — Neon cell block

- **Mục đích:** sprite ô khối dùng cho settled/active/ghost/next của BoardView bài 9; tint màu ở runtime theo loại khối.
- **Nguồn:** `art-source/NeonBlocks/NB-GAME-001-NeonCell-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/Gameplay/NB-GAME-001-NeonCell.png`
- **Kích thước / alpha:** 1263 × 1245 px, RGBA trong suốt.
- **Yêu cầu trước tích hợp:** cần xác nhận/crop về canvas vuông với viền glow không bị cắt; chỉ dùng nếu tint vẫn giữ đủ độ phân biệt của bảy khối. Nếu không, tạo lại sprite ô đơn sắc sạch hơn.
- **Import đề xuất:** Texture Type `Sprite (2D and UI)`; Single; Pivot `Center`; Mesh Type `Full Rect`; Filter Mode `Bilinear`; Compression `Normal Quality`; Sprite Atlas sau khi style ổn định.
- **Trạng thái:** Chờ duyệt và crop; chưa gắn vào PoolManager/BoardView.

## NB-UI-002 — Neon button plate

- **Mục đích:** nền cho Start, Back, Pause và Restart; chữ giữ bằng TextMeshPro để bảo đảm đọc đúng.
- **Nguồn:** `art-source/NeonBlocks/NB-UI-002-NeonButton-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Controls/NB-UI-002-NeonButton.png`
- **Kích thước / alpha:** 2172 × 724 px, RGBA trong suốt.
- **Yêu cầu trước tích hợp:** cần xác nhận vùng border cho Sprite Editor 9-slice; không stretch trực tiếp toàn ảnh vì glow có thể méo.
- **Import đề xuất:** Texture Type `Sprite (2D and UI)`; Sprite Mode `Single`; Mesh Type `Full Rect`; Filter Mode `Bilinear`; Compression `Normal Quality`; cấu hình Border trong Sprite Editor sau khi duyệt.
- **Trạng thái:** Chờ duyệt; chưa thay Button mặc định trong MainMenu/Gameplay.

## NB-UI-003 - Board frame

- **Mục đích:** khung và lưới nền cho playfield 10 cột x 20 hàng của `BoardView` ở bài 9. Chỉ là dependency hình ảnh của Gameplay scene, không tạo Addressable riêng.
- **Nguồn:** `art-source/NeonBlocks/NB-UI-003-BoardFrame-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Gameplay/NB-UI-003-BoardFrame.png`
- **Kích thước / alpha:** 864 x 1728 px, RGBA trong suốt bên ngoài khung; tỷ lệ portrait 1:2.
- **Yêu cầu trước tích hợp:** xác nhận viền glow trên/dưới được chấp nhận. Sprite có lưới 10 x 20 sẵn trong nền, vì vậy ô gameplay đặt phía trên phải canh đúng với lưới, không hiển thị lưới thứ hai.
- **Import đề xuất:** Texture Type `Sprite (2D and UI)`; Sprite Mode `Single`; Mesh Type `Full Rect`; Filter Mode `Bilinear`; Wrap Mode `Clamp`; Compression `Normal Quality`; sRGB bật. Canvas Image bật `Preserve Aspect`; kéo giãn theo tỷ lệ nguyên bản.
- **Trạng thái:** Đã được dùng tạm trong Gameplay để kiểm tra layout. Cần bản `v2` trước khi chốt tích hợp: lưới bên trong phải là **đúng 10 cột x 20 hàng**, ô vuông cùng kích thước, không có dòng/cột thừa, và bounds playfield phải được bàn giao theo pixel. Bản v1 có lưới raster không khớp hoàn toàn board logic nên BoardView đang căn tile theo đáy bằng placeholder calibration.

## NB-UI-004 - Touch directional control

- **Mục đích:** cụm ba vùng chạm Left / Soft Drop / Right cho Android portrait ở bài 10.
- **Nguồn atlas:** `art-source/NeonBlocks/NB-UI-004-TouchDirectional-v1.png`; `v2` là bản crop canvas atlas.
- **Trạng thái atlas:** Không dùng trực tiếp trong UI; đã tách các vùng chạm thành asset riêng bên dưới.
- **Alpha / bố cục:** RGBA nền trong suốt; một sprite ngang gồm đúng ba vùng chạm bằng nhau, theo thứ tự trái / xuống / phải.
- **Canvas v2:** đã cắt sát bounds pixel nhìn thấy của neon control, không giữ transparent padding bao ngoài.
- **Yêu cầu trước tích hợp:** cắt hoặc xác nhận pixel bounds của từng vùng chạm để tạo ba `Button`/vùng Pointer riêng; tuyệt đối không dùng toàn sprite như một button duy nhất.
- **Import đề xuất:** Texture Type `Sprite (2D and UI)`; Single; Mesh Type `Full Rect`; Filter Mode `Bilinear`; Wrap Mode `Clamp`; Compression `Normal Quality`; chỉ tách sprite khi các bounds đã được chốt.
- **Trạng thái:** Thay bằng các PNG riêng, chờ duyệt.

### NB-UI-004A - Touch Move Left

- **Nguồn:** `art-source/NeonBlocks/NB-UI-004-TouchMoveLeft-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Controls/Touch/NB-UI-004-TouchMoveLeft.png`
- **Kích thước / alpha:** 534 x 445 px; RGBA, crop theo alpha lớn hơn ngưỡng glow để không còn transparent padding bên ngoài.
- **Import đề xuất:** Sprite (2D and UI), Single, Full Rect, Bilinear, Clamp, Normal Quality.
- **Trạng thái:** Chờ duyệt; gán cho Image có `GameplayTouchButton.Action = MoveLeft`.

### NB-UI-004B - Touch Soft Drop

- **Nguồn:** `art-source/NeonBlocks/NB-UI-004-TouchSoftDrop-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Controls/Touch/NB-UI-004-TouchSoftDrop.png`
- **Kích thước / alpha:** 724 x 445 px; RGBA, crop theo alpha lớn hơn ngưỡng glow để không còn transparent padding bên ngoài.
- **Import đề xuất:** Sprite (2D and UI), Single, Full Rect, Bilinear, Clamp, Normal Quality.
- **Trạng thái:** Chờ duyệt; gán cho Image có `GameplayTouchButton.Action = SoftDrop`.

### NB-UI-004C - Touch Move Right

- **Nguồn:** `art-source/NeonBlocks/NB-UI-004-TouchMoveRight-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Controls/Touch/NB-UI-004-TouchMoveRight.png`
- **Kích thước / alpha:** 535 x 445 px; RGBA, crop theo alpha lớn hơn ngưỡng glow để không còn transparent padding bên ngoài.
- **Import đề xuất:** Sprite (2D and UI), Single, Full Rect, Bilinear, Clamp, Normal Quality.
- **Trạng thái:** Chờ duyệt; gán cho Image có `GameplayTouchButton.Action = MoveRight`.

## NB-UI-005 - Touch action controls

- **Mục đích:** cụm action Rotate CCW và Hard Drop cho Android portrait ở bài 10.
- **Nguồn atlas:** `art-source/NeonBlocks/NB-UI-005-TouchActions-v1.png`; `v2` là bản crop canvas atlas.
- **Trạng thái atlas:** Không dùng trực tiếp trong UI; đã tách hai button thành asset riêng bên dưới.
- **Alpha / bố cục:** RGBA nền trong suốt; hai nút tròn bằng nhau theo thứ tự Rotate CCW / Hard Drop.
- **Canvas v2:** đã cắt sát bounds pixel nhìn thấy của hai button, không giữ transparent padding bao ngoài.
- **Yêu cầu trước tích hợp:** xác nhận crop hai nút thành hai sprite riêng hoặc prefab button riêng; không stretch cả atlas lên một button.
- **Import đề xuất:** Texture Type `Sprite (2D and UI)`; Single trước khi crop; Mesh Type `Full Rect`; Filter Mode `Bilinear`; Wrap Mode `Clamp`; Compression `Normal Quality`; SpriteAtlas sau khi UI ổn định.
- **Trạng thái:** Thay bằng các PNG riêng, chờ duyệt.

### NB-UI-005A - Touch Rotate CCW

- **Nguồn:** `art-source/NeonBlocks/NB-UI-005-TouchRotateCCW-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Controls/Touch/NB-UI-005-TouchRotateCCW.png`
- **Kích thước / alpha:** 755 x 757 px; RGBA, crop theo alpha lớn hơn ngưỡng glow để không còn transparent padding bên ngoài.
- **Import đề xuất:** Sprite (2D and UI), Single, Full Rect, Bilinear, Clamp, Normal Quality.
- **Trạng thái:** Chờ duyệt; gán cho Image có `GameplayTouchButton.Action = RotateCounterClockwise`.

### NB-UI-005B - Touch Hard Drop

- **Nguồn:** `art-source/NeonBlocks/NB-UI-005-TouchHardDrop-v1.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Controls/Touch/NB-UI-005-TouchHardDrop.png`
- **Kích thước / alpha:** 757 x 756 px; RGBA, crop theo alpha lớn hơn ngưỡng glow để không còn transparent padding bên ngoài.
- **Import đề xuất:** Sprite (2D and UI), Single, Full Rect, Bilinear, Clamp, Normal Quality.
- **Trạng thái:** Chờ duyệt; gán cho Image có `GameplayTouchButton.Action = HardDrop`.

## Asset chưa cần tạo

- **Logo/wordmark NeonBlocks:** dùng TextMeshPro trong giai đoạn placeholder để tên game luôn chính xác và dễ đổi kiểu chữ. Chỉ tạo raster logo khi đã chốt typography/branding ở bài 13.
- **SFX, nhạc, icon cảm ứng và art tetromino riêng:** chưa cần cho scene hiện tại; đánh giá lại ở bài 10 và bài 13.

## NB-UI-006 - Pause

- **Nguồn:** `art-source/NeonBlocks/NB-UI-006-Pause-v2.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Controls/Touch/NB-UI-006-Pause.png`
- **Mục đích:** biểu tượng tạm dừng, hai thanh neon cyan/trắng.
- **Kích thước / alpha:** 1266 x 1242 px, RGBA; bản v2 giảm khoảng trống bao ngoài so với bản đầu.
- **Import đề xuất:** Sprite (2D and UI), Single, Full Rect, Bilinear, Clamp.
- **Trạng thái:** Chờ duyệt; chưa tích hợp Unity.

## NB-UI-005C - Touch Rotate CW

- **Nguồn:** `art-source/NeonBlocks/NB-UI-005C-TouchRotateCW-v2.png`
- **Đích sau duyệt:** `Assets/_Project/Art/UI/Controls/Touch/NB-UI-005C-TouchRotateCW.png`
- **Mục đích:** biểu tượng xoay theo chiều kim đồng hồ.
- **Kích thước / alpha:** 1254 x 1254 px, RGBA; bản v2 giảm glow rộng và khoảng trống bao ngoài.
- **Import đề xuất:** Sprite (2D and UI), Single, Full Rect, Bilinear, Clamp.
- **Trạng thái:** Chờ duyệt; chưa tích hợp Unity.
