namespace FinalSiteConstituentsExporter
{
    partial class Main
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
            this.label1 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.go = new System.Windows.Forms.Button();
            this.close = new System.Windows.Forms.Button();
            this.directory = new System.Windows.Forms.Label();
            this.textBoxDir = new System.Windows.Forms.TextBox();
            this.chooseDir = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start Date for Export:";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker1.Location = new System.Drawing.Point(161, 24);
            this.dateTimePicker1.MaxDate = new System.DateTime(2099, 12, 31, 0, 0, 0, 0);
            this.dateTimePicker1.MinDate = new System.DateTime(2015, 1, 1, 0, 0, 0, 0);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(99, 20);
            this.dateTimePicker1.TabIndex = 1;
            this.dateTimePicker1.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
            // 
            // go
            // 
            this.go.Location = new System.Drawing.Point(166, 113);
            this.go.Name = "go";
            this.go.Size = new System.Drawing.Size(75, 23);
            this.go.TabIndex = 5;
            this.go.Text = "&Go";
            this.go.UseVisualStyleBackColor = true;
            this.go.Click += new System.EventHandler(this.go_Click);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(297, 113);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(75, 23);
            this.close.TabIndex = 6;
            this.close.Text = "&Close";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // directory
            // 
            this.directory.AutoSize = true;
            this.directory.Location = new System.Drawing.Point(40, 61);
            this.directory.Name = "directory";
            this.directory.Size = new System.Drawing.Size(87, 13);
            this.directory.TabIndex = 2;
            this.directory.Text = "Output Directory:";
            // 
            // textBoxDir
            // 
            this.textBoxDir.Location = new System.Drawing.Point(161, 58);
            this.textBoxDir.Name = "textBoxDir";
            this.textBoxDir.Size = new System.Drawing.Size(292, 20);
            this.textBoxDir.TabIndex = 3;
            this.textBoxDir.Text = "c:\\";
            // 
            // chooseDir
            // 
            this.chooseDir.Location = new System.Drawing.Point(459, 56);
            this.chooseDir.Name = "chooseDir";
            this.chooseDir.Size = new System.Drawing.Size(40, 23);
            this.chooseDir.TabIndex = 4;
            this.chooseDir.Text = "...";
            this.chooseDir.UseVisualStyleBackColor = true;
            this.chooseDir.Click += new System.EventHandler(this.chooseDir_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.HelpRequest += new System.EventHandler(this.folderBrowserDialog1_HelpRequest);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 148);
            this.Controls.Add(this.chooseDir);
            this.Controls.Add(this.textBoxDir);
            this.Controls.Add(this.directory);
            this.Controls.Add(this.close);
            this.Controls.Add(this.go);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.Text = "Finalsite Constituents Exporter";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button go;
        private System.Windows.Forms.Button close;
        private System.Windows.Forms.Label directory;
        private System.Windows.Forms.TextBox textBoxDir;
        private System.Windows.Forms.Button chooseDir;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}

