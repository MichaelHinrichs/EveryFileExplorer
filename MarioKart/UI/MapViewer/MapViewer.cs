using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibEveryFileExplorer.GameData;
using LibEveryFileExplorer.Collections;
using OpenTK.Graphics.OpenGL;
using System.Reflection;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Math;

namespace MarioKart.UI.MapViewer
{
	public partial class MapViewer : UserControl
	{
		public List<RenderGroup> Groups { get; private set; }

		public object[] SelectedEntries { get; set; }

		public delegate void Init3DEventHandler();
		public event Init3DEventHandler Init3D;

		public delegate void RenderCollisionEventHandler(bool Picking);
		public event RenderCollisionEventHandler RenderCollision;

		public delegate void GroupMemberSelectedEventHandler(Object Entry);
		public event GroupMemberSelectedEventHandler GroupMemberSelected;

		public float MaximumViewport { get; set; }
		public float MinimumViewport { get; set; }

		private float _viewport;
		public float Viewport
		{
			get { return _viewport; }
			set
			{
				if (value > MaximumViewport) _viewport = MaximumViewport;
				else if (value < MinimumViewport) _viewport = MinimumViewport;
				else _viewport = value;
			}
		}

		public Vector2 ViewportOffset { get; set; }

		public MapViewer()
		{
			Groups = new List<RenderGroup>();
			MaximumViewport = 8192;
			MinimumViewport = 256;
			Viewport = MaximumViewport;
			ViewportOffset = new Vector2(0, 0);
			InitializeComponent();
			MouseWheel += new MouseEventHandler(MapViewer_MouseWheel);
		}

		void MapViewer_MouseWheel(object sender, MouseEventArgs e)
		{
			float Diff = (MaximumViewport - MinimumViewport) / 15f;
			if (e.Delta >= 0) Viewport -= Diff;
			else Viewport += Diff;

			RectangleF disp = GetDisplayRect(false);
			if (disp.Width < MaximumViewport * 2)
			{
				hScrollBar1.Maximum = (int)(MaximumViewport - disp.Width / 2f);
				hScrollBar1.Minimum = -(int)(MaximumViewport - disp.Width / 2f);
				hScrollBar1.Enabled = true;
			}
			else
			{
				hScrollBar1.Enabled = false;
				hScrollBar1.Value = hScrollBar1.Maximum = hScrollBar1.Minimum = 0;
				ViewportOffset = new Vector2(0, ViewportOffset.Y);
			}
			if (disp.Height < MaximumViewport * 2)
			{
				vScrollBar1.Maximum = (int)(MaximumViewport - disp.Height / 2f);
				vScrollBar1.Minimum = -(int)(MaximumViewport - disp.Height / 2f);
				vScrollBar1.Enabled = true;
			}
			else
			{
				vScrollBar1.Enabled = false;
				vScrollBar1.Value = vScrollBar1.Maximum = vScrollBar1.Minimum = 0;
				ViewportOffset = new Vector2(ViewportOffset.X, 0);
			}
			Render();
			Render();
		}

		bool init = false;
		private void MapViewer_Load(object sender, EventArgs e)
		{
			simpleOpenGlControl1.InitializeContexts();
			GL.Enable(EnableCap.ColorMaterial);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Always);
			GL.Enable(EnableCap.IndexLogicOp);
			GL.Disable(EnableCap.CullFace);
			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.LineSmooth);
			GL.Enable(EnableCap.Blend);

			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Init3D?.Invoke();

            init = true;
			Render();
			Render();
		}

		private RectangleF GetDisplayRect(bool AddViewportOffset)
		{
			float x = Viewport / simpleOpenGlControl1.Width;
			x *= 2;
			float y = Viewport / simpleOpenGlControl1.Height;
			y *= 2;
			float m = (x > y) ? x : y;
			if (AddViewportOffset)
				return new RectangleF(-(m * simpleOpenGlControl1.Width) / 2f + ViewportOffset.X, -(m * simpleOpenGlControl1.Height) / 2f + ViewportOffset.Y, (m * simpleOpenGlControl1.Width), (m * simpleOpenGlControl1.Height));
			else
				return new RectangleF(-(m * simpleOpenGlControl1.Width) / 2f, -(m * simpleOpenGlControl1.Height) / 2f, (m * simpleOpenGlControl1.Width), (m * simpleOpenGlControl1.Height));
		}

		private void RenderStart()
		{
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Viewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);

			RectangleF r = GetDisplayRect(true);
			GL.Ortho(
				r.Left, r.Right,
				r.Bottom, r.Top,
				-8192, 8192);

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
			GL.LoadIdentity();
		}

		public void Render()
		{
			if (!init) return;
			RenderStart();

			GL.Enable(EnableCap.PolygonSmooth);

			if (RenderCollision != null)
			{
				GL.DepthFunc(DepthFunction.Lequal);
				RenderCollision(false);
				GL.DepthFunc(DepthFunction.Always);
			}
			RenderGroups();

			if(drawSelRect)
			{
				GL.Disable(EnableCap.PolygonSmooth);
				GL.Disable(EnableCap.PointSmooth);
				GL.Disable(EnableCap.LineSmooth);

                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				GL.Color4(0, 0.6f, 0.8f, 0.5f);
				GL.Rect(SelRect.Left, SelRect.Top, SelRect.Right, SelRect.Bottom);

				GL.Enable(EnableCap.PolygonOffsetFill);
				GL.PolygonOffset(0.5f, 1);
				
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				GL.LineWidth(1.0f);

				GL.Color4(0, 0.6f, 0.8f, 1f);

				GL.Rect(SelRect.Left + 0.5f, SelRect.Top + 0.5f, SelRect.Right - 0.3f, SelRect.Bottom - 0.3f);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

			simpleOpenGlControl1.Refresh();
		}

		private class PickingResult
		{
			public int Index;
			public int GroupId;
		}

		private PickingResult GroupPick(Point Position)
		{
			if (!init) return null;
			RenderStart();

			GL.Disable(EnableCap.PolygonSmooth);
			GL.Disable(EnableCap.PointSmooth);

			int NrBits = MathUtil.GetNearest2Power(Groups.Count);
			RenderGroups(true, NrBits);
			byte[] pic = new byte[4];
			GL.ReadPixels(Position.X, (int)simpleOpenGlControl1.Height - Position.Y, 1, 1, PixelFormat.Bgra, PixelType.UnsignedByte, pic);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Render();
			uint v = IOUtil.ReadU32LE(pic, 0);
			if ((int)v == -1) return null;
			v &= 0xFFFFFF;
			return new PickingResult() { GroupId = (int)(v >> (24 - NrBits)), Index = (int)(v & (~(0xFFFFFFFF << (24 - NrBits)))) };
		}

		private int CollisionPick(Point Position)
		{
			if (!init || RenderCollision == null) return -1;
			RenderStart();
			GL.Disable(EnableCap.PolygonSmooth);
            GL.Disable(EnableCap.PointSmooth);
            GL.DepthFunc(DepthFunction.Lequal);
			RenderCollision(true);
			GL.DepthFunc(DepthFunction.Always);
			byte[] pic = new byte[4];
			GL.ReadPixels(Position.X, (int)simpleOpenGlControl1.Height - Position.Y, 1, 1, PixelFormat.Bgra, PixelType.UnsignedByte, pic);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			Render();
			return (int)(IOUtil.ReadU32LE(pic, 0) & 0xFFFFFF);
		}

		private void RenderGroups()
		{
			RenderGroups(false, 0);
		}

		private void RenderGroups(bool Picking, int NrPickingBits)
		{
			int idx = -1;
			foreach (var s in Groups)
			{
				idx++;
				if (Picking && !s.Interactable) continue;
				s.Render(SelectedEntries, Picking, (idx << (24 - NrPickingBits)) & 0xFFFFFF);
			}
		}

		private void simpleOpenGlControl1_Resize(object sender, EventArgs e)
		{
			Render();
			Render();
		}

		private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
		{
			ViewportOffset = new Vector2(hScrollBar1.Value, vScrollBar1.Value);
			Render();
			Render();
		}

		PickingResult Selection = null;

		private void simpleOpenGlControl1_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
			Selection = GroupPick(e.Location);
			/*if (Selection == null)
			{
				if (GroupMemberSelected != null) GroupMemberSelected(null);
				return;
			}
			if (GroupMemberSelected != null) GroupMemberSelected(Groups[Selection.GroupId].GetEntry(Selection.Index - 1));*/
		}

		private void simpleOpenGlControl1_MouseUp(object sender, MouseEventArgs e)
		{
			startmove = false;
			if (drawSelRect)
			{
				drawSelRect = false;

			}
			if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
			Selection = GroupPick(e.Location);
			if (Selection == null)
			{
				if (GroupMemberSelected != null) GroupMemberSelected(null);
				return;
			}
			if (GroupMemberSelected != null) GroupMemberSelected(Groups[Selection.GroupId].GetEntry(Selection.Index - 1));
			Selection = null;
			Render();
			Render();
		}

		bool startmove = false;
		Vector2 MousePointOffs;

		bool drawSelRect = false;
		RectangleF SelRect;
		private void simpleOpenGlControl1_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Left && Selection != null)
			{
				Vector3 Pos = MouseToMapPosXZ(e.Location);
				if (!startmove)
				{
					startmove = true;
					Vector3 OrigPos = Groups[Selection.GroupId].GetPosition(Selection.Index - 1);
					MousePointOffs = new Vector2(OrigPos.X - Pos.X, OrigPos.Z - Pos.Z);
					return;
				}
				Pos.X += MousePointOffs.X;
				Pos.Z += MousePointOffs.Y;
				Groups[Selection.GroupId].SetPosition(Selection.Index - 1, Pos);
				Render();
				Render();
			}
			else if (e.Button == System.Windows.Forms.MouseButtons.Left && Selection == null)
			{
				//Currently no multi-selection
				/*Vector3 Pos = MouseToMapPosXZ(e.Location);
				if (!startmove)
				{
					startmove = true;
					drawSelRect = true;
					SelRect = new RectangleF();
					SelRect.Location = new PointF(Pos.X, Pos.Z);
					return;
				}
				SelRect.Size = new SizeF(Pos.X, Pos.Z) - new SizeF(SelRect.Location);
				Render();
				Render();*/
			}
		}

		private Vector3 MouseToMapPosXZ(Point MousePoint)
		{
			RectangleF r = GetDisplayRect(true);
			float dX = r.Width / simpleOpenGlControl1.Width;
			float dY = r.Height / simpleOpenGlControl1.Height;
			Vector3 Pos = new Vector3();
			Pos.X = (float)MousePoint.X * dX + r.Left;
			Pos.Z = (float)MousePoint.Y * dY + r.Top;
			return Pos;
		}
	}
}
