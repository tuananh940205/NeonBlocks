namespace NeonBlocks.Gameplay
{
    /// <summary>Event bất biến có session/sequence để không phát lặp khi yêu cầu snapshot.</summary>
    public sealed class GameplayFeedback
    {
        public string SessionId { get; }
        public long Sequence { get; }
        public FeedbackKind Kind { get; }
        public int Value { get; }
        /// <summary>Tạo một phản hồi đã commit.</summary>
        public GameplayFeedback(string sessionId, long sequence, FeedbackKind kind, int value = 0)
        { SessionId = sessionId; Sequence = sequence; Kind = kind; Value = value; }
    }
}