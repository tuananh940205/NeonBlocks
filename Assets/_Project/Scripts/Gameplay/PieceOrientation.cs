namespace NeonBlocks.Gameplay
{
    /// <summary>
    /// Bốn orientation chuẩn SRS theo chiều kim đồng hồ: Spawn (0), Right (R), Reverse (2) và Left (L).
    /// </summary>
    public enum PieceOrientation
    {
        /// <summary>Hướng spawn 0.</summary>
        Spawn = 0,

        /// <summary>Hướng R, quay 90 độ theo chiều kim đồng hồ từ Spawn.</summary>
        Right = 1,

        /// <summary>Hướng 2, quay 180 độ từ Spawn qua hai lần quay 90 độ.</summary>
        Reverse = 2,

        /// <summary>Hướng L, quay 90 độ ngược chiều kim đồng hồ từ Spawn.</summary>
        Left = 3
    }
}
