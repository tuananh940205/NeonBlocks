# NeonBlocks — Kế hoạch theo từng bài

## Cách gọi
Prompt ngắn: **Triển khai bài 1**. Thay số để chọn bài tương ứng.
Prompt đầy đủ dùng trong chat mới: “Trong dự án D:\Repositories\PuzzlePortfolio\NeonBlocks, đọc AGENTS.md và docs/PROJECT_CONTEXT.md, docs/LESSON_PLAN.md, docs/PROGRESS.md, rồi triển khai bài N đúng phạm vi; kiểm tra kết quả và cập nhật tiến độ.”

Thực hiện tuần tự. Không cần dán lại nội dung từng bài. Không triển khai bài kế tiếp tự động. Nếu bài trước chưa được xác minh trong Unity, kiểm tra lại và ghi rõ điều kiện còn thiếu trước khi tiếp tục. Bài được đánh dấu hoàn thành chỉ khi tiêu chí đã kiểm tra bằng bằng chứng thật.

## Quy trình chung cho mỗi bài
1. Đọc hướng dẫn và kiểm tra hiện trạng.
2. Thực hiện code/asset/setup thuộc bài, giữ thay đổi người dùng đang có.
3. Kiểm tra phù hợp: compile và test logic khi khả dụng, PlayMode/thiết bị khi bài yêu cầu.
4. Ghi hướng dẫn Editor tại docs/lessons/BAI_N.md khi triển khai, gồm object, component, Inspector, giá trị và cách test; thư mục/file này được tạo trong bài tương ứng.
5. Cập nhật PROGRESS.md: file thay đổi, kết quả thật, việc chưa xác minh và bước thủ công.
6. Tóm tắt kết quả trực tiếp liên quan trò chơi. Không tự commit, publish hoặc thêm tính năng ngoài bài.
7. Vẽ sơ đồ workflow cho từng flow được tạo ra/thay đổi trong lần triển khai

## Tổng quan
1. Nền tảng dự án và Entry scene
2. Dữ liệu JSON và cấu hình luật chơi
3. Manager xuyên suốt game, Addressables và vòng đời scene
4. BoardModel và khối đang rơi
5. Xoay khối và wall kick
6. Nguồn khối và preview
7. Game loop và gravity
8. Xóa hàng, tính điểm và kết thúc
9. Playtest placeholder: BoardView, pool, ghost và bàn phím
10. Điều khiển cảm ứng và hoàn thiện input
11. Hold và hoàn thiện HUD
12. Menu, popup và lưu dữ liệu
13. Mỹ thuật neon và âm thanh
14. Polish và hướng dẫn chơi
15. Kiểm thử và bản Android
16. Profiling và tối ưu có số đo
17. Hoàn tất portfolio

## Bài 1 — Nền tảng dự án và Entry scene

- **Prompt:** `Triển khai bài 1`
- **Phụ thuộc:** Bộ người dùng đã import tại `Assets/Scripts/Core/ServiceLocator.cs` và `Assets/Scripts/Core/Signals.cs`.
- **Phạm vi:**
  1. Tạo các thư mục còn thiếu trong Assets/_Project: Scripts, Scenes, Configs, Prefabs, Art, Audio, Tests. Giữ nguyên vị trí và .meta của bộ Core đã import.
  2. Đọc và tái sử dụng `ServiceLocator.Instance.RegisterSingleton(object)`, `Resolve<T>()`, `UnregisterSingleton(object)`. Không tạo ServiceLocator hoặc Signal thứ hai, không thay bộ người dùng bằng framework khác.
  3. Tạo Entry bootstrap với thứ tự khởi tạo/đăng ký rõ ràng và cleanup theo ownership. API hiện đăng ký theo concrete runtime type: resolve đúng type đã đăng ký, không giả định hỗ trợ interface registration hoặc tự tạo signal. Không tạo service singleton static riêng.
  4. Kiểm tra lifecycle khi Entry khởi tạo lại, chuyển scene và Play Mode bật/tắt Domain Reload: không đăng ký trùng, giữ reference đã dispose hoặc tích lũy listener. Chỉ bổ sung API reset/cleanup tối thiểu vào bộ Core nếu kiểm tra cho thấy cần, giữ tương thích API hiện có và giải thích thay đổi.
  5. Thiết lập assembly runtime/test phù hợp bộ Core. Không để asmdef runtime phụ thuộc Assembly-CSharp: chọn cấu trúc assembly cho Core tại chỗ nếu cần, không tự di chuyển thư viện. Thêm XML comment tiếng Việt cho method thực sự tạo/sửa; không refactor toàn bộ Signals.cs chỉ để đổi cấu trúc file trong bài này.
  6. Cài Addressables qua Package Manager, xác minh phiên bản tương thích trong Editor đang dùng; không tự đoán version. Việc tải asset đi qua AddressableManager ở bài 3.
  7. Chuẩn bị Android portrait, URP 2D, safe area và Entry scene. Chưa triển khai gameplay hoặc hệ thống scene loader thuộc bài 3.
- **Lưu ý Signal:** Tái sử dụng các lớp đã import. Signals.cs hiện có AddListener/RemoveListener/RemoveAllListeners nhưng chưa có API phát sự kiện; ghi nhận điểm này trong bàn giao, bổ sung tối thiểu khi bài cần phát signal, không tạo hệ Signal song song.
- **Nghiệm thu:** Entry compile/chạy không lỗi; đăng ký → resolve đúng cùng instance → unregister được xác minh; chạy lại Entry/Play Mode không có lỗi đăng ký trùng hay service cũ còn tồn tại. Kiểm tra trường hợp Domain Reload tắt nếu môi trường cho phép. Ghi phiên bản package thực tế, cấu trúc assembly, API Core đã dùng và hướng dẫn Editor; đánh dấu chờ xác minh nếu chưa chạy được Unity.
- **Bàn giao:** Bootstrap/setup thuộc bài 1, hướng dẫn Editor, kết quả kiểm tra thật, danh sách bổ sung Core tối thiểu nếu có và cập nhật tiến độ. Chưa sửa code chỉ vì đang cập nhật kế hoạch.

## Bài 2 — Dữ liệu JSON và cấu hình luật chơi

- **Prompt:** `Triển khai bài 2`
- **Phụ thuộc:** 1.
- **Phạm vi:** Thiết kế GameConfig, LevelData và PieceDefinition immutable, constructor-only; dùng DTO riêng để deserialize JSON rồi validate/map. Một type mỗi file. Cấu hình bảng 10×20 và 4 hàng spawn ẩn, 7 tetromino, tốc độ rơi, điểm, input timings. Tọa độ x sang phải, y lên trên; spawn/rotation pivot ghi rõ. Chưa làm gameplay. Tất cả các game config sẽ được load từ Addressables. general game config sẽ tập trung hết game general data trong 1 file duy nhất.
- **Nghiệm thu:** Test JSON hợp lệ/sai, cell trùng, shape ngoài biên, giá trị âm; không direct runtime asset reference.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 3 — Manager xuyên suốt game, Addressables và vòng đời scene

- **Prompt:** `Triển khai bài 3`
- **Phụ thuộc:** 1, 2; đọc kết quả thực tế và các bước Unity còn chờ xác minh.
- **Quy tắc kiến trúc:** Class điều phối logic cho từng chức năng đặt tên `{Function}Manager`, là class thuần C#, không kế thừa MonoBehaviour, không có static Instance riêng. Khởi tạo và đăng ký một instance qua `SessionManager.RegisterSingleton(...)` tại Entry; resolve bằng ServiceLocator hiện có. Manager sống xuyên suốt game, không tạo lại hoặc dispose khi đổi scene. Kết thúc session khi Restart Game/thoát game hoặc Play Mode mới cleanup và unregister.
- **Phạm vi:**
  1. Tạo `AddressableManager` quản lý load/instantiate và ownership handle; `SceneFlowManager` điều phối chuyển scene; `ConfigManager` tải GameConfig JSON qua Addressables rồi dùng GameConfigParser hiện có để parse/validate. Khởi tạo theo thứ tự dependency rõ ràng và chỉ bắt đầu tải scene khi config đã sẵn sàng.
  2. Manager không phụ thuộc serialized field, GameObject host hoặc vòng đời Awake/OnEnable/OnDestroy của scene. Truyền dependency rõ ràng khi khởi tạo. SessionManager hiện có là cầu nối MonoBehaviour sở hữu session, không biến các Manager logic thành component.
  3. Giữ `SceneAssetScope` là object thuần C# quản lý ownership tài nguyên của từng scene, không phải Manager singleton. Khi rời scene, dừng input/job → dispose native data nếu có → hủy pool instances active/inactive → release scope. Manager vẫn tồn tại, chỉ bỏ state/reference thuộc scene cũ; config dùng chung giữ trong scope session.
  4. Chỉ Entry trong Build Settings; MainMenu và Gameplay tải Addressables với key bằng tên asset/root GameObject, duy nhất. Tạo scene/view tối thiểu để xác minh luồng; giao diện đầy đủ thuộc bài sau. Popup/panel tạo động qua Addressables, không đặt sẵn trong scene.
  5. Chỉ AddressableManager và SceneAssetScope thực hiện release handle. Theo dõi owner, load failure, hủy khi đang tải, kết quả tải về sau khi scene/session đã kết thúc; phân biệt ReleaseInstance/Release theo cách instantiate, release đúng một lần. Không release prefab template khi pool còn instance.
  6. MonoBehaviour giao tiếp với MonoBehaviour khác qua class Signal riêng để register/dispatch; không serialized cross-reference hoặc gọi callback trực tiếp giữa hai component. Manager chỉ có logic; nếu cần sự kiện thì dùng Signal hiện có, hủy listener/reference của view khi rời scene.
  7. Các method Manager trả Task tuân thủ quy tắc exception trong AGENTS.md, bọc failure đồng bộ/bất đồng bộ bằng loại exception đã tài liệu hóa. Không tạo framework lifecycle, ServiceLocator hay Signal thứ hai.
- **Nghiệm thu:** Kiểm tra đăng ký/resolve cùng instance của từng Manager qua Entry→Menu→Gameplay→Menu; đổi scene không tạo Manager mới. Config tải và validate thành công trước gameplay; tải thất bại có kết quả rõ ràng. Kiểm tra hủy tải/chuyển scene liên tiếp, không double release, không giữ listener/view scene cũ. Restart Game kết thúc session cũ, cleanup theo dependency và tạo bộ Manager mới; chạy lại Play Mode không đăng ký trùng. Ghi rõ các kiểm tra chưa chạy được.
- **Bàn giao:** Code, test phù hợp và hướng dẫn Unity tại docs/lessons/BAI_3.md khi triển khai; bảng Manager/dependency/owner/lifetime, key Addressables, ownership tài nguyên session/scene và kết quả kiểm tra thật. Cập nhật PROGRESS.md; không đánh dấu đã triển khai chỉ vì đã sửa kế hoạch.

## Bài 4 — BoardModel và khối đang rơi

- **Prompt:** `Triển khai bài 4`
- **Phụ thuộc:** 2.
- **Phạm vi:** Pure C# BoardModel, ActivePiece, grid trạng thái đã khóa; kiểm tra fit/collision, spawn, di chuyển trái/phải, rơi một ô và lock. ActivePiece tách khỏi settled cells. API trả kết quả rõ ràng; thao tác sai không mutate.
- **Nghiệm thu:** EditMode kiểm tra mép bảng, đáy, hàng ẩn, collision, lock và spawn bị chặn.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 5 — Xoay khối và wall kick

- **Prompt:** `Triển khai bài 5`
- **Phụ thuộc:** 4.
- **Phạm vi:** Thực hiện SRS với bảng kick riêng I và JLSTZ, O giữ hình; clockwise/counterclockwise, không xoay 180. Ghi nguồn/qui ước tọa độ khi triển khai và chuyển dấu y nhất quán. Pure C#, chỉ commit phép xoay khi hợp lệ.
- **Nghiệm thu:** Test từng orientation, kick cạnh/tường/sàn, khối I/O, mọi kick thất bại giữ nguyên trạng thái.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 6 — Nguồn khối và preview

- **Prompt:** `Triển khai bài 6`
- **Phụ thuộc:** 4.
- **Phạm vi:** 7-bag có seed, Next queue 5 khối, lấy khối đúng thứ tự; spawn theo dữ liệu đã chốt. RNG nằm trong service/model có thể thay thế để test; không dùng Unity random trong model.
- **Nghiệm thu:** Mỗi bag chứa đủ 7 loại, cùng seed cùng chuỗi, queue bù đúng; chưa hold.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 7 — Game loop và gravity

- **Prompt:** `Triển khai bài 7`
- **Phụ thuộc:** 5, 6.
- **Phạm vi:** Session state Ready/Playing/Paused/GameOver, tick deterministic, gravity theo level, soft drop, hard drop, lock delay 0.5 giây và tối đa 15 reset do move/rotate hợp lệ khi chạm đất. Pause không tích lũy thời gian; hard drop khóa ngay. Tách clock khỏi model.
- **Nghiệm thu:** Test delta lớn, pause/resume, reset cap, hard drop và khóa đúng một lần; phần hiển thị có thể còn placeholder.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 8 — Xóa hàng, tính điểm và kết thúc

- **Prompt:** `Triển khai bài 8`
- **Phụ thuộc:** 7.
- **Phạm vi:** Xóa đồng thời hàng đầy rồi compact; điểm Single/Double/Triple/Tetris=100/300/500/800 × level trước lượt xóa; soft drop 1/ô, hard drop 2/ô. Bắt đầu level 1, tăng mỗi 10 hàng. MVP chưa T-spin/combo/B2B. Sau clear, nếu còn cell hàng ẩn thì game over; spawn không fit cũng game over.
- **Nghiệm thu:** Test 1–4 hàng, hàng không liền nhau, scoring/level boundary, lock→clear→top-out→spawn đúng thứ tự.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 9 — Playtest placeholder: BoardView, pool, ghost và bàn phím

- **Prompt:** `Triển khai bài 9`
- **Phụ thuộc:** 3, 8.
- **Mục tiêu mới:** Đây là mốc **playtest desktop đầu tiên**. Sau bài này phải chơi được một ván placeholder bằng bàn phím, dùng toàn bộ luật đã có đến bài 8; không chờ bài 10.
- **Phạm vi:**
  1. Dựng `GameplayPresenter`/driver MonoBehaviour mỏng cho scene Gameplay: tạo model từ `ConfigManager.Config`, nối `GameLoop`/logic bài 4–8 với `Update` chỉ để truyền delta time, và chuyển snapshot model sang view. Không đưa luật vào view.
  2. Dựng BoardView cho settled/active/ghost/next bằng placeholder đơn giản, có thể dùng Image/UI hoặc SpriteRenderer hình vuông màu lập trình; **không cần asset mới**. Mapping ô phải dùng cùng quy ước x sang phải, y lên. Ghost là kết quả chiếu xuống từ model, không sửa model.
  3. Tạo `PoolManager` là class thuần C# xuyên suốt session, đăng ký qua SessionManager. Pool sở hữu instance theo `SceneAssetScope`; template Addressable giữ sống trong khi còn instance, và scope hủy active/inactive instance trước khi release template/handle.
  4. Đưa bộ điều khiển bàn phím tối thiểu vào bài này bằng Input System: Left/A, Right/D, Down/S, Z (CCW), X/Up (CW), Space (hard drop), P/Escape (pause). Bind/unbind theo lifecycle scene; input bị chặn khi Pause/GameOver. Bài 10 không được dời mốc playtest này.
  5. Gameplay scene tự bắt đầu một ván sau khi MainMenu gửi Start; có text placeholder tối thiểu cho state/score/lines/level và phím Restart `R` khi GameOver. Không làm menu/popup hoàn chỉnh thuộc bài 12.
  6. Thay hành vi bootstrap tạm thời: sau khi `GameConfig` và MainMenu đã tải thành công, MainMenu được đặt active rồi **unload Entry scene**. `SessionManager` và manager session đã DontDestroyOnLoad vẫn sống; EntryLauncher, BootVisuals và BootVisualsView bị hủy theo Entry. Chỉ unload sau thành công; nếu tải lỗi thì giữ Entry để log/báo lỗi. Việc unload native Entry không phải release Addressables handle.
- **Nghiệm thu:** Play Mode desktop từ Entry → MainMenu → Gameplay; chơi được một ván placeholder bằng các phím đã nêu, gravity/rotate/drop/lock/clear/score/level/top-out hoạt động; mapping ô chính xác; ghost trùng vị trí hard drop; Pause chặn tick/input; GameOver cho phép Restart; Hierarchy không còn scene Entry sau khi MainMenu tải; trả pool trước khi scope release khi thoát Gameplay.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 10 — Điều khiển cảm ứng và hoàn thiện input

- **Prompt:** `Triển khai bài 10`
- **Phụ thuộc:** 9.
- **Phạm vi:** Giữ nguyên và regression-test keyboard của bài 9; bổ sung Android bằng nút cảm ứng rõ ràng hoặc gesture đã tài liệu hóa cho trái/phải, xoay CW/CCW, soft/hard drop và pause. Áp dụng DAS 0.15s, ARR 0.05s từ JSON; bind/unbind theo lifecycle, chặn input khi pause/popup và thao tác nhiều ngón không tạo lệnh thừa.
- **Nghiệm thu:** Playtest desktop vẫn chạy với bộ phím bài 9; test giữ phím/nút, tap xoay, hủy chạm, mất focus, pause; kiểm tra trên thiết bị nếu có, ghi rõ nếu chưa.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 11 — Hold và hoàn thiện HUD

- **Prompt:** `Triển khai bài 11`
- **Phụ thuộc:** 10.
- **Phạm vi:** Hold một lần mỗi lượt khối, reset quyền hold sau lock/spawn; swap về spawn orientation/position, spawn fail kết thúc. HUD score/level/lines/next/hold, Signal typed cho model→UI, Signal riêng register/dispatch cho yêu cầu giữa các MonoBehaviour views; không serialized cross-view.
- **Nghiệm thu:** Test hold lần đầu/đổi khối/chặn hold lần hai/reset lượt/top-out; HUD đồng bộ khi restart.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 12 — Menu, popup và lưu dữ liệu

- **Prompt:** `Triển khai bài 12`
- **Phụ thuộc:** 11.
- **Phạm vi:** Menu Start/Settings, Pause/Resume/Restart/Home, GameOver và high score. Mọi panel/popup Addressable prefab, InstantiateAsync và release khi đóng qua owner được phép. Save local versioned cho best score, âm lượng và setting; xử lý save hỏng bằng kết quả lỗi/fallback rõ ràng theo quy định exception.
- **Nghiệm thu:** Chạy trọn Entry→Menu→Game→Pause→GameOver→Restart→Home; không nhân listener/popup/handle sau nhiều vòng.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 13 — Mỹ thuật neon và âm thanh

- **Prompt:** `Triển khai bài 13`
- **Phụ thuộc:** 12.
- **Phạm vi:** Chốt palette neon, shape dễ phân biệt ngoài màu, nền tối, typography dễ đọc; tạo raster bằng imagegen nếu cần theo AGENTS, PNG và kiểm tra alpha. SpriteAtlas cho sprite ổn định; prefab dependency không đánh Addressable riêng. Thêm SFX đặt/xoay/drop/clear/gameover, nhạc và setting volume; tránh chớp mạnh.
- **Nghiệm thu:** Kiểm tra 9:16 và màn hình dài/safe area, đọc rõ ghost/active/settled, âm lượng lưu đúng; asset có nguồn/quyền sử dụng ghi rõ.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 14 — Polish và hướng dẫn chơi

- **Prompt:** `Triển khai bài 14`
- **Phụ thuộc:** 13.
- **Phạm vi:** Tutorial ngắn cách di chuyển/xoay/drop/hold/xóa hàng; feedback clear, landing, chuyển level; animation không khóa hoặc làm lệch model. Hủy animation và input khi thoát scene. Thêm setting giảm hiệu ứng.
- **Nghiệm thu:** Người mới hoàn thành một lượt; spam restart/pause/thoát khi animation không để state treo; UI tiếng Việt nhất quán.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 15 — Kiểm thử và bản Android

- **Prompt:** `Triển khai bài 15`
- **Phụ thuộc:** 14.
- **Phạm vi:** Chạy EditMode/PlayMode, bổ sung regression cho lỗi thật; cấu hình Android package, portrait và build phù hợp SDK thực có. Kiểm thử cài/chạy, back/pause/resume, mất focus, cảm ứng, save, tỷ lệ màn hình. Không ký/phát hành store trong bài này.
- **Nghiệm thu:** Ghi kết quả và đường dẫn build; nếu thiếu SDK/thiết bị thì đánh dấu chờ xác minh, không báo pass.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 16 — Profiling và tối ưu có số đo

- **Prompt:** `Triển khai bài 16`
- **Phụ thuộc:** 15.
- **Phạm vi:** Đo CPU/GC/memory/frame time trên workload gameplay thật và lặp scene; lưu baseline cùng thiết bị/build/protocol ở docs/profiling. Tối ưu allocation, pooling, atlas và Addressables theo kết quả. Jobs/Burst chỉ thử khi chứng minh hotspot và workload đủ lớn, nếu không ghi quyết định không dùng.
- **Nghiệm thu:** So sánh trước/sau theo cùng protocol; lặp vào/thoát 20 lần kiểm tra tài nguyên; không bịa benchmark hoặc cam kết FPS chưa đo.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Bài 17 — Hoàn tất portfolio

- **Prompt:** `Triển khai bài 17`
- **Phụ thuộc:** 16.
- **Phạm vi:** README mô tả game, điều khiển, kiến trúc, chạy/build, giới hạn luật chơi; ảnh/video thật, evidence test/profiling, asset credits và checklist bàn giao. Tổng rà các tiêu chí, đánh dấu việc chưa xác minh.
- **Nghiệm thu:** Có bản build đã xác minh hoặc nêu blocker cụ thể; tài liệu khớp code và progress; không dùng tên/logo/asset Tetris chính thức.
- **Bàn giao:** code/asset và hướng dẫn Editor cần thiết, kết quả kiểm tra thật, cập nhật tiến độ.

## Mốc chơi được
- Sau bài 8: luật gameplay kiểm tra độc lập.
- Sau bài 9: chơi được trên desktop với hình placeholder và bàn phím.
- Sau bài 10: playtest cảm ứng trên Android/Device Simulator, nếu môi trường sẵn sàng.
- Sau bài 12: vòng chơi/menu/save đầy đủ.
- Sau bài 15: ứng viên bản Android, phụ thuộc SDK và thiết bị thực có.
- Sau bài 17: portfolio và bàn giao.

## Thay đổi phạm vi
Các luật MVP trong PROJECT_CONTEXT.md là mặc định đã ghi rõ. Yêu cầu mới có thể đổi chúng; phải ghi tác động và cập nhật bài liên quan, giữ nguyên số hiệu bài đã triển khai. Không thêm Jobs/Burst chỉ để có tên công nghệ trong portfolio.
