using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenGl.Text
{

    public class TextDrawer
    {
        private int _vao, _vbo;
        private Shader _shader;
        private Matrix4 _projection;
        private Font _font;

        public TextDrawer(int windowWidth, int windowHeight)
        {
            _shader = new Shader("text.vert", "text.frag");

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // Each vertex has: vec2 position + vec2 texCoord
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindVertexArray(0);

            _projection = Matrix4.CreateOrthographicOffCenter(0, windowWidth, 0, windowHeight, -1, 1);

            // --- Load the font once ---
            var collection = new FontCollection();
            FontFamily family = collection.Add("Fonts/Arial.ttf"); // Make sure the path is correct
            _font = family.CreateFont(32); // 16 px size
        }

        public void DrawString(string text, float centerX, float centerY)
        {
            int tex = CreateTextTexture(text, _font, out int w, out int h);

            float x = centerX - w / 2f;
            float y = centerY - h / 2f; // bottom-left origin

            float[] vertices = new float[]
            {
        x,     y,     0f, 0f,
        x + w, y,     1f, 0f,
        x + w, y + h, 1f, 1f,
        x,     y + h, 0f, 1f
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            _shader.Use();
            _shader.SetMatrix4("uProjection", _projection);
            _shader.SetInt("uTexture", 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            GL.BindVertexArray(0);

            // Keep the texture alive until after drawing
            GL.DeleteTexture(tex);
        }

        private int CreateTextTexture(string text, Font font, out int width, out int height)
        {
            TextOptions options = new TextOptions(font) { WrappingLength = float.MaxValue };
            FontRectangle rect = TextMeasurer.MeasureBounds(text, options);

            int padding = 4; //px
            width = (int)Math.Ceiling(rect.Width) + padding * 2;
            height = (int)Math.Ceiling(rect.Height) + padding * 2;
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            using var img = new Image<Rgba32>(width, height);
            img.Mutate(ctx =>
            {
                ctx.Clear(Color.Transparent);
                ctx.DrawText(text, font, Color.White, new PointF(0, 2));
                ctx.Flip(FlipMode.Vertical);
            });

            int texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            Rgba32[] pixels = new Rgba32[width * height];
            img.CopyPixelDataTo(pixels);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height,
                0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return texture;
        }
    }

}
