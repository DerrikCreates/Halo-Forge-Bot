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

        private List<ForgeItem>? _itemsToSpawn;
        public static string? selectedMapPath;

        private void LoadMap_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MapFiles (*.mvar;*.dcjson;*.json)|*.mvar;*.dcjson;*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo info = new FileInfo(openFileDialog.FileName);

                var extension = info.Extension.ToLower();
                if (extension == ".dcjson" || extension == ".json")
                {
                    //PROCESS THE BLENDER DATA

                    _itemsToSpawn = JsonConvert.DeserializeObject<BlenderMap>(File.ReadAllText(openFileDialog.FileName))
                        .ItemList;
                }

                if (extension == ".mvar")
                {
                    //PROCESS THE MVAR DATA

                    var mvar = BondHelper.ProcessFile<BondSchema>(openFileDialog.FileName);
                    _itemsToSpawn = Utils.SchemaToItemList(mvar);
                }

                if (_itemsToSpawn == null)
                {
                    return;
                }

                selectedMapPath = Path.GetDirectoryName(openFileDialog.FileName);
                MapItemCount.Content = _itemsToSpawn.Count;
                string estimate = $"{Math.Round(TimeSpan.FromSeconds(_itemsToSpawn.Count * 4).TotalHours, 2)}h";
                EstimatedTime.Content = estimate;
            }
        }

        private async void StartBot_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_itemsToSpawn == null)
                {
                    ShowErrorPage("Either no map has been selected or the data is bad");
                    Log.Error("Selected map is null, select a map first");
                    return;
                }

                var saveAmmount = int.Parse(SaveItemCount.Text);
                Log.Information("-----STARTING BOT-----");
                await Bot.StartBot(_itemsToSpawn, saveAmmount, int.Parse(ItemRangeStart.Text),
                    int.Parse(ItemRangeEnd.Text));
                Log.Information("-----STOPPING BOT-----");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                ShowErrorPage(exception);
                throw;
            }
        }

        private async void ResumeBot_OnClick(object sender, RoutedEventArgs e)
        {
            /*
                        if (_itemsToSpawn == null)
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
                            await Bot.StartBot(Utils.SchemaToItemList(_itemsToSpawn), int.Parse(ItemRangeStart.Text),
                                int.Parse(ItemRangeEnd.Text), true);
                            Log.Information("-----STOPPING BOT-----");
                        }
                        catch (Exception exception)
                        {
                            ShowErrorPage(exception);
            
                            Log.Fatal("Exception: {ExceptionMessage} , StackTrace: {Trace}", exception.Message,
                                exception.StackTrace);
                        }
                        */
        }

        private void ShowErrorPage(Exception exception)
        {
            Error error = new Error();
            error.ErrorTextBox.Text = exception.Message + Environment.NewLine + exception.StackTrace;
            error.Show();
        }

        private void ShowErrorPage(string exception)
        {
            Error error = new Error();
            error.ErrorTextBox.Text = exception;
            error.Show();
        }

        private async void LoadBlender_OnClick(object sender, RoutedEventArgs e)
        {
            List<ForgeItem> items = new();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "DCjson files (*.dcjson)|*.dcjson|All files (*.*)|*.*";
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
                var saveAmmount = int.Parse(SaveItemCount.Text);
                await Bot.StartBot(items, isBlender: true, itemStart: int.Parse(ItemRangeStart.Text),
                    itemEnd: int.Parse(ItemRangeEnd.Text), saveAmmount: saveAmmount);
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