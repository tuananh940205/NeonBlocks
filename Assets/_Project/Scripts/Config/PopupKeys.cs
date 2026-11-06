using System;
using System.Linq;
namespace NeonBlocks.Config
{
    /// <summary>
    /// Key Addressable panel trong config chung.
    /// </summary>
    public sealed class PopupKeys
    {
        public string MainMenu { get; }
        public string Pause { get; }
        public string Settings { get; }
        public string GameOver { get; }
        /// <summary>
        /// Kiểm tra key không rỗng và không trùng.
        /// </summary>
        public PopupKeys(string mainMenu, string pause, string settings, string gameOver)
        {
            string[] keys = { mainMenu, pause, settings, gameOver };
            if (keys.Any(string.IsNullOrWhiteSpace) || keys.Distinct().Count() != 4)
            { throw new ConfigValidationException("ui: cần bốn prefab key khác nhau, không rỗng."); }
            MainMenu = mainMenu; Pause = pause; Settings = settings; GameOver = gameOver;
        }
    }
}