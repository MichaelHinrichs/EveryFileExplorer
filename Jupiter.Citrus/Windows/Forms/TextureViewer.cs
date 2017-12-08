using System;
using System.Windows.Forms;
using Jupiter.Citrus.Drawing;

namespace Jupiter.Citrus.Windows.Forms
{
    /// <summary>
    /// Opens a form which allow a user to show an instance of <see cref="Texture"/>.
    /// </summary>
    public partial class TextureViewer : Form
    {
        Texture texture;

        /// <summary>
        /// Initializes a new instance of <see cref="TextureViewer"/>.
        /// </summary>
        /// <param name="texture">Texture to show.</param>
        public TextureViewer(Texture texture)
        {
            this.texture = texture;
            InitializeComponent();
        }

        /// <summary>
        /// Loads content in components.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event arguments.</param>
        private void LoadForm(object sender, EventArgs e)
        {
            pictureBox.Image = texture.GetBitmap();
        }
    }
}
