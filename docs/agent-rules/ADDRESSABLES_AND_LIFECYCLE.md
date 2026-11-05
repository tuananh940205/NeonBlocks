# Addressables, manager và vòng đời scene

## Addressables ownership

- Runtime asset dùng Addressables string key chính xác bằng tên asset hoặc root GameObject. `LevelData` chỉ lưu string key, không direct prefab/sprite reference.
- Chỉ `AddressableManager` release Addressables handle, kể cả khi thao tác được gọi qua `SceneAssetScope`.
- Gameplay/view/UI không gọi trực tiếp `Addressables.Release`, `ReleaseInstance` hay `UnloadSceneAsync`.
- Backend `UnityAddressableBackend` là hạ tầng nội bộ. Test EditMode dùng `IAddressableBackend` giả, không gọi Addressables thật.
- Mỗi resource gắn `AssetOwnerToken`; mỗi handle release đúng một lần. Kết quả đến sau cancellation/owner release phải release ngay.
- Lỗi Addressables/backend: `AddressableLoadException`; trạng thái owner/manager không hợp lệ: `InvalidOperationException`; cancellation: `OperationCanceledException`.

## Cache, prefab và pool

- Cache asset tách theo scope/owner. Cùng key + type trong một `SceneAssetScope` dùng chung một success handle; thất bại/cancel không cache.
- Không release prefab template khi `PoolManager` còn instance. Pool dọn active/inactive instance trước scope release template/handle.
- `PoolManager` là manager session; object scene và scope vẫn có lifecycle độc lập.
- Không tạo asset Addressable riêng nếu nó chỉ là dependency của Addressable prefab, trừ khi lesson yêu cầu.

## Scene lifecycle

- Chỉ Entry nằm trong scene list; scene khác tải Addressables.
- Rời scene đúng thứ tự: `SceneStopSignal(sceneKey)` → `SceneDisposeSignal(sceneKey)` → destroy/dispose/pool release → `SceneAssetScope.ReleaseAsync`.
- View nhận lifecycle signal phải so `gameObject.scene.name` với `sceneKey`; scene mới load Additive không được phản ứng với signal scene cũ.
- Popup/panel đóng gói thành Addressable prefab, instantiate async và release khi đóng; không đặt sẵn trong scene.
- Config chung thuộc session; `SceneAssetScope` thuộc scene. Không dispose manager session chỉ vì đổi scene.
