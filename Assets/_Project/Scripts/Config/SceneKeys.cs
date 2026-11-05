using System;
using System.Collections.Generic;

namespace NeonBlocks.Config
{
    /// <summary>
    /// Tên các scene của game lấy từ GameConfig: scene Entry (Build Settings) và address key của MainMenu, Gameplay.
    /// Bất biến, chỉ set qua constructor; kiểm tra không rỗng và không trùng nhau.
    /// </summary>
    public sealed class SceneKeys
    {
        /// <summary>Tên scene Entry trong Build Settings.</summary>
        public string EntryScene { get; }

        /// <summary>Address key của scene MainMenu.</summary>
        public string MainMenu { get; }

        /// <summary>Address key của scene Gameplay.</summary>
        public string Gameplay { get; }

        /// <summary>
        /// Tạo bộ tên scene và kiểm tra từng tên không rỗng, không trùng.
        /// </summary>
        /// <param name="entryScene">Tên scene Entry.</param>
        /// <param name="mainMenu">Address key MainMenu.</param>
        /// <param name="gameplay">Address key Gameplay.</param>
        public SceneKeys(string entryScene, string mainMenu, string gameplay)
        {
            if (string.IsNullOrWhiteSpace(entryScene) || string.IsNullOrWhiteSpace(mainMenu) || string.IsNullOrWhiteSpace(gameplay))
            {
                throw new ConfigValidationException("scenes.entry, scenes.mainMenu, scenes.gameplay không được rỗng.");
            }

            var distinct = new HashSet<string> { entryScene, mainMenu, gameplay };
            if (distinct.Count != 3)
            {
                throw new ConfigValidationException("scenes.entry, scenes.mainMenu, scenes.gameplay phải khác nhau.");
            }

            EntryScene = entryScene;
            MainMenu = mainMenu;
            Gameplay = gameplay;
        }
    }
}
