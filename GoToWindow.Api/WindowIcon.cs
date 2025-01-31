using System;
using System.Runtime.InteropServices;

namespace GoToWindow.Api
{
    public static class WindowIcon
    {
        public const int PerIconTimeoutMilliseconds = 50;

        // ReSharper disable InconsistentNaming
        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        public const int ICON_SMALL2 = 2;
        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        public const int WM_GETICON = 0x7F;

        public const int SMTO_ABORTIFHUNG = 0x0002;
        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint  msg, uint  wParam, int lParam, uint  fuFlags, uint  uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetClassLongPtr")]
        public static extern IntPtr GetClassLongPtr64(IntPtr hWnd, int nIndex);

        public static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size > 4)
                return GetClassLongPtr64(hWnd, nIndex);
            else
                return new IntPtr(GetClassLongPtr32(hWnd, nIndex));
        }

        public static IntPtr GetIcon(IntPtr hwnd)
        {
            IntPtr iconHandle = IntPtr.Zero;

            var result = SendMessageTimeout(hwnd, WM_GETICON, ICON_BIG, 0, SMTO_ABORTIFHUNG, PerIconTimeoutMilliseconds, out iconHandle);
            if (result != IntPtr.Zero && iconHandle == IntPtr.Zero)
                result = SendMessageTimeout(hwnd, WM_GETICON, ICON_SMALL, 0, SMTO_ABORTIFHUNG, PerIconTimeoutMilliseconds, out iconHandle);
            if (result != IntPtr.Zero && iconHandle == IntPtr.Zero)
                result = SendMessageTimeout(hwnd, WM_GETICON, ICON_SMALL2, 0, SMTO_ABORTIFHUNG, PerIconTimeoutMilliseconds, out iconHandle);
            if (result != IntPtr.Zero && iconHandle != IntPtr.Zero)
                return iconHandle;

            iconHandle = GetClassLongPtr(hwnd, GCL_HICON);
            if (iconHandle == IntPtr.Zero)
                iconHandle = GetClassLongPtr(hwnd, GCL_HICONSM);
            return iconHandle;
        }
    }
}
