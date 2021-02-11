using OpenTK.Graphics.OpenGL;

namespace LibEveryFileExplorer._3D
{
    public class Glu
    {
        /// <summary>
        /// Replaces gluPerspective. Sets the frustum to perspective mode.
        /// http://nehe.gamedev.net/article/replacement_for_gluperspective/21002/
        /// </summary>
        /// <param name="fovY">Field of vision in degrees in the y direction</param>
        /// <param name="aspect">Aspect ratio of the viewport</param>
        /// <param name="zNear">The near clipping distance</param>
        /// <param name="zFar">The far clipping distance</param>
        public static void gluPerspective(float fovY, float aspect, float zNear, float zFar)
        {
            float fW, fH;
            fH = System.Convert.ToSingle(System.Math.Tan((fovY / 2) / 180 * System.Math.PI)) * zNear;
            fH = System.Convert.ToSingle(System.Math.Tan(fovY / 360 * System.Math.PI)) * zNear;
            fW = fH * aspect;
            GL.Frustum(-fW, fW, -fH, fH, zNear, zFar);
        }
    }
}
