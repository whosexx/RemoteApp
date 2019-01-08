using AxMSTSCLib;
using System.Windows.Forms;

namespace LibRDP
{
    partial class RDPControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RDPControl));
            this.TextLabel = new System.Windows.Forms.Label();
            this.rdpc = new AxMSTSCLib.AxMsRdpClient8NotSafeForScripting();
            ((System.ComponentModel.ISupportInitialize)(this.rdpc)).BeginInit();
            this.SuspendLayout();
            // 
            // TextLabel
            // 
            this.TextLabel.BackColor = System.Drawing.Color.Black;
            this.TextLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextLabel.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TextLabel.ForeColor = System.Drawing.Color.Lime;
            this.TextLabel.Location = new System.Drawing.Point(0, 0);
            this.TextLabel.Name = "TextLabel";
            this.TextLabel.Size = new System.Drawing.Size(720, 576);
            this.TextLabel.TabIndex = 0;
            this.TextLabel.Text = "正在连接……";
            this.TextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rdpc
            // 
            this.rdpc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdpc.Enabled = true;
            this.rdpc.Location = new System.Drawing.Point(0, 0);
            this.rdpc.Name = "rdpc";
            this.rdpc.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("rdpc.OcxState")));
            this.rdpc.Size = new System.Drawing.Size(720, 576);
            this.rdpc.TabIndex = 2;
            // 
            // RDPControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.TextLabel);
            this.Controls.Add(this.rdpc);
            this.Name = "RDPControl";
            this.Size = new System.Drawing.Size(720, 576);
            ((System.ComponentModel.ISupportInitialize)(this.rdpc)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label TextLabel;
        private AxMsRdpClient8NotSafeForScripting rdpc;
    }
}
