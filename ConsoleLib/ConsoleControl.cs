using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleLib
{
    /// <summary>
    /// The console event handler is used for console events.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ConsoleEventArgs"/> instance containing the event data.</param>
    public delegate void ConsoleEventHanlder(object sender, ConsoleEventArgs args);

    /// <summary>
    /// The Console Control allows you to embed a basic console in your application.
    /// </summary>
    [ToolboxBitmap("ConsoleControl.ConsoleControl.bmp")]
    public partial class ConsoleControl : UserControl
    {
        private bool IsMatchOk = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleControl"/> class.
        /// </summary>
        public ConsoleControl()
        {
            InitializeComponent();

            IsInputEnabled = true;
            this.LastCMD = string.Empty;

            //  Wait for key down messages on the rich text box.
            richTextBoxConsole.KeyDown += new KeyEventHandler(RichTextBoxConsole_KeyDown);
            this.richTextBoxConsole.Focus();
        }

        private List<string> HistoryCMD = new List<string>();
        private void AddCMD(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd)
                    || (HistoryCMD.Count > 0 && this.HistoryCMD[0] == cmd))
                return;

            if (this.HistoryCMD.Count >= 500)
                this.HistoryCMD.RemoveAt(this.HistoryCMD.Count - 1);

            this.HistoryCMD.Insert(0, cmd);
        }
        private int Index = -1;
        private bool IsManualInput = true;
        //private bool IsSend = false;
        /// <summary>
        /// Handles the KeyDown event of the richTextBoxConsole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.</param>
        void RichTextBoxConsole_KeyDown(object sender, KeyEventArgs e)
        {
            if (richTextBoxConsole.SelectionStart < InputStart)
            {
                //    //  Allow arrows and Ctrl-C.
                if (!(e.KeyCode == Keys.Left ||
                    e.KeyCode == Keys.Right ||
                    //e.KeyCode == Keys.Up ||
                    //e.KeyCode == Keys.Down ||
                    e.KeyCode == Keys.Home ||
                    e.KeyCode == Keys.End ||
                    e.KeyCode == Keys.PageUp ||
                    e.KeyCode == Keys.PageDown ||
                    (e.KeyCode == Keys.C && e.Control)))
                {
                    e.SuppressKeyPress = true;
                    if (e.KeyCode == Keys.Return)
                        this.InsurePosition();
                    return;
                }
            }

            //  If we're at the input point and it's backspace, bail.
            if (richTextBoxConsole.SelectionStart <= InputStart
                && e.KeyCode == Keys.Back) 
                e.SuppressKeyPress = true;

            if(e.KeyCode == Keys.Tab)
            {
                e.SuppressKeyPress = true;
                return;
            }

            if(e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                if (this.Index >= (this.HistoryCMD.Count - 1))
                    return;

                this.Index++;
                this.ClearLastInput();
                richTextBoxConsole.SelectedText += this.HistoryCMD[this.Index];
                this.IsManualInput = false;
                return;
            }

            if (e.KeyCode == Keys.Down)
            { 
                e.SuppressKeyPress = true;
                if (this.Index <= 0)
                    return;

                this.Index--;
                this.ClearLastInput();
                richTextBoxConsole.SelectedText += this.HistoryCMD[this.Index];
                this.IsManualInput = false;
                return;
            }

            if (e.KeyCode == Keys.C && e.Control)
            {
                this.InsurePosition();
                this.Index = -1;

                var c = (char)0x03;
                WriteInput(c.ToString(), false);
                e.SuppressKeyPress = true;                
                return;
            }

            //  Is it the return key?
            if (e.KeyCode == Keys.Return)
            {
                //  Get the input.
                string input = this.GetLastInputString();                
                WriteInput(input, false);
            }
            this.IsManualInput = true;
        }

        private void ClearLastInput()
        {
            var InputEnd = this.richTextBoxConsole.TextLength;
            if (InputEnd > InputStart)
            {
                this.richTextBoxConsole.Select(InputStart, InputEnd);
                this.richTextBoxConsole.SelectedText = "";
            }
        }

        private string GetLastInputString() => richTextBoxConsole.Text.Substring(InputStart, this.richTextBoxConsole.TextLength - InputStart);

        public static readonly Encoding Default = Encoding.UTF8;
        public void WriteOutput(byte[] output, Encoding en = null)
        {
            //27

        }

        /// <summary>
        /// Writes the output to the console control.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="color">The color.</param>
        public void WriteOutput(string output)
        {            
            if (!this.clearTag || this.Lines >= MaxLines)
            {
                this.ClearOutput();
                this.clearTag = true;
            }

            try
            {
                if (!this.IsMatchOk && this.LastCMD != null)
                {
                    int index = output.IndexOf('\n');
                    if (index > 0)
                    {
                        string test = output.Substring(0, index).Trim();
                        if (test == LastCMD.Trim())
                        {
                            output = output.Substring(index + 1);
                            this.IsMatchOk = true;
                            if (string.IsNullOrEmpty(output))
                                return;                              
                        }                            
                    }
                }
            }catch(Exception e) { MessageBox.Show(e.ToString()); }
            
            Invoke((Action)(() =>
            {
                this.InsurePosition();
                //this.ClearLastInput();

                this.richTextBoxConsole.ScrollToCaret();
                richTextBoxConsole.SelectedText += output;

                InputStart = richTextBoxConsole.SelectionStart;
            }));
        }

        private bool clearTag = true;
        public void ClearOutput(bool tag = true)
        {
            if(!tag)
            {
                this.clearTag = tag;
                return;
            }

            richTextBoxConsole.Clear();
            InputStart = 0;
            richTextBoxConsole.SelectionStart = 0;
        }

        /// <summary>
        /// Writes the input to the console control.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="color">The color.</param>
        /// <param name="echo">if set to <c>true</c> echo the input.</param>
        public void WriteInput(string input, bool echo = false)
        {
            if (this.IsManualInput)
                this.AddCMD(input);

            LastCMD = input;
            input += "\n";
            this.Index = -1;

            Invoke((Action)(() =>
            {
                //  Are we echoing?
                if (echo)
                {
                    this.InsurePosition();                    
                    richTextBoxConsole.SelectedText += input;
                    //InputStart = richTextBoxConsole.SelectionStart;
                }                
            }));
                       
            this.IsMatchOk = false;
            this.IsManualInput = false;

            //  Fire the event.
            FireConsoleInputEvent(input);
        }       
        
        private void InsurePosition()
        {
            if (this.richTextBoxConsole.SelectionStart < InputStart)
                this.richTextBoxConsole.SelectionStart = this.richTextBoxConsole.TextLength;
        }

        /// <summary>
        /// Fires the console output event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireConsoleOutputEvent(string content) => OnConsoleOutput?.Invoke(this, new ConsoleEventArgs(content));

        /// <summary>
        /// Fires the console input event.
        /// </summary>
        /// <param name="content">The content.</param>
        private void FireConsoleInputEvent(string content) => OnConsoleInput?.Invoke(this, new ConsoleEventArgs(content));

        /// <summary>
        /// Current position that input starts at.
        /// </summary>
        private int InputStart = 0;
        //private int InputEnd = 0;

        /// <summary>
        /// The is input enabled flag.
        /// </summary>
        private bool isInputEnabled = true;

        /// <summary>
        /// The last input string (used so that we can make sure we don't echo input twice).
        /// </summary>
        private string LastCMD;

        /// <summary>
        /// Occurs when console output is produced.
        /// </summary>
        public event ConsoleEventHanlder OnConsoleOutput;

        /// <summary>
        /// Occurs when console input is produced.
        /// </summary>
        public event ConsoleEventHanlder OnConsoleInput;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is input enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is input enabled; otherwise, <c>false</c>.
        /// </value>
        [Category("Console Control"), Description("If true, the user can key in input.")]
        public bool IsInputEnabled
        {
            get => isInputEnabled;
            set
            {
                isInputEnabled = value;
                richTextBoxConsole.ReadOnly = !value;
            }
        }

        public int MaxLines { get; set; } = 500;

        public int Lines => this.richTextBoxConsole.Lines.Length;

        /// <summary>
        /// Gets the internal rich text box.
        /// </summary>
        [Browsable(false)]
        public RichTextBox InternalRichTextBox => richTextBoxConsole;

        /// <summary>
        /// Gets or sets the font of the text displayed by the control.
        /// </summary>
        /// <returns>The <see cref="T:System.Drawing.Font" /> to apply to the text displayed by the control. The default is the value of the <see cref="P:System.Windows.Forms.Control.DefaultFont" /> property.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        ///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                richTextBoxConsole.Font = value;
            }
        }

        private void RichTextBoxConsole_LinkClicked(object sender, LinkClickedEventArgs e) => Process.Start(e.LinkText);

        private void RichTextBoxConsole_SelectionChanged(object sender, EventArgs e)
        {            
            var obj = sender as RichTextBox;
            if (obj.SelectionStart < InputStart)
                obj.ReadOnly = true;
            else
            {
                if(this.IsInputEnabled)
                    obj.ReadOnly = false;
            }
        }

        private void MenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if(this.richTextBoxConsole.ReadOnly)
            {
                this.Cut.Enabled = false;
                this.Paste.Enabled = false;
            }
            else
            {
                this.Cut.Enabled = true;
                this.Paste.Enabled = true;
            }

            if (!string.IsNullOrEmpty(this.richTextBoxConsole.SelectedText))
                this.Copy.Enabled = true;
            else
                this.Copy.Enabled = false;
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null)
                return;

            switch((item.Text))
            {
                case "复制":
                {                    
                    this.richTextBoxConsole.Copy();
                    break;
                }
                case "粘帖":
                {
                    this.InsurePosition();
                    if (this.richTextBoxConsole.CanPaste(DataFormats.GetFormat(DataFormats.Text)))
                    {
                        var txt = Clipboard.GetText();
                        this.WriteInput(txt, false);
                    }
                    break;
                }
                case "剪切":
                {
                    this.richTextBoxConsole.Cut();
                    break;
                }
                case "查找":
                {
                    var font = new Font("幼圆", 14, FontStyle.Bold | FontStyle.Underline);
                    var color = Color.Yellow;

                    var search = this.richTextBoxConsole.SelectedText;
                    this.richTextBoxConsole.SelectionFont = font;
                    this.richTextBoxConsole.SelectionColor = color;
                    int index = this.richTextBoxConsole.Find(search, 0, this.richTextBoxConsole.TextLength, RichTextBoxFinds.None);
                    while(index != -1)
                    {
                        index += search.Length;
                        this.richTextBoxConsole.SelectionFont = font;
                        this.richTextBoxConsole.SelectionColor = color;                        
                        index = this.richTextBoxConsole.Find(search, index, this.richTextBoxConsole.TextLength, RichTextBoxFinds.None);
                    }
                    break;
                }
            }
        }
    }
}