using System;
using System.Windows.Forms;

namespace _3DS.UI
{
    public partial class ThemeIconViewer : Form
    {
        ThemeIconFile themeIcon;
        public ThemeIconViewer(ThemeIconFile themeIcon)
        {
            this.themeIcon = themeIcon;
            InitializeComponent();
        }

        private void ThemeIconViewer_Load_1(object sender, EventArgs e)
        {
            pictureBox1.Image = themeIcon.getIconTexture();
        }
    }
}
