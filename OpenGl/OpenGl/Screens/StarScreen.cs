using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl.Screens;

public class StarScreen : Screen
{
    private Star? _star;

    public override void Load(int width, int height)
    {
        GL.ClearColor(Color4.Black);
        GL.Enable(EnableCap.DepthTest);

        _star = new Star();
        _star.Load(width, height);
    }

    public override void Render(FrameEventArgs args)
    {
        _star?.Render();
    }

    public override void Resize(ResizeEventArgs e)
    {
        _star?.Resize(e.Width, e.Height);
    }

    public override void Dispose()
    {
        // Crucial: Clean up the cube's GPU resources when we leave this screen
        _star?.Unload();
        GL.Disable(EnableCap.DepthTest); // Disable 3D specific settings
        base.Dispose();
    }

    public override void Update(FrameEventArgs args)
    {
        // Update logic for the cube (e.g., rotation)
        _star?.Update((float)args.Time);
    }

    public override void MouseDown(MouseButtonEventArgs e, Vector2 mouse)
    {
        if (e.Button == MouseButton.Left) ParentWindow.LoadScreen(new MainMenuScreen());
    }

    public override void MouseUp(MouseButtonEventArgs e, Vector2 mousePosition)
    {
    }
}