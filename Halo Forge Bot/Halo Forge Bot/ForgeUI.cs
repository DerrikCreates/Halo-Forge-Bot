using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ForgeMacros;

namespace Halo_Forge_Bot;

public static class ForgeUI
{
    public static Rect ProcessRect;
    public static Rect UIRect;

    public async static void Init()
    {
        await Task.Run(SetupForgeUIArea);
    }

    public static void EnsureUIRect()
    {
        //if (NativeHelper.HaloProcess.MainWindowHandle)
    }

    private static void CaptureUITopLeft(object? sender, MouseEventArgs args)
    {
        UIRect.X = args.X;
        UIRect.Y = args.Y;
    }
    
    private static void CaptureUIWidthHeight(object? sender, MouseEventArgs args)
    {
        if (args.X > UIRect.X)
        {
            UIRect.Width = args.X - UIRect.X;
        }
        else
        {
            UIRect.Width = UIRect.X - args.X;
            UIRect.X = args.X;
        }
            
        if (args.Y > UIRect.Y)
        {
            UIRect.Height = args.Y - UIRect.Y;
        }
        else
        {
            UIRect.Height = UIRect.Y - args.Y;
            UIRect.Y = args.Y;
        }
    }

    private static void SetupForgeUIArea()
    {
        Input.MouseHook.MouseDown += CaptureUITopLeft;
        Input.MouseHook.MouseUp += CaptureUIWidthHeight;

        while (UIRect.Height == 0 || UIRect.Width == 0)
        {
            Task.Delay(10);
        }
        
        Input.MouseHook.MouseDown -= CaptureUITopLeft;
        Input.MouseHook.MouseUp -= CaptureUIWidthHeight;
    }
}