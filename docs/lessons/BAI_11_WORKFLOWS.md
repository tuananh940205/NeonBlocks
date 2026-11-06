# Bài 11 — Workflow diagrams

Các sơ đồ Mermaid sau mô tả những luồng tạo hoặc thay đổi trong Bài 11. Xem bằng Markdown preview hỗ trợ Mermaid.

## 1. Hold lần đầu và swap
```mermaid
flowchart TD
    A["Yêu cầu Hold"] --> B{"Playing, có active và chưa Hold?"}
    B -->|Không| C["Bỏ qua; giữ board, queue và timer"]
    B -->|Có| D{"Hold đang rỗng?"}
    D -->|Có| E["Incoming = TakeNext; bù queue"]
    D -->|Không| F["Incoming = khối đang giữ; queue không đổi"]
    E --> G["Lưu definition active vào Hold"]
    F --> G
    G --> H["Đánh dấu đã Hold; reset gravity và lock timer"]
    H --> I["Bỏ active cũ, không lock hoặc cộng điểm"]
    I --> J["Spawn incoming tại origin cấu hình, orientation Spawn"]
    J --> K{"Fit board?"}
    K -->|Không| L["GameOver; không còn active"]
    K -->|Có| M["Tiếp tục Playing; khóa quyền Hold"]
    M --> N["Lock bằng hard drop hoặc lock delay"]
    N --> O["Clear, score, kiểm tra top-out"]
    O --> P{"Ván còn tiếp tục?"}
    P -->|Có| Q["Spawn Next thành công; mở lại quyền Hold"]
    P -->|Không| L
```

## 2. Input và giao tiếp view qua Signal
```mermaid
flowchart LR
    A["Keyboard C hoặc Left Shift"] --> B["GameplayKeyboardInput"]
    C["Nút Hold trên Canvas"] --> D["GameplayCanvas: AddListener"]
    E["GameplayTouchButton Action Hold nếu cấu hình riêng"] --> F["GameplayCommandSignal: Hold"]
    B --> F
    D --> F
    F --> G["GameplayPresenter: lọc state"]
    G --> H["GameLoop.TryHold"]
    H --> I["PublishSnapshot"]
    I --> J["GameplaySnapshotSignal"]
    J --> K["GameplayCanvas: quyền Hold/Restart"]
    J --> L["GameplayHudView"]
    J --> M["BoardView: active và ghost"]
```

## 3. HUD và view bật lại
```mermaid
flowchart TD
    A["HUD OnEnable"] --> B["Đăng ký Snapshot và SceneStop"]
    B --> C["GameplaySnapshotRequestSignal.Dispatch"]
    C --> D{"Presenter đã sẵn sàng và chưa stop?"}
    D -->|Có| E["PublishSnapshot"]
    F["Tick hoặc command thay model"] --> E
    E --> G["Snapshot bất biến, sao chép queue và cells"]
    G --> H["TMP: score, level, lines, state"]
    G --> I["Hold ID và CanHold"]
    G --> J["Danh sách Next IDs"]
    I --> K["Render 4 Image theo shape spawn; Hold rỗng tắt cell"]
    J --> L["Render từng slot Next; căn giữa bounds shape"]
    K --> M["Tái sử dụng cell ở lần render sau"]
    L --> M
    I --> N["Canvas bật/tắt Hold theo quyền model"]
```

## 4. Restart và lifecycle
```mermaid
flowchart TD
    A["R hoặc nút Restart"] --> B["GameplayCommandSignal: Restart"]
    B --> C{"State là GameOver?"}
    C -->|Không| D["Bỏ qua"]
    C -->|Có| E["Tạo GameLoop, board, score, nguồn 7-bag và repeat input mới"]
    E --> F["Spawn khối đầu; Hold rỗng"]
    F --> G["Phát snapshot ngay"]
    G --> H["HUD reset score/level/lines/next/hold; Board vẽ lại"]
    I["SceneStopSignal sceneKey"] --> J{"Đúng scene sở hữu?"}
    J -->|Không| K["Không tác động scene mới"]
    J -->|Có| L["Presenter dừng tick; HUD dừng render; Canvas khóa nút"]
    L --> M["OnDisable gỡ listener đối xứng"]
    M --> N["Cleanup pool và scope theo flow scene hiện có"]
```

## 5. Import hai asset đã duyệt
```mermaid
flowchart TD
    A["Người dùng duyệt NB-UI-007 và NB-UI-008"] --> B["Menu Lesson 11: Import and configure"]
    B --> C{"Play Mode hoặc có scene chưa lưu?"}
    C -->|Có| D["Dừng setup, báo lý do để giữ thay đổi"]
    C -->|Không| E["Copy art-source vào đúng Art path nếu chưa có"]
    E --> F["TextureImporter: Sprite Single FullRect Bilinear Clamp"]
    F --> G["sRGB và alpha bật, max 2048, compression Normal"]
    G --> H["Mở Gameplay bằng EditorSceneManager"]
    H --> I["Tạo hoặc cập nhật Lesson11HUD và gán serialized child fields"]
    I --> J["Gán Hold sprite; panel cho Hold và 5 Next"]
    J --> K["Lưu scene và asset bằng Unity API; giữ meta khi chạy lại"]
```

## 6. Kiểm tra tự động trong Unity
```mermaid
flowchart TD
    A["Run EditMode tests"] --> B["Unity TestRunnerApi"]
    B --> C["Lưu lesson11-editmode.xml"]
    D["Run PlayMode smoke"] --> E["Mở Entry và vào Play Mode"]
    E --> F["MainMenu tải xong"]
    F --> G["SceneRequestSignal yêu cầu Gameplay"]
    G --> H["Kiểm tra Hold, swap, pause và preview thật"]
    H --> I["Hard drop đến GameOver"]
    I --> J["Restart; kiểm tra model, queue, HUD và lifecycle"]
    J --> K["Lưu lesson11-playmode.txt; thoát Play Mode"]
```
