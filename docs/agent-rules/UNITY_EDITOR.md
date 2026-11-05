# Unity view, input và Editor integration

- View chỉ render/input; không đặt gameplay rule trong MonoBehaviour.
- Button listener luôn dùng `AddListener`/`RemoveListener`; không nối view với view bằng serialized reference.
- Input/job phải dừng khi scene rời; native data dispose trước pool/scope release.
- Bài 9 là mốc playtest desktop đầu tiên: renderer placeholder, bridge logic bài 4–8 và keyboard thuộc Bài 9. Bài 10 bổ sung touch Android/polish, không được dời keyboard sang đó.
- Khi thay scene prefab/canvas, kiểm tra không còn EventSystem hoặc AudioListener trùng sau khi transition hoàn tất.
- Hướng dẫn setup phải đủ Hierarchy, components, Inspector, values và test case; không sửa YAML scene bằng tay nếu không có quy trình Unity Editor an toàn.
