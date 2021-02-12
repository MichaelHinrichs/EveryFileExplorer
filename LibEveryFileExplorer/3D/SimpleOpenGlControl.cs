#region License
/*
Copyright ©2019 Arves100.
Updated for using a TaoFramework application with OpenTK (EveryFileExplorer)
License: MIT
*/
/*
MIT License
Copyright ©2003-2006 Tao Framework Team
http://www.taoframework.com
All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace OpenTK
{
    #region Class Documentation
    /// <summary>
    ///     Provides a simple OpenGL control allowing quick development of Windows Forms-based
    ///     OpenGL applications.
    /// </summary>
    #endregion Class Documentation
    public class SimpleOpenGlControl : OpenTK.GLControl
    {
        // --- Fields ---
        #region Private Fields
        private IContainer components;                                      // Required for designer support
        private bool autoCheckErrors = false;                               // Should we provide glGetError()?
        private bool autoFinish = false;                                    // Should we provide a glFinish()?
        private bool autoMakeCurrent = true;                                // Should we automatically make the rendering context current?
        private bool autoSwapBuffers = true;                                // Should we automatically swap buffers?
        private byte accumBits = 0;                                         // Accumulation buffer bits
        private byte colorBits = 32;                                        // Color buffer bits
        private byte depthBits = 16;                                        // Depth buffer bits
        private byte stencilBits = 0;                                       // Stencil buffer bits
        private ErrorCode errorCode = ErrorCode.NoError;                    // The GL error code
        #endregion Private Fields

        #region Public Properties
        #region AccumBits
        /// <summary>
        ///     Gets and sets the OpenGL control's accumulation buffer depth.
        /// </summary>
        [Category("OpenGL Properties"), Description("Accumulation buffer depth in bits.")]
        public byte AccumBits
        {
            get
            {
                return accumBits;
            }
            set
            {
                accumBits = value;
            }
        }
        #endregion AccumBits

        #region ColorBits
        /// <summary>
        ///     Gets and sets the OpenGL control's color buffer depth.
        /// </summary>
        [Category("OpenGL Properties"), Description("Color buffer depth in bits.")]
        public byte ColorBits
        {
            get
            {
                return colorBits;
            }
            set
            {
                colorBits = value;
            }
        }
        #endregion ColorBits

        #region DepthBits
        /// <summary>
        ///     Gets and sets the OpenGL control's depth buffer (Z-buffer) depth.
        /// </summary>
        [Category("OpenGL Properties"), Description("Depth buffer (Z-buffer) depth in bits.")]
        public byte DepthBits
        {
            get
            {
                return depthBits;
            }
            set
            {
                depthBits = value;
            }
        }
        #endregion DepthBits

        #region StencilBits
        /// <summary>
        ///     Gets and sets the OpenGL control's stencil buffer depth.
        /// </summary>
        [Category("OpenGL Properties"), Description("Stencil buffer depth in bits.")]
        public byte StencilBits
        {
            get
            {
                return stencilBits;
            }
            set
            {
                stencilBits = value;
            }
        }
        #endregion StencilBits

        #region AutoCheckErrors
        /// <summary>
        ///     Gets and sets the OpenGL control's automatic sending of a glGetError command
        ///     after drawing.
        /// </summary>
        [Category("OpenGL Properties"), Description("Automatically send a glGetError command after drawing?")]
        public bool AutoCheckErrors
        {
            get
            {
                return autoCheckErrors;
            }
            set
            {
                autoCheckErrors = value;
            }
        }
        #endregion AutoCheckErrors

        #region AutoFinish
        /// <summary>
        ///     Gets and sets the OpenGL control's automatic sending of a glFinish command
        ///     after drawing.
        /// </summary>
        [Category("OpenGL Properties"), Description("Automatically send a glFinish command after drawing?")]
        public bool AutoFinish
        {
            get
            {
                return autoFinish;
            }
            set
            {
                autoFinish = value;
            }
        }
        #endregion AutoFinish

        #region AutoMakeCurrent
        /// <summary>
        ///     Gets and sets the OpenGL control's automatic forcing of the rendering context to
        ///     be current before drawing.
        /// </summary>
        [Category("OpenGL Properties"), Description("Automatically make the rendering context current before drawing?")]
        public bool AutoMakeCurrent
        {
            get
            {
                return autoMakeCurrent;
            }
            set
            {
                autoMakeCurrent = value;
            }
        }
        #endregion AutoMakeCurrent

        #region AutoSwapBuffers
        /// <summary>
        ///     Gets and sets the OpenGL control's automatic sending of a SwapBuffers command
        ///     after drawing.
        /// </summary>
        [Category("OpenGL Properties"), Description("Automatically send a SwapBuffers command after drawing?")]
        public bool AutoSwapBuffers
        {
            get
            {
                return autoSwapBuffers;
            }
            set
            {
                autoSwapBuffers = value;
            }
        }
        #endregion AutoSwapBuffers
        #endregion Public Properties

        #region Protected Property Overloads
        #region CreateParams CreateParams
        /// <summary>
        ///     Overrides the control's class style parameters.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                int CS_VREDRAW = 0x1;
                int CS_HREDRAW = 0x2;
                int CS_OWNDC = 0x20;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle = cp.ClassStyle | CS_VREDRAW | CS_HREDRAW | CS_OWNDC;
                return cp;
            }
        }
        #endregion CreateParams CreateParams
        #endregion Protected Property Overloads

        // --- Constructors & Destructors ---
        #region SimpleOpenGlControl()
        /// <summary>
        ///     Constructor.  Creates contexts and sets properties.
        /// </summary>
        public SimpleOpenGlControl()
        {
            InitializeStyles();
            InitializeComponent();
            InitializeBackground();
        }
        #endregion SimpleOpenGlControl()

        #region Dispose(bool disposing)
        /// <summary>
        ///     Disposes the control.
        /// </summary>
        /// <param name="disposing">Was the disposed manually called?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            DestroyContexts();
            base.Dispose(disposing);
        }
        #endregion Dispose(bool disposing)

        // --- Private Methods ---
        #region InitializeBackground()
        /// <summary>
        ///     Loads the bitmap from the assembly's manifest resource.
        /// </summary>
        private void InitializeBackground()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream imageStream = assembly.GetManifestResourceStream("TaoButton.jpg"))
                {
                    BackgroundImage = Image.FromStream(imageStream);
                }
            }
            catch (Exception e)
            {
                e.ToString();
                BackgroundImage = null;
            }
        }
        #endregion InitializeBackground()

        #region InitializeComponent()
        /// <summary>
        ///     Required for designer support.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            // 
            // SimpleOpenGlControl
            // 
            BackColor = Color.Black;
            Size = new Size(50, 50);
        }
        #endregion InitializeComponent()

        #region InitializeStyles()
        /// <summary>
        ///     Initializes the control's styles.
        /// </summary>
        private void InitializeStyles()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, false);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
        }
        #endregion InitializeStyles()

        // --- Public Methods ---
        #region DestroyContexts()
        /// <summary>
        /// 
        /// </summary>
        public void DestroyContexts()
        {
            Context?.Dispose();
        }
        #endregion DestroyContexts()

        public void InitializeContexts()
        {
            MakeCurrent();
        }

        #region Draw()
        /// <summary>
        ///     Sends an see cref="UserControl.Invalidate"  command to this control, thus
        ///     forcing a redraw to occur.
        /// </summary>
        public void Draw()
        {
            Invalidate();
        }
        #endregion Draw()

        // --- Events ---
        #region OnPaint(PaintEventArgs e)
        /// <summary>
        ///     Paints the control.
        /// </summary>
        /// <param name="e">The paint event arguments.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                e.Graphics.Clear(BackColor);
                if (BackgroundImage != null)
                    e.Graphics.DrawImage(BackgroundImage, ClientRectangle, 0, 0, BackgroundImage.Width, BackgroundImage.Height, GraphicsUnit.Pixel);
                e.Graphics.Flush();
                return;
            }

            if (autoMakeCurrent)
            {
                MakeCurrent();
            }

            base.OnPaint(e);

            if (autoFinish)
            {
                GL.Finish();
            }

            if (autoCheckErrors)
            {
                errorCode = GL.GetError();
                if (errorCode != ErrorCode.NoError)
                {
                    switch (errorCode)
                    {
                        case ErrorCode.InvalidEnum:
                            MessageBox.Show("GL_INVALID_ENUM - An unacceptable value has been specified for an enumerated argument.  The offending function has been ignored.", "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        case ErrorCode.InvalidValue:
                            MessageBox.Show("GL_INVALID_VALUE - A numeric argument is out of range.  The offending function has been ignored.", "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        case ErrorCode.InvalidOperation:
                            MessageBox.Show("GL_INVALID_OPERATION - The specified operation is not allowed in the current state.  The offending function has been ignored.", "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        case ErrorCode.StackOverflow:
                            MessageBox.Show("GL_STACK_OVERFLOW - This function would cause a stack overflow.  The offending function has been ignored.", "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        case ErrorCode.StackUnderflow:
                            MessageBox.Show("GL_STACK_UNDERFLOW - This function would cause a stack underflow.  The offending function has been ignored.", "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        case ErrorCode.OutOfMemory:
                            MessageBox.Show("GL_OUT_OF_MEMORY - There is not enough memory left to execute the function.  The state of OpenGL has been left undefined.", "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                        default:
                            MessageBox.Show("Unknown GL error.  This should never happen.", "OpenGL Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                    }
                }
            }

            if (autoSwapBuffers)
            {
                SwapBuffers();
            }
        }
        #endregion OnPaint(PaintEventArgs e)

        #region OnPaintBackground(PaintEventArgs e)
        /// <summary>
        ///     Paints the background.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
        #endregion OnPaintBackground(PaintEventArgs e)
    }
}
