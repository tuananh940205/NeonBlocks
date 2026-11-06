using Newtonsoft.Json;
namespace NeonBlocks.Config.Dto
{
    /// <summary>DTO key prefab UI.</summary>
    public sealed class PopupKeysDto
    {
        public readonly string mainMenu, pause, settings, gameOver;
        /// <summary>Nhận key JSON theo tên field.</summary>
        [JsonConstructor]
        public PopupKeysDto(string mainMenu, string pause, string settings, string gameOver)
        { this.mainMenu = mainMenu; this.pause = pause; this.settings = settings; this.gameOver = gameOver; }
    }
}