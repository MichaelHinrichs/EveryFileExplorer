using System;
using System.Windows.Forms;
using GCNWii.JSystem;

namespace GCNWii.UI
{
    public partial class BTIViewer : Form
	{
		BTI Image;
		public BTIViewer(BTI Image)
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
