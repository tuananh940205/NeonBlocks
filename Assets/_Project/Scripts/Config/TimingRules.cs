namespace NeonBlocks.Config
{
    /// <summary>
    /// Thời gian điều khiển và khóa khối: lock delay, số lần reset tối đa, DAS và ARR (giây).
    /// Bất biến, chỉ set qua constructor.
    /// </summary>
    public sealed class TimingRules
    {
        /// <summary>Thời gian chờ sau khi khối chạm đất trước khi khóa, tính bằng giây; luôn &gt; 0.</summary>
        public float LockDelaySeconds { get; }

        /// <summary>Số lần thao tác hợp lệ được phép làm reset lock delay; không âm.</summary>
        public int MaxLockResets { get; }

        /// <summary>Độ trễ trước khi lặp di chuyển ngang khi giữ phím (DAS), tính bằng giây; không âm.</summary>
        public float DasSeconds { get; }

        /// <summary>Khoảng cách giữa các lần lặp di chuyển ngang (ARR), tính bằng giây; không âm.</summary>
        public float ArrSeconds { get; }

        /// <summary>
        /// Tạo cấu hình thời gian và kiểm tra giá trị không hợp lệ.
        /// </summary>
        /// <param name="lockDelaySeconds">Lock delay, phải &gt; 0.</param>
        /// <param name="maxLockResets">Số lần reset tối đa, không âm.</param>
        /// <param name="dasSeconds">DAS, không âm.</param>
        /// <param name="arrSeconds">ARR, không âm.</param>
        public TimingRules(float lockDelaySeconds, int maxLockResets, float dasSeconds, float arrSeconds)
        {
            if (lockDelaySeconds <= 0f)
            {
                throw new ConfigValidationException($"timing.lockDelaySeconds phải > 0; giá trị hiện tại: {lockDelaySeconds}.");
            }

            if (maxLockResets < 0)
            {
                throw new ConfigValidationException($"timing.maxLockResets không được âm; giá trị hiện tại: {maxLockResets}.");
            }

            if (dasSeconds < 0f)
            {
                throw new ConfigValidationException($"timing.dasSeconds không được âm; giá trị hiện tại: {dasSeconds}.");
            }

            if (arrSeconds < 0f)
            {
                throw new ConfigValidationException($"timing.arrSeconds không được âm; giá trị hiện tại: {arrSeconds}.");
            }

            LockDelaySeconds = lockDelaySeconds;
            MaxLockResets = maxLockResets;
            DasSeconds = dasSeconds;
            ArrSeconds = arrSeconds;
        }
    }
}
