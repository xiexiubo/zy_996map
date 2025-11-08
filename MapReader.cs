using System;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ImageMagick.Formats;
using ImageMagick;

namespace zy_996map
{
    internal class MapReader
    {
        // 定义与JSON结构对应的类
        class Root_map
        {
            [JsonProperty("map")]
            public Dictionary<string, Cfg_Map> Items { get; set; }
        }

        public class Cfg_Map
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("img")]
            public long img { get; set; }

            [JsonProperty("fmap")]
            public string fmap { get; set; }

            [JsonProperty("data")]
            public long data { get; set; }

            [JsonProperty("name")]
            public string name { get; set; }

            [JsonProperty("width")]
            public int width { get; set; }
            public int height { get; set; }


        }

        static Dictionary<int, Size> dicConst = new Dictionary<int, Size>()
        {
            { 154, new Size(512 * 93, 256 * 100) },
            { 1025, new Size(512 * 5, 256 * 7) },
            { 2701, new Size(512 * 13, 256 * 20) },
            { 4902, new Size(512 * 9, 256 * 11) },
            { 5002, new Size(512 * 37, 256 * 50) },
            { 5003, new Size(512 * 37, 256 * 50) },
            { 5005, new Size(512 * 37, 256 * 50) },
            { 5562, new Size(512 * 14, 256 * 23) },
            { 5563, new Size(512 * 26, 256 * 35) },
            { 5564, new Size(512 * 14, 256 * 19) },
            { 5565, new Size(512 * 12, 256 * 17) },
            //{ 5564, new Size(512*5,256*7) }, // 注释掉的重复键
            { 5566, new Size(512 * 18, 256 * 30) },
            { 5567, new Size(512 * 7, 256 * 10) },
            { 5568, new Size(512 * 7, 256 * 10) },
            { 5569, new Size(512 * 7, 256 * 10) },
            { 5570, new Size(512 * 10, 256 * 13) },
            { 5571, new Size(512 * 15, 256 * 23) },
            { 5572, new Size(512 * 15, 256 * 20) },
            { 5573, new Size(512 * 5, 256 * 8) },
            { 5574, new Size(512 * 19, 256 * 23) },
            { 5592, new Size(512 * 22, 256 * 31) },
            { 6110, new Size(512 * 21, 256 * 28) },
            { 6211, new Size(512 * 16, 256 * 25) },
            { 6213, new Size(512 * 19, 256 * 25) },
            { 6217, new Size(512 * 37, 256 * 50) },
            { 6224, new Size(512 * 28, 256 * 50) },
            { 6242, new Size(512 * 28, 256 * 37) },
            { 6259, new Size(512 * 9, 256 * 12) },
            { 9401, new Size(512 * 37, 256 * 65) },
            //{ 12558, new Size(512*18,256*7) }, // 注释掉的重复键
            { 12558, new Size(512 * 6, 256 * 8) },
            { 12574, new Size(512 * 7, 256 * 11) },
            { 12582, new Size(512 * 7, 256 * 10) },
            { 20000, new Size(512 * 20, 256 * 27) },
            { 150000, new Size(512 * 8, 256 * 16) }
        };
        static int MAP_GRID_WIDTH = 48;
        static int MAP_GRID_HEIGHT = 32;

        #region DoneRes_MapData
        public static async Task DoneRes_MapData(string imgPath,string directory)
        {
            string imageName = Path.GetFileNameWithoutExtension(imgPath);
            Dictionary<int, MapData> mapDatas = new Dictionary<int, MapData>();
            Root_map config = null;

            //读取配置表
            using (var httpClient = new HttpClient())
            {
                var url = $"https://cdn.ascq.zlm4.com/aoshi_20240419/0config{Form1.Txt_ver}.json?v=20251017185057";
                // 发送HTTP请求并获取响应
                HttpResponseMessage response = await httpClient.GetAsync(url);

                // 确保请求成功
                //response.EnsureSuccessStatusCode();
                if (!response.IsSuccessStatusCode)
                {
                    Form1.AddLog($"没有下载成功 code:{response.StatusCode} {url}");
                    return;
                }

                // 读取响应内容
                string json = await response.Content.ReadAsStringAsync();

               // Console.WriteLine($"文件num: {json}");

                // 反序列化为配置对象
                config = JsonConvert.DeserializeObject<Root_map>(json);
                Console.WriteLine($"文件num: {config.Items.Count}");
                Form1.AddLog($"-----Map conifg:{config.Items.Count}", Color.Green);
                if (config == null)
                {
                    throw new InvalidOperationException("配置文件内容为空或格式不正确");
                }
            }

            await Task.Delay(15);
            byte[] data =null;
            try
            {
                string url = "https://cdn.ascq.zlm4.com/aoshi_20240419/map1.29318.3.dat?ver=1.0.1";
                url = $"https://cdn.ascq.zlm4.com/aoshi_20240419/map{Form1.Txt_ver}.dat?ver=1.0.1";
                Form1.AddLog($"下载map.data");
                using (var httpClient = new HttpClient())
                {
                    // 发送HTTP请求并获取响应
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    // 读取响应内容
                    byte[] compressedData = await response.Content.ReadAsByteArrayAsync();
                    Console.WriteLine($"下载的压缩数据大小: {compressedData.Length}字节");

                    using (MemoryStream compressedStream = new MemoryStream(compressedData))
                    using (MemoryStream decompressedStream = new MemoryStream())
                    {
                        //Form1.AddLog($"尝试不同的解压方法");
                        // 尝试不同的解压方法
                        Console.WriteLine($"未解压的地图文件大小为{compressedData.Length}字节");
                        try
                        {
                            compressedStream.Position = 0;
                            byte[] header = new byte[4];
                            compressedStream.Read(header, 0, 4);
                            compressedStream.Position = 0;

                            using (var decompressedStream2 = new MemoryStream())
                            {
                                Stream decompressionStream;

                                // 检测 GZip 格式 (0x1F 0x8B)
                                if (header[0] == 0x1F && header[1] == 0x8B)
                                {
                                    decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                                    Console.WriteLine($"GZip解压检测 GZip 格式 (0x1F 0x8B)");

                                }
                                // 检测 ZLib 格式
                                else if (header[0] == 0x78 && (header[1] == 0x01 || header[1] == 0x9C || header[1] == 0xDA))
                                {
                                    decompressionStream = new ZLibStream(compressedStream, CompressionMode.Decompress);
                                    Console.WriteLine($"GZip解压 检测 ZLib 格式");

                                }
                                else
                                {
                                    throw new InvalidDataException("Unknown compression format");
                                }

                                using (decompressionStream)
                                {
                                    decompressionStream.CopyTo(decompressedStream2);
                                }

                                data = decompressedStream2.ToArray();
                            }

                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine($"GZip解压失败: {ex1.Message}, 尝试Deflate解压");

                            // 方法2: 尝试Deflate解压
                            try
                            {
                                compressedStream.Position = 0;
                                decompressedStream.SetLength(0);
                                using (DeflateStream decompressionStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                                {
                                    decompressionStream.CopyTo(decompressedStream);
                                }
                                data = decompressedStream.ToArray();
                            }
                            catch (Exception ex2)
                            {
                                Console.WriteLine($"Deflate解压也失败: {ex2.Message}, 尝试原始数据");
                                // 方法3: 可能数据已经是解压状态
                                data = compressedData;
                            }
                        }

                        Console.WriteLine($"解压后的地图文件大小为{data.Length}字节");

                       
                    }

                    Console.WriteLine($"完成mapdata解读 : {mapDatas.Count}");
                    Form1.AddLog($"完成mapdata解读 : {mapDatas.Count}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP请求错误: {ex.Message}");
                Form1.AddLog($"HTTP请求错误: {ex.Message}", Color.Red);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                Form1.AddLog($"发生错误6: {ex.Message}", Color.Red);
            }

            if (data == null) return;

            // 使用MemoryStream读取数据，尝试不同的字节序
            using (MemoryStream stream = new MemoryStream(data))
            {
                // 方法1: 尝试小端序
                short count = ReadShort(stream, false);
                Console.WriteLine($"小端序读取的地图数量：{count}");

                // 如果读取到负数，尝试大端序
                if (count < 0)
                {
                    stream.Position = 0;
                    count = ReadShort(stream, true);
                    Console.WriteLine($"大端序读取的地图数量：{count}");
                }

                // 如果还是负数，可能是数据格式问题
                if (count < 0)
                {
                    Console.WriteLine("警告：地图数量为负数，可能数据格式有误");
                    // 可以尝试继续处理或者直接返回
                    return;
                }

                // 使用正确的字节序读取剩余数据
                bool useBigEndian = true; // 根据上面的测试确定
                Form1.AddLog($"地图数据量：{count}");
                for (int j = 0; j < count; j++)
                {
                    int key = ReadInt(stream, useBigEndian);
                    int dataLength = ReadInt(stream, useBigEndian);

                    if (dataLength < 0 || dataLength > stream.Length - stream.Position)
                    {
                        Console.WriteLine($"错误的数据长度: {dataLength}，位置: {stream.Position}");
                        break;
                    }

                    byte[] da = new byte[dataLength];
                    int bytesRead = stream.Read(da, 0, dataLength);

                    if (bytesRead != dataLength)
                    {
                        Console.WriteLine($"读取数据不完整，期望: {dataLength}，实际: {bytesRead}");
                        break;
                    }

                    // 存储地图数据
                    if (!mapDatas.ContainsKey(key))
                    {
                        var mapdata = new MapData();
                        mapDatas.Add(key, mapdata);

                        Cfg_Map cfg;
                        if (!config.Items.TryGetValue(key.ToString(), out cfg))
                        {
                            Form1.AddLog($"配置表不存在 ID：{key}", Color.Red);
                            continue;
                        }
                        int mapWidth = cfg.width;
                        int mapHeight = cfg.height;
                        Size size;
                        if (dicConst.TryGetValue((int)key, out size))
                        {
                            mapWidth = size.Width;
                            mapHeight = size.Height;
                        }
                        // 计算网格行列数
                        int cols = (int)Math.Ceiling(mapWidth / (double)MAP_GRID_WIDTH);
                        int rows = (int)Math.Ceiling(mapHeight / (double)MAP_GRID_HEIGHT);

                        //if (j < 10) // 只打印前10个避免输出太多
                        //Console.WriteLine($"解析并存储地图 ID：{key} ({cols},{rows})，数据长度：{dataLength} 字节");
                        //Form1.AddLog($"解析并存储地图 ID：{key} ({cols},{rows})，数据长度：{dataLength} 字节");
                        //if (cols * rows > 32767)
                        //    Form1.AddLog($"({cols} x {rows}={cols * rows}) > 32767", Color.Red);
                        

                        if (key.ToString() == imageName)
                        {
                            Console.WriteLine($"解析并存储地图 ID：{key} ({cols},{rows})，数据长度：{dataLength} 字节");
                            Form1.AddLog($"解析并存储地图 ID：{key} ({cols},{rows})，数据长度：{dataLength} 字节");
                            string s = "";

                            int _colCount;
                            int _rowCount;
                            int[][] _mapIndex;
                            Dictionary<int, bool> _coverDic = new Dictionary<int, bool>();
                            Dictionary<int, bool> _safeDic = new Dictionary<int, bool>();
                            List<short[]> stallageArea = new List<short[]>();
                            List<short[]> guajiArea = new List<short[]>();
                            List<short[]> safeArea = new List<short[]>();

                            using (MemoryStream stream2 = new MemoryStream(da))
                            using (BinaryReader reader = new BinaryReader(stream2))
                            {
                                // 设置小端序
                                // BinaryReader默认就是小端序，所以不需要特别设置

                                // 跳过第一个int（可能是版本号或其他标识）
                                reader.ReadInt32();

                                _colCount = cols - 1;
                                _rowCount = rows - 1;

                                // 定义标志位常量
                                const byte FLAG_N = 8;  // 1000
                                const byte FLAG_O = 4;  // 0100  
                                const byte FLAG_S = 2;  // 0010

                                bool l = true;
                                byte d = 0;
                                byte u = 0;
                                int c = 0;

                                _mapIndex = new int[rows][];

                                // 读取地图网格数据
                                for (int p = 0; p < rows; p++)
                                {
                                    int[] gridRow = new int[cols];

                                    for (int f = 0; f < cols; f++)
                                    {
                                        if (l)
                                        {
                                            d = reader.ReadByte();
                                            if (d > 127) // 处理有符号byte转无符号
                                                d = (byte)(d + 256);
                                            l = false;
                                            u = (byte)(d >> 4); // 取高4位

                                        }
                                        else
                                        {
                                            u = (byte)(d & 0x0F); // 取低4位
                                            l = true;
                                        }

                                        int a = (f << 16) + p; // 生成唯一索引

                                        // 解析标志位
                                        if ((u & FLAG_N) > 0)
                                            gridRow[f] = 1;
                                        else
                                            gridRow[f] = 0;

                                        if ((u & FLAG_O) > 0)
                                            _coverDic[a] = true;

                                        if ((u & FLAG_S) > 0)
                                            _safeDic[a] = true;
                                        s += gridRow[f];
                                        c++;
                                    }
                                    s += "\n";
                                    _mapIndex[p] = gridRow;
                                    if (p % 100 == 0)
                                        Form1.AddLog($"----读取二进制格子数据 rows{rows} cols {cols} {_mapIndex.Length}{_mapIndex[0].Length}  {p + 1}/{rows}");
                                }
                                var mapPath = Path.Combine(directory, $"{key}.map");
                                Form1.AddLog($"开始生成 map文件{mapPath}");

                                //切图并设置madata数据
                                CutBigPix(imgPath, directory,mapdata, _mapIndex);
                                if (SaveAsBinary(mapPath, mapdata))
                                {
                                    Form1.AddLog($"成功生成 map文件{mapPath}");
                                }
                                #region 额外数据
                                //// 读取摊位区域
                                //short h = reader.ReadInt16();
                                //if (h > 0)
                                //{
                                //    stallageArea = new List<short[]>();
                                //    for (int m = 0; m < h; m++)
                                //    {
                                //        short x = reader.ReadInt16();
                                //        short y = reader.ReadInt16();
                                //        stallageArea.Add(new short[] { x, y });
                                //    }
                                //}

                                //// 读取挂机区域
                                //short v = reader.ReadInt16();
                                //if (v > 0)
                                //{
                                //    guajiArea = new List<short[]>();
                                //    for (int y = 0; y < v; y++)
                                //    {
                                //        short x = reader.ReadInt16();
                                //        short yCoord = reader.ReadInt16();
                                //        guajiArea.Add(new short[] { x, yCoord });
                                //    }
                                //}

                                //// 读取安全区域
                                //short _ = reader.ReadInt16();
                                //if (_ > 0)
                                //{
                                //    safeArea = new List<short[]>();
                                //    for (int b = 0; b < _; b++)
                                //    {
                                //        short x = reader.ReadInt16();
                                //        short y = reader.ReadInt16();
                                //        safeArea.Add(new short[] { x, y });
                                //    }
                                //}
                                #endregion

                                Console.WriteLine($"地图 {key} 解析完成:pos {reader.BaseStream.Position}- length {reader.BaseStream.Length}");
                            }

                            Console.WriteLine($"地图 {key} 解析完成");
                            Console.WriteLine($"  - 网格: {_rowCount + 1}x{_colCount + 1}");
                            Console.Write(s);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"警告：地图 ID {key} 已存在，跳过重复数据");
                    }
                }
            }
       //------------------------
        }

        public static bool SaveAsBinary(string binaryFilePath, MapData mapDate)
        {
            try
            {
                var dir = Path.GetDirectoryName(binaryFilePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var mapGrid = mapDate.Matrix;
                using (var fs = new FileStream(binaryFilePath, FileMode.Create, FileAccess.Write))
                using (var writer = new BinaryWriter(fs))
                {
                    ushort width = (ushort)mapGrid[0].Length;
                    ushort height = (ushort)mapGrid.Length;
                    //头文件 
                    //宽
                    writer.Write(width);
                    //高
                    writer.Write(height);
                    byte[] titleBytes = new byte[16];
                    Encoding.ASCII.GetBytes("传奇地图").CopyTo(titleBytes, 0);
                    //标题
                    writer.Write(titleBytes);
                    //日期
                    writer.Write(DateTime.Now.ToOADate());
                    //预留
                    writer.Write(new byte[24]);


                    int elementSize = mapDate.Version;//12 或者14
                    long columnSize = elementSize * height;

                    //碰撞  碎图索引
                    for (int x = 0; x < width; x++)
                    {
                        long position = 52 + (columnSize * x);
                        writer.BaseStream.Seek(position, SeekOrigin.Begin);

                        for (int y = 0; y < height; y++)
                        {
                            var dat = mapGrid[y][x];
                            //int index = y * width + x;
                            //index += 1;
                            //if (index > 32767)
                            //{
                            //    Form1.AddLog($"index>32767了   -----{index}");
                            //}
                         
                            //bg
                            writer.Write((ushort)(dat.BkImg));
                            //mid
                            writer.Write((ushort)dat.MidImg);
                            //front
                            writer.Write((ushort)dat.FrImg);
                            //doorIndex
                            writer.Write((byte)dat.DoorIndex);
                            //doorOffset
                            writer.Write((byte)dat.DoorOffset);
                            //aniframe
                            writer.Write((byte)dat.AniFrame);
                            //anitick
                            writer.Write((byte)dat.AniTick);
                            //area 
                            writer.Write((byte)Form1.code_id);
                            //light
                            writer.Write((byte)dat.Light);
                            if (elementSize == 14)
                            {
                                //area bk
                                writer.Write((byte)Form1.code_id);
                                //area mid
                                writer.Write((byte)Form1.code_id);
                            }

                        }
                    }

                    long fileSize = fs.Length;
                    Console.WriteLine($"成功保存为二进制: {binaryFilePath}");
                    Console.WriteLine($"生成文件大小: {fileSize} 字节");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存二进制文件时出错: {ex.Message}");
                Form1.AddLog($"保存二进制文件时出错: {ex.Message}", Color.Red);
                return false;
            }
        }

        /// <summary>
        /// 切割大图为指定大小的小图，边缘不足部分用黑色填充  返回madata数据
        /// 使用Magick.NET优化大图像处理
        /// </summary>       
        public static void CutBigPix(string imagePath, string pathOut, MapData mapDate, int[][] mapIndex)
        {
            string bigImageName = Path.GetFileNameWithoutExtension(imagePath);
            var cutSize = new Size(48, 32);
            mapDate.Version = 14;//12 14 数据字节数
            mapDate.Width = (ushort)mapIndex[0].Length;
            mapDate.Height = (ushort)mapIndex.Length;
            mapDate.Matrix = new MapCell[mapDate.Height][];
            for (int i = 0; i < mapDate.Height; i++)
            {
                mapDate.Matrix[i] = new MapCell[mapDate.Width];
                for (int j = 0; j < mapDate.Width; j++)
                {//阻挡设置
                    mapDate.Matrix[i][j] = new MapCell();
                    mapDate.Matrix[i][j].BkImg = (ushort)(mapIndex[i][j] * 32768);
                    mapDate.Matrix[i][j].FrImg = (ushort)(mapIndex[i][j] * 32768);
                }
            }

            // 检查文件是否存在
            if (!File.Exists(imagePath))
            {
                Form1.AddLog($"图片文件不存在: {imagePath}", Color.Red);
                return;
            }

            try
            {
                // 使用Magick.NET处理大图像
                using (var originalImage = new MagickImage())
                {
                    // 设置读取大图像的选项，避免一次性加载到内存
                    originalImage.Settings.SetDefine(MagickFormat.Png, "limit-memory", "1GiB");
                    originalImage.Settings.SetDefine(MagickFormat.Png, "limit-map", "2GiB");

                    // 读取图像
                    originalImage.Read(imagePath);

                    Form1.AddLog($"图像信息: {imagePath}, 宽度: {originalImage.Width}, 高度: {originalImage.Height}, 格式: {originalImage.Format}", Color.Green);

                    int originalWidth = mapDate.Width * cutSize.Width;
                    int originalHeight = mapDate.Height * cutSize.Height;

                    // 计算需要切割的行列数（向上取整，确保覆盖整个原图）
                    int colCount = (int)Math.Ceiling((double)originalWidth / cutSize.Width); // 列数（横向切割数）
                    int rowCount = (int)Math.Ceiling((double)originalHeight / cutSize.Height); // 行数（纵向切割数）

                    int tileTotal = colCount * rowCount;
                    const int objCount = 32767;
                    const int smtileCount = 65535;
                    const int tileCount = 32767;

                    int scale = 1;
                    //tpye 1 objs count
                    if (tileTotal <= objCount)
                    {
                        mapDate.Version = 12;
                    }
                    else if (tileTotal <= objCount + smtileCount)
                    {
                        mapDate.Version = 14;
                    }
                    else if (tileTotal <= objCount + smtileCount + tileCount)
                    {
                        mapDate.Version = 14;
                    }
                    else
                    {
                        mapDate.Version = 14;
                        cutSize = new Size(cutSize.Width * 2, cutSize.Height * 2);
                        colCount = (int)Math.Ceiling((double)originalWidth / cutSize.Width); // 列数（横向切割数）
                        rowCount = (int)Math.Ceiling((double)originalHeight / cutSize.Height); // 行数（纵向切割数）
                        tileTotal = colCount * rowCount;
                        if (tileTotal <= objCount + smtileCount + tileCount)
                        {
                            mapDate.Version = 14;
                            scale = 2;
                        }
                        else
                        {
                            mapDate.Version = 14;
                            cutSize = new Size(cutSize.Width * 2, cutSize.Height * 2);
                            colCount = (int)Math.Ceiling((double)originalWidth / cutSize.Width); // 列数（横向切割数）
                            rowCount = (int)Math.Ceiling((double)originalHeight / cutSize.Height); // 行数（纵向切割数）
                            tileTotal = colCount * rowCount;
                            scale = 4;
                        }

                    }

                    Form1.AddLog($"重新切大图 ({colCount},{rowCount})  mapDate.Version:{mapDate.Version} {imagePath}", Color.Green);
                    int imageIndex = 0; // 小图命名索引（从0开始）

                    // 遍历每个小图的位置
                    for (int row = 0; row < rowCount; row++)
                    {
                        for (int col = 0; col < colCount; col++)
                        {
                            try
                            {
                                // 计算当前小图在原图中的位置和实际尺寸
                                int srcX = col * cutSize.Width; // 原图中X坐标起点
                                int srcY = row * cutSize.Height; // 原图中Y坐标起点

                                // 实际要绘制的宽度（避免超出原图范围）
                                int drawWidth = Math.Min(cutSize.Width, originalWidth - srcX);
                                // 实际要绘制的高度（避免超出原图范围）
                                int drawHeight = Math.Min(cutSize.Height, originalHeight - srcY);

                                // 创建新的小图像（背景为黑色）
                                using (var smallImage = new MagickImage(MagickColors.Black, cutSize.Width, cutSize.Height))
                                {
                                    // 如果有实际内容要绘制
                                    if (drawWidth > 0 && drawHeight > 0)
                                    {
                                        // 使用Clone方法创建原图的裁剪区域
                                        using (var croppedImage = originalImage.Clone())
                                        {
                                            // 设置裁剪区域
                                            croppedImage.Crop(new MagickGeometry(srcX, srcY, drawWidth, drawHeight));

                                            // 将裁剪的图像绘制到黑色背景上
                                            smallImage.Composite(croppedImage, 0, 0, CompositeOperator.Over);
                                        }
                                    }

                                    //obj28  // smtiles123 //tiles147
                                    // 保存小图（路径格式：输出文件夹/索引.png）
                                    var imageDir = Path.Combine(pathOut, $"{bigImageName}");
                                    var picDir = Path.Combine(imageDir, $"obj{Form1.code_id + 1}");
                                    string savePath = "";

                                    // 计算原始索引
                                    int originalIndex = row * mapDate.Width + col * scale;
                                    int orRow = row;
                                    int orCol = col * scale;

                                    if (imageIndex < objCount)
                                    {
                                        int indexP = imageIndex + 1;
                                        string picName = $"obj{Form1.code_id + 1}_{imageIndex.ToString("D6")}.png";
                                        savePath = Path.Combine(picDir, picName);

                                        if (orRow < mapDate.Matrix.Length && orCol < mapDate.Matrix[orRow].Length)
                                        {
                                            mapDate.Matrix[orRow][orCol].FrImg = (ushort)(mapDate.Matrix[orRow][orCol].FrImg + indexP);
                                            mapDate.Matrix[orRow][orCol].Area = (byte)Form1.code_id;
                                        }
                                    }
                                    else if (imageIndex < objCount + smtileCount)
                                    {
                                        int indexP = imageIndex - objCount + 1;
                                        string picName = $"smtiles{Form1.code_id + 1}_{(indexP).ToString("D6")}.png";
                                        picDir = Path.Combine(imageDir, $"smtiles{Form1.code_id + 1}");
                                        savePath = Path.Combine(picDir, picName);

                                        if (orRow < mapDate.Matrix.Length && orCol < mapDate.Matrix[orRow].Length)
                                        {
                                            mapDate.Matrix[orRow][orCol].AreaMid = (byte)Form1.code_id;
                                            mapDate.Matrix[orRow][orCol].MidImg = (ushort)(mapDate.Matrix[orRow][orCol].BkImg + indexP);
                                        }
                                    }
                                    else if (imageIndex < objCount + smtileCount + tileCount)
                                    {
                                        int indexP = imageIndex - objCount - smtileCount + 1;
                                        string picName = $"tiles{Form1.code_id + 1}_{(indexP).ToString("D6")}.png";
                                        picDir = Path.Combine(imageDir, $"tiles{Form1.code_id + 1}");
                                        savePath = Path.Combine(picDir, picName);

                                        if (orRow < mapDate.Matrix.Length && orCol < mapDate.Matrix[orRow].Length)
                                        {
                                            mapDate.Matrix[orRow][orCol].BkImg = (ushort)(mapDate.Matrix[orRow][orCol].BkImg + indexP);
                                            mapDate.Matrix[orRow][orCol].AreaBk = (byte)Form1.code_id;
                                        }
                                    }

                                    // 创建输出文件夹（若不存在）
                                    if (!Directory.Exists(picDir))
                                    {
                                        Directory.CreateDirectory(picDir);
                                    }

                                    // 保存图像
                                    smallImage.Write(savePath,MagickFormat.Png);

                                    imageIndex++; // 索引自增

                                    // 每处理100个小图输出一次进度
                                    if (imageIndex % 100 == 0)
                                    {
                                        Form1.AddLog($"已处理 {imageIndex}/{tileTotal} 个小图", Color.Green);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Form1.AddLog($"处理小图时出错 (行: {row}, 列: {col}): {ex.Message}", Color.Red);
                            }
                        }
                    }

                    Form1.AddLog($"图像切割完成，共生成 {imageIndex} 个小图", Color.Green);
                }
            }
            catch (Exception ex)
            {
                Form1.AddLog($"处理大图像时出错: {ex.Message}", Color.Red);
                // 记录详细的错误信息到日志文件
                string errorLogPath = Path.Combine(pathOut, "error.log");
                File.AppendAllText(errorLogPath, $"[{DateTime.Now}] 图像处理错误: {ex.ToString()}\n");
            }
        }

        // 辅助方法：读取short，支持大端序和小端序
        private static short ReadShort(Stream stream, bool bigEndian)
        {
            byte[] buffer = new byte[2];
            int bytesRead = stream.Read(buffer, 0, 2);
            if (bytesRead != 2)
                throw new EndOfStreamException("无法读取足够的short数据");

            if (bigEndian)
            {
                return (short)((buffer[0] << 8) | buffer[1]);
            }
            else
            {
                return (short)((buffer[1] << 8) | buffer[0]);
            }
        }

        // 辅助方法：读取int，支持大端序和小端序
        private static int ReadInt(Stream stream, bool bigEndian)
        {
            byte[] buffer = new byte[4];
            int bytesRead = stream.Read(buffer, 0, 4);
            if (bytesRead != 4)
                throw new EndOfStreamException("无法读取足够的int数据");

            if (bigEndian)
            {
                return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
            }
            else
            {
                return (buffer[3] << 24) | (buffer[2] << 16) | (buffer[1] << 8) | buffer[0];
            }
        }
        #endregion
    }
}
