using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Hàm hỗ trợ test đọc file GameConfig.json thật của dự án và chuyển thành JObject để sửa từng trường trong test.
    /// </summary>
    public static class ConfigTestFiles
    {
        /// <summary>
        /// Đọc nội dung file GameConfig.json trong Assets/_Project/Configs.
        /// </summary>
        /// <returns>Chuỗi JSON của file cấu hình chung.</returns>
        public static string ReadGameConfigJson()
        {
            return File.ReadAllText(Path.Combine(Application.dataPath, "_Project/Configs/GameConfig.json"));
        }

        /// <summary>
        /// Tạo JObject mới từ file cấu hình thật; mỗi lần gọi là một bản sao độc lập để test sửa một trường.
        /// </summary>
        /// <returns>JObject đại diện cho cấu hình hợp lệ.</returns>
        public static JObject LoadValidJson()
        {
            return JObject.Parse(ReadGameConfigJson());
        }
    }
}
