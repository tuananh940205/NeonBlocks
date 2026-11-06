using System;
namespace NeonBlocks.Save
{
    /// <summary>Dữ liệu lưu bất biến đã kiểm tra, không chứa Unity Object.</summary>
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;
        public int BestScore { get; }
        public float MusicVolume { get; }
        public float SfxVolume { get; }
        public bool Muted { get; }
        /// <summary>Tạo save hợp lệ, không cho điểm âm hoặc volume NaN/vượt giới hạn.</summary>
        public SaveData(int bestScore = 0, float musicVolume = 1f, float sfxVolume = 1f, bool muted = false)
        {
            if (bestScore < 0 || float.IsNaN(musicVolume) || float.IsNaN(sfxVolume) || musicVolume < 0 || musicVolume > 1 || sfxVolume < 0 || sfxVolume > 1)
            { throw new ArgumentOutOfRangeException(nameof(bestScore), "Save không hợp lệ."); }
            BestScore = bestScore; MusicVolume = musicVolume; SfxVolume = sfxVolume; Muted = muted;
        }
    }
}