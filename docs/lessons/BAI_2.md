# Bài 2 — Hướng dẫn Unity Editor

Trạng thái: code và test đã viết; chưa chạy Test Runner trong Unity Editor trong phiên này.

## 1. Dữ liệu
- File duy nhất chứa toàn bộ cấu hình chung: `Assets/_Project/Configs/GameConfig.json`.
- Gồm: bảng 10×20 cộng 4 hàng ẩn, khung spawn, Next 5 khối, level và tốc độ rơi, điểm xóa hàng, soft/hard drop, lock delay, DAS/ARR.
- Tọa độ: x tăng sang phải, y tăng lên trên. Hàng y = 0 là đáy; y = 0..19 là vùng hiển thị; y = 20..23 là 4 hàng ẩn.
- Khung spawn: góc đáy trái đặt tại `spawn = (3, 20)`, khung 4×4 chiếm x = 3..6, y = 20..23 (nằm trong vùng ẩn).
- Tâm xoay (`rotationCenter`) đo trong khung 4×4 cùng hệ tọa độ; khối I dùng (1.5, 1.5), O dùng (1.5, 2.5), các khối còn lại dùng (1, 1).

## 2. Thiết lập Editor
0. Package `com.unity.nuget.newtonsoft-json` 3.2.2 đã được thêm vào `Packages/manifest.json`. Mở lại Editor hoặc bấm **Window → Package Manager → Refresh** để Unity tải package; xác nhận Package Manager hiển thị Newtonsoft Json 3.2.2 là installed.
1. Trong Project, chọn `Assets/_Project/Configs/GameConfig.json`. Inspector hiển thị TextAsset (không cần gán gì thêm).
2. Mở **Window → Asset Management → Addressables → Groups**. Nếu chưa có Addressables Settings, bấm **Create Addressables Settings** (cấu hình chi tiết thuộc bài 3).
3. Kéo `GameConfig.json` vào group mặc định (hoặc group bạn chọn). Trong Inspector của entry, đặt **Address** = `GameConfig`.
   - Key phải trùng tên asset `GameConfig` để tuân theo quy tắc key của dự án; hằng số `GameConfigParser.AddressKey` cũng là `"GameConfig"`.
   - Bài 2 chưa tải file này khi chạy game; việc tải qua `AddressableManager` thuộc bài 3.
4. Không cần tạo prefab, scene hay asset ảnh mới cho bài này.

## 3. Kiểm tra
1. **Window → General → Test Runner**, tab **EditMode**, bấm **Run All**.
2. Kỳ vọng các fixture mới pass:
   - `GameConfigParserTests` — 23 test: file thật được parse đúng; JSON sai cú pháp; thiếu/thừa khối; mã khối trùng hoặc lạ; ô trùng; ô ngoài khung; khối không liền; tâm xoay sai; bảng âm; spawn tràn; điểm âm; tốc độ rơi = 0; level không liên tiếp; lock delay/DAS/lines không hợp lệ.
   - `GameConfigRulesTests` — 4 test: field readonly; không có tham chiếu `UnityEngine.Object`; `GetLevel` vượt bảng; `GetLevel` dưới startLevel.
   - `PieceDefinitionTests` — 7 test: khối I hợp lệ; ô trùng; ô ngoài khung; ô âm; không liền; sai số ô; tâm xoay ngoài khung.
3. Test `Parse_MalformedJson_Throws` kỳ vọng `JsonException` (Newtonsoft). Kiểm tra cả việc Newtonsoft tìm thấy DLL qua `precompiledReferences` trong asmdef; nếu lỗi biên dịch về `Newtonsoft`, ghi lại trong PROGRESS.
4. Nếu lỗi biên dịch, mở Console và gửi lỗi đầu tiên.

## 4. Ghi chú
- Tốc độ rơi mặc định (1.0 → 0.1 giây/ô qua 10 level) là giá trị khởi tạo để điều chỉnh khi chơi thử ở các bài sau.
- Các luật điểm, lock delay, DAS/ARR lấy từ kế hoạch; nếu đổi, sửa `GameConfig.json` và chạy lại test.
- Không có asset mới cho bài 2; không tạo mục trong `docs/ASSET_HANDOFF.md`.

## Tích hợp ở bài 3
ConfigManager là class thuần C# singleton xuyên suốt game, dùng AddressableManager tải GameConfig rồi gọi GameConfigParser hiện có. Không đổi GameConfigParser/DTO/domain thành Manager; config chung và handle cần giữ thuộc scope session, không giải phóng theo scene.
- Scene keys: GameConfig.json có mục `scenes` (entry, mainMenu, gameplay). Mục này được map qua DTO `SceneKeysDto` sang `SceneKeys`; không hằng số tên scene nào nằm trong class.
