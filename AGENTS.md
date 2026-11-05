# NeonBlocks — Agent Router

Đây là điểm vào ngắn cho mọi coding agent. Quy tắc chi tiết nằm trong `docs/agent-rules/`; các module đó có cùng hiệu lực với file này. Khi có mâu thuẫn, yêu cầu mới hơn của người dùng, rồi đến lesson hiện hành, rồi đến module chuyên trách sẽ được ưu tiên.

## Đọc theo loại công việc

| Công việc | Phải đọc |
| --- | --- |
| Mọi thay đổi trong dự án | `docs/agent-rules/CORE.md` và lesson/tài liệu được yêu cầu trực tiếp |
| `Triển khai bài N` | `docs/PROJECT_CONTEXT.md`, đúng mục **Bài N** trong `docs/LESSON_PLAN.md`, `docs/PROGRESS.md`, rồi các module trong bảng dưới |
| Gameplay, view, input, UI | `CORE.md`, `UNITY_EDITOR.md` |
| Config, JSON, DTO, domain data, test | `CORE.md`, `DATA_AND_TESTING.md` |
| Scene, Addressables, pool, singleton manager | `CORE.md`, `ADDRESSABLES_AND_LIFECYCLE.md` |
| Tạo, duyệt, import hoặc thay asset | `ASSETS_AND_EDITOR.md`, `docs/ASSET_HANDOFF.md` |
| Profiling, Jobs/Burst, evidence | `QUALITY.md` |

## Module mặc định theo bài

- Bài 1–2: `CORE.md`, `DATA_AND_TESTING.md`, `QUALITY.md`.
- Bài 3 trở đi có scene/Addressables: thêm `ADDRESSABLES_AND_LIFECYCLE.md`.
- Bài có UI, input hoặc Play Mode: thêm `UNITY_EDITOR.md`.
- Bài cần asset hoặc chỉnh asset: thêm `ASSETS_AND_EDITOR.md`.
- Nếu lesson nêu rõ module khác, đọc module đó dù bảng này không liệt kê.

## Nguyên tắc luôn áp dụng

- Chỉ làm đúng phạm vi người dùng yêu cầu; không tự làm bài kế tiếp.
- Không nói đã pass nếu chưa có validation khả dụng; nêu rõ phần cần kiểm tra trong Unity Editor.
- Không sửa `Library/`, `Temp/` hay file Unity-generated.
- Cập nhật `docs/PROGRESS.md` khi hoàn thành phạm vi một bài.

## Danh mục module

- [CORE.md](docs/agent-rules/CORE.md)
- [ADDRESSABLES_AND_LIFECYCLE.md](docs/agent-rules/ADDRESSABLES_AND_LIFECYCLE.md)
- [ASSETS_AND_EDITOR.md](docs/agent-rules/ASSETS_AND_EDITOR.md)
- [DATA_AND_TESTING.md](docs/agent-rules/DATA_AND_TESTING.md)
- [UNITY_EDITOR.md](docs/agent-rules/UNITY_EDITOR.md)
- [QUALITY.md](docs/agent-rules/QUALITY.md)
