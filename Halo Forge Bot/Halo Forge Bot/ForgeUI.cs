using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using ForgeMacros;

namespace Halo_Forge_Bot;

public static class ForgeUI
{
    public static Process HaloProcess;

    public static Process SetHaloActive()
    {
        if (!Input.InputActive) Input.InitInput();
        Process[] haloProcesses = Process.GetProcessesByName("HaloInfinite");

        foreach (var process in haloProcesses)
        {
            if (process != null)
            {
                NativeHelper.SetForegroundWindow(process.MainWindowHandle);
                NativeHelper.SetActiveWindow(process.MainWindowHandle);
                return process;
            }
        }

        throw new InvalidOperationException($"Halo infinite process wasn't found");
    }

    private static Rectangle GetRectFromMouse()
    {
        Rectangle rectangle = new Rectangle();
        Input.MouseHook.MouseDown += CaptureUiTopLeft;
        Input.MouseHook.MouseUp += CaptureUiWidthHeight;

        while (rectangle.Height == 0 || rectangle.Width == 0)
        {
            Task.Delay(10);
        }

        Input.MouseHook.MouseDown -= CaptureUiTopLeft;
        Input.MouseHook.MouseUp -= CaptureUiWidthHeight;

        return rectangle;

        void CaptureUiTopLeft(object? sender, MouseEventArgs args)
        {
            rectangle.X = args.X;
            rectangle.Y = args.Y;
        }

        void CaptureUiWidthHeight(object? sender, MouseEventArgs args)
        {
            if (args.X > rectangle.X)
            {
                rectangle.Width = args.X - rectangle.X;
            }
            else
            {
                rectangle.Width = rectangle.X - args.X;
                rectangle.X = args.X;
            }

            if (args.Y > rectangle.Y)
            {
                rectangle.Height = args.Y - rectangle.Y;
            }
            else
            {
                rectangle.Height = rectangle.Y - args.Y;
                rectangle.Y = args.Y;
            }
        }
    }
}