
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

    }
}

