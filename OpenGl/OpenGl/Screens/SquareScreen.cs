using OpenGl;
using OpenGl.Screens;
using OpenGl.Shapes;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class SquareScreen : Screen
{
    private SquareShape? _square;

    public override void Load(int width, int height)
    {
        GL.ClearColor(Color4.Black);

        _square = new SquareShape();
        _square?.Load();
    }

    public override void Render(FrameEventArgs args)
    {
        _square?.Render();
    }

    public override void Dispose()
    {
        // Crucial: Clean up the cube's GPU resources when we leave this screen
        _square?.Unload();
        GL.Disable(EnableCap.DepthTest); // Disable 3D specific settings
        base.Dispose();
    }

    public override void MouseDown(MouseButtonEventArgs e, Vector2 mousePosition)
    {
        // A simple way to return to the main menu (e.g., click anywhere)
        // A better way is to render a "Back" button!
        if (e.Button == OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
        {
            // Swap back to the MainMenuScreen
            ParentWindow.LoadScreen(new MainMenuScreen());
        }
    }

    public override void Resize(ResizeEventArgs e)
    {
    }

    public override void Update(FrameEventArgs args)
    {
        //throw new NotImplementedException();
    }
}