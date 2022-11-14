using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ForgeMacros;
using Image = System.Windows.Controls.Image;

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
            ForgeUI.OnInitDone += ReadingStart;
            NativeHelper.ReadContentBrowser();
        }

        private Bitmap? lastImage;

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
    }
}