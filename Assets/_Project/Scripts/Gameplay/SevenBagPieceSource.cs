using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlocks.Config;

namespace NeonBlocks.Gameplay
{
    /// <summary>
    /// Nguồn tetromino 7-bag thuần C#: xáo đúng bảy PieceDefinition một lần mỗi bag và luôn giữ hàng đợi preview đủ kích thước.
    /// </summary>
    public sealed class SevenBagPieceSource
    {
        private const int PiecesPerBag = 7;
        private readonly IReadOnlyList<PieceDefinition> pieces;
        private readonly IPieceRandom random;
        private readonly Queue<PieceDefinition> nextPieces = new Queue<PieceDefinition>();
        private readonly Queue<PieceDefinition> pendingBagPieces = new Queue<PieceDefinition>();
        private readonly int previewSize;

        /// <summary>Số phần tử preview phải luôn hiển thị sau khi nguồn được khởi tạo hoặc lấy một piece.</summary>
        public int PreviewSize => previewSize;

        /// <summary>
        /// Snapshot chỉ đọc của các piece tiếp theo, từ piece sẽ được lấy trước đến cuối hàng đợi preview.
        /// </summary>
        public IReadOnlyList<PieceDefinition> NextPieces => Array.AsReadOnly(nextPieces.ToArray());

        /// <summary>
        /// Tạo nguồn 7-bag tái lập được từ seed.
        /// </summary>
        /// <param name="pieces">Đúng bảy PieceDefinition có mã không trùng.</param>
        /// <param name="seed">Seed cho thứ tự xáo bag.</param>
        /// <param name="previewSize">Số phần tử phải giữ trong preview, MVP dùng 5.</param>
        public SevenBagPieceSource(IEnumerable<PieceDefinition> pieces, int seed, int previewSize)
            : this(pieces, new SeededPieceRandom(seed), previewSize)
        {
        }

        /// <summary>
        /// Tạo nguồn 7-bag với RNG được truyền vào để test hoặc thay chiến lược tạo số.
        /// </summary>
        /// <param name="pieces">Đúng bảy PieceDefinition có mã không trùng.</param>
        /// <param name="random">RNG thuần C#, không được null.</param>
        /// <param name="previewSize">Số phần tử phải giữ trong preview, phải lớn hơn 0.</param>
        public SevenBagPieceSource(IEnumerable<PieceDefinition> pieces, IPieceRandom random, int previewSize)
        {
            List<PieceDefinition> pieceList = pieces?.ToList() ?? throw new ArgumentNullException(nameof(pieces));
            if (pieceList.Count != PiecesPerBag)
            {
                throw new ArgumentException("Nguồn 7-bag cần đúng bảy PieceDefinition.", nameof(pieces));
            }

            if (pieceList.Any(piece => piece == null) || pieceList.Select(piece => piece.Id).Distinct().Count() != pieceList.Count)
            {
                throw new ArgumentException("Các PieceDefinition của nguồn 7-bag phải khác null và có mã không trùng.", nameof(pieces));
            }

            if (previewSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(previewSize));
            }

            this.random = random ?? throw new ArgumentNullException(nameof(random));
            this.previewSize = previewSize;
            this.pieces = Array.AsReadOnly(pieceList.ToArray());
            FillPreview();
        }

        /// <summary>
        /// Lấy piece đầu hàng đợi và bù ngay preview về kích thước đã cấu hình.
        /// </summary>
        /// <returns>PieceDefinition tiếp theo để spawn.</returns>
        public PieceDefinition TakeNext()
        {
            PieceDefinition next = nextPieces.Dequeue();
            FillPreview();
            return next;
        }

        /// <summary>
        /// Bù hàng đợi preview bằng các bag mới cho đến khi đủ số phần tử yêu cầu.
        /// </summary>
        private void FillPreview()
        {
            while (nextPieces.Count < previewSize)
            {
                if (pendingBagPieces.Count == 0)
                {
                    foreach (PieceDefinition piece in CreateShuffledBag())
                    {
                        pendingBagPieces.Enqueue(piece);
                    }
                }

                nextPieces.Enqueue(pendingBagPieces.Dequeue());
            }
        }

        /// <summary>
        /// Tạo một bag mới gồm đúng bảy piece rồi xáo Fisher-Yates bằng RNG đã truyền vào.
        /// </summary>
        /// <returns>Danh sách bảy piece theo thứ tự xáo.</returns>
        private IReadOnlyList<PieceDefinition> CreateShuffledBag()
        {
            PieceDefinition[] bag = pieces.ToArray();
            for (int i = bag.Length - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                if (swapIndex < 0 || swapIndex > i)
                {
                    throw new InvalidOperationException("IPieceRandom trả chỉ số nằm ngoài phạm vi Fisher-Yates.");
                }

                PieceDefinition temporary = bag[i];
                bag[i] = bag[swapIndex];
                bag[swapIndex] = temporary;
            }

            return Array.AsReadOnly(bag);
        }
    }
}
