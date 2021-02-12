using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using NDS.NitroSystem.G3D;
using LibEveryFileExplorer._3D;

namespace NDS.UI
{
	public partial class MDL0Viewer : UserControl
	{
		MDL0.Model Model;
		public MDL0Viewer(MDL0.Model Model)
		{
			this.Model = Model;
			InitializeComponent();
			simpleOpenGlControl1.MouseWheel += new MouseEventHandler(simpleOpenGlControl1_MouseWheel);
		}
		void simpleOpenGlControl1_MouseWheel(object sender, MouseEventArgs e)
		{
			if ((ModifierKeys & Keys.Shift) == Keys.Shift) { dist += ((float)e.Delta / 10) * Model.info.posScale / 8f; }
			else { dist += ((float)e.Delta / 100) * Model.info.posScale / 8f; }
			Render();
		}

		bool init = false;

		float X = 0.0f;
		float Y = 0.0f;
		float ang = 0.0f;
		float dist = 0.0f;
		float elev = 0.0f;
		private void MDL0Viewer_Load(object sender, EventArgs e)
		{
			simpleOpenGlControl1.InitializeContexts();
            //GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.RescaleNormal);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Normalize);
            GL.Disable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.Enable(EnableCap.Texture2D);
            GL.ClearDepth(1);
            GL.Enable(EnableCap.AlphaTest);
            GL.AlphaFunc(AlphaFunction.Greater, 0f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, Gl.GL_BLEND);

            GL.ShadeModel(ShadingModel.Smooth);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.ClearColor(51f / 255f, 51f / 255f, 51f / 255f, 0f);

			init = true;
			Render();
        }

        public void Render()
		{
			if (!init) return;
			float aspect = (float)simpleOpenGlControl1.Width / (float)simpleOpenGlControl1.Height;
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Viewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);

            Glu.gluPerspective(30, aspect, 0.1f, 20480000f);//0.02f, 32.0f);
            //OpenTK.Matrix4 prespectiveMatrix = OpenTK.Matrix4.CreatePerspectiveFieldOfView(30.0f, aspect, 0.1f, 20480000f);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.Color3(1.0f, 1.0f, 1.0f);
            //Gl.glEnable(Gl.GL_DEPTH_TEST);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            /*Gl.Rotate(elev, 1, 0, 0);
			Gl.glFogfv(Gl.GL_FOG_COLOR, new float[] { 0, 0, 0, 1 });
			Gl.glFogf(Gl.GL_FOG_DENSITY, 1);
			Gl.Rotate(-elev, 1, 0, 0);*/

            GL.Translate(X, Y, -dist);
			GL.Rotate(elev, 1, 0, 0);
			GL.Rotate(ang, 0, 1, 0);

			GL.PushMatrix();
			RenderModel();
			GL.PopMatrix();
			simpleOpenGlControl1.Refresh();
		}

		void RenderModel()
		{
			Model.ProcessSbc();
		}

		bool wire = false;
		bool licht = true;
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Right:
					ang += 1;
					Render();
					return true;
				case Keys.Left:
					ang -= 1;
					Render();
					return true;
				case Keys.Up:
					elev += 1;
					Render();
					return true;
				case Keys.Down:
					elev -= 1;
					Render();
					return true;
				case Keys.Z:
					X -= 0.05f * Model.info.posScale / 2;//(((Control.ModifierKeys & Keys.Shift) != 0) ? 100f : 1f);
					Render();
					return true;
				case Keys.X:
					X += 0.05f * Model.info.posScale / 2;//* (((Control.ModifierKeys & Keys.Shift) != 0) ? 100f : 1f);
					Render();
					return true;
				case Keys.A:
					Y -= 0.05f * Model.info.posScale / 2;//* (((Control.ModifierKeys & Keys.Shift) != 0) ? 100f : 1f);
					Render();
					return true;
				case Keys.S:
					Y += 0.05f * Model.info.posScale / 2;//* (((Control.ModifierKeys & Keys.Shift) != 0) ? 100f : 1f);
					Render();
					return true;
				case Keys.W:
					wire = !wire;
					if (wire) { GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); Render(); }
					else { GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); Render(); }
					return true;
				case Keys.Escape:
					X = 0.0f;
					Y = 0.0f;
					ang = 0.0f;
					dist = 0.0f;
					elev = 0.0f;
					Render();
					return true;
				case Keys.T:
					elev = 90;
					ang = 0;
					Render();
					return true;
				case Keys.L:
					licht = !licht;
					Render();
					return true;
				case Keys.OemMinus:
				case Keys.OemMinus | Keys.Shift:
					if ((ModifierKeys & Keys.Shift) == Keys.Shift) dist += 12f * Model.info.posScale / 8f;
					else dist += 1.2f * Model.info.posScale / 8f;
					Render();
					return true;
				case Keys.Oemplus:
				case Keys.Oemplus | Keys.Shift:
					if ((ModifierKeys & Keys.Shift) == Keys.Shift) dist -= 12f * Model.info.posScale / 8f;
					else dist -= 1.2f * Model.info.posScale / 8f;
					Render();
					return true;
				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}
	}
}
