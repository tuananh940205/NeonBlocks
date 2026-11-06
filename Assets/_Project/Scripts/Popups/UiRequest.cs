namespace NeonBlocks.Popups
{
    /// <summary>Yêu cầu UI có scene key và payload settings bất biến.</summary>
    public sealed class UiRequest
    {
        public string SceneKey { get; }
        public UiAction Action { get; }
        public float Music { get; }
        public float Sfx { get; }
        public bool Muted { get; }
        /// <summary>Tạo yêu cầu từ prefab thuộc scene.</summary>
        public UiRequest(string sceneKey, UiAction action, float music = 1f, float sfx = 1f, bool muted = false)
        { SceneKey = sceneKey; Action = action; Music = music; Sfx = sfx; Muted = muted; }
    }
}