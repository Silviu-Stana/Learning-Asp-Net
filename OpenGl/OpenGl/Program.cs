using OpenGl.Windows;
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
                ClientSize = new Vector2i(800, 600),
                Title = "Main Menu",
                // Request a Compatibility Profile to enable GL.Begin/GL.End functions
                Flags = ContextFlags.Default | ContextFlags.Debug, // Optional: add Debug flag
                Profile = ContextProfile.Compatability,
                APIVersion = new Version(3, 3)
            };

            var gameSettings = new GameWindowSettings{ UpdateFrequency = 60.0};

            using var window = new MainMenuWindow(gameSettings, nativeSettings);
            window.Run();
        }
    }
}


