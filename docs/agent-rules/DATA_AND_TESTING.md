# Serialization, data và testing

## JSON và tính bất biến

- Mọi serialize/deserialize mới dùng Newtonsoft.Json (`com.unity.nuget.newtonsoft-json` 3.2.2); không dùng `JsonUtility`.
- Asmdef có JSON, kể cả test, khai báo `precompiledReferences: ["Newtonsoft.Json.dll"]`.
- Mọi field mới mặc định readonly và gán duy nhất trong constructor. Ngoại lệ là runtime private state của service/manager (scope, cancellation, trạng thái lifecycle); chúng không public và chỉ gán ở một nơi phù hợp.
- DTO ở namespace `*.Dto`, `sealed`, `public readonly` field, constructor `[JsonConstructor]`; parameter trùng key JSON; không setter.
- Domain validate trong constructor và ném đúng exception (`ConfigValidationException` cho config sai). Mapping DTO → domain là hàm thuần; DTO không đi vào gameplay.
- Config chung dùng một JSON file; key Addressables bằng tên asset và không chứa direct asset reference.

## Test

- Biến thể JSON test: parse `JObject`, sửa trường, serialize/parse lại; không reflection sửa DTO/domain.
- JSON sai cú pháp kỳ vọng `JsonException`; dữ liệu sai kỳ vọng `ConfigValidationException`.
- `GameConfigRulesTests` kiểm tra readonly và không có `UnityEngine.Object`; khi reflection field luôn dùng `BindingFlags.DeclaredOnly`.
- EditMode test MonoBehaviour không dựa vào `Awake` sau `AddComponent`; tạo object inactive, gọi method khởi tạo tường minh như `TryBeginSession()`.
- Try/catch chỉ dùng cho: CancellationTokenSource; test kỳ vọng exception (catch đúng loại và có timeout); Task service chuẩn hóa mọi failure; không dùng try/catch để bắt Unity log.
- Test kích hoạt error log dùng `LogAssert.Expect` với `LogType` và exact regex; không bỏ qua mọi lỗi.
