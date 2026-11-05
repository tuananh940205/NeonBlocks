using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO tên scene trong JSON: tên scene Entry trong Build Settings và address key của scene Addressables. Trường chỉ đọc, gán qua constructor.
    /// </summary>
    public sealed class SceneKeysDto
    {
        /// <summary>Tên scene Entry trong Build Settings.</summary>
        public readonly string entry;

        /// <summary>Address key của scene MainMenu.</summary>
        public readonly string mainMenu;

        /// <summary>Address key của scene Gameplay.</summary>
        public readonly string gameplay;

        /// <summary>
        /// Constructor để Newtonsoft gán tên scene từ JSON.
        /// </summary>
        /// <param name="entry">Tên scene Entry.</param>
        /// <param name="mainMenu">Address key MainMenu.</param>
        /// <param name="gameplay">Address key Gameplay.</param>
        [JsonConstructor]
        public SceneKeysDto(string entry, string mainMenu, string gameplay)
        {
            this.entry = entry;
            this.mainMenu = mainMenu;
            this.gameplay = gameplay;
        }
    }
}
