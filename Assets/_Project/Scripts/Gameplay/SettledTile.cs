using NeonBlocks.Config;
namespace NeonBlocks.Gameplay
{
    /// <summary>Ô đã khóa bất biến, giữ loại khối khi compact hàng.</summary>
    public sealed class SettledTile
    {
        public GridPoint Position { get; }
        public string PieceId { get; }
        /// <summary>Ghi tọa độ và danh tính sprite.</summary>
        public SettledTile(GridPoint position, string pieceId) { Position = position; PieceId = pieceId; }
    }
}