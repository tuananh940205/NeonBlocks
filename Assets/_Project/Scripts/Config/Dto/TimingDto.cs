using Newtonsoft.Json;

namespace NeonBlocks.Config.Dto
{
    /// <summary>
    /// DTO luật thời gian trong JSON: lock delay, số lần reset, DAS và ARR. Trường chỉ đọc, gán qua constructor.
    /// </summary>
    public sealed class TimingDto
    {
        /// <summary>Lock delay, tính bằng giây.</summary>
        public readonly float lockDelaySeconds;

        /// <summary>Số lần reset lock delay tối đa.</summary>
        public readonly int maxLockResets;

        /// <summary>DAS, tính bằng giây.</summary>
        public readonly float dasSeconds;

        /// <summary>ARR, tính bằng giây.</summary>
        public readonly float arrSeconds;

        /// <summary>
        /// Constructor để Newtonsoft gán luật thời gian từ JSON.
        /// </summary>
        /// <param name="lockDelaySeconds">Lock delay.</param>
        /// <param name="maxLockResets">Số lần reset tối đa.</param>
        /// <param name="dasSeconds">DAS.</param>
        /// <param name="arrSeconds">ARR.</param>
        [JsonConstructor]
        public TimingDto(float lockDelaySeconds, int maxLockResets, float dasSeconds, float arrSeconds)
        {
            this.lockDelaySeconds = lockDelaySeconds;
            this.maxLockResets = maxLockResets;
            this.dasSeconds = dasSeconds;
            this.arrSeconds = arrSeconds;
        }
    }
}
