using OpenGl;
using OpenGl.Screens;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class TexturedCubeScreen : Screen
{
    private TexturedCube? _texturedCube;
    private Matrix4 _view;
    private Matrix4 _projection;
    private int _width;
    private int _height;

    public override void Load(int width, int height)
    {
        _width = width;
        _height = height;

        GL.ClearColor(Color4.Black);
        GL.Enable(EnableCap.DepthTest);

        _texturedCube = new TexturedCube();

        _view = Matrix4.LookAt(
            new Vector3(2, 2, 2),
            Vector3.Zero,
            Vector3.UnitY);

        _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60),  width / (float)height,  0.1f,  100f);
    }

    public override void Render(FrameEventArgs args)
    {
        _texturedCube?.Render((float)args.Time, _view, _projection);
    }

    public override void Resize(ResizeEventArgs e)
    {
        // Update the GL viewport to match the new window size
        GL.Viewport(0, 0, _width, _height);
    }

    public override void Dispose()
    {
        _texturedCube?.Dispose();
    }

    public override void Update(FrameEventArgs args){}
    public override void MouseUp(MouseButtonEventArgs e, Vector2 mousePosition) { }

    public override void MouseDown(MouseButtonEventArgs e, Vector2 mouse)
    {
        if (e.Button == MouseButton.Left) ParentWindow.LoadScreen(new MainMenuScreen());
    }
}