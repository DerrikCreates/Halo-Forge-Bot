using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BondReader;
using BondReader.Schemas;
using Halo_Forge_Bot.DataModels;
using Halo_Forge_Bot.GameUI;
using Halo_Forge_Bot.Utilities;
using Halo_Forge_Bot.Windows;
using Memory;
using Microsoft.Win32;
using Newtonsoft.Json;
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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
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

            MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess().Id);
            Task.Run(Overlay.Setup);

            /*  var staticFields = typeof(HaloPointers).GetFields();
              foreach (var field in staticFields)
              {
                  if (field.FieldType == typeof(string))
                  {
                      CurrentPointersLabel.Text += Environment.NewLine + field.Name.ToString() + ": " +
                                                   (string)field.GetValue(null);
                  }
              }
              */
        }

        private BondSchema? _selectedMap;
        public static string? SelecteMapPath;

        private void LoadMvar_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "mvar files (*.mvar)|*.mvar|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                _selectedMap = BondHelper.ProcessFile<BondSchema>(openFileDialog.FileName);
                SelecteMapPath = Path.GetDirectoryName(openFileDialog.FileName);
                MapItemCount.Content = _selectedMap.Items.Count;
                string estimate = $"{Math.Round(TimeSpan.FromSeconds(_selectedMap.Items.Count * 7).TotalHours, 2)}h";
                EstimatedTime.Content = estimate;
            }
        }

        private async void StartBot_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedMap == null)
            {
                Log.Error("Selected map is null, select a map first");
                return;
            }

            Log.Information("-----STARTING BOT-----");
            await Bot.StartBot(Utils.SchemaToItemList(_selectedMap), int.Parse(ItemRangeStart.Text),
                int.Parse(ItemRangeEnd.Text));
            Log.Information("-----STOPPING BOT-----");
        }

        private async void ResumeBot_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedMap == null)
            {
                Log.Error("Selected map is null, select a map first");
                return;
            }

            if (!File.Exists(Utils.ExePath + "/recovery/ObjectRecoveryData.json"))
            {
                Log.Error("No recovery data found.");
                return;
            }

            Log.Information("-----STARTING BOT-----");
            try
            {
                await Bot.StartBot(Utils.SchemaToItemList(_selectedMap), int.Parse(ItemRangeStart.Text),
                    int.Parse(ItemRangeEnd.Text), true);
                Log.Information("-----STOPPING BOT-----");
            }
            catch (Exception exception)
            {
                ShowErrorPage(exception);

                Log.Fatal("Exception: {ExceptionMessage} , StackTrace: {Trace}", exception.Message,
                    exception.StackTrace);
            }
        }

        private void ShowErrorPage(Exception exception)
        {
            Error error = new Error();
            error.ErrorTextBox.Text = exception.Message + Environment.NewLine + exception.StackTrace;
            error.Show();
        }

        private async void LoadBlender_OnClick(object sender, RoutedEventArgs e)
        {
            List<ForgeItem> items = new();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                items = JsonConvert.DeserializeObject<BlenderMap>(File.ReadAllText(openFileDialog.FileName)).ItemList;

                // MapItemCount.Content = _selectedMap.Items.Count;
                // string estimate = $"{Math.Round(TimeSpan.FromSeconds(_selectedMap.Items.Count * 7).TotalHours, 2)}h";
                // EstimatedTime.Content = estimate;
            }

            //todo make the bot use blender rotation and not the forward/up

            try
            {
                await Bot.StartBot(items, isBlender: true, itemStart: int.Parse(ItemRangeStart.Text),
                    itemEnd: int.Parse(ItemRangeEnd.Text));
            }
            catch (Exception exception)
            {
                ShowErrorPage(exception);
                Log.Fatal("Exception: {ExceptionMessage} , StackTrace: {Trace}", exception.Message,
                    exception.StackTrace);
            }
        }

        private static readonly DevUI DevWindow = new DevUI();

        private void EnterDev_OnClick(object sender, RoutedEventArgs e)
        {
            DevWindow.Show();
        }

        private void Next_OnClick(object sender, RoutedEventArgs e)
        {
            Bot.next = true;
        }
    }
}