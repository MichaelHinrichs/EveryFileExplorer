using System;
using System.Windows.Forms;

namespace LibEveryFileExplorer._3D
{
    public partial class GLControl : OpenTK.GLControl
    {
        public GLControl()
        {
            InitializeComponent();
        }

        protected override void OnResize(EventArgs e)
        {
            if (!IsHandleCreated)
                MakeCurrent();

            base.OnResize(e); // Call user resize event
            SwapBuffers();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!IsHandleCreated)
                MakeCurrent();

            base.OnPaint(e); // Call user paint event
            SwapBuffers();
        }

        private void GLControl_Load(object sender, EventArgs e)
        {

        }
    }
}
