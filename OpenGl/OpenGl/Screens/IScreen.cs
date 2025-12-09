using OpenTK.Windowing.Common;
using OpenTK.Mathematics;

namespace OpenGl.Screens
{
    public interface IScreen : IDisposable
    {
        // Reference to the main window to allow screen switching
        public Window ParentWindow { get; set; }

        // Methods called by the ParentWindow's GameLoop
        void Load(int width, int height);
        void Update(FrameEventArgs args);
        void Render(FrameEventArgs args);
        void MouseDown(MouseButtonEventArgs e, Vector2 mousePosition);
        void MouseUp(MouseButtonEventArgs e, Vector2 mousePosition);
        void MouseMove(MouseMoveEventArgs e, Vector2 mousePosition);
        void Resize(ResizeEventArgs e);
    }
}