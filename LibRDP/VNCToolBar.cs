using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibRDP
{
    public partial class VNCToolBar : UserControl
    {
        public event Action CloseEvent;
        public event Action BackEvent;

        public VNCToolBar(string host)
        {            
            InitializeComponent();
            this.HostName.Text = host;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            this.BackEvent?.Invoke();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.CloseEvent?.Invoke();
        }

        private void VNCToolBar_Load(object sender, EventArgs e)
        {
           // this.Dock = DockStyle.Top;
        }

        private void Close_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Drawing2D.GraphicsPath buttonPath =
            new System.Drawing.Drawing2D.GraphicsPath();

            // Set a new rectangle to the same size as the button's 
            // ClientRectangle property.
            System.Drawing.Rectangle newRectangle = Close.ClientRectangle;

            // Decrease the size of the rectangle.
            newRectangle.Inflate(0, 0);

            // Draw the button's border.
            e.Graphics.DrawEllipse(System.Drawing.Pens.Transparent, newRectangle);

            // Increase the size of the rectangle to include the border.
            newRectangle.Inflate(0, 0);

            // Create a circle within the new rectangle.
            buttonPath.AddEllipse(newRectangle);

            // Set the button's Region property to the newly created 
            // circle region.
            Close.Region = new System.Drawing.Region(buttonPath);
        }

        private void Exit_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Drawing2D.GraphicsPath buttonPath =
            new System.Drawing.Drawing2D.GraphicsPath();

            // Set a new rectangle to the same size as the button's 
            // ClientRectangle property.
            System.Drawing.Rectangle newRectangle = Exit.ClientRectangle;

            // Decrease the size of the rectangle.
            newRectangle.Inflate(0, 0);

            // Draw the button's border.
            e.Graphics.DrawEllipse(System.Drawing.Pens.Transparent, newRectangle);

            // Increase the size of the rectangle to include the border.
            newRectangle.Inflate(0, 0);

            // Create a circle within the new rectangle.
            buttonPath.AddEllipse(newRectangle);

            // Set the button's Region property to the newly created 
            // circle region.
            Exit.Region = new System.Drawing.Region(buttonPath);
        }
    }
}
