using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;


namespace OpenGl.Screens
{

    public class MainMenuScreen : Screen
    {
        private int _width;
        // Button rectangles: x, y, width, height
        private RectangleF _triangleButton = new RectangleF(300, 100, 200, 50);
        private RectangleF _squareButton = new RectangleF(300, 200, 200, 50);
        private RectangleF _cubeButton = new RectangleF(300, 300, 200, 50);
        private RectangleF _textureButton = new RectangleF(300, 400, 200, 50);



        public MainMenuScreen()
        {
            
        }

        private ButtonRenderer? _button;

        public override void Load(int width, int height)
        {
            _button = new ButtonRenderer(width, height);
            _button.Resize(width, height);

        }

        void RecalculateButtonPositions()
        {
            _triangleButton.X = ParentWindow.FramebufferSize.X / 2 - _triangleButton.Width/2;
            _squareButton.X = ParentWindow.FramebufferSize.X / 2 - _squareButton.Width/2;
            _cubeButton.X = ParentWindow.FramebufferSize.X / 2 - _cubeButton.Width/2;
            _textureButton.X = ParentWindow.FramebufferSize.X / 2- _textureButton.Width/2;
        }

        public override void Resize(ResizeEventArgs e)
        {
            RecalculateButtonPositions();
            _button?.Resize(e.Width, e.Height);
        }
        public override void Update(FrameEventArgs args)
        {
        }


        public override void Render(FrameEventArgs args)
        {
            GL.Disable(EnableCap.DepthTest); // important for 2D UI

            RedrawAllButtons();
        }

        void RedrawAllButtons()
        {
            _button?.DrawButton(_triangleButton.X, _triangleButton.Y, _triangleButton.Width, _triangleButton.Height, new Vector4(1, 0, 0, 1), 20, 15, "Triangle");
            _button?.DrawButton(_squareButton.X, _squareButton.Y, _squareButton.Width, _squareButton.Height, new Vector4(0, 1, 0, 1), 20, 15, "Square");
            _button?.DrawButton(_cubeButton.X, _cubeButton.Y, _cubeButton.Width, _cubeButton.Height, new Vector4(0, 0, 1, 1), 20, 15, "Cube");
            _button?.DrawButton(_textureButton.X, _textureButton.Y, _textureButton.Width, _textureButton.Height, new Vector4(0.59f, 0.29f, 0.0f, 1f), 20, 15, "Textured");
        }

        public override void MouseDown(MouseButtonEventArgs e, Vector2 mouse)
        {
            if (_triangleButton.Contains(mouse)) ParentWindow.LoadScreen(new TriangleScreen());
            else if (_squareButton.Contains(mouse)) ParentWindow.LoadScreen(new SquareScreen());
            else if (_cubeButton.Contains(mouse)) ParentWindow.LoadScreen(new CubeScreen());
            else if (_textureButton.Contains(mouse)) ParentWindow.LoadScreen(new TexturedCubeScreen());
        }


        public struct RectangleF
        {
            public float X, Y, Width, Height;
            public RectangleF(float x, float y, float w, float h) { X = x; Y = y; Width = w; Height = h; }
            public bool Contains(Vector2 point) => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
        }

    }
}