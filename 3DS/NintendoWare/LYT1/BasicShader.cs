using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace _3DS.NintendoWare.LYT1
{
	public class BasicShader
	{
		public void Enable()
		{
			GL.UseProgram(program);
		}
		public void Compile(bool texture)
		{
			// w.e good for now
			//uint sampler_count = 1;
			//if (sampler_count == 0)
			//{
			//	sampler_count = 1;
			//}
			// generate vertex/fragment shader code
			//{
			StringBuilder vert_ss = new StringBuilder();
			//String vert_ss = "";

			vert_ss.AppendLine("void main()");
			vert_ss.AppendLine("{");
			{
				vert_ss.AppendLine("gl_FrontColor = gl_Color;");
				vert_ss.AppendLine("gl_BackColor = gl_Color;");
				//if (texture)
				//{
					//vert_ss.AppendLine("gl_TexCoord[0] = gl_TextureMatrix[0];");
				//}

				vert_ss.AppendLine("gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;");
			}
			vert_ss.AppendLine("}");

			// create/compile vertex shader
			vertex_shader = GL.CreateShader(ShaderType.VertexShader);

			{
				var vert_src_str = vert_ss.ToString();
				//const GLchar* vert_src = vert_src_str.c_str();
				GL.ShaderSource(vertex_shader, 1, new string[] { vert_src_str }, new int[] { vert_src_str.Length });
			}

			//}	// done generating vertex shader

			GL.CompileShader(vertex_shader);

			// generate fragment shader code
			//{
			StringBuilder frag_ss = new StringBuilder();
			//frag_ss += "uniform sampler2D tex;";
			// uniforms

			frag_ss.AppendLine("void main()");
			frag_ss.AppendLine("{");
			{
				if (!texture)
				{
					frag_ss.AppendLine("gl_FragColor = gl_Color;");
				}
				else
				{
					//frag_ss.AppendLine("gl_Color = vec4(1,1,1,1);");
					//frag_ss.AppendLine("gl_FragColor = texture2D(0, gl_TexCoord[0].st);");
				}
			}
			frag_ss.AppendLine("}");

			//std::cout << frag_ss.str() << '\n';

			// create/compile fragment shader
			fragment_shader = GL.CreateShader(ShaderType.FragmentShader);

			{
				var frag_src_str = frag_ss.ToString();
                GL.ShaderSource(fragment_shader, 1, new String[] { frag_src_str }, new int[] { frag_src_str.Length });
			}

            //}	// done generating fragment shader

            GL.CompileShader(fragment_shader);

			// check compile status of both shaders
			//{
			int vert_compiled = 0;
			int frag_compiled = 0;

			GL.GetShader(vertex_shader, ShaderParameter.CompileStatus, out vert_compiled);
            GL.GetShader(fragment_shader, ShaderParameter.CompileStatus, out frag_compiled);

			if (vert_compiled == 0)
			{
				//std::cout << "Failed to compile vertex shader\n";
			}

			if (frag_compiled == 0)
			{
				//std::cout << "Failed to compile fragment shader\n";
			}

			// create program, attach shaders
			program = GL.CreateProgram();
			GL.AttachShader(program, vertex_shader);
            GL.AttachShader(program, fragment_shader);

			// link program, check link status
			GL.LinkProgram(program);
			int link_status;
			GL.GetProgram(program, GetProgramParameterName.LinkStatus, out link_status);

			if (link_status == 0)
			{
				//std::cout << "Failed to link program!\n";
			}

			GL.UseProgram(program);

            // print log
            //{
            //StringBuilder infolog = new StringBuilder();
            string infolog = "";
            int length = 0;
			GL.GetProgramInfoLog(program, 10240, out length, out infolog);
			//std::cout << infolog;
			//}

			// pause
			//std::cin.get();
			//}
		}
		public int program = 0, fragment_shader = 0, vertex_shader = 0;
	}
}
