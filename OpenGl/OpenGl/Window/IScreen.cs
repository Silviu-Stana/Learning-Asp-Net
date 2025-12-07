using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenGl.Windows;

namespace OpenGl.Screens
{
    public interface IScreen : IDisposable
    {
        // Reference to the main window to allow screen switching
        public AppWindow ParentWindow { get; set; }

        // Methods called by the ParentWindow's GameLoop
        void Load(int width, int height);
        void Update(FrameEventArgs args);
        void Render(FrameEventArgs args);
        void MouseDown(MouseButtonEventArgs e, Vector2 mousePosition);
        void Resize(ResizeEventArgs e);
    }
}