# Asset handoff và Unity Editor

## Asset workflow

- Tạo/chỉnh raster thuộc chat Asset; coding agent phụ trách code và tích hợp Unity, không tự duyệt asset thay người dùng.
- Asset đang tạo/chờ duyệt nằm ở `art-source/` ngoài `Assets/`; chỉ ảnh đã duyệt được đưa vào `Assets/_Project/Art/`.
- Mọi yêu cầu/bàn giao nằm trong `docs/ASSET_HANDOFF.md`: mã, mục đích, source/destination, kích thước, alpha, sprite sheet, import đề xuất, trạng thái duyệt.
- Khi cần asset mới, ghi yêu cầu cụ thể vào handoff và dùng placeholder để tiếp tục code; ghi rõ phần còn chờ asset.
- Khi cập nhật ảnh đã tích hợp, giữ đường dẫn và `.meta`; nếu layout/size sprite sheet đổi, kiểm tra slicing và consumer.
- Dùng SpriteAtlas khi sprite gameplay/UI đã ổn định.

## Integration và hướng dẫn Editor

- Coding agent cấu hình import, `.meta`, SpriteAtlas, prefab, scene và Addressables cho asset đã duyệt; sau đó kiểm tra và cập nhật progress.
- Mọi hướng dẫn Unity Editor phải nêu object tạo ở đâu trong Hierarchy/Project, component cần thêm, field Inspector cần gán, giá trị đặt và cách test.
- Prompt “Tích hợp các asset đã sẵn sàng trong docs/ASSET_HANDOFF.md, kiểm tra và cập nhật tiến độ” yêu cầu kiểm tra trạng thái duyệt trước, chỉ tích hợp asset đã duyệt, rồi cập nhật checklist/progress.
