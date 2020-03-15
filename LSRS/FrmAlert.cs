using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace LSRS
{
    public partial class FrmAlert : Form
    {
        public FrmAlert()
        {
            InitializeComponent();
        }

        public FrmAlert(Stream imgPath,string link)
        {
            InitializeComponent();

            _link = link;
            this.BackgroundImage = Image.FromStream(imgPath);
        }

        private string _link = string.Empty;

        private void label1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmAlert_Load(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHight = Screen.PrimaryScreen.WorkingArea.Height;

            int x = screenWidth - this.Width - 5;
            int y = screenHight - this.Height - 5;
            this.Location = new Point(x, y);
        }

        private void FrmAlert_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(_link);
            }
            catch (Exception ex)
            {
                ZtTools.Tools.Error(ex.ToString());
            }
        }
    }
}
