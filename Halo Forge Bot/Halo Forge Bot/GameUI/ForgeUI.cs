using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Properties;
using Size = System.Drawing.Size;

namespace Halo_Forge_Bot.GameUI;

public static class ForgeUI
{
    public static Process HaloProcess;

    //todo make all rectangles adjustable from the ui
    public static Rectangle ForgeMenu { get; set; } =
        new Rectangle(new System.Drawing.Point(46, 25), new Size(399, 671));

    public static Rectangle RenameBox { get; set; } =
        new Rectangle(new System.Drawing.Point(669, 545), new Size(578, 33));

    public static Rectangle TransformHUD =
        new Rectangle(new System.Drawing.Point(556, 846), new Size(803, 206));

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

    public static ForgeUIObjectModeEnum GetDefaultObjectMode()
    {
        using (var image = PixelReader.ScreenshotArea(Screen.PrimaryScreen.Bounds))
        {
            var staticByDefault = image.GetPixel(77, 433);
            if (staticByDefault == Color.FromArgb(255, 57, 57, 57))
            {
                return ForgeUIObjectModeEnum.STATIC_FIRST;
                //  Log.Information("static by default");
            }

            var dynamicOnly = image.GetPixel(77, 296);
            if (dynamicOnly == Color.FromArgb(255, 57, 57, 57))
            {
                return ForgeUIObjectModeEnum.DYNAMIC;
                // Log.Information("dynamic only");
            }

            var dynamicDefault = image.GetPixel(77, 331);
            if (dynamicDefault == Color.FromArgb(255, 57, 57, 57))
            {
                return ForgeUIObjectModeEnum.DYNAMIC_FIRST;
            }

            throw new Exception("No Object Mode Detected");
            //image.Save("z:/josh/fuckoff.png", ImageFormat.Png);
        }
    }
}