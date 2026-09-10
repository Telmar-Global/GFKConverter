namespace GFKConverter
{
    partial class GFKConvert
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdProcessToIntermediate = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnGFKProcessed = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.txtGFKProcessed = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textGFFDir = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textGfkDir = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.listFiles = new System.Windows.Forms.ListBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lbDates = new System.Windows.Forms.ListBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tvDemo = new System.Windows.Forms.TreeView();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdProcessToIntermediate
            // 
            this.cmdProcessToIntermediate.Location = new System.Drawing.Point(576, 226);
            this.cmdProcessToIntermediate.Name = "cmdProcessToIntermediate";
            this.cmdProcessToIntermediate.Size = new System.Drawing.Size(165, 23);
            this.cmdProcessToIntermediate.TabIndex = 5;
            this.cmdProcessToIntermediate.Text = "Process To GFF";
            this.cmdProcessToIntermediate.UseVisualStyleBackColor = true;
            this.cmdProcessToIntermediate.Click += new System.EventHandler(this.cmdProcessToIntermediate_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(665, 255);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(76, 23);
            this.button6.TabIndex = 6;
            this.button6.Text = "Done";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Location = new System.Drawing.Point(12, 354);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(523, 20);
            this.txtInfo.TabIndex = 9;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnGFKProcessed);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.txtGFKProcessed);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textGFFDir);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textGfkDir);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(523, 118);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Directories";
            // 
            // btnGFKProcessed
            // 
            this.btnGFKProcessed.Location = new System.Drawing.Point(484, 56);
            this.btnGFKProcessed.Name = "btnGFKProcessed";
            this.btnGFKProcessed.Size = new System.Drawing.Size(28, 23);
            this.btnGFKProcessed.TabIndex = 29;
            this.btnGFKProcessed.Text = "...";
            this.btnGFKProcessed.UseVisualStyleBackColor = true;
            this.btnGFKProcessed.Click += new System.EventHandler(this.btnGFKProcessed_Click_1);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 13);
            this.label7.TabIndex = 28;
            this.label7.Text = "GfK Files processed";
            // 
            // txtGFKProcessed
            // 
            this.txtGFKProcessed.Location = new System.Drawing.Point(117, 56);
            this.txtGFKProcessed.Name = "txtGFKProcessed";
            this.txtGFKProcessed.Size = new System.Drawing.Size(361, 20);
            this.txtGFKProcessed.TabIndex = 27;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(484, 84);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(28, 23);
            this.button3.TabIndex = 23;
            this.button3.Text = "...";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "GFF files";
            // 
            // textGFFDir
            // 
            this.textGFFDir.Location = new System.Drawing.Point(117, 84);
            this.textGFFDir.Name = "textGFFDir";
            this.textGFFDir.Size = new System.Drawing.Size(361, 20);
            this.textGFFDir.TabIndex = 21;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(484, 28);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 23);
            this.button1.TabIndex = 17;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "GfK Files";
            // 
            // textGfkDir
            // 
            this.textGfkDir.Location = new System.Drawing.Point(117, 28);
            this.textGfkDir.Name = "textGfkDir";
            this.textGfkDir.Size = new System.Drawing.Size(361, 20);
            this.textGfkDir.TabIndex = 15;
            this.textGfkDir.TextChanged += new System.EventHandler(this.textGfkDir_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.listFiles);
            this.groupBox2.Location = new System.Drawing.Point(541, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 208);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "GfK files found";
            // 
            // listFiles
            // 
            this.listFiles.FormattingEnabled = true;
            this.listFiles.Location = new System.Drawing.Point(15, 19);
            this.listFiles.Name = "listFiles";
            this.listFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listFiles.Size = new System.Drawing.Size(170, 173);
            this.listFiles.TabIndex = 4;
            this.listFiles.Click += new System.EventHandler(this.listFiles_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lbDates);
            this.groupBox3.Location = new System.Drawing.Point(12, 140);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(146, 208);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Dates";
            // 
            // lbDates
            // 
            this.lbDates.FormattingEnabled = true;
            this.lbDates.Location = new System.Drawing.Point(12, 19);
            this.lbDates.Name = "lbDates";
            this.lbDates.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lbDates.Size = new System.Drawing.Size(122, 173);
            this.lbDates.TabIndex = 0;
            this.lbDates.SelectedIndexChanged += new System.EventHandler(this.lbDates_SelectedIndexChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tvDemo);
            this.groupBox4.Location = new System.Drawing.Point(164, 141);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(359, 206);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Demographics";
            // 
            // tvDemo
            // 
            this.tvDemo.Location = new System.Drawing.Point(6, 19);
            this.tvDemo.Name = "tvDemo";
            this.tvDemo.Size = new System.Drawing.Size(337, 172);
            this.tvDemo.TabIndex = 0;
            this.tvDemo.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // GFKConvert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(751, 386);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.cmdProcessToIntermediate);
            this.Name = "GFKConvert";
            this.Text = "GFK Converter";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button cmdProcessToIntermediate;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGFKProcessed;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtGFKProcessed;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textGFFDir;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textGfkDir;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox listFiles;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox lbDates;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TreeView tvDemo;
    }
}
