using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using ForgeMacros;
using Microsoft.VisualBasic.CompilerServices;
using Serilog;
using Serilog.Formatting.Compact;
using TextCopy;
using WindowsInput.Native;
using WK.Libraries.SharpClipboardNS;
using Clipboard = System.Windows.Forms.Clipboard;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;
using Size = System.Drawing.Size;

namespace Halo_Forge_Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File("Z://josh/log.txt")
                .WriteTo.File(new CompactJsonFormatter(), "Z://josh/log.json")
                .WriteTo.Debug()
                .CreateLogger();

            Log.Information("----------APP START----------");

            // BotClipboard.clipboard.ClipboardChanged += BotClipboard.ClipboardChanged;
            InitializeComponent();
            Input.InitInput();
            ForgeUI.SetHaloProcess();
        }

        private void TestBot_OnClick(object sender, RoutedEventArgs e)
        {
        }


        private async void DownloadMvarButton_OnClick(object sender, RoutedEventArgs e)
        {
            await Utils.DownloadMvar(FileShareLinkTextBox.Text, "");
        }


        private void GetItemNames_OnClick(object sender, RoutedEventArgs e)
        {
            Bot.GatherItemStrings();
        }


        private async void DebugRect_OnClick(object sender, RoutedEventArgs e)
        {
            //46 25
            //399 671
            // new Rectangle(new System.Drawing.Point(669, 545), new Size(578, 33));
            var rectangle = await Task.Run(ForgeUI.GetRectFromMouse);
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();


            Task.Run(Bot.DevTesting);

            // await Task.Run(Bot.DevTesting);
        }
    }
}