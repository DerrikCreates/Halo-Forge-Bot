using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using ForgeMacros;
using Size = System.Drawing.Size;

namespace Halo_Forge_Bot;

public static class ForgeUI
{
    public static Process HaloProcess;

    public static Rectangle ForgeMenu { get; set; } =
        new Rectangle(new System.Drawing.Point(45, 49), new Size(372, 623));

    public static Rectangle RenameBox { get; set; } =
        new Rectangle(new System.Drawing.Point(669, 545), new Size(578, 33));

    public static Process SetHaloProcess()
    {
        if (!Input.InputActive) Input.InitInput();
        Process[] haloProcesses = Process.GetProcessesByName("HaloInfinite");

        foreach (var process in haloProcesses)
        {
            if (process != null)
            {
                NativeHelper.SetForegroundWindow(process.MainWindowHandle);
                NativeHelper.SetActiveWindow(process.MainWindowHandle);
                HaloProcess = process;
                return process;
            }
        }

        //throw new InvalidOperationException($"Halo infinite process wasn't found");
        return null;
    }

    public static async Task<Rectangle> GetRectFromMouse()
    {
        Rectangle rectangle = new Rectangle();
        Input.MouseHook.MouseDown += CaptureUiTopLeft;
        Input.MouseHook.MouseUp += CaptureUiWidthHeight;

        while (rectangle.Height == 0 || rectangle.Width == 0)
        {
            await Task.Delay(10);
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