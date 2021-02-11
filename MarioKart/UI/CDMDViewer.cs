using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.Files;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using LibEveryFileExplorer._3D;
using LibEveryFileExplorer;
using LibEveryFileExplorer.UI;
using LibEveryFileExplorer.GameData;
using MarioKart.MK7;
using LibEveryFileExplorer.Collections;
using MarioKart.MK7.KMP;

namespace MarioKart.UI
{
	public partial class CDMDViewer : Form, IUseOtherFiles
	{
		List<IGameDataSectionViewer> SectionViewers = new List<IGameDataSectionViewer>();

		CDMD MapData;
		KCL KCL = null;
		public CDMDViewer(CDMD MapData)
		{
			this.MapData = MapData;
			InitializeComponent();
			simpleOpenGlControl1.InitializeContexts();
			simpleOpenGlControl1.MouseWheel += new MouseEventHandler(simpleOpenGlControl1_MouseWheel);
		}

		int scale = 1;
		bool first = true;
		void simpleOpenGlControl1_MouseWheel(object sender, MouseEventArgs e)
		{
			if (!(e.Delta < 0 && scale == 1) && !(e.Delta > 0 && scale == 32))
			{
				scale = (int)(scale * (e.Delta < 0 ? 1f / 2f : 2));
				vScrollBar1.Maximum = scale - 1;
				vScrollBar1.Minimum = -(scale - 1);
				hScrollBar1.Maximum = scale - 1;
				hScrollBar1.Minimum = -(scale - 1);
				if (scale == 1) { vScrollBar1.Enabled = false; hScrollBar1.Enabled = false; }
				else { vScrollBar1.Enabled = true; hScrollBar1.Enabled = true; }
				Render();
			}
		}
		bool init = false;
		private void NKMDViewer_Load(object sender, EventArgs e)
		{
			GL.Enable(EnableCap.ColorMaterial);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Always);
			GL.Enable(EnableCap.IndexLogicOp);
			GL.Disable(EnableCap.CullFace);
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.LineSmooth);
			GL.Enable(EnableCap.Blend);

			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(KCL));
			menuItem1.MenuItems.Clear();
			foreach (var vv in v)
			{
				menuItem1.MenuItems.Add(vv.File.Name);
			}
			if (v.Length != 0)
			{
				menuItem1.MenuItems[0].Checked = true;
				KCL = v[0].FileFormat;
			}

			if (MapData.KartPoint != null) AddTab<KTPT.KTPTEntry>("KTPT", MapData.KartPoint);
			if (MapData.EnemyPoint != null) AddTab<ENPT.ENPTEntry>("ENPT", MapData.EnemyPoint);
			if (MapData.EnemyPointPath != null) AddTab<ENPH.ENPHEntry>("ENPH", MapData.EnemyPointPath);
			if (MapData.ItemPoint != null) AddTab<ITPT.ITPTEntry>("ITPT", MapData.ItemPoint);
			if (MapData.ItemPointPath != null) AddTab<ITPH.ITPHEntry>("ITPH", MapData.ItemPointPath);
			if (MapData.CheckPoint != null) AddTab<CKPT.CKPTEntry>("CKPT", MapData.CheckPoint);
			if (MapData.CheckPointPath != null) AddTab<CKPH.CKPHEntry>("CKPH", MapData.CheckPointPath);
			if (MapData.GlobalObject != null) AddTab<GOBJ.GOBJEntry>("GOBJ", MapData.GlobalObject);
			if (MapData.Area != null) AddTab<AREA.AREAEntry>("AREA", MapData.Area);
			if (MapData.Camera != null) AddTab<CAME.CAMEEntry>("CAME", MapData.Camera);
			if (MapData.JugemPoint != null) AddTab<JGPT.JGPTEntry>("JGPT", MapData.JugemPoint);
			if (MapData.GliderPoint != null) AddTab<GLPT.GLPTEntry>("GLPT", MapData.GliderPoint);
			if (MapData.GliderPointPath != null) AddTab<GLPH.GLPHEntry>("GLPH", MapData.GliderPointPath);

			/*Bitmap b3 = OBJI.OBJ_2D01;
			System.Resources.ResourceSet s = OBJI.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, false, false);
			foreach (Object b in s)
			{
				Bitmap b2 = ((Bitmap)((System.Collections.DictionaryEntry)b).Value);
				BitmapData bd = b2.LockBits(new Rectangle(0, 0, b2.Width, b2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				GL.glTexEnvf(GL.GL_TEXTURE_ENV, GL.GL_TEXTURE_ENV_MODE, GL.GL_MODULATE);
				if ((String)((System.Collections.DictionaryEntry)b).Key != "start")
				{
					GL.glBindTexture(GL.GL_TEXTURE_2D, BitConverter.ToUInt16(BitConverter.GetBytes(ushort.Parse(((String)((System.Collections.DictionaryEntry)b).Key).Split('_')[1], System.Globalization.NumberStyles.HexNumber)).Reverse().ToArray(), 0));
				}
				else
				{
					GL.glBindTexture(GL.GL_TEXTURE_2D, -1);
				}
				GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA8, b2.Width, b2.Height, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, bd.Scan0);
				b2.UnlockBits(bd);
				GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP);
				GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP);
				GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_NEAREST);
				GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
			}*/
			init = true;
			Render();
			Render();
		}

		private void AddTab<T>(String Name, GameDataSection<T> Section) where T : GameDataSectionEntry, new()
		{
			TabPage p = new TabPage(Name);
			var v = new GameDataSectionViewer<T>(Section) { Dock = DockStyle.Fill };
			v.OnSelected += new SelectedEventHandler(GameDataSectionViewer_OnSelected);
			SectionViewers.Add(v);
			p.Controls.Add(v);
			tabControl1.TabPages.Add(p);
		}

		void GameDataSectionViewer_OnSelected(IGameDataSectionViewer Viewer, object Entry)
		{
			propertyGrid1.SelectedObject = Entry;
			propertyGrid1.ExpandAllGridItems();
			foreach (var v in SectionViewers)
			{
				if (v != Viewer) v.RemoveSelection();
			}
		}

		float min = -8192f;
		float max = 8192f;
		byte[] pic;
		float mult = 0;
		private void Render(bool pick = false, Point mousepoint = new Point(), bool kclpick = false)
		{
			if (!init) return;
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Viewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
			float x = (8192f / (float)scale) / simpleOpenGlControl1.Width;
			x *= 2;
			float y = (8192f / (float)scale) / simpleOpenGlControl1.Height;
			y *= 2;
			//GL.glTranslatef(0, 0, 0);
			//GL.glOrtho(-8192, 8192, 8192, -8192, -1000, 1000);
			if (x > y)
			{
				GL.Ortho((-(x * simpleOpenGlControl1.Width) / 2f) + (hScrollBar1.Value * (8192f / (float)scale)), (x * simpleOpenGlControl1.Width) / 2f + (hScrollBar1.Value * (8192f / (float)scale)), (x * simpleOpenGlControl1.Height) / 2f + (vScrollBar1.Value * (8192f / (float)scale)), (-(x * simpleOpenGlControl1.Height) / 2f) + (vScrollBar1.Value * (8192f / (float)scale)), -8192, 8192);
				mult = x;
			}
			else
			{
				GL.Ortho((-(y * simpleOpenGlControl1.Width) / 2f) + (hScrollBar1.Value * (8192f / (float)scale)), (y * simpleOpenGlControl1.Width) / 2f + (hScrollBar1.Value * (8192f / (float)scale)), (y * simpleOpenGlControl1.Height) / 2f + (vScrollBar1.Value * (8192f / (float)scale)), (-(y * simpleOpenGlControl1.Height) / 2f) + (vScrollBar1.Value * (8192f / (float)scale)), -8192, 8192);
				mult = y;
			}

			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			GL.ClearColor(1, 1, 1, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Color4(1, 1, 1, 1);
			GL.Enable(EnableCap.Texture2D);
			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.Color4(1, 1, 1, 1);
			GL.Disable(EnableCap.CullFace);
			GL.Enable(EnableCap.AlphaTest);
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.PointSmooth);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			GL.AlphaFunc(AlphaFunction.Always, 0f);

			if (pick)
			{
				GL.LoadIdentity();
				GL.Disable(EnableCap.PolygonSmooth);
				GL.Disable(EnableCap.PointSmooth);
				if (!kclpick) RenderNKM(true);
				else
				{
					GL.DepthFunc(DepthFunction.Lequal);
					RenderKCL(true);
					GL.DepthFunc(DepthFunction.Always);
				}
				pic = new byte[4];
				GL.ReadPixels(mousepoint.X, (int)simpleOpenGlControl1.Height - mousepoint.Y, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pic);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				Render();
				return;
			}
			else
			{
				GL.LoadIdentity();
				RenderNKM();
			}

			simpleOpenGlControl1.Refresh();
		}
		private void RenderNKM(bool picking = false)
		{
			if (first && KCL != null)
			{
				first = false;
			}
			if (!picking)
			{
				GL.Enable(EnableCap.PolygonSmooth);
			}
			if (!picking && KCL != null)
			{
				GL.DepthFunc(DepthFunction.Lequal);
				RenderKCL();
				GL.DepthFunc(DepthFunction.Always);
			}
			GL.PointSize((picking ? 6f : 5));

			int objidx = 1;
			if (!picking)
			{
				GL.Color4(Color.CornflowerBlue.R / 255f, Color.CornflowerBlue.G / 255f, Color.CornflowerBlue.B / 255f, 0.25f);
			}
            /*GL.Begin(GL.GL_QUADS);
			//if (aREAToolStripMenuItem.Checked)
			{
				foreach (var o in NKMD.Area.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (17 << 18)).R / 255f, Color.FromArgb(objidx | (17 << 18)).G / 255f, Color.FromArgb(objidx | (17 << 18)).B / 255f, 1);
						objidx++;
					}
					Vector3[] cube = o.GetCube();
					//We're interested in points 0, 1, 5 and 3 (ground plane)
					Vector3 Point1 = cube[3];
					Vector3 Point2 = cube[5];
					Vector3 Point3 = cube[1];
					Vector3 Point4 = cube[0];
					GL.Vertex2(Point1.X, Point1.Z);
					GL.Vertex2(Point2.X, Point2.Z);
					GL.Vertex2(Point3.X, Point3.Z);
					GL.Vertex2(Point4.X, Point4.Z);
				}
			}
			GL.End();*/

            GL.Begin(PrimitiveType.Points);
            if (!picking)
			{
				GL.Color3(0, 0, 0.5f);
			}
			objidx = 1;
			//if (pOITToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.Point.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (2 << 18)).R / 255f, Color.FromArgb(objidx | (2 << 18)).G / 255f, Color.FromArgb(objidx | (2 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}*/
			GL.End();
			GL.LineWidth(1.5f);
			int idx = 0;
			//if (pOITToolStripMenuItem.Checked)
			/*{
				if (!picking)
				{
					foreach (var o in NKMD.Path.Entries)
					{
						if (NKMD.Point.NrEntries < o.NrPoit + idx) break;
						GL.Begin(GL.GL_LINE_STRIP);
						for (int i = 0; i < o.NrPoit; i++)
						{
							GL.Vertex2(NKMD.Point[idx + i].Position.X, NKMD.Point[idx + i].Position.Z);
							if (!(i + 1 < o.NrPoit) && o.Loop)
							{
								GL.Vertex2(NKMD.Point[idx].Position.X, NKMD.Point[idx].Position.Z);
							}
						}
						GL.End();
						idx += o.NrPoit;
					}
				}
			}*/
			GL.Begin(PrimitiveType.Points);
			if (!picking)
			{
				GL.Color3(1, 0, 0);
			}
			objidx = 1;
			//if (oBJIToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.GlobalObject.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (5 << 18)).R / 255f, Color.FromArgb(objidx | (5 << 18)).G / 255f, Color.FromArgb(objidx | (5 << 18)).B / 255f, 1);
						objidx++;
					}
					//Bitmap b;
					//if ((b = (Bitmap)OBJI.ResourceManager.GetObject("OBJ_" + BitConverter.ToString(BitConverter.GetBytes(o.ObjectID), 0).Replace("-", ""))) == null)
					//{
						GL.Vertex2(o.Position.X, o.Position.Z);
					/*}
					else
					{
						GL.End();
						if (!picking)
						{
							GL.Color3(1, 1, 1);
							GL.glBindTexture(GL.GL_TEXTURE_2D, o.ObjectID);
						}
						GL.glPushMatrix();
						GL.glTranslatef(o.Position.X, o.Position.Z, 0);

						GL.Rotate(o.Rotation.Y, 0, 0, 1);

						GL.glScalef(mult, mult, 1);

						GL.Begin(GL.GL_QUADS);
						GL.glTexCoord2f(0, 0);
						GL.Vertex2(-b.Width / 2f, -b.Height / 2f);
						GL.glTexCoord2f(1, 0);
						GL.Vertex2(b.Width / 2f, -b.Height / 2f);
						GL.glTexCoord2f(1, 1);
						GL.Vertex2(b.Width / 2f, b.Height / 2f);
						GL.glTexCoord2f(0, 1);
						GL.Vertex2(-b.Width / 2f, b.Height / 2f);
						GL.End();
						GL.glPopMatrix();
						if (!picking)
						{
							GL.Color3(1, 0, 0);
							GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
						}
						GL.Begin(GL.GL_POINTS);
					}*/
				}
			}
			if (!picking)
			{
				GL.Color3(0, 0, 0);
			}
			objidx = 1;
			//if (kTPSToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.KartPoint.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (0 << 18)).R / 255f, Color.FromArgb(objidx | (0 << 18)).G / 255f, Color.FromArgb(objidx | (0 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}
			if (!picking)
			{
				GL.Color3(1, 0, 0.5f);
			}
			objidx = 1;
			//if (kTPCToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.KartPointCannon.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (7 << 18)).R / 255f, Color.FromArgb(objidx | (7 << 18)).G / 255f, Color.FromArgb(objidx | (7 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}*/
			if (!picking)
			{
				GL.Color3(0, 0.9f, 1);
			}
			objidx = 1;
			//if (kTP2ToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.KartPointSecond.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (6 << 18)).R / 255f, Color.FromArgb(objidx | (6 << 18)).G / 255f, Color.FromArgb(objidx | (6 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}*/
			if (!picking)
			{
				GL.Color3(Color.MediumPurple.R / 255f, Color.MediumPurple.G / 255f, Color.MediumPurple.B / 255f);
			}
			objidx = 1;
			//if (kTPMToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.KartPointMission.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (8 << 18)).R / 255f, Color.FromArgb(objidx | (8 << 18)).G / 255f, Color.FromArgb(objidx | (8 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}*/
			if (!picking)
			{
				GL.Color3(1, 0.6f, 0);
			}
			objidx = 1;
			//if (kTPJToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.JugemPoint.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (10 << 18)).R / 255f, Color.FromArgb(objidx | (10 << 18)).G / 255f, Color.FromArgb(objidx | (10 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}
			if (!picking)
			{
				GL.Color3(0, 0.8f, 0);
			}
			objidx = 1;
			//if (ePOIToolStripMenuItem.Checked)
			{
				if (MapData.EnemyPoint != null)
				{
					foreach (var o in MapData.EnemyPoint.Entries)
					{
						if (picking)
						{
							GL.Color4(Color.FromArgb(objidx | (1 << 18)).R / 255f, Color.FromArgb(objidx | (1 << 18)).G / 255f, Color.FromArgb(objidx | (1 << 18)).B / 255f, 1);
							objidx++;
						}
						GL.Vertex2(o.Position.X, o.Position.Z);
					}
				}
				/*else
				{
					foreach (var o in MapData.MiniGameEnemyPoint.Entries)
					{
						if (picking)
						{
							GL.Color4(Color.FromArgb(objidx | (15 << 18)).R / 255f, Color.FromArgb(objidx | (15 << 18)).G / 255f, Color.FromArgb(objidx | (15 << 18)).B / 255f, 1);
							objidx++;
						}
						GL.Vertex2(o.Position.X, o.Position.Z);
					}
				}*/
			}

			if (!picking)
			{
				GL.Color3(1, 0.9f, 0);
			}
			objidx = 1;
			//if (iPOIToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.ItemPoint.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (2 << 18)).R / 255f, Color.FromArgb(objidx | (2 << 18)).G / 255f, Color.FromArgb(objidx | (2 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}

			if (!picking)
			{
				GL.Color3(0.5f, 0, 1);
			}
			objidx = 1;
			//if (iPOIToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.GliderPoint.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (15 << 18)).R / 255f, Color.FromArgb(objidx | (15 << 18)).G / 255f, Color.FromArgb(objidx | (15 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}

			if (!picking)
			{
				GL.Color3(Color.CornflowerBlue.R / 255f, Color.CornflowerBlue.G / 255f, Color.CornflowerBlue.B / 255f);
			}
			objidx = 1;
			//if (aREAToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.Area.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (7 << 18)).R / 255f, Color.FromArgb(objidx | (7 << 18)).G / 255f, Color.FromArgb(objidx | (7 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
				}
			}
			if (!picking)
			{
				GL.Color3(Color.BurlyWood.R / 255f, Color.BurlyWood.G / 255f, Color.BurlyWood.B / 255f);
			}
			objidx = 1;
			//if (cAMEToolStripMenuItem.Checked)
			/*{
				foreach (var o in NKMD.Camera.Entries)
				{
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (18 << 18)).R / 255f, Color.FromArgb(objidx | (18 << 18)).G / 255f, Color.FromArgb(objidx | (18 << 18)).B / 255f, 1);
					}
					GL.Vertex2(o.Position.X, o.Position.Z);
					if (o.CameraType == 3 || o.CameraType == 4)
					{
						if (picking)
						{
							GL.Color4(Color.FromArgb(objidx | (19 << 18)).R / 255f, Color.FromArgb(objidx | (19 << 18)).G / 255f, Color.FromArgb(objidx | (19 << 18)).B / 255f, 1);
						}
						GL.Vertex2(o.Viewpoint1.X, o.Viewpoint1.Z);
						if (picking)
						{
							GL.Color4(Color.FromArgb(objidx | (20 << 18)).R / 255f, Color.FromArgb(objidx | (20 << 18)).G / 255f, Color.FromArgb(objidx | (20 << 18)).B / 255f, 1);
							objidx++;
						}
						GL.Vertex2(o.Viewpoint2.X, o.Viewpoint2.Z);
					}
				}
			}*/



			GL.End();

			//if (cPOIToolStripMenuItem.Checked)
			{
				if (!picking)
				{
                    GL.Begin(PrimitiveType.Lines);
                    foreach (var o in MapData.CheckPoint.Entries)
					{
						GL.Color3(0, 170f / 255f, 0);
						//GL.Color3(0.5f, 0.5f, 0.5f);
						GL.Vertex2(o.Point1.X, o.Point1.Y);
						GL.Color3(170f / 255f, 0, 0);//181f / 255f, 230f / 255f, 29f / 255f);
						//GL.Color3(1, 1, 1);
						GL.Vertex2(o.Point2.X, o.Point2.Y);
					}
					for (int j = 0; j < MapData.CheckPointPath.Entries.Count; j++)
					{
						if (MapData.CheckPoint.Entries.Count < MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length) break;
						for (int i = MapData.CheckPointPath[j].Start; i < MapData.CheckPointPath.Entries[j].Start + MapData.CheckPointPath[j].Length - 1; i++)
						{
							GL.Color3(0, 170f / 255f, 0);
							GL.Vertex2(MapData.CheckPoint[i].Point1.X, MapData.CheckPoint[i].Point1.Y);
							GL.Vertex2(MapData.CheckPoint[i + 1].Point1.X, MapData.CheckPoint[i + 1].Point1.Y);
							GL.Color3(170f / 255f, 0, 0);
							GL.Vertex2(MapData.CheckPoint[i].Point2.X, MapData.CheckPoint[i].Point2.Y);
							GL.Vertex2(MapData.CheckPoint[i + 1].Point2.X, MapData.CheckPoint[i + 1].Point2.Y);
						}

						/*for (int i = 0; i < 3; i++)
						{
							if (MapData.CheckPointPath[j].Next[i] == -1 || MapData.CheckPointPath[j].Next[i] >= MapData.CheckPointPath.Entries.Count) continue;
							GL.Color3(0, 170f / 255f, 0);
							GL.Vertex2(MapData.CheckPoint[MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length - 1].Point1.X, MapData.CheckPoint[MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length - 1].Point1.Y);
							GL.Vertex2(MapData.CheckPoint[MapData.CheckPointPath[MapData.CheckPointPath[j].Next[i]].Start].Point1.X, MapData.CheckPoint[MapData.CheckPointPath[MapData.CheckPointPath[j].Next[i]].Start].Point1.Y);
							GL.Color3(170f / 255f, 0, 0);
							GL.Vertex2(MapData.CheckPoint[MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length - 1].Point2.X, MapData.CheckPoint[MapData.CheckPointPath[j].Start + MapData.CheckPointPath[j].Length - 1].Point2.Y);
							GL.Vertex2(MapData.CheckPoint[MapData.CheckPointPath[MapData.CheckPointPath[j].Next[i]].Start].Point2.X, MapData.CheckPoint[MapData.CheckPointPath[MapData.CheckPointPath[j].Next[i]].Start].Point2.Y);
						}*/
					}
					GL.End();
				}
			}

            GL.Begin(PrimitiveType.Points);
            objidx = 1;
			//if (cPOIToolStripMenuItem.Checked)
			{
				foreach (var o in MapData.CheckPoint.Entries)
				{
					if (!picking)
					{
						GL.Color3(0, 170f / 255f, 0);
					}
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (3 << 18)).R / 255f, Color.FromArgb(objidx | (3 << 18)).G / 255f, Color.FromArgb(objidx | (3 << 18)).B / 255f, 1);
					}
					GL.Vertex2(o.Point1.X, o.Point1.Y);
					if (!picking)
					{
						GL.Color3(170f / 255f, 0, 0);//181f / 255f, 230f / 255f, 29f / 255f);
					}
					if (picking)
					{
						GL.Color4(Color.FromArgb(objidx | (4 << 18)).R / 255f, Color.FromArgb(objidx | (4 << 18)).G / 255f, Color.FromArgb(objidx | (4 << 18)).B / 255f, 1);
						objidx++;
					}
					GL.Vertex2(o.Point2.X, o.Point2.Y);
				}
			}
			GL.End();
			//GL.Enable(GL.GL_LINE_SMOOTH);
		}

		public void RenderKCL(bool picking = false)
		{
			int i = 0;
			foreach (var p in KCL.Planes)
			{
				//Vector3 PositionA, PositionB, PositionC, Normal;
				Triangle t = KCL.GetTriangle(p);

				Color c = Color.Gray;//MKDS.KCL.GetColor(p.CollisionType);
				if (picking && c.A != 0) c = Color.FromArgb(i + 1 | 0xFF << 24);
				GL.Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
				GL.Begin(PrimitiveType.Triangles);
				//GL.Normal3(t.Normal.X, t.Normal.Y, t.Normal.Z);
				GL.Vertex3(t.PointA.X, t.PointA.Z, t.PointA.Y);
				GL.Vertex3(t.PointB.X, t.PointB.Z, t.PointB.Y);
				GL.Vertex3(t.PointC.X, t.PointC.Z, t.PointC.Y);
				GL.End();
				i++;
			}
		}

		private void simpleOpenGlControl1_Resize(object sender, EventArgs e)
		{
			Render();
			Render();
		}

		private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPageIndex == 0)
			{
				Render();
				simpleOpenGlControl1.Focus();
				simpleOpenGlControl1.Select();
			}
		}

		private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
		{
			Render();
		}

		public void FileOpened(ViewableFile File)
		{
			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(KCL));
			menuItem1.MenuItems.Clear();
			bool curavab = false;
			foreach (var vv in v)
			{
				var m = menuItem1.MenuItems.Add(vv.File.Name);
				if (vv.FileFormat == KCL)
				{
					curavab = true;
					m.Checked = true;
				}
			}
			if (!curavab && v.Length != 0)
			{
				menuItem1.MenuItems[0].Checked = true;
				KCL = v[0].FileFormat;
			}
			Render();
			Render();
		}

		public void FileClosed(ViewableFile File)
		{
			if (File.FileFormat is KCL && File.FileFormat == KCL) KCL = null;
			ViewableFile[] v = EveryFileExplorerUtil.GetOpenFilesOfType(typeof(KCL));
			menuItem1.MenuItems.Clear();
			foreach (var vv in v)
			{
				menuItem1.MenuItems.Add(vv.File.Name);
			}
			if (v.Length != 0)
			{
				menuItem1.MenuItems[0].Checked = true;
				KCL = v[0].FileFormat;
			}
			Render();
			Render();
		}

		private void NKMDViewer_Shown(object sender, EventArgs e)
		{
			Render();
			Render();
		}

		private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
		{
			foreach (var v in SectionViewers) v.UpdateListViewEntry(propertyGrid1.SelectedObject);
			Render();
			Render();
		}
	}
}
