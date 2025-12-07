using OpenGl.Screens;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace OpenGl
{
    public abstract class Screen : IScreen
    {
        public Window ParentWindow { get; set; }

        // Abstract methods force implementation in derived classes
        public abstract void Load(int width, int height);
        public abstract void Update(FrameEventArgs args);
        public abstract void Render(FrameEventArgs args);
        public abstract void MouseDown(MouseButtonEventArgs e, Vector2 mousePosition);
        public abstract void Resize(ResizeEventArgs e);

        // Default Dispose implementation
        public virtual void Dispose() { }
    }
}