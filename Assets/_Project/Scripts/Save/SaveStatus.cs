namespace NeonBlocks.Save
{
    /// <summary>Trạng thái load/write tường minh cho UI.</summary>
    public enum SaveStatus { Loaded, Missing, Corrupt, UnsupportedVersion, IoFailure, Saved, WriteBlocked }
}