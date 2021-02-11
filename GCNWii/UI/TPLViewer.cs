using System;
using System.Windows.Forms;

namespace GCNWii.UI
{
    public partial class TPLViewer : Form
	{
		TPL Image;
		public TPLViewer(TPL Image)
		{
			this.Image = Image;
			InitializeComponent();
		}

		private void CLIMViewer_Load(object sender, EventArgs e)
		{
			pictureBox1.Image = Image.Textures[0].ToBitmap();
		}
	}
}
