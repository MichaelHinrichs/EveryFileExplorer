using System;
using System.Windows.Forms;

namespace _3DS.UI
{
    public partial class CGFXGenDialog : Form
	{
		public String ModelName;
		public CGFXGenDialog()
		{
			InitializeComponent();
			ModelName = textBox1.Text;
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			ModelName = textBox1.Text;
			if (textBox1.Text.Length == 0) button1.Enabled = false;
		}
	}
}
