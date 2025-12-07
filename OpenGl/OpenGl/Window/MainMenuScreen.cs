using OpenGl.Screens;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace OpenGl.Windows
{
    public struct GlyphInfo
    {
        public float U, V, Width, Height; // U, V are texture coordinates
        public float XAdvance;            // How far to move cursor after drawing
    }

    public class MainMenuScreen : Screen
    {
        // Button rectangles: x, y, width, height
        private readonly RectangleF triangleButton = new RectangleF(300, 150, 200, 50);
        private readonly RectangleF squareButton = new RectangleF(300, 250, 200, 50);
        private readonly RectangleF cubeButton = new RectangleF(300, 350, 200, 50);

        // Define the radius for the rounded corners (e.g., 10 pixels)
        private const float CornerRadius = 20.0f;
        // Define the number of line segments to approximate the curve (e.g., 8-16)
        private const int CornerSegments = 15;


        // 💡 TEXT RENDERING FIELDS
        private int _fontTextureId;
        private const string FontName = "Arial";
        private const int FontSize = 18;
        private const int FontAtlasSize = 256; // Texture dimensions (must be power of two)
        private readonly Dictionary<char, GlyphInfo> _glyphInfo = new Dictionary<char, GlyphInfo>();

        public override void Load(int width, int height)
        {
        }

        public override void Update(FrameEventArgs args) { }

        public override void Render(FrameEventArgs args)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(0);     // Ensure no shader is active
            GL.BindVertexArray(0); // Ensure no VAO is bound

            // 1. Set up Orthographic Projection for 2D UI
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, ParentWindow.Size.X, 0, ParentWindow.Size.Y, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            DrawButton(triangleButton, Color4.Red, "Triangle");
            DrawButton(squareButton, Color4.Green, "Square");
            DrawButton(cubeButton, Color4.Blue, "Cube");

        }

        public override void MouseDown(MouseButtonEventArgs e, Vector2 mouse)
        {
            if (triangleButton.Contains(mouse)) ParentWindow.LoadScreen(new TriangleScreen());
            else if (squareButton.Contains(mouse)) ParentWindow.LoadScreen(new SquareScreen());
            else if (cubeButton.Contains(mouse)) ParentWindow.LoadScreen(new CubeScreen());
        }

        private void DrawButton(RectangleF rect, Color4 color, string text)
        {
            float x = rect.X;
            float y = rect.Y;
            float w = rect.Width;
            float h = rect.Height;
            float r = CornerRadius; // Shorthand for radius

            GL.Color4(color);
            GL.Begin(PrimitiveType.Quads);
            {
                // 1. Draw the central rectangle (excludes the 4 corners)
                GL.Vertex2(x + r, y);
                GL.Vertex2(x + w - r, y);
                GL.Vertex2(x + w - r, y + h);
                GL.Vertex2(x + r, y + h);

                // 2. Draw the side strips (fills the gaps left by excluding the corners)

                // Left strip
                GL.Vertex2(x, y + r);
                GL.Vertex2(x + r, y + r);
                GL.Vertex2(x + r, y + h - r);
                GL.Vertex2(x, y + h - r);

                // Right strip
                GL.Vertex2(x + w - r, y + r);
                GL.Vertex2(x + w, y + r);
                GL.Vertex2(x + w, y + h - r);
                GL.Vertex2(x + w - r, y + h - r);
            }
            GL.End();

            // 3. Draw the four rounded corners using polygons (GL.Begin(PrimitiveType.TriangleFan))

            // Bottom-Left Corner (Center: x + r, y + r. Angle: 180 to 270 degrees)
            DrawArc(x + r, y + r, r, 180, 270, CornerSegments);

            // Bottom-Right Corner (Center: x + w - r, y + r. Angle: 270 to 360/0 degrees)
            DrawArc(x + w - r, y + r, r, 270, 360, CornerSegments);

            // Top-Right Corner (Center: x + w - r, y + h - r. Angle: 0 to 90 degrees)
            DrawArc(x + w - r, y + h - r, r, 0, 90, CornerSegments);

            // Top-Left Corner (Center: x + r, y + h - r. Angle: 90 to 180 degrees)
            DrawArc(x + r, y + h - r, r, 90, 180, CornerSegments);


            float textX = x + (w / 2);
            float textY = y + (h / 2); // Center of the button in bottom-up coordinates


            DrawString(text, textX, textY);
        }


        /// <summary>
        /// Draws a filled arc (sector) using GL_TRIANGLE_FAN.
        /// </summary>
        private void DrawArc(float centerX, float centerY, float radius, float startAngleDegrees, float endAngleDegrees, int segments)
        {
            // The arc is drawn as a triangle fan originating from the center.
            GL.Begin(PrimitiveType.TriangleFan);
            {
                GL.Vertex2(centerX, centerY); // Center point

                float angleStep = (endAngleDegrees - startAngleDegrees) / segments;

                for (int i = 0; i <= segments; i++)
                {
                    float angleDeg = startAngleDegrees + i * angleStep;
                    float angleRad = angleDeg * (float)Math.PI / 180.0f;

                    float x = centerX + radius * (float)Math.Cos(angleRad);
                    float y = centerY + radius * (float)Math.Sin(angleRad);

                    GL.Vertex2(x, y);
                }
            }
            GL.End();
        }

        public override void Resize(ResizeEventArgs e) { }

        public struct RectangleF
        {
            public float X, Y, Width, Height;
            public RectangleF(float x, float y, float w, float h) { X = x; Y = y; Width = w; Height = h; }
            public bool Contains(Vector2 point) => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
        }

        private readonly System.Drawing.Font _uiFont = new("Arial", 16);

        private void DrawString(string text, float centerX, float centerY)
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