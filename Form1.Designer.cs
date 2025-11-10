namespace zy_996map
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btn_go = new Button();
            label1 = new Label();
            txt_input = new TextBox();
            btn_open = new Button();
            txt_name = new TextBox();
            groupTest = new GroupBox();
            label2 = new Label();
            group_map = new GroupBox();
            btn_openImage = new Button();
            txt_input_image = new TextBox();
            label3 = new Label();
            txt_ver = new TextBox();
            lb_version = new Label();
            groupBox1 = new GroupBox();
            ck_pic = new CheckBox();
            num_id = new NumericUpDown();
            button2 = new Button();
            label6 = new Label();
            txt_output = new TextBox();
            label5 = new Label();
            txt_mapname = new TextBox();
            label4 = new Label();
            label7 = new Label();
            textBox3 = new TextBox();
            label8 = new Label();
            label9 = new Label();
            button1 = new Button();
            btn_goMap = new Button();
            label10 = new Label();
            lb_log = new Label();
            btn_all = new Button();
            groupTest.SuspendLayout();
            group_map.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)num_id).BeginInit();
            SuspendLayout();
            // 
            // btn_go
            // 
            btn_go.Location = new Point(404, 94);
            btn_go.Name = "btn_go";
            btn_go.Size = new Size(107, 46);
            btn_go.TabIndex = 0;
            btn_go.Text = "开始";
            btn_go.UseVisualStyleBackColor = true;
            btn_go.Click += btn_go_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 20);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 1;
            label1.Text = "导入路径";
            // 
            // txt_input
            // 
            txt_input.Location = new Point(74, 16);
            txt_input.Name = "txt_input";
            txt_input.Size = new Size(384, 23);
            txt_input.TabIndex = 2;
            txt_input.Text = "             ";
            txt_input.TextChanged += txt_input_TextChanged;
            // 
            // btn_open
            // 
            btn_open.Location = new Point(475, 12);
            btn_open.Name = "btn_open";
            btn_open.Size = new Size(59, 29);
            btn_open.TabIndex = 3;
            btn_open.Text = "open";
            btn_open.UseVisualStyleBackColor = true;
            btn_open.Click += btn_open_Click;
            // 
            // txt_name
            // 
            txt_name.Location = new Point(75, 46);
            txt_name.Name = "txt_name";
            txt_name.Size = new Size(384, 23);
            txt_name.TabIndex = 4;
            txt_name.Text = "cjzc";
            // 
            // groupTest
            // 
            groupTest.Controls.Add(label2);
            groupTest.Controls.Add(txt_name);
            groupTest.Controls.Add(btn_open);
            groupTest.Controls.Add(txt_input);
            groupTest.Controls.Add(label1);
            groupTest.Controls.Add(btn_go);
            groupTest.Location = new Point(9, 16);
            groupTest.Name = "groupTest";
            groupTest.Size = new Size(594, 312);
            groupTest.TabIndex = 5;
            groupTest.TabStop = false;
            groupTest.Text = "测试";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 51);
            label2.Name = "label2";
            label2.Size = new Size(56, 17);
            label2.TabIndex = 5;
            label2.Text = "文件名字";
            // 
            // group_map
            // 
            group_map.Controls.Add(btn_openImage);
            group_map.Controls.Add(txt_input_image);
            group_map.Controls.Add(label3);
            group_map.Controls.Add(txt_ver);
            group_map.Controls.Add(lb_version);
            group_map.Location = new Point(9, 0);
            group_map.Name = "group_map";
            group_map.Size = new Size(775, 162);
            group_map.TabIndex = 6;
            group_map.TabStop = false;
            group_map.Text = "来源";
            // 
            // btn_openImage
            // 
            btn_openImage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btn_openImage.Location = new Point(694, 81);
            btn_openImage.Name = "btn_openImage";
            btn_openImage.Size = new Size(75, 23);
            btn_openImage.TabIndex = 4;
            btn_openImage.Text = "打开";
            btn_openImage.UseVisualStyleBackColor = true;
            btn_openImage.Click += btn_openImage_Click;
            // 
            // txt_input_image
            // 
            txt_input_image.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txt_input_image.Location = new Point(98, 82);
            txt_input_image.Name = "txt_input_image";
            txt_input_image.Size = new Size(590, 23);
            txt_input_image.TabIndex = 3;
            txt_input_image.TextChanged += txt_input_image_TextChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(33, 86);
            label3.Name = "label3";
            label3.Size = new Size(68, 17);
            label3.TabIndex = 2;
            label3.Text = "完整大地图";
            // 
            // txt_ver
            // 
            txt_ver.Location = new Point(98, 38);
            txt_ver.Name = "txt_ver";
            txt_ver.Size = new Size(100, 23);
            txt_ver.TabIndex = 1;
            txt_ver.Text = "1.28929.4";
            txt_ver.TextChanged += txt_ver_TextChanged;
            // 
            // lb_version
            // 
            lb_version.AutoSize = true;
            lb_version.Location = new Point(33, 42);
            lb_version.Name = "lb_version";
            lb_version.Size = new Size(56, 17);
            lb_version.TabIndex = 0;
            lb_version.Text = "资源版本";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(ck_pic);
            groupBox1.Controls.Add(num_id);
            groupBox1.Controls.Add(button2);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(txt_output);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(txt_mapname);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(textBox3);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(button1);
            groupBox1.Location = new Point(10, 168);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(784, 160);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "输出";
            // 
            // ck_pic
            // 
            ck_pic.AutoSize = true;
            ck_pic.Location = new Point(83, 29);
            ck_pic.Name = "ck_pic";
            ck_pic.Size = new Size(111, 21);
            ck_pic.TabIndex = 17;
            ck_pic.Text = "测试格式化碎图";
            ck_pic.UseVisualStyleBackColor = true;
            // 
            // num_id
            // 
            num_id.Location = new Point(196, 52);
            num_id.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            num_id.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            num_id.Name = "num_id";
            num_id.Size = new Size(120, 23);
            num_id.TabIndex = 16;
            num_id.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.Location = new Point(685, 103);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 15;
            button2.Text = "打开";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new Point(621, 54);
            label6.Name = "label6";
            label6.Size = new Size(37, 17);
            label6.TabIndex = 14;
            label6.Text = ".map";
            // 
            // txt_output
            // 
            txt_output.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txt_output.Location = new Point(88, 105);
            txt_output.Name = "txt_output";
            txt_output.Size = new Size(591, 23);
            txt_output.TabIndex = 13;
            txt_output.TextChanged += txt_output_TextChanged;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Location = new Point(26, 108);
            label5.Name = "label5";
            label5.Size = new Size(56, 17);
            label5.TabIndex = 12;
            label5.Text = "输出目录";
            // 
            // txt_mapname
            // 
            txt_mapname.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txt_mapname.Location = new Point(495, 51);
            txt_mapname.Name = "txt_mapname";
            txt_mapname.Size = new Size(123, 23);
            txt_mapname.TabIndex = 11;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(411, 55);
            label4.Name = "label4";
            label4.Size = new Size(82, 17);
            label4.TabIndex = 10;
            label4.Text = "map文件名字";
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Location = new Point(1168, 245);
            label7.Name = "label7";
            label7.Size = new Size(37, 17);
            label7.TabIndex = 9;
            label7.Text = ".map";
            // 
            // textBox3
            // 
            textBox3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBox3.Location = new Point(1042, 241);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(123, 23);
            textBox3.TabIndex = 8;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Location = new Point(977, 245);
            label8.Name = "label8";
            label8.Size = new Size(70, 17);
            label8.TabIndex = 7;
            label8.Text = "map文件名";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(83, 55);
            label9.Name = "label9";
            label9.Size = new Size(113, 17);
            label9.TabIndex = 5;
            label9.Text = "碎图编号（1-256）";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Location = new Point(1287, 81);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "打开";
            button1.UseVisualStyleBackColor = true;
            // 
            // btn_goMap
            // 
            btn_goMap.Anchor = AnchorStyles.Top;
            btn_goMap.Location = new Point(402, 334);
            btn_goMap.Name = "btn_goMap";
            btn_goMap.Size = new Size(226, 51);
            btn_goMap.TabIndex = 16;
            btn_goMap.Text = "开始";
            btn_goMap.UseVisualStyleBackColor = true;
            btn_goMap.Click += btn_goMap_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(24, 355);
            label10.Name = "label10";
            label10.Size = new Size(217, 17);
            label10.TabIndex = 17;
            label10.Text = "注：导出文件包含地图碎图和.map文件";
            // 
            // lb_log
            // 
            lb_log.AutoSize = true;
            lb_log.Location = new Point(24, 384);
            lb_log.Name = "lb_log";
            lb_log.Size = new Size(0, 17);
            lb_log.TabIndex = 18;
            // 
            // btn_all
            // 
            btn_all.Anchor = AnchorStyles.Top;
            btn_all.Location = new Point(645, 334);
            btn_all.Name = "btn_all";
            btn_all.Size = new Size(133, 51);
            btn_all.TabIndex = 19;
            btn_all.Text = "全搞(搜同目录)";
            btn_all.UseVisualStyleBackColor = true;
            btn_all.Click += btn_all_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 410);
            Controls.Add(btn_all);
            Controls.Add(lb_log);
            Controls.Add(label10);
            Controls.Add(btn_goMap);
            Controls.Add(group_map);
            Controls.Add(groupBox1);
            Controls.Add(groupTest);
            Name = "Form1";
            Text = "霸业-->996map解析生成";
            groupTest.ResumeLayout(false);
            groupTest.PerformLayout();
            group_map.ResumeLayout(false);
            group_map.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)num_id).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_go;
        private Label label1;
        private TextBox txt_input;
        private Button btn_open;
        private TextBox txt_name;
        private GroupBox groupTest;
        private Label label2;
        private GroupBox group_map;
        private Button btn_openImage;
        private TextBox txt_input_image;
        private Label label3;
        private TextBox txt_ver;
        private Label lb_version;
        private GroupBox groupBox1;
        private Label label7;
        private TextBox textBox3;
        private Label label8;
        private Label label9;
        private Button button1;
        private TextBox txt_output;
        private Label label5;
        private TextBox txt_mapname;
        private Label label4;
        private Button button2;
        private Label label6;
        private Button btn_goMap;
        private Label label10;
        private Label lb_log;
        private NumericUpDown num_id;
        private Button btn_all;
        private CheckBox ck_pic;
    }
}
