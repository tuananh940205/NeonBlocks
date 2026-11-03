using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NeonBlocks.Config;

namespace NeonBlocks.Tests
{
    /// <summary>
    /// Kiểm thử GameConfigParser (Newtonsoft): JSON hợp lệ được map đúng; JSON sai cú pháp và dữ liệu sai đều bị từ chối.
    /// Mỗi test sửa một trường trên JObject của file thật rồi parse lại chuỗi kết quả.
    /// </summary>
    public class GameConfigParserTests
    {
        /// <summary>
        /// Kiểm tra file cấu hình thật được parse đúng: bảng 10×20 cộng 4 hàng ẩn, spawn (3, 20), 7 khối, 10 level, điểm và timing.
        /// </summary>
        [Test]
        public void Parse_ProjectConfigFile_MapsAllSections()
        {
            GameConfig config = GameConfigParser.Parse(ConfigTestFiles.ReadGameConfigJson());

            Assert.AreEqual(10, config.Board.Width);
            Assert.AreEqual(20, config.Board.VisibleHeight);
            Assert.AreEqual(4, config.Board.HiddenRows);
            Assert.AreEqual(24, config.Board.TotalHeight);
            Assert.AreEqual(new GridPoint(3, 20), config.SpawnOrigin);
            Assert.AreEqual(5, config.NextQueueSize);
            Assert.AreEqual(1, config.StartLevel);
            Assert.AreEqual(10, config.LinesPerLevel);
            Assert.AreEqual(10, config.Levels.Count);
            Assert.AreEqual(7, config.Pieces.Count);
            CollectionAssert.AreEqual(new[] { 100, 300, 500, 800 }, config.Scoring.LineClearPoints);
            Assert.AreEqual(1, config.Scoring.SoftDropPointsPerCell);
            Assert.AreEqual(2, config.Scoring.HardDropPointsPerCell);
            Assert.AreEqual(0.5f, config.Timing.LockDelaySeconds, 0.0001f);
            Assert.AreEqual(15, config.Timing.MaxLockResets);
            Assert.AreEqual(0.15f, config.Timing.DasSeconds, 0.0001f);
            Assert.AreEqual(0.05f, config.Timing.ArrSeconds, 0.0001f);
        }

        /// <summary>
        /// Kiểm tra JSON sai cú pháp bị từ chối bằng JsonException của Newtonsoft.
        /// </summary>
        [Test]
        public void Parse_MalformedJson_ThrowsJsonException()
        {
            Assert.That(() => GameConfigParser.Parse("{ \"board\": "), Throws.InstanceOf<JsonException>());
        }

        /// <summary>
        /// Kiểm tra chuỗi chỉ có khoảng trắng bị từ chối bằng ConfigValidationException.
        /// </summary>
        [Test]
        public void Parse_BlankString_Throws()
        {
            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse("   "));
        }

        /// <summary>
        /// Kiểm tra thiếu danh sách pieces bị từ chối và thông báo nêu đúng tên trường.
        /// </summary>
        [Test]
        public void Parse_MissingPieces_ThrowsNamingField()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root.Remove("pieces");

            var ex = Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
            StringAssert.Contains("pieces", ex.Message);
        }

        /// <summary>
        /// Kiểm tra số khối khác 7 bị từ chối.
        /// </summary>
        [Test]
        public void Parse_SixPieces_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            ((JArray)root["pieces"]).RemoveAt(5);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra hai khối cùng mã bị từ chối.
        /// </summary>
        [Test]
        public void Parse_DuplicatePieceId_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["pieces"][1]["id"] = new JValue("I");

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra mã khối ngoài tập I, O, T, J, L, S, Z bị từ chối.
        /// </summary>
        [Test]
        public void Parse_UnknownPieceId_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["pieces"][0]["id"] = new JValue("X");

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra ô trùng trong một khối bị từ chối.
        /// </summary>
        [Test]
        public void Parse_DuplicateCellInPiece_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["pieces"][0]["cells"][1] = root["pieces"][0]["cells"][0].DeepClone();

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra ô nằm ngoài khung 4×4 của khối bị từ chối.
        /// </summary>
        [Test]
        public void Parse_CellOutsideBox_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["pieces"][0]["cells"][0]["x"] = new JValue(4);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra khối có số ô khác 4 bị từ chối.
        /// </summary>
        [Test]
        public void Parse_PieceWithThreeCells_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            ((JArray)root["pieces"][0]["cells"]).RemoveAt(3);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra khối có các ô không liền nhau bị từ chối.
        /// </summary>
        [Test]
        public void Parse_DisconnectedPiece_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["pieces"][0]["cells"] = JArray.Parse("[{\"x\":0,\"y\":0},{\"x\":0,\"y\":1},{\"x\":3,\"y\":3},{\"x\":3,\"y\":2}]");

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra tâm xoay nằm ngoài khung 4×4 bị từ chối.
        /// </summary>
        [Test]
        public void Parse_RotationCenterOutsideBox_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["pieces"][0]["rotationCenter"]["x"] = new JValue(4.5);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra chiều rộng bảng âm bị từ chối.
        /// </summary>
        [Test]
        public void Parse_NegativeBoardWidth_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["board"]["width"] = new JValue(-10);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra số hàng ẩn âm bị từ chối.
        /// </summary>
        [Test]
        public void Parse_NegativeHiddenRows_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["board"]["hiddenRows"] = new JValue(-1);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra khung spawn tràn ra ngoài bảng bị từ chối.
        /// </summary>
        [Test]
        public void Parse_SpawnOutsideBoard_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["spawn"]["x"] = new JValue(8);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra bảng điểm xóa hàng thiếu phần tử bị từ chối.
        /// </summary>
        [Test]
        public void Parse_LineClearWithThreeEntries_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["scoring"]["lineClear"] = JArray.Parse("[100,300,500]");

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra điểm xóa hàng âm bị từ chối.
        /// </summary>
        [Test]
        public void Parse_NegativeLineClearPoints_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["scoring"]["lineClear"][0] = new JValue(-100);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra điểm soft drop âm bị từ chối.
        /// </summary>
        [Test]
        public void Parse_NegativeSoftDropPoints_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["scoring"]["softDropPerCell"] = new JValue(-1);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra tốc độ rơi bằng 0 bị từ chối.
        /// </summary>
        [Test]
        public void Parse_ZeroGravity_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["levels"][0]["gravitySeconds"] = new JValue(0.0);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra level không bắt đầu đúng tại startLevel bị từ chối.
        /// </summary>
        [Test]
        public void Parse_LevelsNotStartingAtStartLevel_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["startLevel"] = new JValue(2);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra lock delay không dương bị từ chối.
        /// </summary>
        [Test]
        public void Parse_NonPositiveLockDelay_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["timing"]["lockDelaySeconds"] = new JValue(0.0);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra DAS âm bị từ chối.
        /// </summary>
        [Test]
        public void Parse_NegativeDas_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["timing"]["dasSeconds"] = new JValue(-0.1);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }

        /// <summary>
        /// Kiểm tra số hàng để tăng level bằng 0 bị từ chối.
        /// </summary>
        [Test]
        public void Parse_ZeroLinesPerLevel_Throws()
        {
            JObject root = ConfigTestFiles.LoadValidJson();
            root["linesPerLevel"] = new JValue(0);

            Assert.Throws<ConfigValidationException>(() => GameConfigParser.Parse(root.ToString()));
        }
    }
}
