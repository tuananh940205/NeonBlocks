# NeonBlocks — Agent Instructions

AI Agents do not remove anything in this file

## Khi người dùng nói “Triển khai bài N”

1. Đọc toàn bộ `docs/PROJECT_CONTEXT.md`, `docs/LESSON_PLAN.md` và `docs/PROGRESS.md`.
2. Tìm đúng heading `Bài N` trong `docs/LESSON_PLAN.md`.
3. Thực hiện trọn vẹn phạm vi bài đó: code, asset nếu được yêu cầu, Unity Editor setup, test và cập nhật progress.
4. Không thực hiện nội dung của bài tiếp theo khi chưa được người dùng yêu cầu rõ ràng.
5. Không nói bài đã pass nếu chưa chạy được validation khả dụng; nêu rõ phần nào cần người dùng kiểm tra trong Unity Editor.
6. Tóm tắt những thay đổi có liên quan trực tiếp đến trò chơi sau khi công việc hoàn tất

## Unity và kiến trúc bắt buộc

- Unity 6.3 LTS, Universal 2D (URP), Android portrait 9:16.
- Mọi method tạo hoặc sửa trong `Assets/` phải có XML comment tiếng Việt giải thích trách nhiệm.
- Giữ gameplay rule trong pure C# (`BoardModel`), Unity views chỉ render/input.
- Runtime asset dùng Addressables string key. Key phải chính xác bằng tên asset hoặc root GameObject.
- `LevelData` chỉ lưu string key cho runtime asset, không lưu direct prefab/sprite reference.
- Chỉ `AddressableService` và `SceneAssetScope` được release Addressables handles.
- Khi rời scene: dừng input/job -> dispose native data -> destroy active/inactive pool instances -> release SceneAssetScope.
- Không release prefab template khi `PoolService` còn instance.
- Không gọi `Addressables.Release` trực tiếp trong gameplay/view/UI code.
- All popups/panels view should be packed by addressable prefab, not placed in scene at initial, shown by Instantiate Async and release when closed
- Only entry scene set in scene list. The another scene will be loaded by addressables

## Asset và UI

- Dùng SpriteAtlas cho sprite gameplay và UI đã ổn định.
- Việc generate/edit asset hình ảnh do chat Asset đảm nhiệm; Codex phụ trách code và tích hợp Unity. Codex không tự generate ảnh hoặc ghi đè ảnh đã duyệt.
- Ảnh đang tạo/chỉnh hoặc chờ duyệt phải nằm trong `art-source/` tại gốc dự án, ngoài thư mục `Assets/`. Chat Asset dùng Image Generation cho raster mới/chỉnh sửa, lưu PNG và kiểm tra alpha nếu yêu cầu nền trong suốt.
- Chỉ đưa ảnh đã được người dùng duyệt vào `Assets/_Project/Art/`. Sự tồn tại của file ảnh không đồng nghĩa với việc đã được duyệt; không tự duyệt thay người dùng.
- Theo dõi yêu cầu và bàn giao trong `docs/ASSET_HANDOFF.md`; nếu chưa có thì tạo khi phát sinh yêu cầu asset đầu tiên. Mỗi mục ghi mã asset, mục đích, đường dẫn nguồn/đích, kích thước, yêu cầu alpha, bố cục sprite sheet nếu có, thông số import đề xuất và trạng thái duyệt. Khi chỉnh ảnh đã duyệt, phiên bản mới cần được duyệt lại trước khi tích hợp.
- Khi cần asset mới, Codex ghi yêu cầu cụ thể vào file bàn giao và dùng placeholder cho phần có thể tiếp tục; ghi rõ phần tích hợp còn chờ asset.
- Codex phụ trách đưa ảnh đã duyệt vào Unity, cấu hình import, `.meta`, SpriteAtlas, prefab, scene và Addressables; kiểm tra kết quả và cập nhật tiến độ.
- Khi cập nhật ảnh đã tích hợp, giữ nguyên đường dẫn và `.meta` để bảo toàn tham chiếu. Nếu kích thước hoặc bố cục sprite sheet thay đổi, kiểm tra và cập nhật slicing cùng các thành phần sử dụng ảnh.
- Quy trình bàn giao asset này áp dụng cho mọi bài, kể cả khi tài liệu bài học yêu cầu tạo ảnh; không cần người dùng nhắc lại trong mỗi prompt.
- Không tạo asset Addressable riêng nếu nó chỉ là dependency của một Addressable prefab, trừ khi lesson ghi rõ.
- Mọi hướng dẫn Unity Editor phải nêu: object tạo ở đâu trong Hierarchy/Project, component thêm vào, field Inspector cần gán, giá trị cần đặt và cách test.

## Chất lượng và portfolio

- Dùng Jobs/Burst chỉ cho bài toán có baseline và benchmark trên workload đủ lớn.
- Không bịa số benchmark, profiler hoặc memory result.
- Các file evidence cuối dự án nằm ở `docs/profiling/`.
- Không sửa `Library/`, `Temp/` hay Unity-generated files.

## Coding instruction
Use action/callback when call from this view to that view, remove cross dependencies serialized fields
Use button listener by addlistener/removelistener instead of Button component
Every generated function should write explanation comments by Vietnamese
Game config and level config should be load text asset by json, then serialize to serialiable class
Text asset can be loaded by addressable by key
Data class has readonly field, set data from constructor only
1 file should define only class/object with the same name as file name

Every singleton instance managed by ServiceLocator
Use Signal instead of placing serialized monobehaviour script object to call methods
When need to use new signal, define a new class inherit from the needed signal
XML comments should be placed above the atrribute tags of the member definition

Only use try-catch in functions when:
- has CancellationTokenSource
- Test method that expects an exception: wrap the whole call (start the Task and await it) in try-catch, catch only the predicted exception type, keep a timeout so the test cannot hang
- Service methods that return Task must wrap every failure in the documented exception type, including synchronous errors from Addressables
- Test that triggers an exception logged by Unity or a library: try-catch cannot intercept a log, so declare it with LogAssert.Expect(<LogType as shown in the test failure output, e.g. Error>, <exact message regex>) and do not ignore all failing messages


Use one static class object as service locator to store all singletons
Do not use static class or static instance for singleton items

## Bộ ServiceLocator do người dùng import

- Tái sử dụng `Assets/Scripts/Core/ServiceLocator.cs` và `Assets/Scripts/Core/Signals.cs`; không tạo bộ ServiceLocator/Signal thứ hai hoặc thay framework.
- Theo lựa chọn của người dùng, giữ mô hình `ServiceLocator.Instance` của bộ đã import. Đây là ngoại lệ cho yêu cầu static class ServiceLocator ở trên; service khác vẫn không có static singleton riêng.
- Đọc API thực tế trước khi dùng; chỉ bổ sung tối thiểu nếu bài cần, giữ tương thích và xác minh lifecycle. Không dùng GetPrefabs/Resources cho runtime asset NeonBlocks; dùng Addressables theo quy tắc hiện có.


## Giao tiếp giữa MonoBehaviour và class thuần logic

- Signal là nơi quản lý listener và gọi các listener khi dispatch.
- Khi một MonoBehaviour cần gọi logic của MonoBehaviour khác, định nghĩa một class signal riêng kế thừa loại Signal phù hợp trong bộ đã import. Bên nhận register listener vào signal, bên gửi dispatch signal; không kéo serialized field tham chiếu MonoBehaviour bên kia để gọi method.
- Quy tắc signal này được ưu tiên hơn hướng dẫn action/callback giữa các view ở trên khi cả hai bên là MonoBehaviour. Không dùng callback/action trực tiếp giữa hai MonoBehaviour để thay thế signal.
- Lấy signal dùng chung qua ServiceLocator hiện có; đăng ký instance trước khi resolve theo API thực tế. Không tạo các instance signal riêng khiến bên gửi và bên nhận dùng hai kênh khác nhau.
- Register/unregister listener theo vòng đời phù hợp và đối xứng để tránh gọi trùng hoặc gọi object đã bị hủy. Dispatch chỉ gọi các listener đang đăng ký.
- Class thuần logic không kế thừa MonoBehaviour. Nếu cần được gọi từ nhiều nơi, đăng ký một instance singleton qua ServiceLocator hiện có và resolve để sử dụng; không tạo static Instance riêng cho class đó.
- Singleton không có nghĩa là tồn tại mãi: xác định rõ owner và vòng đời, unregister/cleanup khi kết thúc scope tương ứng.
