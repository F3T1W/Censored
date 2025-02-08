using System;
using System.Runtime.InteropServices;

namespace Censored.Helpers
{
    public static class WindowsAPI
    {
        public const int WS_EX_TRANSPARENT = 0x20;
        public const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int value);

        public static void SetWindowExTransparent(IntPtr hwnd)
        {
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
