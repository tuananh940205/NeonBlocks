using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NeonBlocks.Config;
using UnityEngine;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử bất biến và ràng buộc tài nguyên của mọi kiểu trong NeonBlocks.Config (gồm cả DTO):
    /// field chỉ đọc, không tham chiếu UnityEngine.Object; và tra cứu level lấy level cuối khi vượt bảng.
    /// </summary>
    public class GameConfigRulesTests
    {
        /// <summary>
        /// Kiểm tra mọi field instance của các kiểu trong NeonBlocks.Config (gồm DTO) đều là readonly.
        /// </summary>
        [Test]
        public void ConfigTypes_HaveOnlyReadonlyInstanceFields()
        {
            foreach (Type type in ConfigTypes())
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                foreach (FieldInfo field in fields)
                {
                    Assert.IsTrue(field.IsInitOnly, $"{type.FullName}.{field.Name} phải readonly.");
                }
            }
        }

        /// <summary>
        /// Kiểm tra không có kiểu cấu hình nào giữ tham chiếu trực tiếp tới UnityEngine.Object
        /// (prefab, sprite, asset); chỉ được giữ string key.
        /// </summary>
        [Test]
        public void ConfigTypes_HaveNoUnityObjectReferences()
        {
            foreach (Type type in ConfigTypes())
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    Assert.IsFalse(typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType), $"{type.FullName}.{field.Name} không được tham chiếu UnityEngine.Object.");
                }

                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    Assert.IsFalse(typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType), $"{type.FullName}.{property.Name} không được tham chiếu UnityEngine.Object.");
                }
            }
        }

        /// <summary>
        /// Kiểm tra GetLevel trả đúng level trong bảng và trả level cuối khi level vượt bảng.
        /// </summary>
        [Test]
        public void GetLevel_BeyondTable_ReturnsLastLevel()
        {
            GameConfig config = GameConfigParser.Parse(ConfigTestFiles.ReadGameConfigJson());

            Assert.AreEqual(1f, config.GetLevel(1).GravitySeconds, 0.0001f);
            Assert.AreEqual(0.1f, config.GetLevel(10).GravitySeconds, 0.0001f);
            Assert.AreEqual(0.1f, config.GetLevel(99).GravitySeconds, 0.0001f);
        }

        /// <summary>
        /// Kiểm tra GetLevel ném ArgumentOutOfRangeException khi level nhỏ hơn startLevel.
        /// </summary>
        [Test]
        public void GetLevel_BelowStartLevel_Throws()
        {
            GameConfig config = GameConfigParser.Parse(ConfigTestFiles.ReadGameConfigJson());

            Assert.Throws<ArgumentOutOfRangeException>(() => config.GetLevel(0));
        }

        /// <summary>
        /// Lấy các kiểu data/domain và DTO của NeonBlocks.Config, bỏ qua Manager có state runtime và kiểu do trình biên dịch sinh ra.
        /// </summary>
        /// <returns>Danh sách kiểu cấu hình cần kiểm tra.</returns>
        private static Type[] ConfigTypes()
        {
            return typeof(GameConfig).Assembly.GetTypes()
                .Where(t => t.Namespace != null
                    && t.Namespace.StartsWith("NeonBlocks.Config", StringComparison.Ordinal)
                    && !t.Name.EndsWith("Manager", StringComparison.Ordinal)
                    && !t.IsEnum
                    && !t.Name.Contains("<"))
                .ToArray();
        }
    }
}
