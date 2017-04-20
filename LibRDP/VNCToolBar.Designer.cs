namespace LibRDP
{
    partial class VNCToolBar
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.Exit = new System.Windows.Forms.Button();
            this.Close = new System.Windows.Forms.Button();
            this.HostName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Exit
            // 
            this.Exit.BackgroundImage = global::LibRDP.Properties.Resources.ExitFullScreen;
            this.Exit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Exit.FlatAppearance.BorderSize = 0;
            this.Exit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SlateBlue;
            this.Exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Exit.Location = new System.Drawing.Point(171, 6);
            this.Exit.Name = "Exit";
            this.Exit.Size = new System.Drawing.Size(25, 25);
            this.Exit.TabIndex = 1;
            this.Exit.UseVisualStyleBackColor = true;
            this.Exit.Click += new System.EventHandler(this.Exit_Click);
            this.Exit.Paint += new System.Windows.Forms.PaintEventHandler(this.Exit_Paint);
            // 
            // Close
            // 
            this.Close.BackgroundImage = global::LibRDP.Properties.Resources.Close;
            this.Close.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Close.FlatAppearance.BorderSize = 0;
            this.Close.FlatAppearance.MouseDownBackColor = System.Drawing.Color.SlateBlue;
            this.Close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Close.Location = new System.Drawing.Point(206, 6);
            this.Close.Name = "Close";
            this.Close.Size = new System.Drawing.Size(25, 25);
            this.Close.TabIndex = 0;
            this.Close.UseVisualStyleBackColor = true;
            this.Close.Click += new System.EventHandler(this.Close_Click);
            this.Close.Paint += new System.Windows.Forms.PaintEventHandler(this.Close_Paint);
            // 
            // HostName
            // 
            this.HostName.AutoSize = true;
            this.HostName.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.HostName.Location = new System.Drawing.Point(17, 5);
            this.HostName.Name = "HostName";
            this.HostName.Size = new System.Drawing.Size(91, 21);
            this.HostName.TabIndex = 2;
            this.HostName.Text = "HostName";
            this.HostName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // VNCToolBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.HostName);
            this.Controls.Add(this.Exit);
            this.Controls.Add(this.Close);
            this.Name = "VNCToolBar";
            this.Size = new System.Drawing.Size(252, 38);
            this.Load += new System.EventHandler(this.VNCToolBar_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Close;
        private System.Windows.Forms.Button Exit;
        private System.Windows.Forms.Label HostName;
    }
}
