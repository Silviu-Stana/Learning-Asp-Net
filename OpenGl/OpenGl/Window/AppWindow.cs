using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenGl.Screens;

namespace OpenGl.Windows
{
    public class AppWindow(GameWindowSettings g, NativeWindowSettings n) : GameWindow(g, n)
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
            _currentScreen.Load(Size.X, Size.Y);
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

            // Flip Y for screen coordinates (0,0 bottom-left)
            Vector2 mouse = new Vector2(MousePosition.X * (Size.X / FramebufferSize.X),
                            (Size.Y - MousePosition.Y) * (Size.Y / FramebufferSize.Y));

            _currentScreen?.MouseDown(e, mouse);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _currentScreen?.Resize(e);
        }

        protected override void OnUnload()
        {
            _currentScreen?.Dispose();
            base.OnUnload();
        }
    }
}