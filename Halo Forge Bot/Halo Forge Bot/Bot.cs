using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using ForgeMacros;
using WindowsInput.Native;

namespace Halo_Forge_Bot;

public static class Bot
{
    public static void WatchForChange(ref bool hasChanged, Rectangle rectangle, int timeout, int delay = 10)
    {
        var bitmap = new Bitmap(rectangle.Width, rectangle.Height);
        var g = Graphics.FromImage(bitmap);
        var lastPixelArray = new System.Drawing.Color[rectangle.Width * rectangle.Height];

        var point = new System.Drawing.Point(rectangle.X, rectangle.Y);

        var imageSize = new System.Drawing.Size(rectangle.Width, rectangle.Height);

        var destPoint = new System.Drawing.Point(0, 0);

        g.CopyFromScreen(point, destPoint, imageSize);
        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                lastPixelArray[(x * bitmap.Height) + y] = bitmap.GetPixel(x, y);
            }
        }

        int imageCount = 0;
        var endTime = DateTime.Now.AddMilliseconds(timeout);

        while (DateTime.Now < endTime)
        {
            g.CopyFromScreen(point, destPoint, imageSize);
            //bitmap.Save($"Z:/JOSH{i}.png");

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var current = bitmap.GetPixel(x, y);


                    if (current != lastPixelArray[(x * bitmap.Height) + y])
                    {
                        bitmap.SetPixel(x, y, Color.Red);
                        bitmap.Save($"Z:/josh/imageDIFF.png", ImageFormat.Png);

                        hasChanged = true;
                        return;
                    }


                    lastPixelArray[(x * bitmap.Height) + y] = current;
                }
            }

            //bitmap.Save($"Z:/josh/image{imageCount++}.png", ImageFormat.Png);
            Task.Delay(delay).Wait();
        }

        bitmap.Save("Z:/josh/imageNOCHANGE.png", ImageFormat.Png);
        g.Dispose();
        throw new Exception("NO CHANGE IN AREA");
    }

    /// <summary>
    /// Modifier key defaults to control
    /// </summary>
    /// <param name="area"> Area on the screen to monitor</param>
    /// <param name="key"> main key to press</param>
    /// <param name="useMod"> determines if the modifier key is pressed</param>
    /// <param name="mod">the modifier key</param>
    /// <param name="keySleep">the time to sleep in between each key action</param>
    public static void PressWithMonitor(Rectangle area, VirtualKeyCode key, bool useMod = false,
        VirtualKeyCode mod = VirtualKeyCode.CONTROL, int keySleep = 50)
    {
        bool hasChanged = false;
        Task.Run((() => WatchForChange(ref hasChanged, area, 1000, 10)));

        PressKey(key, keySleep, useMod, mod);
        while (hasChanged == false)
        {
        }
    }


    public static void PressKey(VirtualKeyCode key, int sleep = 50, bool useMod = false,
        VirtualKeyCode mod = VirtualKeyCode.CONTROL
    )
    {
        if (!useMod)
        {
            Thread.Sleep(sleep);
            Input.Simulate.Keyboard.KeyDown(key);
            Thread.Sleep(sleep);
            Input.Simulate.Keyboard.KeyUp(key);
            Thread.Sleep(sleep);
            return;
        }

        //todo find solution to halo not detecting ctr+x
        Thread.Sleep(sleep);
        Input.Simulate.Keyboard.KeyDown(mod);
        Thread.Sleep(sleep);
        Input.Simulate.Keyboard.KeyDown(key);
        Thread.Sleep(sleep);
        Input.Simulate.Keyboard.KeyUp(key);
        Thread.Sleep(sleep);
        Input.Simulate.Keyboard.KeyUp(mod);
        Thread.Sleep(sleep);
    }

    public static void PressMultipleTimes(int count, VirtualKeyCode key, Rectangle rectangle, int delay = 10,
        int timeout = 1000)
    {
        for (int i = 0; i < count; i++)
        {
            // await Task.Delay(10);
            //await Task.Delay(100);
            var v = WatchForChange;
            bool hasChanged = false;
            Task.Run(() => { WatchForChange(ref hasChanged, rectangle, 1000, delay); });

            PressKey(key);

            while (hasChanged == false) //todo make this not a bool check use tasls
            {
                // Console.WriteLine("waiting");
            }

            Thread.Sleep(50);
        }
    }

    public static void GatherItemStrings()
    {
        ForgeUI.SetHaloProcess();
        //NativeHelper.SetForegroundWindow(ForgeUI.HaloProcess.MainWindowHandle);
        // PressWithMonitor(ForgeUI.RenameBox,VirtualKeyCode.VK_X);

        PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.F2);
        Thread.Sleep(1000);
        PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_X, true);
    }
}