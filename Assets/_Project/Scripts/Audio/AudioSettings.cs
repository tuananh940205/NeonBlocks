namespace NeonBlocks.Audio
{
    /// <summary>Giá trị âm lượng bất biến; preview không ghi file.</summary>
    public sealed class AudioSettings
    {
        public float Music { get; }
        public float Sfx { get; }
        public bool Muted { get; }
        /// <summary>Giới hạn gain về miền hợp lệ.</summary>
        public AudioSettings(float music, float sfx, bool muted)
        { Music = System.Math.Max(0, System.Math.Min(1, music)); Sfx = System.Math.Max(0, System.Math.Min(1, sfx)); Muted = muted; }
    }
}