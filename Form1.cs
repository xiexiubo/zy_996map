
using System.Text.Json;
using System.Text;
using zy_996map.Properties;
using static System.Net.Mime.MediaTypeNames;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Formats.Tar;
using System.Text.Json.Serialization;

namespace zy_996map
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.txt_input.Text = Settings.Default.txt_input;
        }
        // 选择保存目录的方法
        private string SelectSaveDirectory()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                // 设置对话框标题
                folderDialog.Description = "请选择保存文件的目录";

                // 可选：设置默认打开的目录
                folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // 可选：显示"新建文件夹"按钮
                folderDialog.ShowNewFolderButton = true;

                // 显示对话框并获取用户选择
                DialogResult result = folderDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    // 用户选择了目录，返回选择的路径
                    return folderDialog.SelectedPath;
                }

                // 用户取消了选择或操作失败
                return null;
            }
        }
        private void txt_input_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.txt_input = this.txt_input.Text;
            Console.WriteLine($"保存目录修改{this.txt_input.Text}");
            Settings.Default.Save();
        }
        private void btn_open_Click(object sender, EventArgs e)
        {
            this.txt_input.Text = SelectSaveDirectory();
        }
        public class DataModel
        {
            public int ver { get; set; }
            public List<List<int>> matrix { get; set; }
        }











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

                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (var reader = new BinaryReader(fs))
                    {
                        ReadHeader(reader);
                        ReadMapMatrix(reader);
                        ReadExtraData(reader); // 读取额外数据

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
                long mapDataEnd = 52 + columnSize * mapData.Width;

                Console.WriteLine($"地图数据区域: 52 - {mapDataEnd} 字节");

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
                    Console.WriteLine($"发现额外数据: {extraDataSize} 字节 (位置 {mapDataEnd} - {reader.BaseStream.Length})");

                    reader.BaseStream.Seek(mapDataEnd, SeekOrigin.Begin);
                    mapData.ExtraData = reader.ReadBytes((int)extraDataSize);

                    // 分析额外数据的结构
                    AnalyzeExtraData(mapData.ExtraData);
                }
                else
                {
                    mapData.ExtraData = Array.Empty<byte>();
                    Console.WriteLine("没有额外数据");
                }
            }

            private void AnalyzeExtraData(byte[] extraData)
            {
                Console.WriteLine($"\n=== 额外数据分析 ===");
                Console.WriteLine($"大小: {extraData.Length} 字节");

                if (extraData.Length >= 4)
                {
                    // 尝试解析可能的对象数量
                    int possibleObjectCount = BitConverter.ToInt32(extraData, 0);
                    Console.WriteLine($"前4字节作为整数: {possibleObjectCount}");

                    // 显示前64字节的十六进制
                    int bytesToShow = Math.Min(64, extraData.Length);
                    Console.WriteLine($"前{bytesToShow}字节十六进制:");
                    for (int i = 0; i < bytesToShow; i += 16)
                    {
                        int lineLength = Math.Min(16, extraData.Length - i);
                        string hexLine = BitConverter.ToString(extraData, i, lineLength).Replace("-", " ");
                        Console.WriteLine($"  {i:D4}: {hexLine}");
                    }
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

            public bool SaveAsBinary(string binaryFilePath)
            {
                try
                {
                    using (var fs = new FileStream(binaryFilePath, FileMode.Create, FileAccess.Write))
                    using (var writer = new BinaryWriter(fs))
                    {
                        WriteBinaryHeader(writer);
                        WriteBinaryMapData(writer);
                        WriteExtraData(writer); // 写入额外数据

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

            public void DebugFileStructure(string filePath)
            {
                Console.WriteLine($"\n=== 文件结构分析: {Path.GetFileName(filePath)} ===");

                byte[] bytes = File.ReadAllBytes(filePath);
                Console.WriteLine($"文件总大小: {bytes.Length} 字节");

                if (bytes.Length >= 52)
                {
                    ushort width = BitConverter.ToUInt16(bytes, 0);
                    ushort height = BitConverter.ToUInt16(bytes, 2);
                    string title = Encoding.ASCII.GetString(bytes, 4, 16).TrimEnd('\0');

                    Console.WriteLine($"文件头信息:");
                    Console.WriteLine($"  宽度: {width}");
                    Console.WriteLine($"  高度: {height}");
                    Console.WriteLine($"  标题: '{title}'");

                    long expectedMapDataEnd = 52 + (width * height * 12);
                    Console.WriteLine($"  地图数据结束位置: {expectedMapDataEnd}");
                    Console.WriteLine($"  额外数据大小: {bytes.Length - expectedMapDataEnd} 字节");
                    Console.WriteLine($"  额外数据占比: {(bytes.Length - expectedMapDataEnd) * 100.0 / bytes.Length:F2}%");

                    if (bytes.Length > expectedMapDataEnd)
                    {
                        Console.WriteLine($"  包含额外数据: 是");
                    }
                    else
                    {
                        Console.WriteLine($"  包含额外数据: 否");
                    }
                }
            }

            public void PrintDetailedComparison(string originalFile, string copyFile)
            {
                Console.WriteLine($"\n=== 详细文件对比 ===");

                byte[] originalBytes = File.ReadAllBytes(originalFile);
                byte[] copyBytes = File.ReadAllBytes(copyFile);

                Console.WriteLine($"原始文件: {originalBytes.Length} 字节");
                Console.WriteLine($"副本文件: {copyBytes.Length} 字节");

                if (originalBytes.Length != copyBytes.Length)
                {
                    Console.WriteLine($"❌ 文件大小不同: {originalBytes.Length} vs {copyBytes.Length}");
                    Console.WriteLine($"差异: {Math.Abs(originalBytes.Length - copyBytes.Length)} 字节");

                    // 即使大小不同，也比较共同部分
                    int minLength = Math.Min(originalBytes.Length, copyBytes.Length);
                    CompareCommonPart(originalBytes, copyBytes, minLength);
                    return;
                }

                CompareFullFiles(originalBytes, copyBytes);
            }

            private void CompareCommonPart(byte[] original, byte[] copy, int length)
            {
                Console.WriteLine($"\n--- 共同部分比较 (前{length}字节) ---");

                int differences = 0;
                for (int i = 0; i < length; i++)
                {
                    if (original[i] != copy[i])
                    {
                        differences++;
                        if (differences <= 10)
                        {
                            string context = i < 52 ? "文件头" : (i < 52 + (mapData.Width * mapData.Height * 12) ? "地图数据" : "额外数据");
                            Console.WriteLine($"位置 {i:D6}: 原始=0x{original[i]:X2} 副本=0x{copy[i]:X2} [{context}]");
                        }
                    }
                }

                Console.WriteLine($"共同部分差异数: {differences}");

                if (differences == 0)
                {
                    Console.WriteLine($"✅ 共同部分完全一致，差异仅在于文件末尾");
                }
            }

            private void CompareFullFiles(byte[] original, byte[] copy)
            {
                int differences = 0;
                for (int i = 0; i < original.Length; i++)
                {
                    if (original[i] != copy[i])
                    {
                        differences++;
                        if (differences <= 10)
                        {
                            string context = i < 52 ? "文件头" : "地图数据";
                            Console.WriteLine($"位置 {i:D6}: 原始=0x{original[i]:X2} 副本=0x{copy[i]:X2} [{context}]");
                        }
                    }
                }

                Console.WriteLine($"总差异数: {differences}");
                Console.WriteLine($"一致性: {(differences == 0 ? "✅ 完全一致" : "❌ 存在差异")}");
            }

            public void PrintMapInfo()
            {
                Console.WriteLine($"\n=== 地图信息 ===");
                Console.WriteLine($"标题: {mapData.Title}");
                Console.WriteLine($"尺寸: {mapData.Width} x {mapData.Height}");
                Console.WriteLine($"地图数据大小: {mapData.Width * mapData.Height * 12} 字节");
                Console.WriteLine($"额外数据大小: {mapData.ExtraData.Length} 字节");
                Console.WriteLine($"总数据大小: {52 + (mapData.Width * mapData.Height * 12) + mapData.ExtraData.Length} 字节");
            }
        }

     
                
            

            static bool CompareBinaryFiles(string file1, string file2)
            {
                try
                {
                    byte[] bytes1 = File.ReadAllBytes(file1);
                    byte[] bytes2 = File.ReadAllBytes(file2);

                    if (bytes1.Length != bytes2.Length)
                        return false;

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
        
    
    //--------------------------------------

    private void btn_go_Click(object sender, EventArgs e)
        {
            Console.WriteLine(this.txt_input.Text);
            var dir = this.txt_input.Text;
            var filePath = Path.Combine(dir, "a1001.map");

            var parser = new ModernMapParser();
            string originalMap = filePath;
            string jsonMap = Path.Combine(dir, "a1001.json");
            string binaryCopy = Path.Combine(dir, "a1001_copy.map");




         

            Console.WriteLine("=== 996传奇地图解析器 - 完整版本 ===\n");

            // 1. 加载原始二进制地图（包含额外数据）
            if (parser.LoadBinaryMap(originalMap))
            {
                parser.PrintMapInfo();
                parser.DebugFileStructure(originalMap);

                // 2. 保存为二进制格式（包含额外数据）
                if (parser.SaveAsBinary(binaryCopy))
                {
                    // 3. 比较文件
                    parser.PrintDetailedComparison(originalMap, binaryCopy);

                    // 4. 分析副本文件结构
                    parser.DebugFileStructure(binaryCopy);

                    bool isIdentical = CompareBinaryFiles(originalMap, binaryCopy);
                    Console.WriteLine($"\n=== 最终结果 ===");
                    Console.WriteLine($"文件一致性: {(isIdentical ? "✅ 完全一致" : "❌ 存在差异")}");
                }
            }

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();


            return;

           
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }




        

    }
}

