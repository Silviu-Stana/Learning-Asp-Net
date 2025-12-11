using OpenGl;
using OpenGl.Screens;
using OpenGl.Shapes;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
        if (e.Button == MouseButton.Left) ParentWindow.LoadScreen(new MainMenuScreen());
    }

    public override void Resize(ResizeEventArgs e){}
    public override void Update(FrameEventArgs args){}
    public override void MouseUp(MouseButtonEventArgs e, Vector2 mousePosition){}
}