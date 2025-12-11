using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl.Screens;

public class TransparentCubeScreen : Screen
{
    private TransparentCube? _cube;

    public override void Load(int width, int height)
    {
        GL.ClearColor(Color4.Black);
        GL.Enable(EnableCap.DepthTest);

        _cube = new TransparentCube();
        _cube.Load(width, height);
    }

    public override void Render(FrameEventArgs args)
    {
        _cube?.Render();
    }

    public override void Resize(ResizeEventArgs e)
    {
        _cube?.Resize(e.Width, e.Height);
    }

    public override void Dispose()
    {
        // Crucial: Clean up the cube's GPU resources when we leave this screen
        _cube?.Unload();
        GL.Disable(EnableCap.DepthTest); // Disable 3D specific settings
        base.Dispose();
    }

    public override void Update(FrameEventArgs args)
    {
        // Update logic for the cube (e.g., rotation)
        _cube?.Update((float)args.Time);
    }

    public override void MouseDown(MouseButtonEventArgs e, Vector2 mouse)
    {
        if (e.Button == MouseButton.Left) ParentWindow.LoadScreen(new MainMenuScreen());
    }

    public override void MouseUp(MouseButtonEventArgs e, Vector2 mousePosition)
    {
    }
}