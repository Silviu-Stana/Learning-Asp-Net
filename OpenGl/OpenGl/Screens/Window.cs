using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.Windows;

namespace OpenGl.Screens
{
    public class Window(GameWindowSettings g, NativeWindowSettings n) : GameWindow(g, n)
    {
        // 💡 The current screen (state) being displayed
        private IScreen? _currentScreen;


        // Method to swap the content/state
        public void LoadScreen(IScreen newScreen)
        {
            // 1. Clean up the old screen
            _currentScreen?.Dispose();

            // 2. Set the new screen
            _currentScreen = newScreen;
            _currentScreen.ParentWindow = this; // Give the screen a reference to us

            // 3. Load the new screen's resources
            _currentScreen.Load(FramebufferSize.X, FramebufferSize.Y);
        }

        // --- GameWindow Overrides ---

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(Color4.Black);

            // Start the application in the MainMenuState
            LoadScreen(new MainMenuScreen());
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _currentScreen?.Render(args);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            _currentScreen?.Update(args);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            Vector2 mouse = GetMousePosition();

            Console.WriteLine(mouse);
            
            _currentScreen?.MouseDown(e, mouse);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            _currentScreen?.MouseUp(e, GetMousePosition());
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            _currentScreen?.MouseMove(e, GetMousePosition());
        }

        Vector2 GetMousePosition()
        {
            // Flip Y for screen coordinates (0,0 bottom-left)
            float scaleX = (float)FramebufferSize.X / ClientSize.X;
            float scaleY = (float)FramebufferSize.Y / ClientSize.Y;

            Vector2 mouse = new Vector2(
                MousePosition.X * scaleX,
                (ClientSize.Y - MousePosition.Y) * scaleY
            );

            return mouse;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);

            _currentScreen?.Resize(new ResizeEventArgs(FramebufferSize.X, FramebufferSize.Y));

        }

        protected override void OnUnload()
        {
            _currentScreen?.Dispose();
            base.OnUnload();
        }
    }
}