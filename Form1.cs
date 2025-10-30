
using System.Text.Json;
using System.Text;
using zy_996map.Properties;
using static System.Net.Mime.MediaTypeNames;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Formats.Tar;

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






        // 地图单元格数据结构
        public struct MapCell
        {
            public byte Terrain;   // 地形类型
            public byte Obstacle;  // 障碍物信息

            public MapCell(byte terrain, byte obstacle)
            {
                Terrain = terrain;
                Obstacle = obstacle;
            }
        }

        // 地形类型枚举
        public enum TerrainType : byte
        {
            Ground = 0,    // 地面
            Water = 1,     // 水域
            Grass = 2,     // 草地
            Stone = 3,     // 石地
            Sand = 4,      // 沙地
            Snow = 5       // 雪地
        }

        // 障碍物标志枚举
        [Flags]
        public enum ObstacleFlags : byte
        {
            None = 0,      // 无障碍
            Block = 1,     // 完全阻挡
            Safe = 2,      // 安全区
            Water = 4,     // 水域阻挡
            Cliff = 8      // 悬崖
        }
        public class MapReader
        {
    
            public ushort Width { get; private set; }
            public ushort Height { get; private set; }
            public MapCell[,] MapData { get; private set; }

            public bool ReadMap(string filePath)
            {
                try
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        // 读取地图尺寸
                        Width = reader.ReadUInt16();
                        Height = reader.ReadUInt16();

                        // 初始化地图数据数组
                        MapData = new MapCell[Height, Width];

                        // 读取地图单元格数据
                        for (int y = 0; y < Height; y++)
                        {
                            for (int x = 0; x < Width; x++)
                            {
                                byte terrain = reader.ReadByte();
                                byte obstacle = reader.ReadByte();
                                MapData[y, x] = new MapCell(terrain, obstacle);
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"读取地图文件失败: {ex.Message}");
                    return false;
                }
            }

            public void AnalyzeMap()
            {
                if (MapData == null)
                {
                    Console.WriteLine("没有地图数据可分析");
                    return;
                }

                int obstacleCount = 0;
                HashSet<byte> terrainTypes = new HashSet<byte>();

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        MapCell cell = MapData[y, x];
                        if (cell.Obstacle != 0)
                            obstacleCount++;

                        terrainTypes.Add(cell.Terrain);
                    }
                }

                Console.WriteLine($"地图尺寸: {Width}x{Height}");
                Console.WriteLine($"总单元格数: {Width * Height}");
                Console.WriteLine($"障碍物数量: {obstacleCount}");
                Console.WriteLine($"可通行区域: {Width * Height - obstacleCount}");
                Console.WriteLine($"地形类型数量: {terrainTypes.Count}");

                // 显示地形类型统计
                foreach (byte terrain in terrainTypes)
                {
                    Console.WriteLine($"  地形 {terrain}: {GetTerrainName((TerrainType)terrain)}");
                }
            }

            public void GeneratePreview(int maxWidth = 50, int maxHeight = 30)
            {
                if (MapData == null)
                {
                    Console.WriteLine("没有地图数据可显示");
                    return;
                }

                int displayWidth = Math.Min(Width, maxWidth);
                int displayHeight = Math.Min(Height, maxHeight);

                Console.WriteLine("\n地图预览 (O=障碍物, .=可通行, W=水域, G=草地):");
                for (int y = 0; y < displayHeight; y++)
                {
                    StringBuilder line = new StringBuilder();
                    for (int x = 0; x < displayWidth; x++)
                    {
                        MapCell cell = MapData[y, x];
                        char symbol = GetCellSymbol(cell);
                        line.Append(symbol);
                    }
                    Console.WriteLine(line.ToString());
                }

                if (Width > maxWidth || Height > maxHeight)
                {
                    Console.WriteLine($"... (完整尺寸: {Width}x{Height})");
                }
            }

            private char GetCellSymbol(MapCell cell)
            {
                if (cell.Obstacle != 0)
                    return 'O'; // 障碍物

                return cell.Terrain switch
                {
                    (byte)TerrainType.Water => 'W',
                    (byte)TerrainType.Grass => 'G',
                    (byte)TerrainType.Stone => 'S',
                    (byte)TerrainType.Sand => 'D',
                    (byte)TerrainType.Snow => '*',
                    _ => '.'   // 默认地面
                };
            }

            private string GetTerrainName(TerrainType terrain)
            {
                return terrain switch
                {
                    TerrainType.Ground => "地面",
                    TerrainType.Water => "水域",
                    TerrainType.Grass => "草地",
                    TerrainType.Stone => "石地",
                    TerrainType.Sand => "沙地",
                    TerrainType.Snow => "雪地",
                    _ => "未知"
                };
            }

            // 检查指定坐标是否可通行
            public bool IsWalkable(int x, int y)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return false;

                return MapData[y, x].Obstacle == 0;
            }

            // 获取指定坐标的地形类型
            public TerrainType GetTerrainType(int x, int y)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Height)
                    return TerrainType.Ground;

                return (TerrainType)MapData[y, x].Terrain;
            }
        }



        public class MapWriter
        {
            public bool WriteMap(string filePath, MapCell[,] mapData)
            {
                try
                {
                    int height = mapData.GetLength(0);
                    int width = mapData.GetLength(1);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        // 写入地图尺寸
                        writer.Write((ushort)width);
                        writer.Write((ushort)height);

                        // 写入地图数据
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                writer.Write(mapData[y, x].Terrain);
                                writer.Write(mapData[y, x].Obstacle);
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"写入地图文件失败: {ex.Message}");
                    return false;
                }
            }

            // 创建示例地图
            public MapCell[,] CreateSampleMap(ushort width, ushort height)
            {
                MapCell[,] map = new MapCell[height, width];
                Random random = new Random();

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // 随机地形
                        byte terrain = (byte)random.Next(0, 6);

                        // 10%的概率生成障碍物
                        byte obstacle = (random.Next(0, 100) < 10) ? (byte)ObstacleFlags.Block : (byte)ObstacleFlags.None;

                        // 在水域地形上设置水域阻挡
                        if (terrain == (byte)TerrainType.Water)
                        {
                            obstacle = (byte)ObstacleFlags.Water;
                        }

                        map[y, x] = new MapCell(terrain, obstacle);
                    }
                }

                return map;
            }
        }

        public class MapTxtExporter : MapReader
        {
            /// <summary>
            /// 将地图数据保存为TXT文件
            /// </summary>
            public bool SaveAsTxt(string txtFilePath, TxtExportFormat format = TxtExportFormat.Visual)
            {
                if (MapData == null)
                {
                    Console.WriteLine("没有地图数据可转换");
                    return false;
                }

                try
                {
                    using (StreamWriter writer = new StreamWriter(txtFilePath, false, Encoding.UTF8))
                    {
                        switch (format)
                        {
                            case TxtExportFormat.Visual:
                                ExportVisualFormat(writer);
                                break;
                            case TxtExportFormat.CSV:
                                ExportCsvFormat(writer);
                                break;
                            case TxtExportFormat.Detailed:
                                ExportDetailedFormat(writer);
                                break;
                            case TxtExportFormat.ObstacleOnly:
                                ExportObstacleOnlyFormat(writer);
                                break;
                        }
                    }

                    Console.WriteLine($"地图已成功导出为TXT: {txtFilePath}");
                    FileInfo info = new FileInfo(txtFilePath);
                    Console.WriteLine($"文件大小: {info.Length} 字节");

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"导出TXT失败: {ex.Message}");
                    return false;
                }
            }

            /// <summary>
            /// 可视化格式导出 - 用于直观查看地图
            /// </summary>
            private void ExportVisualFormat(StreamWriter writer)
            {
                // 写入文件头信息
                writer.WriteLine($"# 传奇地图文件导出");
                writer.WriteLine($"# 导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine($"# 地图尺寸: {Width} x {Height}");
                writer.WriteLine($"# 图例: .=地面, G=草地, W=水域, S=石地, D=沙地, *=雪地, O=障碍物, B=阻挡, X=不可通行");
                writer.WriteLine();

                // 统计信息
                var stats = CalculateStatistics();
                writer.WriteLine($"# 统计信息:");
                writer.WriteLine($"# - 总单元格: {stats.TotalCells}");
                writer.WriteLine($"# - 可通行区域: {stats.WalkableCells} ({stats.WalkablePercent:P1})");
                writer.WriteLine($"# - 障碍物: {stats.ObstacleCells} ({stats.ObstaclePercent:P1})");
                writer.WriteLine($"# - 地形分布: {string.Join(", ", stats.TerrainDistribution)}");
                writer.WriteLine();

                // 地图数据
                writer.WriteLine("# 地图数据:");
                for (int y = 0; y < Height; y++)
                {
                    StringBuilder line = new StringBuilder();
                    for (int x = 0; x < Width; x++)
                    {
                        MapCell cell = MapData[y, x];
                        char symbol = GetVisualSymbol(cell);
                        line.Append(symbol);

                        // 每10格加一个空格提高可读性
                        if ((x + 1) % 10 == 0 && x < Width - 1)
                            line.Append(' ');
                    }
                    writer.WriteLine(line.ToString());

                    // 每10行加一个空行提高可读性
                    if ((y + 1) % 10 == 0 && y < Height - 1)
                        writer.WriteLine();
                }
            }
            private string GetTerrainName(TerrainType terrain)
            {
                return terrain switch
                {
                    TerrainType.Ground => "地面",
                    TerrainType.Water => "水域",
                    TerrainType.Grass => "草地",
                    TerrainType.Stone => "石地",
                    TerrainType.Sand => "沙地",
                    TerrainType.Snow => "雪地",
                    _ => "未知"
                };
            }
            /// <summary>
            /// CSV格式导出 - 用于数据分析
            /// </summary>
            private void ExportCsvFormat(StreamWriter writer)
            {
                writer.WriteLine("X,Y,Terrain,TerrainName,Obstacle,Walkable,Description");

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        MapCell cell = MapData[y, x];
                        bool walkable = cell.Obstacle == 0;
                        string terrainName = GetTerrainName((TerrainType)cell.Terrain);
                        string description = GetCellDescription(cell);

                        writer.WriteLine($"{x},{y},{cell.Terrain},{terrainName},{cell.Obstacle},{walkable},\"{description}\"");
                    }
                }
            }

            /// <summary>
            /// 详细格式导出 - 包含所有信息
            /// </summary>
            private void ExportDetailedFormat(StreamWriter writer)
            {
                writer.WriteLine("=== 传奇地图详细数据 ===");
                writer.WriteLine($"地图尺寸: {Width} x {Height}");
                writer.WriteLine($"导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();

                var stats = CalculateStatistics();
                writer.WriteLine("=== 统计信息 ===");
                writer.WriteLine($"总单元格数: {stats.TotalCells}");
                writer.WriteLine($"可通行区域: {stats.WalkableCells} ({stats.WalkablePercent:P1})");
                writer.WriteLine($"障碍物数量: {stats.ObstacleCells} ({stats.ObstaclePercent:P1})");
                writer.WriteLine();

                writer.WriteLine("=== 地形分布 ===");
                foreach (var terrain in stats.TerrainDistribution)
                {
                    writer.WriteLine($"  {terrain}");
                }
                writer.WriteLine();

                writer.WriteLine("=== 障碍物列表 ===");
                int obstacleCount = 0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (MapData[y, x].Obstacle != 0)
                        {
                            obstacleCount++;
                            writer.WriteLine($"  障碍物 {obstacleCount}: 位置({x},{y}), 类型{MapData[y, x].Obstacle}");
                        }
                    }
                }

                writer.WriteLine();
                writer.WriteLine("=== 详细单元格数据 ===");
                writer.WriteLine("坐标\t地形\t障碍\t可通行\t描述");
                writer.WriteLine("----\t----\t----\t------\t----");

                for (int y = 0; y < Math.Min((int)Height, 50); y++) // 限制输出前50行
                {
                    for (int x = 0; x < Math.Min((int)Width, 20); x++) // 限制输出前20列
                    {
                        MapCell cell = MapData[y, x];
                        string description = GetCellDescription(cell);
                        writer.WriteLine($"({x},{y})\t{cell.Terrain}\t{cell.Obstacle}\t{(cell.Obstacle == 0 ? "是" : "否")}\t{description}");
                    }
                }

                if (Height > 50 || Width > 20)
                {
                    writer.WriteLine($"... (只显示前50行x20列，完整数据: {Width}x{Height})");
                }
            }

            /// <summary>
            /// 只导出障碍物信息
            /// </summary>
            private void ExportObstacleOnlyFormat(StreamWriter writer)
            {
                writer.WriteLine($"# 障碍物数据 - 地图尺寸: {Width}x{Height}");
                writer.WriteLine($"# 导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                writer.WriteLine();

                // 障碍物地图
                writer.WriteLine("# 障碍物地图 (1=障碍物, 0=可通行):");
                for (int y = 0; y < Height; y++)
                {
                    StringBuilder line = new StringBuilder();
                    for (int x = 0; x < Width; x++)
                    {
                        line.Append(MapData[y, x].Obstacle != 0 ? '1' : '0');

                        if ((x + 1) % 10 == 0 && x < Width - 1)
                            line.Append(' ');
                    }
                    writer.WriteLine(line.ToString());
                }

                writer.WriteLine();
                writer.WriteLine("# 障碍物坐标列表:");
                int count = 0;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (MapData[y, x].Obstacle != 0)
                        {
                            count++;
                            writer.WriteLine($"{x},{y}");
                        }
                    }
                }
                writer.WriteLine($"# 总计: {count} 个障碍物");
            }

            /// <summary>
            /// 获取可视化符号
            /// </summary>
            private char GetVisualSymbol(MapCell cell)
            {
                if (cell.Obstacle != 0)
                {
                    return cell.Obstacle switch
                    {
                        (byte)ObstacleFlags.Block => 'X',
                        (byte)ObstacleFlags.Safe => 'S',
                        (byte)ObstacleFlags.Water => 'W',
                        (byte)ObstacleFlags.Cliff => 'C',
                        _ => 'O'
                    };
                }

                return cell.Terrain switch
                {
                    (byte)TerrainType.Ground => '.',
                    (byte)TerrainType.Grass => 'G',
                    (byte)TerrainType.Water => '~',
                    (byte)TerrainType.Stone => '#',
                    (byte)TerrainType.Sand => 'd',
                    (byte)TerrainType.Snow => '*',
                    _ => '?'
                };
            }

            /// <summary>
            /// 获取单元格描述
            /// </summary>
            private string GetCellDescription(MapCell cell)
            {
                string terrainName = GetTerrainName((TerrainType)cell.Terrain);

                if (cell.Obstacle != 0)
                {
                    string obstacleType = cell.Obstacle switch
                    {
                        (byte)ObstacleFlags.Block => "完全阻挡",
                        (byte)ObstacleFlags.Safe => "安全区",
                        (byte)ObstacleFlags.Water => "水域阻挡",
                        (byte)ObstacleFlags.Cliff => "悬崖阻挡",
                        _ => "未知阻挡"
                    };
                    return $"{terrainName} - {obstacleType}";
                }
                else
                {
                    return $"{terrainName} - 可通行";
                }
            }

            /// <summary>
            /// 计算统计信息
            /// </summary>
            private MapStatistics CalculateStatistics()
            {
                int totalCells = Width * Height;
                int obstacleCells = 0;
                var terrainCount = new Dictionary<string, int>();

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        MapCell cell = MapData[y, x];

                        if (cell.Obstacle != 0)
                            obstacleCells++;

                        string terrainName = GetTerrainName((TerrainType)cell.Terrain);
                        if (terrainCount.ContainsKey(terrainName))
                            terrainCount[terrainName]++;
                        else
                            terrainCount[terrainName] = 1;
                    }
                }

                int walkableCells = totalCells - obstacleCells;
                var terrainDistribution = new List<string>();
                foreach (var kvp in terrainCount)
                {
                    double percent = (double)kvp.Value / totalCells;
                    terrainDistribution.Add($"{kvp.Key}: {kvp.Value} ({percent:P1})");
                }

                return new MapStatistics
                {
                    TotalCells = totalCells,
                    ObstacleCells = obstacleCells,
                    WalkableCells = walkableCells,
                    ObstaclePercent = (double)obstacleCells / totalCells,
                    WalkablePercent = (double)walkableCells / totalCells,
                    TerrainDistribution = terrainDistribution
                };
            }

            /// <summary>
            /// 批量导出多种格式
            /// </summary>
            public void ExportAllFormats(string baseFileName)
            {
                string[] formats = Enum.GetNames(typeof(TxtExportFormat));

                foreach (string format in formats)
                {
                    TxtExportFormat exportFormat = (TxtExportFormat)Enum.Parse(typeof(TxtExportFormat), format);
                    string fileName = $"{baseFileName}_{format.ToLower()}.txt";

                    if (SaveAsTxt(fileName, exportFormat))
                    {
                        Console.WriteLine($"  ✓ 已生成: {fileName}");
                    }
                    else
                    {
                        Console.WriteLine($"  ✗ 生成失败: {fileName}");
                    }
                }
            }
        }

        /// <summary>
        /// TXT导出格式枚举
        /// </summary>
        public enum TxtExportFormat
        {
            Visual,        // 可视化格式
            CSV,           // CSV格式
            Detailed,      // 详细格式
            ObstacleOnly   // 只包含障碍物
        }

        /// <summary>
        /// 地图统计信息
        /// </summary>
        public struct MapStatistics
        {
            public int TotalCells { get; set; }
            public int ObstacleCells { get; set; }
            public int WalkableCells { get; set; }
            public double ObstaclePercent { get; set; }
            public double WalkablePercent { get; set; }
            public List<string> TerrainDistribution { get; set; }
        }
        //--------------------------------------

        private void btn_go_Click(object sender, EventArgs e)
        {
            Console.WriteLine(this.txt_input.Text);
            var dir = this.txt_input.Text;
            var filePath = Path.Combine(dir, "dld副本.map");





            //MapTxtExporter exporter = new MapTxtExporter();

            //// 读取原始.map文件
            //if (exporter.ReadMap(filePath))
            //{
            //    exporter.AnalyzeMap();
            //    exporter.GeneratePreview(20, 15);

            //    Console.WriteLine("\n开始导出TXT文件...");

            //    // 导出可视化格式
            //    exporter.SaveAsTxt(Path.Combine(dir, "map_visual.txt"), TxtExportFormat.Visual);

            //    // 导出CSV格式
            //    exporter.SaveAsTxt(Path.Combine(dir, "map_data.csv"), TxtExportFormat.CSV);

            //    // 导出详细格式
            //    exporter.SaveAsTxt(Path.Combine(dir, "map_detailed.txt"), TxtExportFormat.Detailed);

            //    // 导出障碍物格式
            //    exporter.SaveAsTxt(Path.Combine(dir, "map_obstacles.txt"), TxtExportFormat.ObstacleOnly);

            //    // 批量导出所有格式
            //    Console.WriteLine("\n批量导出所有格式:");
            //    exporter.ExportAllFormats("mapfile");

            //    // 显示生成的文件信息
            //    ShowGeneratedFiles();
            //}
           
            //Console.ReadLine();


            //return;


            // 读取地图文件
            MapReader reader = new MapReader();
            if (reader.ReadMap(filePath))
            {
                reader.AnalyzeMap();
                reader.GeneratePreview();

                // 检查特定坐标
                int testX = 10, testY = 15;
                Console.WriteLine($"\n坐标({testX},{testY}): " +
                    $"地形={reader.GetTerrainType(testX, testY)}, " +
                    $"可通行={reader.IsWalkable(testX, testY)}");
            }

            // 创建并写入新地图
            MapWriter writer = new MapWriter();
            MapCell[,] newMap = writer.CreateSampleMap(100, 100);
            if (writer.WriteMap(Path.Combine(dir, Path.GetFileNameWithoutExtension(filePath)+ "副本.map"), newMap))
            {
                Console.WriteLine("新地图创建成功!");
            }

            Console.ReadLine();
        }

        static void ShowGeneratedFiles()
        {
            Console.WriteLine("\n生成的文件列表:");
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txt");
            string[] csvFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");

            var allFiles = new List<string>();
            allFiles.AddRange(files);
            allFiles.AddRange(csvFiles);

            foreach (string file in allFiles)
            {
                FileInfo info = new FileInfo(file);
                Console.WriteLine($"  {info.Name} - {info.Length} bytes - {info.LastWriteTime:HH:mm:ss}");
            }
        }
    }
}

