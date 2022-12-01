using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Halo_Forge_Bot.Utilities;
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
        Input.InitMouseHook();
        
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

    private static bool CompareArrow(Color color)
    {
        return color.R < 75 && color.G < 75 && color.B < 75;
    }

    public static ForgeUIObjectModeEnum GetDefaultObjectMode(int screen)
    {
        if (screen >= Screen.AllScreens.Length) throw new Exception("Screen index out of range");
        using (var image = PixelReader.ScreenshotArea(Screen.AllScreens[screen].WorkingArea))
        {
            image.Save("lastScreenshot.png");
            if (CompareArrow(image.GetPixel(77, 400)))
                return ForgeUIObjectModeEnum.STATIC;
            
            if (CompareArrow(image.GetPixel(77, 434)))
                return ForgeUIObjectModeEnum.STATIC_FIRST;
            
            if (CompareArrow(image.GetPixel(77, 296)))
                return ForgeUIObjectModeEnum.DYNAMIC;
            
            if (CompareArrow(image.GetPixel(77, 705)))
                return ForgeUIObjectModeEnum.DYNAMIC_FIRST_VARIANT;
            
            if (CompareArrow(image.GetPixel(77, 331)))
                return ForgeUIObjectModeEnum.DYNAMIC_FIRST;
            
            throw new Exception("No Object Mode Detected");
        }
    }
}