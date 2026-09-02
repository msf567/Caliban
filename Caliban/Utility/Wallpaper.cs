#nullable enable
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using static System.Drawing.Image;
using System.Threading.Tasks;

namespace Caliban.Core.Utility
{
    public sealed class Wallpaper
    {
        private Wallpaper()
        {
        }

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(
            int uAction,
            int uParam,
            string lpvParam,
            int fuWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static async Task Set(Uri uri, Style style)
        {
            using HttpClient client = new();
            using Stream stream = await client.GetStreamAsync(uri);

            using System.Drawing.Image img = FromStream(stream);

            string tempPath = Path.Combine(
                Path.GetTempPath(),
                "wallpaper.bmp");

            img.Save(
                tempPath,
                System.Drawing.Imaging.ImageFormat.Bmp);

            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    @"Control Panel\Desktop",
                    writable: true);

            if (key == null)
                throw new InvalidOperationException(
                    "Could not open the desktop registry key.");

            switch (style)
            {
                case Style.Stretched:
                    key.SetValue("WallpaperStyle", "2");
                    key.SetValue("TileWallpaper", "0");
                    break;

                case Style.Centered:
                    key.SetValue("WallpaperStyle", "1");
                    key.SetValue("TileWallpaper", "0");
                    break;

                case Style.Tiled:
                    key.SetValue("WallpaperStyle", "1");
                    key.SetValue("TileWallpaper", "1");
                    break;
            }

            SystemParametersInfo(
                SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}