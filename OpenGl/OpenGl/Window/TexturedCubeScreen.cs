using OpenGl;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public class TexturedCubeScreen : GameWindow
{
    private TexturedCube _texturedCube;
    private Matrix4 _view;
    private Matrix4 _projection;

    public TexturedCubeScreen(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings)
    {
        _texturedCube = new TexturedCube();
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);
        GL.Enable(EnableCap.DepthTest);

        LoadTexturedCube();
    }

    void LoadTexturedCube()
    {
        GL.Enable(EnableCap.DepthTest);

        _view = Matrix4.LookAt(
            new Vector3(2, 2, 2),
            Vector3.Zero,
            Vector3.UnitY);

        _projection = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(60),
            Size.X / (float)Size.Y,
            0.1f,
            100f);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        _texturedCube.Render((float)args.Time, _view, _projection);
        
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        // Update the GL viewport to match the new window size
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _texturedCube.Dispose();
    }
}