namespace LibRDP
{
    partial class TelnetControl
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
            this.ConsoleUI = new ConsoleLib.ConsoleControl();
            this.SuspendLayout();
            // 
            // ConsoleUI
            // 
            this.ConsoleUI.BackColor = System.Drawing.Color.Black;
            this.ConsoleUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsoleUI.ForeColor = System.Drawing.Color.White;
            this.ConsoleUI.IsInputEnabled = true;
            this.ConsoleUI.Location = new System.Drawing.Point(0, 0);
            this.ConsoleUI.MaxLines = 500;
            this.ConsoleUI.Name = "ConsoleUI";
            this.ConsoleUI.Size = new System.Drawing.Size(720, 576);
            this.ConsoleUI.TabIndex = 0;
            // 
            // TelnetControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ConsoleUI);
            this.Name = "TelnetControl";
            this.Size = new System.Drawing.Size(720, 576);
            this.ResumeLayout(false);

        }

        #endregion

        private ConsoleLib.ConsoleControl ConsoleUI;
    }
}
