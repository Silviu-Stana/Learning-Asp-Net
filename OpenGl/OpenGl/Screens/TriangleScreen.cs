using OpenGl;
using OpenGl.Screens;
using OpenGl.Shapes;
using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class TriangleScreen : Screen
{
    private TriangleShape? _triangle;
    private float AspectRatio;

    public override void Load(int width, int height)
    {
        GL.ClearColor(Color4.Black);

        _triangle = new TriangleShape();
        _triangle?.Load();
    }

    public override void Render(FrameEventArgs args)
    {
        _triangle?.Render();
    }

    public override void Dispose()
    {
        // Crucial: Clean up the cube's GPU resources when we leave this screen
        _triangle?.Unload();
        GL.Disable(EnableCap.DepthTest); // Disable 3D specific settings
        base.Dispose();
    }

    public override void MouseDown(MouseButtonEventArgs e, Vector2 mousePosition)
    {
        if (e.Button == MouseButton.Left) ParentWindow.LoadScreen(new MainMenuScreen());
    }

    public override void Resize(ResizeEventArgs e)
    {
        AspectRatio = (float)e.Width / e.Height;
        //_cube.Resize(e.Width, e.Height);
    }

    public override void Update(FrameEventArgs args)
    {
        //throw new NotImplementedException();
    }

    public override void MouseUp(MouseButtonEventArgs e, Vector2 mousePosition)
    {
    }
}