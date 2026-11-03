using System;
using System.Collections.Generic;
using System.Linq;

namespace NeonBlocks.Config
{
    /// <summary>
    /// Luật tính điểm: điểm cho 1, 2, 3, 4 hàng xóa cùng lúc (trước khi nhân level), điểm soft drop và hard drop mỗi ô.
    /// Bất biến, chỉ set qua constructor.
    /// </summary>
    public sealed class ScoringRules
    {
        /// <summary>Số phần tử bắt buộc của bảng điểm xóa hàng: 1, 2, 3, 4 hàng.</summary>
        public const int LineClearEntries = 4;

        /// <summary>Điểm cơ bản cho 1, 2, 3, 4 hàng; chỉ đọc, phần tử thứ i tương ứng i+1 hàng.</summary>
        public IReadOnlyList<int> LineClearPoints { get; }

        /// <summary>Điểm mỗi ô khi soft drop.</summary>
        public int SoftDropPointsPerCell { get; }

        /// <summary>Điểm mỗi ô khi hard drop.</summary>
        public int HardDropPointsPerCell { get; }

        /// <summary>
        /// Tạo luật điểm và kiểm tra đúng 4 mức điểm xóa hàng, không có giá trị âm.
        /// </summary>
        /// <param name="lineClearPoints">Đúng 4 giá trị cho 1, 2, 3, 4 hàng.</param>
        /// <param name="softDropPointsPerCell">Điểm mỗi ô soft drop, không âm.</param>
        /// <param name="hardDropPointsPerCell">Điểm mỗi ô hard drop, không âm.</param>
        public ScoringRules(IEnumerable<int> lineClearPoints, int softDropPointsPerCell, int hardDropPointsPerCell)
        {
            List<int> points = lineClearPoints?.ToList() ?? throw new ConfigValidationException("scoring.lineClear bị thiếu.");

            if (points.Count != LineClearEntries)
            {
                throw new ConfigValidationException($"scoring.lineClear phải có đúng {LineClearEntries} giá trị; hiện tại: {points.Count}.");
            }

            if (points.Any(p => p < 0))
            {
                throw new ConfigValidationException("scoring.lineClear không được có giá trị âm.");
            }

            if (softDropPointsPerCell < 0)
            {
                throw new ConfigValidationException($"scoring.softDropPerCell không được âm; giá trị hiện tại: {softDropPointsPerCell}.");
            }

            if (hardDropPointsPerCell < 0)
            {
                throw new ConfigValidationException($"scoring.hardDropPerCell không được âm; giá trị hiện tại: {hardDropPointsPerCell}.");
            }

            LineClearPoints = Array.AsReadOnly(points.ToArray());
            SoftDropPointsPerCell = softDropPointsPerCell;
            HardDropPointsPerCell = hardDropPointsPerCell;
        }
    }
}
