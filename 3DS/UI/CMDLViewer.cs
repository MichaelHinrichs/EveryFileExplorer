using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using _3DS.NintendoWare.GFX;
using System.Drawing.Imaging;
using LibEveryFileExplorer._3D;
using LibEveryFileExplorer.Collections;
using CommonFiles;
using System.IO;

namespace _3DS.UI
{
	public partial class CMDLViewer : GLControl
	{
		CGFX Resource;
		CMDL Model;

		public CMDLViewer(CGFX Resource, CMDL Model)
		{
			this.Resource = Resource;
			this.Model = Model;
			InitializeComponent();
			simpleOpenGlControl1.MouseWheel += new MouseEventHandler(simpleOpenGlControl1_MouseWheel);
		}
		void simpleOpenGlControl1_MouseWheel(object sender, MouseEventArgs e)
		{
			if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) { dist += ((float)e.Delta / 10); }
			else { dist += ((float)e.Delta / 100); }
			Render();
		}

		bool init = false;
		String glversion = null;

		float X = 0.0f;
		float Y = 0.0f;
		float ang = 0.0f;
		float dist = 0.0f;
		float elev = 0.0f;
		private void MDL0Viewer_Load(object sender, EventArgs e)
		{
			simpleOpenGlControl1.InitializeContexts();
			//GL.Enable(GL.GL_LIGHTING);
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

			//GL.glTexEnvf(GL.GL_TEXTURE_ENV, GL.GL_TEXTURE_ENV_MODE, GL.GL_BLEND);

			GL.ShadeModel(ShadingModel.Smooth);

			GL.DepthFunc(DepthFunction.Lequal);



			GL.ClearColor(51f / 255f, 51f / 255f, 51f / 255f, 0f);

			if (Resource.Data.Textures != null && Model != null)
			{
				int i = 0;
				foreach (var v in Model.Materials)
				{
					if (v.Tex0 != null) UploadTex(v.Tex0, i * 4 + 0 + 1);
					if (v.Tex1 != null) UploadTex(v.Tex1, i * 4 + 1 + 1);
					if (v.Tex2 != null) UploadTex(v.Tex2, i * 4 + 2 + 1);
					//if (v.Tex3 != null) UploadTex(v.Tex3, i * 4 + 3 + 1);
					i++;
				}
			}
			if (Model != null) Shaders = new CGFXShader[Model.Materials.Length];
			//GlNitro.glNitroBindTextures(file, 1);

			glversion = GL.GetString(StringName.Version);

			init = true;
			Render();
		}

		private void UploadTex(CMDL.MTOB.TexInfo TextureMapper, int Id)
		{
			if (!(TextureMapper.TextureObject is ReferenceTexture))
				return;
			var tex = Resource.Data.Textures[Resource.Data.Dictionaries[1].IndexOf(((ReferenceTexture)TextureMapper.TextureObject).LinkedTextureName)] as ImageTextureCtr;
			if (tex == null)
				return;
			GL.BindTexture(TextureTarget.Texture2D, Id);
			GL.Color3(1, 1, 1);
			Bitmap b = tex.GetBitmap();
			b.RotateFlip(RotateFlipType.RotateNoneFlipY);
			BitmapData d = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, b.Width, b.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, d.Scan0);
			b.UnlockBits(d);

			if (((TextureMapper.Unknown12 >> 1) & 1) == 1) GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			else GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			//if (((TextureMapper.Unknown12 >> 2) & 1) == 1) GL.glTexParameteri(TextureTarget.Texture2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);
			//else GL.glTexParameteri(TextureTarget.Texture2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_NEAREST);
			//A bit confusing, so using this for now:
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

			switch ((TextureMapper.Unknown12 >> 12) & 0xF)
			{
				case 0: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge); break;
				case 1: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder); break;
				case 2: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); break;
				case 3: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat); break;
				default: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); break;
			}

			switch ((TextureMapper.Unknown12 >> 8) & 0xF)
			{
				case 0: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge); break;
				case 1: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder); break;
				case 2: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); break;
				case 3: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat); break;
				default: GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); break;
			}
		}

		CGFXShader[] Shaders;
		public void Render()
		{
			if (!init) return;
			if (Model == null) return;
			//G3D_Binary_File_Format.Shaders.Shader s = new G3D_Binary_File_Format.Shaders.Shader();
			//s.Compile();
			//s.Enable();
			//GL.Viewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
			float aspect = (float)simpleOpenGlControl1.Width / (float)simpleOpenGlControl1.Height;
			//float fov = 60.0f; // in degrees
			//float znear = 0.02f;
			//float zfar = 1000.0f * file.modelSet.models[SelMdl].info.posScale;
			//GL.MatrixMode(GL.GL_PROJECTION);
			//GL.LoadMatrix(BuildPerspProjMat(fov, aspect, znear, zfar));
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Viewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
			//GL.glOrtho(-simpleOpenGlControl1.Width / 2.0f, simpleOpenGlControl1.Width / 2.0f, -simpleOpenGlControl1.Height / 2.0f, simpleOpenGlControl1.Height / 2.0f, 0.02f, 1000f * file.modelSet.models[SelMdl].info.posScale);
			//GL.MatrixMode(GL.GL_PROJECTION);
			//GL.LoadIdentity();
			//GL.glFrustum(-simpleOpenGlControl1.Width / 2f, simpleOpenGlControl1.Width / 2f, -simpleOpenGlControl1.Height / 2f, simpleOpenGlControl1.Height / 2f, 1000, -1000);
			Glu.gluPerspective(30, aspect, 0.1f, 20480000f);//0.02f, 32.0f);

			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			GL.BindTexture(TextureTarget.Texture2D, 0);
			GL.Color3(1.0f, 1.0f, 1.0f);
			//GL.Enable(GL.GL_DEPTH_TEST);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			/*GL.Rotate(elev, 1, 0, 0);
			GL.glFogfv(GL.GL_FOG_COLOR, new float[] { 0, 0, 0, 1 });
			GL.glFogf(GL.GL_FOG_DENSITY, 1);
			GL.Rotate(-elev, 1, 0, 0);*/

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
			CMDL m = Model;
			//foreach (_3DS.CGFX.DATA.CMDL m in cgfx.Data.Models)
			//{
			//int i = 0;
			foreach (var mm in m.Meshes)
			{
				var vv = m.Shapes[mm.ShapeIndex];
				Polygon p = vv.GetVertexData(m);

				var mat = m.Materials[mm.MaterialIndex];
				mat.FragShader.AlphaTest.GlApply();
				mat.FragmentOperation.BlendOperation.GlApply();

				GL.MatrixMode(MatrixMode.Texture);
				if (mat.Tex0 != null)
				{
					GL.ActiveTexture(TextureUnit.Texture0);
					GL.BindTexture(TextureTarget.Texture2D, mm.MaterialIndex * 4 + 0 + 1);
					GL.Enable(EnableCap.Texture2D);
					GL.LoadIdentity();
					float[] mtx = new float[16];
					Array.Copy(mat.TextureCoordiators[0].Matrix, mtx, 4 * 3);
					mtx[15] = 1;
					mtx[12] = mtx[3];
					mtx[13] = mtx[7];
					mtx[14] = mtx[11];
					mtx[3] = 0;
					mtx[7] = 0;
					mtx[11] = 0;
					GL.LoadMatrix(mtx);
					//if (mat.TextureCoordiators[0].MappingMethod == 2)
					//{
					//	GL.glTexGeni(GL.GL_S, GL.GL_TEXTURE_GEN_MODE, GL.GL_SPHERE_MAP);
					//	GL.glTexGeni(GL.GL_T, GL.GL_TEXTURE_GEN_MODE, GL.GL_SPHERE_MAP);
					//}
					//GL.Translate(mat.TextureCoordiators[0].Scale.X / mat.TextureCoordiators[0].Translate.X, mat.TextureCoordiators[0].Translate.Y * mat.TextureCoordiators[0].Scale.Y, 0);
					//GL.Rotate(mat.TextureCoordiators[0].Rotate, 0, 1, 0);
					//GL.glScalef(mat.TextureCoordiators[0].Scale.X, mat.TextureCoordiators[0].Scale.Y, 1);
				}
				if (mat.Tex1 != null)
				{
					GL.ActiveTexture(TextureUnit.Texture1);
					GL.BindTexture(TextureTarget.Texture2D, mm.MaterialIndex * 4 + 1 + 1);
					GL.Enable(EnableCap.Texture2D);
					GL.LoadIdentity();
					float[] mtx = new float[16];
					Array.Copy(mat.TextureCoordiators[1].Matrix, mtx, 4 * 3);
					mtx[15] = 1;
					mtx[12] = mtx[3];
					mtx[13] = mtx[7];
					mtx[14] = mtx[11];
					mtx[3] = 0;
					mtx[7] = 0;
					mtx[11] = 0;
					GL.LoadMatrix(mtx);
					//if (mat.TextureCoordiators[1].MappingMethod == 2)
					//{
					//	GL.glTexGeni(GL.GL_S, GL.GL_TEXTURE_GEN_MODE, GL.GL_SPHERE_MAP);
					//	GL.glTexGeni(GL.GL_T, GL.GL_TEXTURE_GEN_MODE, GL.GL_SPHERE_MAP);
					//}
				}
				if (mat.Tex2 != null)
				{
					GL.ActiveTexture(TextureUnit.Texture2);
					GL.BindTexture(TextureTarget.Texture2D, mm.MaterialIndex * 4 + 2 + 1);
					GL.Enable(EnableCap.Texture2D);
					GL.LoadIdentity();
					float[] mtx = new float[16];
					Array.Copy(mat.TextureCoordiators[2].Matrix, mtx, 4 * 3);
					mtx[15] = 1;
					mtx[12] = mtx[3];
					mtx[13] = mtx[7];
					mtx[14] = mtx[11];
					mtx[3] = 0;
					mtx[7] = 0;
					mtx[11] = 0;
					GL.LoadMatrix(mtx);
					//if (mat.TextureCoordiators[2].MappingMethod == 2)
					//{
					//	GL.glTexGeni(GL.GL_S, GL.GL_TEXTURE_GEN_MODE, GL.GL_SPHERE_MAP);
					//	GL.glTexGeni(GL.GL_T, GL.GL_TEXTURE_GEN_MODE, GL.GL_SPHERE_MAP);
					//}
				}
				/*if (mat.Tex3 != null)
				{
					MessageBox.Show("Tex3!!!!!!!!!!!");
					GL.glActiveTexture(GL.GL_TEXTURE3);
					GL.BindTexture(TextureTarget.Texture2D, mm.MaterialIndex * 4 + 3 + 1);
					GL.Enable(EnableCap.Texture2D);
					GL.LoadIdentity();
				}*/
				GL.MatrixMode(MatrixMode.Modelview);
				GL.PushMatrix();
				GL.Translate(vv.PositionOffset.X, vv.PositionOffset.Y, vv.PositionOffset.Z);

				if (glversion.StartsWith("2.") && Shaders[mm.MaterialIndex] == null)
				{
					List<int> tex = new List<int>();
					if (mat.Tex0 != null) tex.Add((int)mm.MaterialIndex * 4 + 0 + 1);
					else tex.Add(0);
					if (mat.Tex1 != null) tex.Add((int)mm.MaterialIndex * 4 + 1 + 1);
					else tex.Add(0);
					if (mat.Tex2 != null) tex.Add((int)mm.MaterialIndex * 4 + 2 + 1);
					else tex.Add(0);
					//if (mat.Tex3 != null) tex.Add((int)mm.MaterialIndex * 4 + 3 + 1);
					/*else */
					tex.Add(0);

					Shaders[mm.MaterialIndex] = new CGFXShader(mat, tex.ToArray());
					Shaders[mm.MaterialIndex].Compile();
				}
				if (glversion.StartsWith("2.")) Shaders[mm.MaterialIndex].Enable();

				//GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_EMISSION, new float[] { mat.Emission_2.R / 255f, mat.Emission_2.G / 255f, mat.Emission_2.B / 255f, mat.Emission_2.A / 255f });
				//GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_AMBIENT, new float[] { mat.Ambient_1.R / 255f, mat.Ambient_1.G / 255f, mat.Ambient_1.B / 255f, mat.Ambient_1.A / 255f });
				//GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_DIFFUSE, new float[] { mat.Diffuse_2.R / 255f, mat.Diffuse_2.G / 255f, mat.Diffuse_2.B / 255f, mat.Diffuse_2.A / 255f });
				//GL.glColorMaterial(GL.GL_FRONT_AND_BACK, GL.GL_DIFFUSE);
				//GL.Enable(GL.GL_COLOR_MATERIAL);

				foreach (var q in vv.PrimitiveSets[0].Primitives[0].IndexStreams)
				{
					Vector3[] defs = q.GetFaceData();

					GL.Begin(PrimitiveType.Triangles);
					foreach (Vector3 d in defs)
					{
						if (p.Normals != null) GL.Normal3(p.Normals[(int)d.X].X, p.Normals[(int)d.X].Y, p.Normals[(int)d.X].Z);
						if (p.Colors != null) GL.Color4(p.Colors[(int)d.X].R / 255f, p.Colors[(int)d.X].G / 255f, p.Colors[(int)d.X].B / 255f, p.Colors[(int)d.X].A / 255f);
						else GL.Color4(1, 1, 1, 1);
						if (p.TexCoords != null) GL.MultiTexCoord2(TextureUnit.Texture0, p.TexCoords[(int)d.X].X, p.TexCoords[(int)d.X].Y);
						if (p.TexCoords2 != null) GL.MultiTexCoord2(TextureUnit.Texture1, p.TexCoords2[(int)d.X].X, p.TexCoords2[(int)d.X].Y);
						if (p.TexCoords3 != null) GL.MultiTexCoord2(TextureUnit.Texture2, p.TexCoords3[(int)d.X].X, p.TexCoords3[(int)d.X].Y);
						GL.Vertex3(p.Vertex[(int)d.X].X, p.Vertex[(int)d.X].Y, p.Vertex[(int)d.X].Z);

						if (p.Normals != null) GL.Normal3(p.Normals[(int)d.Y].X, p.Normals[(int)d.Y].Y, p.Normals[(int)d.Y].Z);
						if (p.Colors != null) GL.Color4(p.Colors[(int)d.Y].R / 255f, p.Colors[(int)d.Y].G / 255f, p.Colors[(int)d.Y].B / 255f, p.Colors[(int)d.Y].A / 255f);
						else GL.Color4(1, 1, 1, 1);
						if (p.TexCoords != null) GL.MultiTexCoord2(TextureUnit.Texture0, p.TexCoords[(int)d.Y].X, p.TexCoords[(int)d.Y].Y);
						if (p.TexCoords2 != null) GL.MultiTexCoord2(TextureUnit.Texture1, p.TexCoords2[(int)d.Y].X, p.TexCoords2[(int)d.Y].Y);
						if (p.TexCoords3 != null) GL.MultiTexCoord2(TextureUnit.Texture2, p.TexCoords3[(int)d.Y].X, p.TexCoords3[(int)d.Y].Y);
						GL.Vertex3(p.Vertex[(int)d.Y].X, p.Vertex[(int)d.Y].Y, p.Vertex[(int)d.Y].Z);

						if (p.Normals != null) GL.Normal3(p.Normals[(int)d.Z].X, p.Normals[(int)d.Z].Y, p.Normals[(int)d.Z].Z);
						if (p.Colors != null) GL.Color4(p.Colors[(int)d.Z].R / 255f, p.Colors[(int)d.Z].G / 255f, p.Colors[(int)d.Z].B / 255f, p.Colors[(int)d.Z].A / 255f);
						else GL.Color4(1, 1, 1, 1);
						if (p.TexCoords != null) GL.MultiTexCoord2(TextureUnit.Texture0, p.TexCoords[(int)d.Z].X, p.TexCoords[(int)d.Z].Y);
						if (p.TexCoords2 != null) GL.MultiTexCoord2(TextureUnit.Texture1, p.TexCoords2[(int)d.Z].X, p.TexCoords2[(int)d.Z].Y);
						if (p.TexCoords3 != null) GL.MultiTexCoord2(TextureUnit.Texture2, p.TexCoords3[(int)d.Z].X, p.TexCoords3[(int)d.Z].Y);
						GL.Vertex3(p.Vertex[(int)d.Z].X, p.Vertex[(int)d.Z].Y, p.Vertex[(int)d.Z].Z);
					}
					GL.End();
				}
				GL.PopMatrix();
			}
		}

		bool wire = false;
		bool licht = true;
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
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
					X -= 5f;
					Render();
					return true;
				case Keys.X:
					X += 5f;
					Render();
					return true;
				case Keys.A:
					Y -= 5f;
					Render();
					return true;
				case Keys.S:
					Y += 5f;
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
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) dist += 12f;
					else dist += 1.2f;
					Render();
					return true;
				case Keys.Oemplus:
				case Keys.Oemplus | Keys.Shift:
					if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) dist -= 12f;
					else dist -= 1.2f;
					Render();
					return true;
				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			saveFileDialog1.FileName = Model.Name;
			if (saveFileDialog1.ShowDialog() == DialogResult.OK
				&& saveFileDialog1.FileName.Length > 0)
			{
				switch (saveFileDialog1.FilterIndex - 1)
				{
					case 0:
						{
							DAE o = Model.ToDAE(Resource);
							File.Create(saveFileDialog1.FileName).Close();
							File.WriteAllBytes(saveFileDialog1.FileName, o.Write());
							Directory.CreateDirectory(Path.GetDirectoryName(saveFileDialog1.FileName) + "\\Tex");
							foreach (var v in Resource.Data.Textures)
							{
								if (!(v is ImageTextureCtr)) continue;
								((ImageTextureCtr)v).GetBitmap().Save(Path.GetDirectoryName(saveFileDialog1.FileName) + "\\Tex\\" + v.Name + ".png");
							}
							break;
						}
					case 1:
						{
							OBJ o = Model.ToOBJ();
							o.MTLPath = Path.GetFileNameWithoutExtension(saveFileDialog1.FileName) + ".mtl";
							MTL m = Model.ToMTL("Tex");
							byte[] d = o.Write();
							byte[] d2 = m.Write();
							File.Create(saveFileDialog1.FileName).Close();
							File.WriteAllBytes(saveFileDialog1.FileName, d);
							File.Create(Path.ChangeExtension(saveFileDialog1.FileName, "mtl")).Close();
							File.WriteAllBytes(Path.ChangeExtension(saveFileDialog1.FileName, "mtl"), d2);
							Directory.CreateDirectory(Path.GetDirectoryName(saveFileDialog1.FileName) + "\\Tex");
							foreach (var v in Resource.Data.Textures)
							{
								//if (v.NrLevels > 2) v.GetBitmap(2).Save(System.IO.Path.GetDirectoryName(Path) + "\\Tex\\" + v.Name + ".png");
								//else if (v.NrLevels > 1) v.GetBitmap(1).Save(System.IO.Path.GetDirectoryName(Path) + "\\Tex\\" + v.Name + ".png");
								//else v.GetBitmap(0).Save(System.IO.Path.GetDirectoryName(Path) + "\\Tex\\" + v.Name + ".png");
								if (!(v is ImageTextureCtr)) continue;
								((ImageTextureCtr)v).GetBitmap().Save(Path.GetDirectoryName(saveFileDialog1.FileName) + "\\Tex\\" + v.Name + ".png");
							}
							break;
						}
					case 2:
						{
							String result = Model.ToMayaASCII(Resource);
							File.Create(saveFileDialog1.FileName).Close();
							File.WriteAllBytes(saveFileDialog1.FileName, Encoding.ASCII.GetBytes(result));
							Directory.CreateDirectory(Path.GetDirectoryName(saveFileDialog1.FileName) + "\\Tex");
							foreach (var v in Resource.Data.Textures)
							{
								if (!(v is ImageTextureCtr)) continue;
								((ImageTextureCtr)v).GetBitmap().Save(Path.GetDirectoryName(saveFileDialog1.FileName) + "\\Tex\\" + v.Name + ".png");
							}
							break;
						}
				}
			}
		}
	}
}
