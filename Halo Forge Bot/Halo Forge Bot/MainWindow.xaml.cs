using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using ForgeMacros;
using Microsoft.VisualBasic.CompilerServices;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Halo_Forge_Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TestBot_OnClick(object sender, RoutedEventArgs e)
        {
            ForgeUI.OnInitDone += StartBot;
            NativeHelper.ReadContentBrowser();


            //50x 160y   400x 160y

            //50 565     400x 565

            // rough pos of the item menu
            Point topLeft = new Point(50, 160);
            Point topRight = new Point(400, 160);
            Point bottomLeft = new Point(50, 565);
            Point bottomRight = new Point(400, 565);

            var width = topLeft - topRight;
            var height = bottomLeft - topLeft;


            var center = new Point(((int)topLeft.X + (width.Length / 2)), (int)topLeft.Y + (height.Length / 2));
            var rectangle = new Rectangle((int)center.X, (int)center.Y, (int)width.Length, (int)height.Length);

            /* //visualize rectangle position
            Input.Simulate.Mouse.MoveMouseTo(0, 0);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseBy((int)topLeft.X, (int)topLeft.Y);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseTo(0, 0);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseBy((int)topRight.X, (int)topRight.Y);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseTo(0, 0);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseBy((int)bottomLeft.X, (int)bottomLeft.Y);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseTo(0, 0);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseBy((int)bottomRight.X, (int)bottomRight.Y);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseTo(0, 0);
            Thread.Sleep(1000);
            Input.Simulate.Mouse.MoveMouseBy((int)center.X, (int)center.Y);

*/

            //Input.Simulate.Mouse.MoveMouseTo(0, 0);
            //  Thread.Sleep(100);
            // Input.Simulate.Mouse.MoveMouseToPositionOnVirtualDesktop(center.X, center.Y);
            // Thread.Sleep(1000);
            //  var b = Screen.PrimaryScreen.Bounds;
            // var hasChange = WatchForChange(rectangle, 5000);
        }


        private void StartBot()
        {
            var hasChanged = WatchForChange(ConvertRectToRectangle(ForgeUI.UIRect), 1000);
        }

        private Rectangle ConvertRectToRectangle(Rect rect)
        {
            //TODO create a property in the forge ui rect to auto convert;
            Rectangle r = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            return r;
        }

        private Bitmap? lastImage;

        private static bool WatchForChange(Rectangle rectangle, int timeout, int delay = 10)
        {
            Thread.Sleep(1000);
            var bitmap = new Bitmap(rectangle.Width, rectangle.Height);
            var g = Graphics.FromImage(bitmap);
            var lastPixelArray = new System.Drawing.Color[rectangle.Width * rectangle.Height];

            var point = new System.Drawing.Point(rectangle.X, rectangle.Y);
            // var destinationPoint =
            //     new System.Drawing.Point((int)rect.TopLeft.X, (int)rect.TopLeft.Y);
            var imageSize = new System.Drawing.Size(rectangle.Width, rectangle.Height);

            var destPoint = new System.Drawing.Point(0, 0);
            //First run
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
                //bitmap.Save($"Z:/JOSH{i}.png", ImageFormat.Png);

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        var current = bitmap.GetPixel(x, y);
                        if (current != lastPixelArray[(x * bitmap.Height) + y])
                        {
                            bitmap.Save($"Z:/josh/imageDIFF.png", ImageFormat.Png);
                            return true;
                        }

                        lastPixelArray[(x * bitmap.Height) + y] = current;
                    }
                }

                bitmap.Save($"Z:/josh/image{imageCount++}.png", ImageFormat.Png);
                Thread.Sleep(delay);
            }

            bitmap.Save("Z:/josh/imageNOCHANGE.png", ImageFormat.Png);
            g.Dispose();

            return false;
        }

/*
        private static void ReadingStart()
        {
            Thread.Sleep(1000);

            var bitmap = new Bitmap((int)ForgeUI.UIRect.Width, (int)ForgeUI.UIRect.Height);
            var g = Graphics.FromImage(bitmap);
            var lastPixelArray = new System.Drawing.Color[(int)ForgeUI.UIRect.Width * (int)ForgeUI.UIRect.Height];

            var sourcePoint = new System.Drawing.Point((int)ForgeUI.UIRect.TopLeft.X, (int)ForgeUI.UIRect.TopLeft.Y);
            var destinationPoint =
                new System.Drawing.Point((int)ForgeUI.UIRect.TopLeft.X, (int)ForgeUI.UIRect.TopLeft.Y);
            var imageSize = new System.Drawing.Size((int)ForgeUI.UIRect.Size.Width, (int)ForgeUI.UIRect.Size.Height);


            //First run
            g.CopyFromScreen(sourcePoint, destinationPoint, imageSize);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    lastPixelArray[(x * bitmap.Height) + y] = bitmap.GetPixel(x, y);
                }
            }

            //g.Dispose();

            while (true)
            {
                g.CopyFromScreen(sourcePoint, destinationPoint, imageSize);
                //bitmap.Save($"Z:/JOSH{i}.png", ImageFormat.Png);

                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        var current = bitmap.GetPixel(x, y);
                        if (current != lastPixelArray[(x * bitmap.Height) + y])
                        {
                            // bitmap.Save($"Z:/josh/image{i}DIFF.png", ImageFormat.Png);
                        }

                        lastPixelArray[(x * bitmap.Height) + y] = current;
                    }
                }

                // g.Dispose();
                //bitmap.Save($"Z:/josh/image{i}.png", ImageFormat.Png);
                Thread.Sleep(10);
            }

            g.Dispose();
        }
        */
        private async void DownloadMvarButton_OnClick(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            var msg = client.Send(new HttpRequestMessage(HttpMethod.Get, new Uri(FileShareLinkTextBox.Text)));

            var page = await msg.Content.ReadAsStringAsync();
            var startIndex = page.IndexOf("https://blobs-infiniteugc.svc.halowaypoint.com/ugcstorage/");
            var endIndex = page.IndexOf("/images/thumbnail.jpg");
            var link = page.Substring(startIndex, endIndex - startIndex);
            var final = link + "/map.mvar";
            var mapMessage = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri(final)));

            var mvarData = await mapMessage.Content.ReadAsByteArrayAsync();

            await File.WriteAllBytesAsync("Z://Mvar/downloaded.mvar", mvarData);
        }
    }
}