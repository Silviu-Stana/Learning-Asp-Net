using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;

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

            // Flip Y for screen coordinates (0,0 bottom-left)
            Vector2 mouse = new Vector2(MousePosition.X, Size.Y - MousePosition.Y);
Console.WriteLine(MousePosition.X + " ," +(Size.Y-MousePosition.Y));
            
            _currentScreen?.MouseDown(e, mouse);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
            _currentScreen?.Resize(e);
        }

        protected override void OnUnload()
        {
            _currentScreen?.Dispose();
            base.OnUnload();
        }
    }
}