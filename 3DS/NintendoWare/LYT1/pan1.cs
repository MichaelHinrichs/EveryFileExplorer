using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Collections;
using System.IO;
using LibEveryFileExplorer.Files;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using LibEveryFileExplorer.IO;

namespace _3DS.NintendoWare.LYT1
{
	public class pan1
	{
		[Flags]
		public enum PaneFlags : byte
		{
			IsVisible = 1,
			IsInfluencedAlpha = 2,
			IsLocationAdjust = 4
		}
		[Flags]
		public enum PaneMagnifyFlags : byte
		{
			IgnorePartsMagnify = 1,
			AdjustToPartsBounds = 2
		}
		public pan1(String Name) { this.Name = Name; }
		public pan1(EndianBinaryReader er)
		{
			Signature = er.ReadString(Encoding.ASCII, 4);
			if (Signature != "pan1" && Signature != "pic1" && Signature != "txt1" && Signature != "bnd1" && Signature != "wnd1" && Signature != "prt1")
				throw new SignatureNotCorrectException(Signature, "pan1, pic1, txt1, bnd1, wnd1, prt1", er.BaseStream.Position - 4);
			SectionSize = er.ReadUInt32();
			Flags = (PaneFlags)er.ReadByte();
			Origin = er.ReadByte();
			Alpha = er.ReadByte();
			MagnifyFlags = (PaneMagnifyFlags)er.ReadByte();
			Name = er.ReadString(Encoding.ASCII, 24).Replace("\0", "");
			Translation = er.ReadVector3();
			Rotation = er.ReadVector3();
			Scale = er.ReadVector2();
			Size = er.ReadVector2();
		}
		public String Signature;
		public UInt32 SectionSize;
		public PaneFlags Flags;
		public Byte Origin;
		public Byte Alpha;
		public PaneMagnifyFlags MagnifyFlags;
		public String Name;//24
		public Vector3 Translation;
		public Vector3 Rotation;
		public Vector2 Scale;
		public Vector2 Size;

		public pan1 Parent = null;
		public List<pan1> Children = new List<pan1>();

		public enum XOrigin
		{
			Left = 0,
			Center = 1,
			Right = 2
		}
		public enum YOrigin
		{
			Top = 0,
			Center = 1,
			Bottom = 2
		}
		public XOrigin HAlignment { get { return (XOrigin)(Origin % 3); } }
		public YOrigin VAlignment { get { return (YOrigin)(Origin / 3); } }
		public bool InfluencedAlpha { get { return (Flags & PaneFlags.IsInfluencedAlpha) != 0; } }

		public virtual void Render(CLYT Layout, CLIM[] Textures, int InfluenceAlpha)
		{
            GL.PushMatrix();
			{
				GL.Translate(Translation.X, Translation.Y, Translation.Z);
				GL.Rotate(Rotation.X, 1, 0, 0);
				GL.Rotate(Rotation.Y, 0, 1, 0);
				GL.Rotate(Rotation.Z, 0, 0, 1);
				GL.Scale(Scale.X, Scale.Y, 1);
				foreach (pan1 p in Children)
				{
					p.Render(Layout, Textures, InfluencedAlpha ? (int)((float)(Alpha * InfluenceAlpha) / 255f) : Alpha);
				}
			}
			GL.PopMatrix();
		}

		protected float[,] SetupRect()
		{
			/*float[,] Vertex2 = new float[4, 2];
			Vertex2[0, 0] = 0;
			Vertex2[1, 0] = Size.X;
			Vertex2[2, 0] = Size.X;
			Vertex2[3, 0] = 0;

			Vertex2[0, 1] = 0;
			Vertex2[1, 1] = 0;
			Vertex2[2, 1] = -Size.Y;
			Vertex2[3, 1] = -Size.Y;
			return Vertex2;*/
			return SetupRect(Size.X, Size.Y);
		}

		protected static float[,] SetupRect(float Width, float Height)
		{
			float[,] Vertex2 = new float[4, 2];
			Vertex2[0, 0] = 0;
			Vertex2[1, 0] = Width;
			Vertex2[2, 0] = Width;
			Vertex2[3, 0] = 0;

			Vertex2[0, 1] = 0;
			Vertex2[1, 1] = 0;
			Vertex2[2, 1] = -Height;
			Vertex2[3, 1] = -Height;
			return Vertex2;
		}

		protected static void SetupMaterial(CLYT Layout, int MatId)
		{
			var mat = Layout.Materials.Materials[MatId];
			mat.ApplyAlphaCompareBlendMode();

			for (int o = 0; o < mat.TexMaps.Length; o++)
			{
                GL.ActiveTexture(TextureUnit.Texture0 + o);
				GL.BindTexture(TextureTarget.Texture2D, MatId * 4 + o + 1);
                GL.Enable(EnableCap.Texture2D);
            }
			if (mat.TexMaps.Length == 0)
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.Color4(1, 1, 1, 1);
				GL.BindTexture(TextureTarget.Texture2D, MatId * 4 + 1);
				GL.Enable(EnableCap.Texture2D);
			}

			GL.MatrixMode(MatrixMode.Texture);
			for (int o = 0; o < mat.TexCoordGens.Length; o++)
			{
				GL.ActiveTexture(TextureUnit.Texture0 + o);
				GL.LoadIdentity();
				//if ((int)mat.TexCoordGens[o].Source < 4)
				//{
				mat1.MaterialEntry.TexMatrix srt = mat.TexMatrices[o];//(int)mat.TexCoordGens[o].Source];
				GL.Translate(0.5f, 0.5f, 0.0f);
				GL.Rotate(srt.Rotation, 0.0f, 0.0f, 1.0f);
				GL.Scale(srt.Scale.X, srt.Scale.Y, 1.0f);
				GL.Translate(srt.Translation.X / srt.Scale.X - 0.5f, srt.Translation.Y / srt.Scale.Y - 0.5f, 0.0f);
				//}
				//else
				//{

				//}
			}
			GL.MatrixMode(MatrixMode.Modelview);
			mat.GlShader.Enable();
		}

		protected static float MixColors(params float[] c)
		{
			float a = c[0];
			for (int i = 1; i < c.Length; i++)
			{
				a *= c[i];
			}
			for (int i = 1; i < c.Length; i++)
			{
				a /= 255f;
			}
			return a / 255f;
		}

		public TreeNode GetTreeNodes()
		{
			TreeNode t = new TreeNode(Name);
			t.ImageKey = t.SelectedImageKey = Signature;
			foreach (var v in Children)
			{
				t.Nodes.Add(v.GetTreeNodes());
			}
			return t;
		}

		public override string ToString()
		{
			return Signature + ": " + Name;
		}
	}
}
