using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenGl.Text
{

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public class TextDrawer
    {
        public void DrawString(string text, float centerX, float centerY)
        {
            var font = SystemFonts.CreateFont("Arial", 16);
            int tex = CreateTextTexture(text, font, out int w, out int h);

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

       
        private int CreateTextTexture(string text, Font font, out int width, out int height)
        {
            // Measure the text
            TextOptions options = new TextOptions(font)
            {
                WrappingLength = float.MaxValue
            };
            FontRectangle rect = TextMeasurer.MeasureBounds(text, options);

            width = (int)Math.Ceiling(rect.Width);
            height = (int)Math.Ceiling(rect.Height);

            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            // Create an ImageSharp image
            using Image<Rgba32> img = new Image<Rgba32>(width, height);

            img.Mutate(ctx =>
            {
                ctx.Clear(Color.Transparent);
                ctx.DrawText(text, font, Color.White, new PointF(0, 0));
            });

            // Upload image as OpenGL texture
            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            // Lock pixel data
            Rgba32[] pixels = new Rgba32[width * height];
            img.CopyPixelDataTo(pixels);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                width,
                height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                pixels
            );

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return texture;
        }
    }
}
