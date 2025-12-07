using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

namespace OpenGl.Windows
{
    public class MainMenuWindow : GameWindow
    {
        // Button rectangles: x, y, width, height
        private readonly RectangleF triangleButton = new RectangleF(300, 150, 200, 50);
        private readonly RectangleF squareButton = new RectangleF(300, 250, 200, 50);
        private readonly RectangleF cubeButton = new RectangleF(300, 350, 200, 50);

        public MainMenuWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(Color4.Black);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            DrawButton(triangleButton, Color4.Red, "Triangle");
            DrawButton(squareButton, Color4.Green, "Square");
            DrawButton(cubeButton, Color4.Blue, "Cube");

            SwapBuffers();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // Get current mouse position from the window
            Vector2 mouse = new Vector2(MousePosition.X, Size.Y - MousePosition.Y); // Flip Y

            if (triangleButton.Contains(mouse))OpenShapeWindow("Triangle");
            else if (squareButton.Contains(mouse))OpenShapeWindow("Square");
            else if (cubeButton.Contains(mouse))OpenShapeWindow("Cube");
        }

        private void OpenShapeWindow(string shape)
        {
            var nativeSettings = new NativeWindowSettings{Title = shape, ClientSize = new Vector2i(600, 400)
};
            var gameSettings = new GameWindowSettings { UpdateFrequency = 60.0 };

            GameWindow window = shape switch
            {
                "Triangle" => new TriangleWindow(gameSettings, nativeSettings),
                "Square" => new SquareWindow(gameSettings, nativeSettings),
                "Cube" => new CubeWindow(gameSettings, nativeSettings),
                _ => throw new Exception("Shape does not exist")
            };

            window?.Run();
        }

        private void DrawButton(RectangleF rect, Color4 color, string text)
        {
            // Save current state
            GL.PushMatrix();

            // Set up orthographic projection (0,0 bottom-left, window-size top-right)
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Size.X, 0, Size.Y, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Draw the quad
            GL.Color4(color);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(rect.X, rect.Y);
            GL.Vertex2(rect.X + rect.Width, rect.Y);
            GL.Vertex2(rect.X + rect.Width, rect.Y + rect.Height);
            GL.Vertex2(rect.X, rect.Y + rect.Height);
            GL.End();

            // Restore previous matrix state
            GL.PopMatrix();

            // Note: Text drawing still needs a library; currently it's just color blocks
        }

        public struct RectangleF
        {
            public float X, Y, Width, Height;
            public RectangleF(float x, float y, float w, float h) { X = x; Y = y; Width = w; Height = h; }
            public bool Contains(Vector2 point) => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
        }
    }
}