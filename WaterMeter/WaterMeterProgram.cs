using SadConsole;
using SadConsole.Configuration;
using XnaPoint = Microsoft.Xna.Framework.Point;

namespace WaterMeter
{
    internal class WaterMeterProgram
    {
        private const int WIDTH = 20;
        private const int HEIGHT = 40;

        public static void Main(string[] _args)
        {
            Settings.WindowTitle = WaterMeter.TITLE;

            Builder startup = new Builder()
                .SetWindowSizeInCells(WIDTH, HEIGHT)
                .OnStart(OnStart)
                .ConfigureFonts(true);

            Game.Create(startup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static void OnStart(object _sender, GameHost _host)
        {
            // Make the window borderless and place it in the top-left corner of the screen.
            var window = Game.Instance.MonoGameInstance.Window;
            window.IsBorderless = true;
            window.Position = new XnaPoint(0, 0);

            // The SadConsole host is initialized now, so the console (and its socket) can be created.
            WaterMeter meter = new WaterMeter(WIDTH, HEIGHT);
            _host.Screen = meter;
            meter.IsFocused = true;
        }
    }
}