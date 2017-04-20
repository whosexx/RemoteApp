namespace LibRDP
{
    partial class VNCControl
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
            this.RDesk = new VncSharp.RemoteDesktop();
            this.SuspendLayout();
            // 
            // RDesk
            // 
            this.RDesk.AutoScroll = true;
            this.RDesk.AutoScrollMinSize = new System.Drawing.Size(608, 427);
            this.RDesk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RDesk.Location = new System.Drawing.Point(0, 0);
            this.RDesk.Name = "RDesk";
            this.RDesk.Size = new System.Drawing.Size(720, 576);
            this.RDesk.TabIndex = 0;
            this.RDesk.ConnectComplete += new VncSharp.ConnectCompleteHandler(this.RDesk_ConnectComplete);
            this.RDesk.ConnectionLost += new System.EventHandler(this.RDesk_ConnectionLost);
            // 
            // VNCControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RDesk);
            this.Name = "VNCControl";
            this.Size = new System.Drawing.Size(720, 576);
            this.ResumeLayout(false);

        }

        #endregion

        private VncSharp.RemoteDesktop RDesk;
    }
}
