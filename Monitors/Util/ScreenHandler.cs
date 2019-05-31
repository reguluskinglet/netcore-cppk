using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace RzdMonitors.Util
{
    public static class ScreenHandler
    {
        public static Screen GetCurrentScreen(Window window)
        {
            var parentArea = new System.Drawing.Rectangle((int)window.Left, (int)window.Top, (int)window.Width, (int)window.Height);
            return Screen.FromRectangle(parentArea);
        }

        public static Screen GetScreen(int requestedScreen)
        {
            var screens = Screen.AllScreens;
            var mainScreen = 0;
            if (screens.Length > 1 && mainScreen < screens.Length)
            {
                return screens[requestedScreen];
            }
            return screens[0];
        }

        public static int[] GetScreens()
        {
            var screens = Screen.AllScreens;

            var ret = new List<int>();

            var i = 0;
            foreach (var screen in screens)
            {
                ret.Add(i);
                i++;
            }

            return ret.ToArray();
        }
    }
}
