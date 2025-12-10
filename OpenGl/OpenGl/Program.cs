using OpenGl.Screens;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenGl
{
    internal static class Program
    {
        private static void Main()
        {
            var nativeSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(800, 1100),
                Title = "Main Menu",
                // Compatibility: enables GL.Begin/GL.End functions
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3),
                NumberOfSamples = 8
            };

            var gameSettings = new GameWindowSettings{ UpdateFrequency = 60.0};

            using var window = new Window(gameSettings, nativeSettings);
            window.Run();
        }
    }
}


