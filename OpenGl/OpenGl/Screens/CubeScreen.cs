using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace OpenGl.Screens;

public class CubeScreen : Screen
{
    private Cube? _cube;

    public override void Load(int width, int height)
    {
        GL.ClearColor(Color4.Black);
        GL.Enable(EnableCap.DepthTest);

        _cube = new Cube();
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
        // A simple way to return to the main menu (e.g., click anywhere)
        // A better way is to render a "Back" button!
        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
        {
            // Swap back to the MainMenuScreen
            ParentWindow.LoadScreen(new MainMenuScreen());
        }
    }
}