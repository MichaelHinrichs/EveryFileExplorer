using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarioKart.MKDS.NKM;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using LibEveryFileExplorer.Collections;

namespace MarioKart.UI.MapViewer
{
	public class MKDSAreaRenderGroup : RenderGroup
	{
		AREA Areas;

		public MKDSAreaRenderGroup(AREA Areas, Color AreaColor)
		{
			this.Areas = Areas;
			this.AreaColor = AreaColor;
		}

		public Color AreaColor { get; private set; }

		public override bool Interactable { get { return false; } }

		public override void Render(object[] Selection, bool Picking, int PickingId)
		{
			if (Picking) return;
			GL.Color4(AreaColor.R / 255f, AreaColor.G / 255f, AreaColor.B / 255f, AreaColor.A / 255f);
			GL.Begin(PrimitiveType.Quads);
			foreach (var o in Areas.Entries)
			{
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
			GL.End();
		}
	}
}
