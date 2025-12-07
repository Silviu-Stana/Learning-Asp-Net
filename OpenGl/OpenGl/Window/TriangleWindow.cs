using OpenGl.Shapes;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

public class TriangleWindow : GameWindow
{
    private TriangleShape _triangle;

    public TriangleWindow(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
        : base(gameSettings, nativeSettings)
    {
        _triangle = new TriangleShape();
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Color4.Black);
        _triangle.Load();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _triangle.Render();
        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _triangle.Unload();
    }
}