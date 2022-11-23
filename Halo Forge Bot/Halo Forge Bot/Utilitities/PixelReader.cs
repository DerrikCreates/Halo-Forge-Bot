using System;
using System.Drawing;
using System.Threading.Tasks;
using Serilog;

namespace Halo_Forge_Bot.Utilitities;

public static class PixelReader
{
    public static Bitmap ScreenshotArea(Rectangle area)
    {
        var bitmap = new Bitmap(area.Width, area.Height);
        using var graphics = Graphics.FromImage(bitmap);

        var point = new System.Drawing.Point(area.X, area.Y);
        var imageSize = new System.Drawing.Size(area.Width, area.Height);
        graphics.CopyFromScreen(point, new Point(0, 0), imageSize);

        return bitmap;
    }

    public static bool WatchForChange(Rectangle rectangle, int timeout, int delay = 10)
    {
        Log.Information("Staring to WatchForChange Delay:{Delay} , Timeout:{Timeout}",
            delay, timeout);
        using var bitmap = new Bitmap(rectangle.Width, rectangle.Height);
        using var g = Graphics.FromImage(bitmap);
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

        while (DateTime.Now < endTime) //todo make this a task and await it.
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
                        //  bitmap.Save($"Z:/josh/imageDIFF.png", ImageFormat.Png);
                        Log.Information(
                            "WatchForChange Change Detected --- Color Changed: {Color} Delay:{Delay} , Timeout:{Timeout}",
                            current.ToString(), delay, timeout);


                        return true;
                    }


                    lastPixelArray[(x * bitmap.Height) + y] = current;
                }
            }

            //bitmap.Save($"Z:/josh/image{imageCount++}.png", ImageFormat.Png);
            Task.Delay(delay).Wait();
        }

        Log.Fatal("WatchForChange NO Change Detected ---  Delay:{Delay} , Timeout:{Timeout}",
            delay, timeout);
        //  bitmap.Save("Z:/josh/imageNOCHANGE.png", ImageFormat.Png);
        

        return false;
        throw new Exception("NO CHANGE IN AREA");
    }
}