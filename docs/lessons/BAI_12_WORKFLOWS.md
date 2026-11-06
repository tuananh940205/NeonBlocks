# Bài 12 — Workflow

## 1. Bootstrap và menu
```mermaid
flowchart TD
    A["Entry: tạo manager session"] --> B["LocalSaveManager.LoadAsync"]
    B --> C["GameConfig tải ui keys"]
    C --> D["SceneFlow tải MainMenu"]
    D --> E["PopupHost chờ CurrentScope đúng scene"]
    E --> F["InstantiateAsync MainMenuPanel"]
    F --> G["TMP hiển thị best score và trạng thái save"]
    G --> H["START yêu cầu GoToAsync Gameplay"]
```

## 2. Popup async và ownership
```mermaid
flowchart TD
    A["Host thay desired kind"] --> B["Khóa command gameplay"]
    B --> C["Đóng slot hiện tại bằng scope.ReleaseInstance"]
    C --> D["PopupManager.OpenAsync"]
    D --> E{"Cùng key đang tải hoặc đang mở?"}
    E -->|Có| F["Dùng task hiện có"]
    E -->|Không| G["scope.InstantiateAsync theo key"]
    G --> H{"Host còn sống và revision còn đúng?"}
    H -->|Có| I["Hiện prefab Canvas"]
    H -->|Không| J["Release kết quả muộn"]
    I --> K["Đóng hoặc SceneStop"]
    K --> J
    G --> L["Lỗi: thông báo và Retry, không tự spam request"]
```

## 3. Pause, Settings, Resume
```mermaid
flowchart TD
    A["P/Esc hoặc nút Pause"] --> B["Presenter pause model, xóa DAS/ARR"]
    B --> C["Snapshot Paused"]
    C --> D["Host mở PausePopup và khóa input"]
    D --> E["SETTINGS ghi returnKind=Pause"]
    E --> F["SettingsPopup"]
    F --> G["APPLY: save settings async"]
    F --> H["BACK: bỏ thay đổi chưa apply"]
    G --> I{"Ghi thành công?"}
    I -->|Có| D
    I -->|Không| J["Giữ Settings và hiển thị lỗi"]
    H --> D
    D --> K["RESUME qua ModalResume"]
    K --> L["Đóng modal, mở input, model Playing"]
```

## 4. Restart, Home và cleanup
```mermaid
flowchart TD
    A["RESTART trong Pause/GameOver"] --> B["ModalRestart"]
    B --> C["Presenter tạo ván mới"]
    C --> D["Snapshot reset Hold, queue, score và HUD"]
    D --> E["Host đóng popup"]
    F["HOME"] --> G["GoToAsync MainMenu"]
    G --> H["Tải và active scene mới"]
    H --> I["SceneStop đúng scene cũ"]
    I --> J["Host cancel load, PopupManager.Close"]
    J --> K["SceneDispose, pool cleanup, scope release"]
```

## 5. GameOver và best score
```mermaid
flowchart TD
    A["Snapshot chuyển sang GameOver"] --> B["Host chỉ xử lý lần chuyển state đầu"]
    B --> C["Mở GameOverPopup"]
    B --> D["RecordScoreAsync"]
    D --> E["best = max best cũ và score"]
    E --> F["Ghi save dưới gate"]
    F --> G["SaveChangedSignal"]
    G --> H["Popup cập nhật SCORE/BEST và lỗi nếu có"]
    C --> H
```

## 6. Load/save có version và fallback
```mermaid
flowchart TD
    A["Read save"] --> B{"File tồn tại?"}
    B -->|Không| C["Default; status Missing"]
    B -->|Có| D["Parse JSON và kiểm tra version trước schema"]
    D --> E{"Version được hỗ trợ?"}
    E -->|Không| F["Default; UnsupportedVersion; khóa ghi"]
    E -->|Có| G["Validate DTO: điểm, volume, mute"]
    G --> H["Loaded"]
    D --> I["JSON hỏng: Corrupt; default; khóa ghi"]
    G --> I
    A --> J["I/O lỗi: IoFailure; khóa ghi"]
    K["Score/settings update"] --> L["Semaphore gate tránh mất update"]
    L --> M{"Đang khóa ghi?"}
    M -->|Có| N["Chỉ cập nhật session; báo WriteBlocked"]
    M -->|Không| O["Write temp cùng thư mục"]
    O --> P["Replace + backup hoặc Move nếu chưa có file"]
    O --> Q["Ghi lỗi: giữ bản tốt; status IoFailure"]
    P --> R["Saved"]
```
