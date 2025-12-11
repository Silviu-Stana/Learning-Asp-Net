using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGl.Screens;

public class PyramidScreen : Screen
{
    private Pyramid? pyramid;

    private bool _isDragging = false;
    private Vector2 _lastMousePos;
    private float _rotationX = 0f;
    private float _rotationY = 0f;

    public override void Load(int width, int height)
    {
        GL.ClearColor(Color4.Black);
        GL.Enable(EnableCap.DepthTest);

        pyramid = new Pyramid();
        pyramid.Load(width, height);
    }

    public override void Render(FrameEventArgs args)
    {
        if (pyramid != null)
        {
            pyramid.RotationX = _rotationX;
            pyramid.RotationY = _rotationY;
            pyramid?.Render();
        }
    }

    public override void Resize(ResizeEventArgs e)
    {
        pyramid?.Resize(e.Width, e.Height);
    }

    public override void Dispose()
    {
        // Crucial: Clean up the cube's GPU resources when we leave this screen
        pyramid?.Unload();
        GL.Disable(EnableCap.DepthTest); // Disable 3D specific settings
        base.Dispose();
    }

    public override void Update(FrameEventArgs args)
    {
        // Update logic for the cube (e.g., rotation)
        pyramid?.Update((float)args.Time);

    }

    public override void MouseDown(MouseButtonEventArgs e, Vector2 mouse)
    {
        if (e.Button == MouseButton.Left)
        {
            _isDragging = true;
            _lastMousePos = mouse;
        }

        if (e.Button == MouseButton.Right)
        {
            // Return to main menu
            ParentWindow.LoadScreen(new MainMenuScreen());
        }
    }   
    public override void MouseUp(MouseButtonEventArgs e, Vector2 mouse)
    {
         _isDragging = false;
    }

    public override void MouseMove(MouseMoveEventArgs e, Vector2 mousePosition)
    {
        if (_isDragging)
        {
            Vector2 delta = mousePosition - _lastMousePos;
            _rotationY -= delta.X * 0.01f; // sensitivity
            _rotationX += delta.Y * 0.01f;
            _lastMousePos = mousePosition;
        }
    }
}