using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using LibEveryFileExplorer.Files;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using LibEveryFileExplorer;
using _3DS.NintendoWare.LYT1;

namespace _3DS.UI
{
	public partial class CLYTViewer : Form
	{
		bool init = false;
		CLYT NWLayout;
		CLIM[] Textures;
		BasicShader BShader = new BasicShader();
		ImageList ImageL;

		public CLYTViewer(CLYT Layout)
		{
			this.NWLayout = Layout;
			InitializeComponent();
			Win32Util.SetWindowTheme(treeView1.Handle, "explorer", null);
			Win32Util.SetWindowTheme(treeView2.Handle, "explorer", null);
			Win32Util.SetWindowTheme(treeView3.Handle, "explorer", null);
			Win32Util.SetWindowTheme(treeView4.Handle, "explorer", null);
			Win32Util.SetWindowTheme(treeView5.Handle, "explorer", null);
		}

		private void CLYTViewer_Load(object sender, EventArgs e)
		{
			simpleOpenGlControl1.InitializeContexts();
			simpleOpenGlControl1.Width = (int)NWLayout.Layout.LayoutSize.X;
			simpleOpenGlControl1.Height = (int)NWLayout.Layout.LayoutSize.Y;
			GL.Enable(EnableCap.ColorMaterial);
			GL.Disable(EnableCap.DepthTest);
			//GL.glDepthFunc(GL.GL_ALWAYS);
			GL.Enable(EnableCap.IndexLogicOp); // From Tao.OpenGL.GL => public const Int32 GL_LOGIC_OP = 0x0BF1; => 3057 => GL_INDEX_LOGIC_OP
            GL.Disable(EnableCap.CullFace);
			GL.Enable(EnableCap.Texture2D);

			//GL.Enable(GL.GL_LINE_SMOOTH);
			GL.Enable(EnableCap.Blend);

			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			if (NWLayout.TextureList != null)
			{
				int i = 0;
				Textures = new CLIM[NWLayout.TextureList.NrTextures];
				foreach (String s in NWLayout.TextureList.TextureNames)
				{
					byte[] data = data = ((ViewableFile)Tag).File.FindFileRelative("../timg/" + s);
					if (data == null) data = ((ViewableFile)Tag).File.FindFileRelative(s);
					if (data != null) Textures[i] = new CLIM(data);
					i++;
				}
			}
			if (NWLayout.Materials != null)
			{
				int i = 0;
				foreach (var v in NWLayout.Materials.Materials)
				{
					int j = 0;
					foreach (var t in v.TexMaps)
					{
						if (Textures[t.TexIndex] != null) UploadTex(t, Textures[t.TexIndex], i * 4 + j + 1);
						j++;
					}
					v.SetupShader();
					i++;
				}
			}
			BShader.Compile(false);

			ImageL = new ImageList();
			ImageL.ColorDepth = ColorDepth.Depth32Bit;
			ImageL.ImageSize = new System.Drawing.Size(16, 16);
			ImageL.Images.Add("pan1", Resource.zone16);
			ImageL.Images.Add("pic1", Resource.image16);
			ImageL.Images.Add("wnd1", Resource.slide);
			ImageL.Images.Add("txt1", Resource.edit);
			ImageL.Images.Add("grp1", Resource.zones_stack);
			ImageL.Images.Add(Resource.t_shirt);
			ImageL.Images.Add(Resource.image_sunset);
			ImageL.Images.Add(Resource.edit_language);
			treeView1.ImageList = ImageL;
			treeView2.ImageList = ImageL;
			treeView3.ImageList = ImageL;
			treeView4.ImageList = ImageL;
			treeView5.ImageList = ImageL;

			treeView1.BeginUpdate();
			treeView1.Nodes.Clear();
			treeView1.Nodes.Add(NWLayout.RootPane.GetTreeNodes());
			treeView1.EndUpdate();

			treeView2.BeginUpdate();
			treeView2.Nodes.Clear();
			treeView2.Nodes.Add(NWLayout.RootGroup.GetTreeNodes());
			treeView2.EndUpdate();

			if (NWLayout.Materials != null)
			{
				treeView3.BeginUpdate();
				treeView3.Nodes.Clear();
				foreach (var v in NWLayout.Materials.Materials)
					treeView3.Nodes.Add(new TreeNode(v.Name, 5, 5));
				treeView3.EndUpdate();
			}

			int q = 0;
			if (NWLayout.TextureList != null)
			{
				treeView4.BeginUpdate();
				treeView4.Nodes.Clear();
				foreach (var v in NWLayout.TextureList.TextureNames)
					treeView4.Nodes.Add(new TreeNode(v, 6, 6) { ForeColor = (Textures[q] == null) ? Color.Red : Color.Black });
				treeView4.EndUpdate();
				q++;
			}

			if (NWLayout.FontList != null)
			{
				treeView5.BeginUpdate();
				treeView5.Nodes.Clear();
				foreach (var v in NWLayout.FontList.FontNames)
					treeView5.Nodes.Add(new TreeNode(v, 7, 7));
				treeView5.EndUpdate();
			}


			init = true;
			Render();
		}

		private void UploadTex(mat1.MaterialEntry.TexMap TexMap, CLIM Texture, int Id)
		{
			GL.BindTexture(TextureTarget.Texture2D, Id);
			GL.Color3(1, 1, 1);
			Bitmap b = Texture.ToBitmap();
			//b.RotateFlip(RotateFlipType.RotateNoneFlipY);
			BitmapData d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, b.Width, b.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, d.Scan0);
			b.UnlockBits(d);

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (TexMap.MagFilter == mat1.MaterialEntry.TexMap.FilterMode.Linear) ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (TexMap.MinFilter == mat1.MaterialEntry.TexMap.FilterMode.Linear) ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);

			switch (TexMap.WrapS)
			{
				case mat1.MaterialEntry.TexMap.WrapMode.Clamp:
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
					break;
				case mat1.MaterialEntry.TexMap.WrapMode.Repeat:
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
					break;
				case mat1.MaterialEntry.TexMap.WrapMode.Mirror:
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
					break;
                default:
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
					break;
			}

			switch (TexMap.WrapT)
			{
				case mat1.MaterialEntry.TexMap.WrapMode.Clamp:
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
					break;
				case mat1.MaterialEntry.TexMap.WrapMode.Repeat:
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
					break;
				case mat1.MaterialEntry.TexMap.WrapMode.Mirror:
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);
					break;
				default:
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
					break;
			}
		}

		public void Render()
		{
			if (!init) return;
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Viewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);

			GL.Ortho(-NWLayout.Layout.LayoutSize.X / 2.0f, NWLayout.Layout.LayoutSize.X / 2.0f, -NWLayout.Layout.LayoutSize.Y / 2.0f, NWLayout.Layout.LayoutSize.Y / 2.0f, -1000, 1000);
			//Glu.gluPerspective(90, aspect, 0.02f, 1000.0f);//0.02f, 32.0f);
			//GL.glTranslatef(0, 0, -100);


			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();

			/*if (!picking)*/
			GL.ClearColor(1, 1, 1, 1);//BGColor.R / 255f, BGColor.G / 255f, BGColor.B / 255f, 1f);
			//else GL.glClearColor(0, 0, 0, 1);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			GL.Color4(1, 1, 1, 1);
			GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.Color4(1, 1, 1, 1);
			GL.Disable(EnableCap.CullFace);
			GL.Enable(EnableCap.AlphaTest);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			GL.AlphaFunc(AlphaFunction.Always, 0f);

			GL.LoadIdentity();


			//if (!picking)
			{
				GL.BindTexture(TextureTarget.Texture2D, 0);
				BShader.Enable();
				GL.BindTexture(TextureTarget.Texture2D, 0);
				GL.Color4(204 / 255f, 204 / 255f, 204 / 255f, 1);
				int xbase = 0;
				for (int y = 0; y < simpleOpenGlControl1.Height; y += 8)
				{
					for (int x = xbase; x < simpleOpenGlControl1.Width; x += 16)
					{
						GL.Rect(x - simpleOpenGlControl1.Width / 2f, y - simpleOpenGlControl1.Height / 2f, x - simpleOpenGlControl1.Width / 2f + 8, y - simpleOpenGlControl1.Height / 2f + 8);
					}
					if (xbase == 0) xbase = 8;
					else xbase = 0;
				}
			}
			/*if (picking)
			{
				BasicShaderb.Enable();
				int idx = 0;
				Layout.PAN1.Render(Layout, ref idx, 255, picking);
				pic = new byte[4];
				Bitmap b = IO.Util.ScreenShot(simpleOpenGlControl1);
				GL.glReadPixels(MousePoint.X, (int)simpleOpenGlControl1.Height - MousePoint.Y, 1, 1, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, pic);
				GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
				Render();
				//simpleOpenGlControl1.Refresh();
				//Render();
			}
			else
			{*/
			NWLayout.RootPane.Render(NWLayout, Textures, 255);
			simpleOpenGlControl1.Refresh();
			//}
		}

		private void CLYTViewer_Layout(object sender, LayoutEventArgs e)
		{
			Render();
		}

		private void CLYTViewer_Resize(object sender, EventArgs e)
		{
			Render();
			Render();
		}

		private void CLYTViewer_Activated(object sender, EventArgs e)
		{
			for (int i = 0; i < 8; i++) Render();
		}
	}
}
