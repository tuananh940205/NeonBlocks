using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NeonBlocks.Config.Dto;

namespace NeonBlocks.Config
{
    /// <summary>
    /// Chuyển chuỗi JSON của GameConfig thành GameConfig bất biến: deserialize bằng Newtonsoft sang DTO chỉ đọc,
    /// rồi map và kiểm tra qua constructor. Hàm thuần, không đọc file và không giữ trạng thái;
    /// việc tải TextAsset qua Addressables thuộc bài 3.
    /// </summary>
    public static class GameConfigParser
    {
        /// <summary>
        /// Address key của file cấu hình chung; đặt bằng tên asset "GameConfig" để khớp quy tắc key của dự án.
        /// </summary>
        public const string AddressKey = "GameConfig";

        /// <summary>
        /// Phân tích chuỗi JSON thành GameConfig. JSON sai cú pháp ném JsonException từ Newtonsoft;
        /// JSON đúng cú pháp nhưng sai dữ liệu ném ConfigValidationException.
        /// </summary>
        /// <param name="json">Nội dung file JSON cấu hình.</param>
        /// <returns>Cấu hình đã được kiểm tra và bất biến.</returns>
        public static GameConfig Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ConfigValidationException("Nội dung JSON cấu hình rỗng.");
            }

            GameConfigDto dto = JsonConvert.DeserializeObject<GameConfigDto>(json);

            if (dto == null)
            {
                throw new ConfigValidationException("Không đọc được đối tượng cấu hình từ JSON.");
            }

            return Map(dto);
        }

        /// <summary>
        /// Chuyển DTO đã deserialize thành các đối tượng bất biến, kiểm tra thiếu trường trước khi gọi constructor.
        /// </summary>
        /// <param name="dto">DTO lấy trực tiếp từ JSON.</param>
        /// <returns>GameConfig tương ứng.</returns>
        private static GameConfig Map(GameConfigDto dto)
        {
            PopupKeysDto ui = Require(dto.ui, "ui");
            BoardDto boardDto = Require(dto.board, "board");
            GridPointDto spawnDto = Require(dto.spawn, "spawn");
            SceneKeysDto sceneDto = Require(dto.scenes, "scenes");
            ScoringDto scoringDto = Require(dto.scoring, "scoring");
            TimingDto timingDto = Require(dto.timing, "timing");

            var board = new BoardSettings(boardDto.width, boardDto.visibleHeight, boardDto.hiddenRows);
            var levels = Require(dto.levels, "levels")
                .Select(l => { var level = Require(l, "levels[]"); return new LevelData(level.level, level.gravitySeconds); })
                .ToList();
            var pieces = Require(dto.pieces, "pieces").Select(MapPiece).ToList();
            var scoring = new ScoringRules(Require(scoringDto.lineClear, "scoring.lineClear"), scoringDto.softDropPerCell, scoringDto.hardDropPerCell);
            var timing = new TimingRules(timingDto.lockDelaySeconds, timingDto.maxLockResets, timingDto.dasSeconds, timingDto.arrSeconds);

            return new GameConfig(
                board,
                new GridPoint(spawnDto.x, spawnDto.y),
                dto.nextQueueSize,
                dto.startLevel,
                dto.linesPerLevel,
                new SceneKeys(sceneDto.entry, sceneDto.mainMenu, sceneDto.gameplay),
                levels,
                scoring,
                timing,
                pieces, new PopupKeys(ui.mainMenu, ui.pause, ui.settings, ui.gameOver));
        }

        /// <summary>
        /// Chuyển một PieceDto thành PieceDefinition sau khi kiểm tra các ô và tâm xoay không bị thiếu.
        /// </summary>
        /// <param name="pieceDto">DTO của một khối.</param>
        /// <returns>PieceDefinition bất biến của khối đó.</returns>
        private static PieceDefinition MapPiece(PieceDto pieceDto)
        {
            Require(pieceDto, "pieces[]");
            List<GridPoint> cells = Require(pieceDto.cells, $"pieces[{pieceDto.id}].cells")
                .Select(c => { var p = Require(c, "cells[]"); return new GridPoint(p.x, p.y); })
                .ToList();
            FloatPointDto center = Require(pieceDto.rotationCenter, $"pieces[{pieceDto.id}].rotationCenter");

            return new PieceDefinition(pieceDto.id, cells, center.x, center.y);
        }

        /// <summary>
        /// Trả về giá trị nếu không null; ngược lại ném ConfigValidationException nêu tên trường bị thiếu.
        /// </summary>
        /// <typeparam name="T">Kiểu tham chiếu của giá trị.</typeparam>
        /// <param name="value">Giá trị cần kiểm tra.</param>
        /// <param name="fieldName">Tên trường dùng trong thông báo lỗi.</param>
        /// <returns>Chính giá trị đã kiểm tra.</returns>
        private static T Require<T>(T value, string fieldName) where T : class
        {
            return value ?? throw new ConfigValidationException($"Thiếu trường '{fieldName}'.");
        }
    }
}
