using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace zy_996map
{
    public class MapData
    {
        [JsonPropertyName("ver")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("width")]
        public ushort Width { get; set; }

        [JsonPropertyName("height")]
        public ushort Height { get; set; }

        [JsonPropertyName("updateDate")]
        public DateTime UpdateDate { get; set; }

        [JsonPropertyName("reserved")]
        public byte[] Reserved { get; set; } = new byte[24];

        [JsonPropertyName("matrix")]
        public MapCell[][] Matrix { get; set; } = Array.Empty<MapCell[]>();

       

        [JsonPropertyName("extraData")]
        public byte[] ExtraData { get; set; } = Array.Empty<byte>();
    }

    public class MapCell
    {
        [JsonPropertyName("bkImg")]
        public ushort BkImg { get; set; }

        [JsonPropertyName("midImg")]
        public ushort MidImg { get; set; }

        [JsonPropertyName("frImg")]
        public ushort FrImg { get; set; }

        [JsonPropertyName("doorIndex")]
        public byte DoorIndex { get; set; }

        [JsonPropertyName("doorOffset")]
        public byte DoorOffset { get; set; }

        [JsonPropertyName("aniFrame")]
        public byte AniFrame { get; set; }

        [JsonPropertyName("aniTick")]
        public byte AniTick { get; set; }

        [JsonPropertyName("area")]
        public byte Area { get; set; }

        [JsonPropertyName("light")]
        public byte Light { get; set; }

        // 计算属性
        [JsonIgnore]
        public bool CanWalk => (BkImg & 0x8000) == 0 && (FrImg & 0x8000) == 0;

        [JsonIgnore]
        public bool HasDoor => (DoorIndex & 0x80) != 0;

        [JsonIgnore]
        public bool IsDoorOpen => (DoorOffset & 0x80) != 0;

        [JsonIgnore]
        public ushort BkImageIndex => (ushort)(BkImg & 0x7FFF);

        [JsonIgnore]
        public ushort FrImageIndex => (ushort)(FrImg & 0x7FFF);
    }

    // 地图对象（NPC、怪物、物品等）
    public class MapObject
    {
        [JsonPropertyName("x")]
        public ushort X { get; set; }

        [JsonPropertyName("y")]
        public ushort Y { get; set; }

        [JsonPropertyName("type")]
        public ushort Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("data")]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    // 地面砖块
    public class SmTile
    {
        [JsonPropertyName("x")]
        public ushort X { get; set; }

        [JsonPropertyName("y")]
        public ushort Y { get; set; }

        [JsonPropertyName("imageIndex")]
        public ushort ImageIndex { get; set; }

        [JsonPropertyName("data")]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    // 砖块
    public class Tile
    {
        [JsonPropertyName("x")]
        public ushort X { get; set; }

        [JsonPropertyName("y")]
        public ushort Y { get; set; }

        [JsonPropertyName("imageIndex")]
        public ushort ImageIndex { get; set; }

        [JsonPropertyName("data")]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }

    public class ModernMapParser
    {
        private MapData mapData = new MapData();
        private byte[] originalReserved = new byte[24];

        public bool LoadBinaryMap(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"地图文件不存在: {filePath}");
                return false;
            }
            Console.WriteLine($"filePath:{filePath}");
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var reader = new BinaryReader(fs))
                {
                    ReadHeader(reader);
                    ReadMapMatrix(reader);
                    ReadExtraData(reader);

                    Console.WriteLine($"成功加载二进制地图: {mapData.Title}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取二进制地图文件时出错: {ex.Message}");
                return false;
            }
        }

        private void ReadHeader(BinaryReader reader)
        {
            mapData.Width = reader.ReadUInt16();
            mapData.Height = reader.ReadUInt16();

        
            byte[] titleBytes = reader.ReadBytes(16);
            mapData.Title = Encoding.ASCII.GetString(titleBytes).TrimEnd('\0');

            double dateValue = reader.ReadDouble();
            mapData.UpdateDate = DateTime.FromOADate(dateValue);

            originalReserved = reader.ReadBytes(24);
            Array.Copy(originalReserved, mapData.Reserved, 24);
            Console.WriteLine($" Width:{mapData.Width} Height:{mapData.Height}   Title:{mapData.Title}  UpdateDate:{mapData.UpdateDate} originalReserved:{Encoding.ASCII.GetString(originalReserved).TrimEnd('\0')}");
        }

        private void ReadMapMatrix(BinaryReader reader)
        {
            mapData.Matrix = new MapCell[mapData.Height][];
            for (int i = 0; i < mapData.Height; i++)
            {
                mapData.Matrix[i] = new MapCell[mapData.Width];
            }

            int elementSize = 12;
            long columnSize = elementSize * mapData.Height;

            for (int x = 0; x < mapData.Width; x++)
            {
                long position = 52 + (columnSize * x);

                if (position >= reader.BaseStream.Length)
                    break;

                reader.BaseStream.Seek(position, SeekOrigin.Begin);

                for (int y = 0; y < mapData.Height; y++)
                {
                    if (reader.BaseStream.Position + elementSize > reader.BaseStream.Length)
                        break;

                    mapData.Matrix[y][x] = ReadMapCell(reader);
                }
            }
        }

        private void ReadExtraData(BinaryReader reader)
        {
            long mapDataEnd = 52 + (12 * mapData.Width * mapData.Height);
            long extraDataSize = reader.BaseStream.Length - mapDataEnd;

            if (extraDataSize > 0)
            {
                Console.WriteLine($"发现额外数据: {extraDataSize} 字节");

                reader.BaseStream.Seek(mapDataEnd, SeekOrigin.Begin);
                mapData.ExtraData = reader.ReadBytes((int)extraDataSize);

               
            }
            else
            {
                mapData.ExtraData = Array.Empty<byte>();
                Console.WriteLine("没有额外数据");
            }
        }

      
        private MapCell ReadMapCell(BinaryReader reader)
        {
            return new MapCell
            {
                BkImg = reader.ReadUInt16(),
                MidImg = reader.ReadUInt16(),
                FrImg = reader.ReadUInt16(),
                DoorIndex = reader.ReadByte(),
                DoorOffset = reader.ReadByte(),
                AniFrame = reader.ReadByte(),
                AniTick = reader.ReadByte(),
                Area = reader.ReadByte(),
                Light = reader.ReadByte()
            };
        }

        public bool SaveAsJson(string jsonFilePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                string json = JsonSerializer.Serialize(mapData, options);
                File.WriteAllText(jsonFilePath, json, Encoding.UTF8);

                long fileSize = new FileInfo(jsonFilePath).Length;
                Console.WriteLine($"成功保存为JSON: {jsonFilePath}");
                Console.WriteLine($"JSON文件大小: {fileSize} 字节");

                // 显示JSON结构信息
                DisplayJsonInfo();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存JSON文件时出错: {ex.Message}");
                return false;
            }
        }

        private void DisplayJsonInfo()
        {
            Console.WriteLine($"\n=== JSON结构信息 ===");
            Console.WriteLine($"地图尺寸: {mapData.Width} x {mapData.Height}");
            Console.WriteLine($"单元格总数: {mapData.Width * mapData.Height}");
            Console.WriteLine($"额外数据大小: {mapData.ExtraData.Length} 字节");

            // 显示一些示例数据
            if (mapData.Matrix.Length > 0)
            {
                Console.WriteLine($"\n前3个单元格示例:");
                for (int y = 0; y < Math.Min((byte)3, mapData.Height); y++)
                {
                    for (int x = 0; x < Math.Min((byte)3, mapData.Width); x++)
                    {
                        var cell = mapData.Matrix[y][x];
                        Console.WriteLine($"  [{y},{x}]: Bk=0x{cell.BkImg:X4}, Mid=0x{cell.MidImg:X4}, Fr=0x{cell.FrImg:X4}");
                    }
                }
            }

        }

        public bool SaveAsBinary(string binaryFilePath)
        {
            try
            {
                using (var fs = new FileStream(binaryFilePath, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(fs))
                {
                    WriteBinaryHeader(writer);
                    WriteBinaryMapData(writer);
                    WriteExtraData(writer);

                    long fileSize = fs.Length;
                    Console.WriteLine($"成功保存为二进制: {binaryFilePath}");
                    Console.WriteLine($"生成文件大小: {fileSize} 字节");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存二进制文件时出错: {ex.Message}");
                return false;
            }
        }

        private void WriteBinaryHeader(BinaryWriter writer)
        {
            writer.Write(mapData.Width);
            writer.Write(mapData.Height);

            byte[] titleBytes = new byte[16];
            Encoding.ASCII.GetBytes(mapData.Title).CopyTo(titleBytes, 0);
            writer.Write(titleBytes);

            writer.Write(mapData.UpdateDate.ToOADate());
            writer.Write(originalReserved);
        }

        private void WriteBinaryMapData(BinaryWriter writer)
        {
            int elementSize = 12;
            long columnSize = elementSize * mapData.Height;

            for (int x = 0; x < mapData.Width; x++)
            {
                long position = 52 + (columnSize * x);
                writer.BaseStream.Seek(position, SeekOrigin.Begin);

                for (int y = 0; y < mapData.Height; y++)
                {
                    WriteMapCell(writer, mapData.Matrix[y][x]);
                }
            }
        }

        private void WriteExtraData(BinaryWriter writer)
        {
            if (mapData.ExtraData.Length > 0)
            {
                long mapDataEnd = 52 + (12 * mapData.Width * mapData.Height);
                writer.BaseStream.Seek(mapDataEnd, SeekOrigin.Begin);
                writer.Write(mapData.ExtraData);
                Console.WriteLine($"已写入额外数据: {mapData.ExtraData.Length} 字节");
            }
        }

        private void WriteMapCell(BinaryWriter writer, MapCell cell)
        {
            writer.Write(cell.BkImg);
            writer.Write(cell.MidImg);
            writer.Write(cell.FrImg);
            writer.Write(cell.DoorIndex);
            writer.Write(cell.DoorOffset);
            writer.Write(cell.AniFrame);
            writer.Write(cell.AniTick);
            writer.Write(cell.Area);
            writer.Write(cell.Light);
        }

        public void PrintMapInfo()
        {
            Console.WriteLine($"\n=== 地图信息 ===");
            Console.WriteLine($"标题: {mapData.Title}");
            Console.WriteLine($"尺寸: {mapData.Width} x {mapData.Height}");
            Console.WriteLine($"单元格总数: {mapData.Width * mapData.Height}");
            Console.WriteLine($"额外数据: {mapData.ExtraData.Length} 字节");
        }
    }

    class LegendOfMirMapParser
    {
       public static void main(string originalMap, string jsonFile, string binaryCopy)
        {
            var parser = new ModernMapParser();
          

            //Console.WriteLine("=== 996传奇地图解析器 - JSON输出版本 ===\n");

            // 1. 加载原始二进制地图
            if (parser.LoadBinaryMap(originalMap))
            {
                parser.PrintMapInfo();

                // 2. 保存为JSON格式
                if (parser.SaveAsJson(jsonFile))
                {
                    Console.WriteLine($"\n✅ JSON文件已生成: {jsonFile}");
                    Console.WriteLine($"   你可以用文本编辑器查看JSON内容");

                    // 3. 从JSON测试重新加载（可选）
                    TestJsonReload(jsonFile);

                    // 4. 保存回二进制格式验证一致性
                    if (parser.SaveAsBinary(binaryCopy))
                    {
                        bool isIdentical = CompareBinaryFiles(originalMap, binaryCopy);
                        Console.WriteLine($"\n=== 最终验证 ===");
                        Console.WriteLine($"二进制一致性: {(isIdentical ? "✅ 通过" : "❌ 失败")}");
                    }
                }
            }

            //Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }

        static void TestJsonReload(string jsonFile)
        {
            try
            {
                string json = File.ReadAllText(jsonFile, Encoding.UTF8);
                var mapData = JsonSerializer.Deserialize<MapData>(json);
                if (mapData != null)
                {
                    Console.WriteLine($"✅ JSON文件可正确加载");
                    Console.WriteLine($"   地图: {mapData.Width}x{mapData.Height}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ JSON文件加载失败: {ex.Message}");
            }
        }

        static bool CompareBinaryFiles(string file1, string file2)
        {
            try
            {
                byte[] bytes1 = File.ReadAllBytes(file1);
                byte[] bytes2 = File.ReadAllBytes(file2);

                if (bytes1.Length != bytes2.Length)
                {
                    Console.WriteLine($"文件大小不同: {bytes1.Length} vs {bytes2.Length}");
                    return false;
                }

                for (int i = 0; i < bytes1.Length; i++)
                {
                    if (bytes1[i] != bytes2[i])
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"比较文件时出错: {ex.Message}");
                return false;
            }
        }
    }
}