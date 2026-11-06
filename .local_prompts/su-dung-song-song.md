Giải pháp phù hợp là chia thành hai chat, dùng chung dự án và tài liệu bàn giao:
| Chat | Trách nhiệm |
|---|---|
| **Chat này — Asset** | Generate/edit hình nền, block, icon, nút, hiệu ứng dạng ảnh; cung cấp file và thông số import |
| **Chat Codex — Code** | Viết/sửa C#, tạo scene/prefab, gắn asset, cấu hình Addressables, test và build |

Để hai bên phối hợp ổn định, dùng một file chung docs/ASSET_HANDOFF.md. Mỗi asset ghi: mã asset, mục đích, đường dẫn, kích thước, nền trong suốt hay không, bố cục sprite sheet nếu có, thông số import đề xuất và trạng thái duyệt.

Quy trình mỗi bài:
1. Bạn gọi Codex: “Triển khai bài N. Nếu cần asset mới, ghi yêu cầu cụ thể vào docs/ASSET_HANDOFF.md; dùng placeholder để tiếp tục phần code.”
2. Bạn gọi tôi: “Tạo asset theo yêu cầu đang chờ trong docs/ASSET_HANDOFF.md.” Tôi đọc yêu cầu, tạo/chỉnh ảnh và cập nhật bàn giao.
3. Bạn gọi Codex: “Tích hợp các asset đã sẵn sàng trong docs/ASSET_HANDOFF.md, kiểm tra và cập nhật tiến độ.”

Nên đặt ảnh chờ duyệt trong art-source/; ảnh đã duyệt mới đưa vào Assets/_Project/Art/. Codex quản lý .meta, cấu hình import, atlas, prefab và Addressables để giữ tham chiếu Unity ổn định. Khi sửa ảnh đã dùng, giữ đường dẫn và .meta; nếu thay kích thước hoặc bố cục sprite sheet thì ghi rõ trong bàn giao.

Bạn có thể mở đầu chat Codex bằng prompt:
```
Bạn phụ trách code và tích hợp Unity cho dự án D:\Repositories\PuzzlePortfolio\NeonBlocks. Đọc AGENTS.md và các tài liệu trong docs trước khi triển khai từng bài.

Asset hình ảnh được tạo/chỉnh ở chat khác. Khi cần asset, ghi yêu cầu vào docs/ASSET_HANDOFF.md gồm mã asset, mục đích, đường dẫn mong muốn, kích thước, yêu cầu alpha và bố cục sprite sheet nếu có. Dùng placeholder cho phần có thể tiếp tục.

Bạn phụ trách import setting, .meta, SpriteAtlas, prefab, scene, Addressables, test và build. Khi asset được bàn giao, tích hợp và kiểm tra thực tế. Không tự generate ảnh hoặc ghi đè ảnh đã duyệt.
```

## Resume
```
Tiếp tục dự án NeonBlocks qua kết nối neonblocks_filesystem. Trước khi làm, đọc AGENTS.md và các tài liệu liên quan theo router; không mặc định thư mục đang mở là dự án đích. Với asset, đọc ASSETS_AND_EDITOR.md và ASSET_HANDOFF.md, giảm tối đa transparent bao ngoài và lưu vào art-source/ để tôi duyệt.
```