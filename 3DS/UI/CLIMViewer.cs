using System;
using System.Windows.Forms;
using _3DS.NintendoWare.LYT1;

namespace _3DS.UI
{
    public partial class CLIMViewer : Form
	{
		CLIM Image;
		public CLIMViewer(CLIM Image)
		{
			this.Image = Image;
			InitializeComponent();
		}

		private void CLIMViewer_Load(object sender, EventArgs e)
		{
			pictureBox1.Image = Image.ToBitmap();
		}
	}
}
