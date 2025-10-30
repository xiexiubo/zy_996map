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
            SuspendLayout();
            // 
            // btn_go
            // 
            btn_go.Location = new Point(344, 190);
            btn_go.Name = "btn_go";
            btn_go.Size = new Size(107, 81);
            btn_go.TabIndex = 0;
            btn_go.Text = "开始";
            btn_go.UseVisualStyleBackColor = true;
            btn_go.Click += btn_go_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(23, 36);
            label1.Name = "label1";
            label1.Size = new Size(56, 17);
            label1.TabIndex = 1;
            label1.Text = "导入路径";
            // 
            // txt_input
            // 
            txt_input.Location = new Point(83, 32);
            txt_input.Name = "txt_input";
            txt_input.Size = new Size(384, 23);
            txt_input.TabIndex = 2;
            txt_input.TextChanged += txt_input_TextChanged;
            // 
            // btn_open
            // 
            btn_open.Location = new Point(484, 28);
            btn_open.Name = "btn_open";
            btn_open.Size = new Size(59, 29);
            btn_open.TabIndex = 3;
            btn_open.Text = "open";
            btn_open.UseVisualStyleBackColor = true;
            btn_open.Click += btn_open_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btn_open);
            Controls.Add(txt_input);
            Controls.Add(label1);
            Controls.Add(btn_go);
            Name = "Form1";
            Text = "996map解析生成";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_go;
        private Label label1;
        private TextBox txt_input;
        private Button btn_open;
    }
}
