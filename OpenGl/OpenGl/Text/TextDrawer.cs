using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenGl.Text
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class TextDrawer
    {
        private readonly System.Drawing.Font _uiFont = new("Arial", 16);

        public void DrawString(string text, float centerX, float centerY)
        {
            int tex = CreateTextTexture(text, _uiFont, out int w, out int h);

            // Center text
            float x = centerX - w / 2f;
            float y = centerY - h / 2f;

            DrawTexture(tex, x, y, w, h);

            GL.DeleteTexture(tex); // Not needed long-term, so delete now
        }

        private void DrawTexture(int texId, float x, float y, float w, float h)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.Color4(Color4.White);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 1); GL.Vertex2(x, y);
            GL.TexCoord2(1, 1); GL.Vertex2(x + w, y);
            GL.TexCoord2(1, 0); GL.Vertex2(x + w, y + h);
            GL.TexCoord2(0, 0); GL.Vertex2(x, y + h);

            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);
        }

       
        private int CreateTextTexture(string text, System.Drawing.Font font, out int width, out int height)
        {
            using Bitmap bmp = new Bitmap(1, 1);
            using Graphics g = Graphics.FromImage(bmp);

            // Measure the required text size
            SizeF size = g.MeasureString(text, font);
            width = (int)Math.Ceiling(size.Width);
            height = (int)Math.Ceiling(size.Height);

            using Bitmap tex = new Bitmap(width, height);
            using Graphics gfx = Graphics.FromImage(tex);
            gfx.Clear(Color.Transparent);
            gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // Draw text
            gfx.DrawString(text, font, Brushes.White, 0, 0);

            // Upload to OpenGL
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            BitmapData data = tex.LockBits(
                new Rectangle(0, 0, tex.Width, tex.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba,
                data.Width, data.Height,
                0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            tex.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return texture;
        }
    }
}
