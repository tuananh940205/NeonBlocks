namespace NeonBlocks.Audio
{
    /// <summary>Trạng thái phát nhạc cho adapter Unity.</summary>
    public sealed class AudioState
    {
        public AudioSettings Settings { get; }
        public bool Gameplay { get; }
        public float Duck { get; }
        /// <summary>Đóng gói gain và lựa chọn track.</summary>
        public AudioState(AudioSettings settings, bool gameplay, float duck) { Settings = settings; Gameplay = gameplay; Duck = duck; }
    }
}