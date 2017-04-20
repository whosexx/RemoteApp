namespace ConsoleLib
{
    partial class ConsoleControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.richTextBoxConsole = new System.Windows.Forms.RichTextBox();
            this.MenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.sep1 = new System.Windows.Forms.ToolStripSeparator();
            this.Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.sep2 = new System.Windows.Forms.ToolStripSeparator();
            this.Cut = new System.Windows.Forms.ToolStripMenuItem();
            this.sep3 = new System.Windows.Forms.ToolStripSeparator();
            this.Find = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBoxConsole
            // 
            this.richTextBoxConsole.AcceptsTab = true;
            this.richTextBoxConsole.BackColor = System.Drawing.Color.Black;
            this.richTextBoxConsole.ContextMenuStrip = this.MenuStrip;
            this.richTextBoxConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxConsole.EnableAutoDragDrop = true;
            this.richTextBoxConsole.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxConsole.ForeColor = System.Drawing.Color.LightGray;
            this.richTextBoxConsole.HideSelection = false;
            this.richTextBoxConsole.Location = new System.Drawing.Point(0, 0);
            this.richTextBoxConsole.Name = "richTextBoxConsole";
            this.richTextBoxConsole.ReadOnly = true;
            this.richTextBoxConsole.Size = new System.Drawing.Size(100, 100);
            this.richTextBoxConsole.TabIndex = 0;
            this.richTextBoxConsole.Text = "";
            this.richTextBoxConsole.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RichTextBoxConsole_LinkClicked);
            this.richTextBoxConsole.SelectionChanged += new System.EventHandler(this.RichTextBoxConsole_SelectionChanged);
            // 
            // MenuStrip
            // 
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Copy,
            this.sep1,
            this.Paste,
            this.sep2,
            this.Cut,
            this.sep3,
            this.Find});
            this.MenuStrip.Name = "MenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(176, 110);
            this.MenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.MenuStrip_Opening);
            // 
            // Copy
            // 
            this.Copy.Name = "Copy";
            this.Copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Insert)));
            this.Copy.Size = new System.Drawing.Size(175, 22);
            this.Copy.Text = "复制";
            this.Copy.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // sep1
            // 
            this.sep1.Name = "sep1";
            this.sep1.Size = new System.Drawing.Size(172, 6);
            // 
            // Paste
            // 
            this.Paste.Name = "Paste";
            this.Paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.Paste.Size = new System.Drawing.Size(175, 22);
            this.Paste.Text = "粘帖";
            this.Paste.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // sep2
            // 
            this.sep2.Name = "sep2";
            this.sep2.Size = new System.Drawing.Size(172, 6);
            // 
            // Cut
            // 
            this.Cut.Name = "Cut";
            this.Cut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.Cut.Size = new System.Drawing.Size(175, 22);
            this.Cut.Text = "剪切";
            this.Cut.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // sep3
            // 
            this.sep3.Name = "sep3";
            this.sep3.Size = new System.Drawing.Size(172, 6);
            // 
            // Find
            // 
            this.Find.Name = "Find";
            this.Find.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.Find.Size = new System.Drawing.Size(175, 22);
            this.Find.Text = "查找";
            this.Find.Click += new System.EventHandler(this.MenuItem_Click);
            // 
            // ConsoleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.richTextBoxConsole);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "ConsoleControl";
            this.Size = new System.Drawing.Size(100, 100);
            this.MenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.RichTextBox richTextBoxConsole;
        private System.Windows.Forms.ContextMenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem Copy;
        private System.Windows.Forms.ToolStripMenuItem Paste;
        private System.Windows.Forms.ToolStripMenuItem Cut;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripSeparator sep2;
        private System.Windows.Forms.ToolStripSeparator sep3;
        private System.Windows.Forms.ToolStripMenuItem Find;
    }
}
