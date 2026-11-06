using Newtonsoft.Json;
namespace NeonBlocks.Save.Dto
{
    /// <summary>Schema JSON version 1, constructor-only.</summary>
    public sealed class SaveDataDto
    {
        [JsonProperty(Required = Required.Always)] public readonly int version;
        [JsonProperty(Required = Required.Always)] public readonly int bestScore;
        [JsonProperty(Required = Required.Always)] public readonly float musicVolume;
        [JsonProperty(Required = Required.Always)] public readonly float sfxVolume;
        [JsonProperty(Required = Required.Always)] public readonly bool muted;
        /// <summary>Gán DTO qua Newtonsoft theo đúng key JSON.</summary>
        [JsonConstructor]
        public SaveDataDto(int version, int bestScore, float musicVolume, float sfxVolume, bool muted)
        { this.version = version; this.bestScore = bestScore; this.musicVolume = musicVolume; this.sfxVolume = sfxVolume; this.muted = muted; }
    }
}