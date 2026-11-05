using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ServiceLocator {
   private static ServiceLocator instance;
   private Dictionary<Type, object> singletons;
   private Dictionary<string, GameObject> prefabsSingletons;
   private Dictionary<string, GameObject> prefabDict;
   private Dictionary<Type, string> nameTypeDict;

   public static ServiceLocator Instance => instance ?? (instance = new ServiceLocator());

   private ServiceLocator() {
      singletons = new Dictionary<Type, object>();
      prefabsSingletons=new Dictionary<string, GameObject>();
      prefabDict = new Dictionary<string, GameObject>();
      nameTypeDict = new Dictionary<Type, string>();
   }

   public void RegisterSingleton(object obj) {
      // Debug.Log($"Register Singleton {obj.GetType()}");
      if (!singletons.ContainsKey(obj.GetType())) {
         singletons[obj.GetType()] = obj;
         return;
      }

      Debug.LogError($"Already registered {obj.GetType()}");
      // singletons[obj.GetType()] = obj;
   }

   public void RegisterSingleton(Type type, object obj) {
      if (singletons.TryAdd(type, obj)) {
         return;
      }

      Debug.LogError($"Already registered {obj.GetType()}");
   }

   public void UnregisterSingleton(object obj) {
      if (singletons.ContainsKey(obj.GetType())) {
         singletons.Remove(obj.GetType());
      }
   }

   /// <summary>
   /// Kiểm tra đã có singleton đăng ký theo kiểu runtime này hay chưa; không ghi log lỗi khi thiếu.
   /// </summary>
   public bool IsRegistered(Type type) {
      return type != null && singletons.ContainsKey(type);
   }

   /// <summary>
   /// Xóa toàn bộ singleton đã đăng ký; dùng khi shutdown hoặc reset test vì Domain Reload tắt có thể giữ static instance.
   /// </summary>
   public void UnregisterAll() {
      singletons.Clear();
   }

   /// <summary>
   /// Lấy singleton theo kiểu runtime. Lớp kế thừa BaseSignal được tạo lazy ở lần Resolve đầu tiên và đăng ký tự động,
   /// nên luôn trả về instance. Các kiểu khác phải đã được đăng ký ở đầu session; thiếu đăng ký thì ghi log lỗi và trả null.
   /// </summary>
   /// <typeparam name="T">Kiểu singleton cần lấy.</typeparam>
   /// <returns>Instance đã đăng ký hoặc vừa tạo; null nếu kiểu không phải signal và chưa đăng ký.</returns>
   public T Resolve<T>() where T : class {
      if (singletons.TryGetValue(typeof(T), out object registered) && registered is T result1)
         return result1;

      if (typeof(BaseSignal).IsAssignableFrom(typeof(T))) {
         var signal = Activator.CreateInstance<T>();
         singletons[typeof(T)] = signal;
         RegisterSingleton(typeof(T), signal);
         return signal;
      }

      Debug.LogError($"Type {typeof(T)} not registered");
      return default(T);
   }
}