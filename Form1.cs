
using System.Text.Json;
using System.Text;
using zy_996map.Properties;
using static System.Net.Mime.MediaTypeNames;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json.Serialization;

using System.Windows.Forms;
using Timer =  System.Windows.Forms.Timer;
using System.Diagnostics;
namespace zy_996map
{
    public partial class Form1 : Form
    {
        public static Form1 Instance;
        public static string Txt_ver { get { return Instance.txt_ver.Text; } }
        public static byte code_id { get { return (byte)(Instance.num_id.Value - 1); } }
        public static bool isDebuge { get { return Instance.ck_pic.Checked; } }
        // 帧循环定时器
        static private Timer _frameTimer;
        public Form1()
        {
            Instance = this;
            InitializeComponent();
            this.txt_input.Text = Settings.Default.txt_input;
            this.txt_input_image.Text = Settings.Default.path_input;
            this.txt_ver.Text = Settings.Default.txt_ver;
            this.num_id.Value = Settings.Default.id;
            this.txt_output.Text = Settings.Default.path_output;
            //this.txt_mapname.Text = Settings.Default.mapname;


            _frameTimer = new Timer
            {
                Interval = 16,
                Enabled = false // 初始禁用，窗体加载后启动
            };

            // 绑定每帧执行的事件
            _frameTimer.Tick += update;


        }
        private void update(object sender, EventArgs e)
        {
            this.lb_log.Text = listStr[listStr.Count - 1];
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

        public static string SelectFileSelectDialog(string filter = "图片文件|*.jpg;*.png;*.bmp|所有文件|*.*")
        {
            // 参数验证
            if (string.IsNullOrWhiteSpace(filter))
                throw new ArgumentNullException(nameof(filter), "文件筛选格式不能为空");

            // WinForms版本（推荐Windows桌面应用）
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // 设置对话框属性
                openFileDialog.Filter = filter; // 文件筛选格式
                openFileDialog.Title = "选择文件"; // 对话框标题
                openFileDialog.Multiselect = false; // 仅允许选择单个文件
                openFileDialog.CheckFileExists = true; // 验证文件是否存在
                openFileDialog.CheckPathExists = true; // 验证路径是否存在
                openFileDialog.RestoreDirectory = true; // 关闭后恢复初始目录

                // 设置初始目录（为空则使用系统默认）
                //if (!string.IsNullOrWhiteSpace(initialDirectory))
                //{
                //    openFileDialog.InitialDirectory = initialDirectory;
                //}

                try
                {
                    // 显示对话框并获取结果
                    DialogResult result = openFileDialog.ShowDialog();
                    return result == DialogResult.OK ? openFileDialog.FileName : null;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("文件选择对话框调用失败", ex);
                }
            }
        }
        private void txt_ver_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.txt_ver = this.txt_ver.Text;
            Console.WriteLine($"保存版本{this.txt_ver.Text}");
            Settings.Default.Save();
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

        //--------------------------------------

        private void btn_go_Click(object sender, EventArgs e)
        {
            Console.WriteLine(this.txt_input.Text);
            var dir = this.txt_input.Text;
            string fileName = txt_name.Text;

            //var files = Directory.GetFiles(dir, "*.map");
            //files = new[] { "00.map", "01.map", "02.map", "06.map", "013.map", "36.map" };
            //foreach ( var file in files ) 
            {
                //    fileName = Path.GetFileNameWithoutExtension(file);
                //    if (fileName == "3") continue;

                var filePath = Path.Combine(dir, $"{fileName}.map");

                var parser = new ModernMapParser();
                string originalMap = filePath;
                string jsonMap = Path.Combine(dir, $"{fileName}.json");
                string binaryCopy = Path.Combine(dir, $"{fileName}_copy.map");

                LegendOfMirMapParser.main(originalMap, jsonMap, binaryCopy);
            }

        }

        private void txt_input_image_TextChanged(object sender, EventArgs e)
        {
            var p = this.txt_input_image.Text;
            if (string.IsNullOrEmpty(Settings.Default.path_output))
            {
                this.txt_output.Text = Path.GetDirectoryName(p).Replace("resource", "resource_cut");
            }
            if (string.IsNullOrEmpty(this.txt_mapname.Text))
            {
                this.txt_mapname.Text = Path.GetFileNameWithoutExtension(p);
            }
            Settings.Default.path_input = this.txt_input_image.Text;
            Settings.Default.Save();
        }

        private void btn_openImage_Click(object sender, EventArgs e)
        {
            this.txt_input_image.Text = SelectFileSelectDialog();
        }

        private void txt_output_TextChanged(object sender, EventArgs e)
        {
            Settings.Default.path_output = this.txt_output.Text;
            Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.txt_output.Text = SelectSaveDirectory();
        }


        static List<string> listStr = new List<string>();
        public static void AddLog(string message)
        {
            AddLog(message, Color.Black);
        }
        public static void AddLog(string message, Color color)
        {

            listStr.Add(message);
            //Instance.lb_log.Text = message;
            Console.WriteLine(message);

        }

        private async void btn_goMap_Click(object sender, EventArgs e)
        {
            Console.WriteLine("btn_goMap_Click");
            await Task.Delay(1000);

            var step9Start = DateTime.Now;
            await Task.Run(() => MapReader.DoneRes_MapData(this.txt_input_image.Text, this.txt_output.Text));
            var step9End = DateTime.Now;
            AddLog($"结束 解读生成.map，耗时：{(step9End - step9Start).TotalSeconds:F2}秒", Color.Green);
            // 启动资源管理器并指定目录
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{this.txt_output.Text}\"", // 使用引号包裹路径，处理包含空格的路径
                UseShellExecute = true
            });
        
            AddLog("dfdfdf2");
            await Task.Delay(1000);
            AddLog("dfdfdf3");
            await Task.Delay(1000);
            AddLog("dfdfdf4");
            await Task.Delay(1000);
            AddLog("dfdfdf5");
            await Task.Delay(1000);
            AddLog("dfdfdf6");
            await Task.Delay(1000);
            AddLog("dfdfdf7");
            await Task.Delay(1000);
            AddLog("dfdfdf8");
        }

        private async void btn_all_Click(object sender, EventArgs e)
        {
            var step9Start = DateTime.Now;
            var files = Directory.GetFiles(Path.GetDirectoryName(this.txt_input_image.Text), "*.jpg", SearchOption.TopDirectoryOnly);
           
            int i= 1;
            foreach (var path in files)
            {
                AddLog($"-----------------{i}开始解读生成 {path}----------------------", Color.Green);
                this.num_id.Value = (byte)i;
                bool b = await Task.Run(() => MapReader.DoneRes_MapData(path, this.txt_output.Text));
                if (b)
                    i++;
            }
            var step9End = DateTime.Now;
            AddLog($"结束 解读生成.map，耗时：{(step9End - step9Start).TotalMinutes:F2}分 成功{i}个图", Color.Green);
            
            // 启动资源管理器并指定目录
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{this.txt_output.Text}\"", // 使用引号包裹路径，处理包含空格的路径
                UseShellExecute = true
            });
        }
    }
}

