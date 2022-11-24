using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Halo_Forge_Bot.GameUI;
using Halo_Forge_Bot.Utilities;
using Memory;
using Serilog;
using Serilog.Formatting.Compact;
using Utils = Halo_Forge_Bot.Utilities.Utils;

namespace Halo_Forge_Bot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess().Id);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.File($"{Utils.ExePath}/log.txt")
                .WriteTo.File(new CompactJsonFormatter(), $"{Utils.ExePath}/log.json")
                .WriteTo.Debug()
                .CreateLogger();

            Log.Information("----------APP START----------");

            // BotClipboard.clipboard.ClipboardChanged += BotClipboard.ClipboardChanged;

            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);

            Directory.CreateDirectory(Utils.ExePath + "/images/");

            InitializeComponent();
            Input.InitInput();

            var staticFields = typeof(MemoryHelper).GetFields();
            foreach (var field in staticFields)
            {
                if (field.FieldType == typeof(string))
                {
                    CurrentPointersLabel.Text += Environment.NewLine + field.Name.ToString() + ": " +
                                                 (string)field.GetValue(null);
                }
            }
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


            await Bot.DevTesting();

            // await Task.Run(Bot.DevTesting);
        }


        private void UpdateMem_OnClick(object sender, RoutedEventArgs e)
        {
            MemoryTestUI.Text = MemoryHelper.GetEditBoxText();
        }


        private void ReadAddressButton_OnClick(object sender, RoutedEventArgs e)
        {
            var address = DebugMemoryAddressTextBox.Text;
            if (int.TryParse(DebugMemoryAddressLengthTextBox.Text, out int result))
            {
                var data = MemoryHelper.Memory.ReadBytes(address, result);
                DebugMemoryLabel.Text = Convert.ToHexString(data);
            }
        }

        private void AttachToHalo_OnClick(object sender, RoutedEventArgs e)
        {
            MemoryHelper.Memory.OpenProcess(ForgeUI.HaloProcess.Id);
        }
    }
}