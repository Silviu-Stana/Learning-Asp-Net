using OpenGl.Screens;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenGl.Text;
using SixLabors.Fonts;

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
        private RectangleF triangleButton = new RectangleF(300, 150, 200, 50);
        private RectangleF squareButton = new RectangleF(300, 250, 200, 50);
        private RectangleF cubeButton = new RectangleF(300, 350, 200, 50);
        private RectangleF textureButton = new RectangleF(300, 450, 200, 50);

        private ButtonRenderer? _button;
        public override void Load(int width, int height)
        {
            var framebufferWidth = ParentWindow.Size.X;  // Or ParentWindow.FramebufferSize.X if available
            var framebufferHeight = ParentWindow.Size.Y; // Or ParentWindow.FramebufferSize.Y
            _button = new ButtonRenderer(framebufferWidth, framebufferHeight);
        }

        public override void Resize(ResizeEventArgs e){}
        public override void Update(FrameEventArgs args) { }

        
        public override void Render(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Disable(EnableCap.DepthTest); // important for 2D UI

            _button?.DrawButton(triangleButton.X, triangleButton.Y, triangleButton.Width, triangleButton.Height, new Vector4(1, 0, 0, 1), 20, 15, "Triangle");
            _button?.DrawButton(squareButton.X, squareButton.Y, squareButton.Width, squareButton.Height, new Vector4(0, 1, 0, 1), 20, 15, "Square");
            _button?.DrawButton(cubeButton.X, cubeButton.Y, cubeButton.Width, cubeButton.Height, new Vector4(0, 0, 1, 1), 20, 15, "Cube");
            _button?.DrawButton(textureButton.X, textureButton.Y, textureButton.Width, textureButton.Height, new Vector4(0, 0, 1, 1), 20, 15, "Textured");
        }

        public override void MouseDown(MouseButtonEventArgs e, Vector2 mouse)
        {
            if (triangleButton.Contains(mouse)) ParentWindow.LoadScreen(new TriangleScreen());
            else if (squareButton.Contains(mouse)) ParentWindow.LoadScreen(new SquareScreen());
            else if (cubeButton.Contains(mouse)) ParentWindow.LoadScreen(new CubeScreen());
            else if (textureButton.Contains(mouse)) ParentWindow.LoadScreen(new TexturedCubeScreen());
        }


        public struct RectangleF
        {
            public float X, Y, Width, Height;
            public RectangleF(float x, float y, float w, float h) { X = x; Y = y; Width = w; Height = h; }
            public bool Contains(Vector2 point) => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
        }






    }
}