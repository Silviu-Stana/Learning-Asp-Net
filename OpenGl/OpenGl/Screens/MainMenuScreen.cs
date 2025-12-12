using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;


namespace OpenGl.Screens
{

    public class MainMenuScreen : Screen
    {
        // Button rectangles: x, y, width, height
        private RectangleF _triangleButton = new RectangleF(300, 100, 300, 70);
        private RectangleF _squareButton = new RectangleF(300, 250, 300, 70);
        private RectangleF _cubeButton = new RectangleF(300, 400, 300, 70);
        private RectangleF _textureButton = new RectangleF(300, 550, 300, 70);
        private RectangleF _transparentButton = new RectangleF(300, 700, 300, 70);
        private RectangleF _pyramidButton = new RectangleF(300, 850, 300, 70);
        private RectangleF _starButton = new RectangleF(300, 1000, 300, 70);
        private RectangleF _shadowButton = new RectangleF(300, 1150, 300, 70);

        private ButtonRenderer? _button;

        public override void Load(int width, int height)
        {
            RecalculateButtonPositions();
            _button = new ButtonRenderer(width, height);

        }

        void RecalculateButtonPositions()
        {
            //Put buttons in middle of Window, no matter how you scale it:
            _triangleButton.X = ParentWindow.FramebufferSize.X / 2 - _triangleButton.Width/2;
            _squareButton.X = ParentWindow.FramebufferSize.X / 2 - _squareButton.Width/2;
            _cubeButton.X = ParentWindow.FramebufferSize.X / 2 - _cubeButton.Width/2;
            _textureButton.X = ParentWindow.FramebufferSize.X / 2- _textureButton.Width/2;
            _transparentButton.X = ParentWindow.FramebufferSize.X / 2 - _transparentButton.Width / 2;
            _pyramidButton.X = ParentWindow.FramebufferSize.X / 2 - _pyramidButton.Width / 2;
            _starButton.X = ParentWindow.FramebufferSize.X / 2 - _starButton.Width / 2;
            _shadowButton.X = ParentWindow.FramebufferSize.X / 2 - _shadowButton.Width / 2;
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
            _button?.DrawButton(_transparentButton.X, _transparentButton.Y, _transparentButton.Width, _transparentButton.Height, new Vector4(0.204f, 0.835f, 0.878f, 1f), 20, 15, "Transparent");
            _button?.DrawButton(_pyramidButton.X, _pyramidButton.Y, _pyramidButton.Width, _pyramidButton.Height, new Vector4(0.867f, 0.204f, 0.878f, 1f), 20, 15, "Pyramid");
            _button?.DrawButton(_starButton.X, _starButton.Y, _starButton.Width, _starButton.Height, new Vector4(0.855f, 0.647f, 0.125f, 1f), 20, 15, "Star");
            _button?.DrawButton(_shadowButton.X, _shadowButton.Y, _shadowButton.Width, _shadowButton.Height, new Vector4(0.2f, 0.2f, 0.2f, 1f), 20, 15, "Shadows");
        }

        public override void MouseDown(MouseButtonEventArgs e, Vector2 mouse)
        {
            if (_triangleButton.Contains(mouse)) ParentWindow.LoadScreen(new TriangleScreen());
            else if (_squareButton.Contains(mouse)) ParentWindow.LoadScreen(new SquareScreen());
            else if (_cubeButton.Contains(mouse)) ParentWindow.LoadScreen(new CubeScreen());
            else if (_textureButton.Contains(mouse)) ParentWindow.LoadScreen(new TexturedCubeScreen());
            else if (_transparentButton.Contains(mouse)) ParentWindow.LoadScreen(new TransparentCubeScreen());
            else if (_pyramidButton.Contains(mouse)) ParentWindow.LoadScreen(new PyramidScreen());
            else if (_starButton.Contains(mouse)) ParentWindow.LoadScreen(new StarScreen());
            else if (_shadowButton.Contains(mouse)) ParentWindow.LoadScreen(new ShadowScreen());
        }

        public override void MouseUp(MouseButtonEventArgs e, Vector2 mousePosition)
        {
        }

        public struct RectangleF
        {
            public float X, Y, Width, Height;
            public RectangleF(float x, float y, float w, float h) { X = x; Y = y; Width = w; Height = h; }
            public bool Contains(Vector2 point) => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;
        }

    }
}