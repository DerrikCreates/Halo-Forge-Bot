using System;
using System.Runtime.InteropServices;
using System.Text;
using ManagedWinapi.Windows;

namespace Halo_Forge_Bot.Utilitities;

public static class NativeHelper
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetActiveWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    public delegate bool CallBackPtr(int hwnd, int lParam);

    [DllImport("user32.dll")]
    public static extern int EnumWindows(CallBackPtr callPtr, int lPar);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy,
        uint uFlags);

    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    public static string GetText(IntPtr hWnd)
    {
        // Allocate correct string length first
        int length = NativeHelper.GetWindowTextLength(hWnd);
        StringBuilder sb = new StringBuilder(length + 1);
        NativeHelper.GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    

    

   


    
}