using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace MivtzarNaki.App;

// Adapted from Acer's HWND helper. XAML owns mirroring: mirroring its HWND again
// reverses column order and glyphs. RTLREADING controls native caption reading only.
internal static class RTLHelper
{
    private const int ExStyle = -20;
    private const long LayoutRtl = 0x00400000, RtlReading = 0x00002000, NoInheritLayout = 0x00100000;
    public static bool HighContrast
    {
        get
        {
            var info = new HighContrastInfo { Size = (uint)Marshal.SizeOf<HighContrastInfo>() };
            return SystemParametersInfoW(0x0042, info.Size, ref info, 0) && (info.Flags & 1) != 0;
        }
    }
    [StructLayout(LayoutKind.Sequential)] private struct HighContrastInfo { public uint Size, Flags; public IntPtr Scheme; }
    [DllImport("user32.dll")] private static extern bool SystemParametersInfoW(uint action, uint parameter, ref HighContrastInfo value, uint flags);
    public static void Apply(Window window, FrameworkElement content)
    {
        content.FlowDirection = FlowDirection.RightToLeft;
        content.Language = "he-IL";
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var style = GetWindowLongPtrW(hwnd, ExStyle).ToInt64();
        SetWindowLongPtrW(hwnd, ExStyle, new IntPtr((style & ~LayoutRtl) | RtlReading | NoInheritLayout));
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, 0x0027); // FRAMECHANGED | NOMOVE | NOSIZE | NOZORDER
    }
    [DllImport("user32.dll")] private static extern IntPtr GetWindowLongPtrW(IntPtr hwnd, int index);
    [DllImport("user32.dll")] private static extern IntPtr SetWindowLongPtrW(IntPtr hwnd, int index, IntPtr value);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int width, int height, uint flags);
}
