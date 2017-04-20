namespace LibRDP
{
    partial class RDPViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RDPViewer));
            this.TextLabel = new System.Windows.Forms.Label();
            this.axRDPViewer = new AxRDPCOMAPILib.AxRDPViewer();
            ((System.ComponentModel.ISupportInitialize)(this.axRDPViewer)).BeginInit();
            this.SuspendLayout();
            // 
            // TextLabel
            // 
            this.TextLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextLabel.Font = new System.Drawing.Font("微软雅黑", 24F);
            this.TextLabel.ForeColor = System.Drawing.Color.Lime;
            this.TextLabel.Location = new System.Drawing.Point(0, 0);
            this.TextLabel.Name = "TextLabel";
            this.TextLabel.Size = new System.Drawing.Size(720, 576);
            this.TextLabel.TabIndex = 1;
            this.TextLabel.Text = "正在连接服务端。。。";
            this.TextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // axRDPViewer
            // 
            this.axRDPViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axRDPViewer.Enabled = true;
            this.axRDPViewer.Location = new System.Drawing.Point(0, 0);
            this.axRDPViewer.Name = "axRDPViewer";
            this.axRDPViewer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axRDPViewer.OcxState")));
            this.axRDPViewer.Size = new System.Drawing.Size(720, 576);
            this.axRDPViewer.TabIndex = 0;
            // 
            // RDPViewer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.TextLabel);
            this.Controls.Add(this.axRDPViewer);
            this.Name = "RDPViewer";
            this.Size = new System.Drawing.Size(720, 576);
            ((System.ComponentModel.ISupportInitialize)(this.axRDPViewer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxRDPCOMAPILib.AxRDPViewer axRDPViewer;
        private System.Windows.Forms.Label TextLabel;
    }
}
