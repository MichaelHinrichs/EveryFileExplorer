using MarioKart.MKDS.NKM;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace MarioKart.UI.MapViewer
{
	public class MKDSCheckPointLineRenderGroup : RenderGroup
	{
		CPOI CheckPoints;
		CPAT CheckPointPaths;

		public MKDSCheckPointLineRenderGroup(CPOI CheckPoints, CPAT CheckPointPaths, Color Point1Color, Color Point2Color)
		{
			this.CheckPoints = CheckPoints;
			this.CheckPointPaths = CheckPointPaths;
			this.Point1Color = Point1Color;
			this.Point2Color = Point2Color;
		}

		public Color Point1Color { get; private set; }
		public Color Point2Color { get; private set; }

		public override bool Interactable { get { return false; } }

		public override void Render(object[] Selection, bool Picking, int PickingId)
		{
			if (Picking) return;
			GL.LineWidth(1.5f);
			GL.Begin(PrimitiveType.Lines);
			foreach (var o in CheckPoints.Entries)
			{
				GL.Color3(Point1Color.R / 255f, Point1Color.G / 255f, Point1Color.B / 255f);
				GL.Vertex2(o.Point1.X, o.Point1.Y);
				GL.Color3(Point2Color.R / 255f, Point2Color.G / 255f, Point2Color.B / 255f);
				GL.Vertex2(o.Point2.X, o.Point2.Y);
			}
			for (int j = 0; j < CheckPointPaths.Entries.Count; j++)
			{
				if (CheckPoints.Entries.Count < CheckPointPaths[j].StartIndex + CheckPointPaths[j].Length) break;
				for (int i = CheckPointPaths[j].StartIndex; i < CheckPointPaths.Entries[j].StartIndex + CheckPointPaths[j].Length - 1; i++)
				{
					GL.Color3(Point1Color.R / 255f, Point1Color.G / 255f, Point1Color.B / 255f);
					GL.Vertex2(CheckPoints[i].Point1.X, CheckPoints[i].Point1.Y);
					GL.Vertex2(CheckPoints[i + 1].Point1.X, CheckPoints[i + 1].Point1.Y);
					GL.Color3(Point2Color.R / 255f, Point2Color.G / 255f, Point2Color.B / 255f);
					GL.Vertex2(CheckPoints[i].Point2.X, CheckPoints[i].Point2.Y);
					GL.Vertex2(CheckPoints[i + 1].Point2.X, CheckPoints[i + 1].Point2.Y);
				}

				for (int i = 0; i < 3; i++)
				{
					if (CheckPointPaths[j].GoesTo[i] == -1 || CheckPointPaths[j].GoesTo[i] >= CheckPointPaths.Entries.Count) continue;
					GL.Color3(Point1Color.R / 255f, Point1Color.G / 255f, Point1Color.B / 255f);
					GL.Vertex2(CheckPoints[CheckPointPaths[j].StartIndex + CheckPointPaths[j].Length - 1].Point1.X, CheckPoints[CheckPointPaths[j].StartIndex + CheckPointPaths[j].Length - 1].Point1.Y);
					GL.Vertex2(CheckPoints[CheckPointPaths[CheckPointPaths[j].GoesTo[i]].StartIndex].Point1.X, CheckPoints[CheckPointPaths[CheckPointPaths[j].GoesTo[i]].StartIndex].Point1.Y);
					GL.Color3(Point2Color.R / 255f, Point2Color.G / 255f, Point2Color.B / 255f);
					GL.Vertex2(CheckPoints[CheckPointPaths[j].StartIndex + CheckPointPaths[j].Length - 1].Point2.X, CheckPoints[CheckPointPaths[j].StartIndex + CheckPointPaths[j].Length - 1].Point2.Y);
					GL.Vertex2(CheckPoints[CheckPointPaths[CheckPointPaths[j].GoesTo[i]].StartIndex].Point2.X, CheckPoints[CheckPointPaths[CheckPointPaths[j].GoesTo[i]].StartIndex].Point2.Y);
				}
			}
			GL.End();
		}
	}
}
