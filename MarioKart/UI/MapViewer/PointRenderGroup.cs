using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LibEveryFileExplorer.GameData;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using LibEveryFileExplorer.Collections;

namespace MarioKart.UI.MapViewer
{
	public class PointRenderGroup<T> : RenderGroup where T : GameDataSectionEntry
	{
		private MemberInfo PositionMember;
		private GameDataSection<T> GameDataSection;

		public PointRenderGroup(Color PointColor, GameDataSection<T> GameDataSection, MemberInfo PositionMember)
		{
			this.PointColor = PointColor;
			this.GameDataSection = GameDataSection;
			this.PositionMember = PositionMember;
		}

		public Color PointColor { get; private set; }

		public override bool Interactable { get { return true; } }

		public override void Render(object[] Selection, bool Picking, int PickingId)
		{
			GL.PointSize((Picking ? 6f : 5));

            GL.Begin(PrimitiveType.Points);
            if (!Picking) GL.Color3(PointColor.R / 255f, PointColor.G / 255f, PointColor.B / 255f);
			int objidx = 1;
			foreach (var o in GameDataSection.Entries)
			{
				if (Picking)
				{
					Color c = Color.FromArgb(objidx | PickingId);
					GL.Color4(c.R / 255f, c.G / 255f, c.B / 255f, 1);
					objidx++;
				}
				Vector3 Position = GetPointPosition(o);
				GL.Vertex2(Position.X, Position.Z);

				if (!Picking && Selection != null && Selection.Contains(o))
				{
					GL.End();
					GL.PointSize(2f);
					GL.Begin(PrimitiveType.Points);
					GL.Color3(1, 1, 1);
					GL.Vertex2(Position.X, Position.Z);
					GL.End();
					GL.PointSize((Picking ? 6f : 5));
                    GL.Begin(PrimitiveType.Points);
                    GL.Color3(PointColor.R / 255f, PointColor.G / 255f, PointColor.B / 255f);
				}

			}
			GL.End();
		}

		private Vector3 GetPointPosition(T Entry)
		{
			if (PositionMember is PropertyInfo) return (Vector3)((PropertyInfo)PositionMember).GetValue(Entry, null);
			else if (PositionMember is FieldInfo) return (Vector3)((FieldInfo)PositionMember).GetValue(Entry);
			throw new Exception("PositionMember is no Property and no Field!");
		}

		private void SetPointPosition(T Entry, Vector3 Position)
		{
			if (PositionMember is PropertyInfo) ((PropertyInfo)PositionMember).SetValue(Entry, Position, null);
			else if (PositionMember is FieldInfo) ((FieldInfo)PositionMember).SetValue(Entry, Position);
			else throw new Exception("PositionMember is no Property and no Field!");
		}

		public override Vector3 GetPosition(int Index)
		{
			return GetPointPosition(GameDataSection[Index]);
		}

		public override void SetPosition(int Index, Vector3 Position, bool ValidY = false)
		{
			Vector3 NewPos = GetPointPosition(GameDataSection[Index]);
			NewPos.X = Position.X;
			if (ValidY) NewPos.Y = Position.Y;
			NewPos.Z = Position.Z;
			SetPointPosition(GameDataSection[Index], NewPos);
		}

		public override object GetEntry(int Index)
		{
			return GameDataSection[Index];
		}
	}
}
