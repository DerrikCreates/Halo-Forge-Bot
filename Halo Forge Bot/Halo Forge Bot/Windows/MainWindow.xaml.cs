using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using BondReader;
using BondReader.Schemas;
using Halo_Forge_Bot.Core;
using Halo_Forge_Bot.DataModels;
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
        private BotSettings _botSettings = new();
        private BotState _botState = new();

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

            // Directory.CreateDirectory(Utils.ExePath + "/images/");

            InitializeComponent();
            BotVersionLabel.Content = Bot.Version;
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

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Process.GetCurrentProcess().Kill();
        }

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

                _botState = new BotState()
                {
                    MapName = Path.GetFileNameWithoutExtension(openFileDialog.FileName)
                };
                

                selectedMapPath = Path.GetDirectoryName(openFileDialog.FileName);
                MapItemCount.Content = _itemsToSpawn.Count;

                var timeSpan = TimeSpan.FromSeconds(_itemsToSpawn.Count * 4);

                EstimatedTime.Content = new DateTime(timeSpan.Ticks).ToString("HH:mm");
            }

            StartBotButton.Visibility = Visibility.Visible;

            this.InvalidateVisual();
        }

        private async void StartBot_OnClick(object sender, RoutedEventArgs e)
        {
            _botSettings.ItemStartIndex = int.Parse(ItemRangeStart.Text);
            _botSettings.ItemEndIndex = int.Parse(ItemRangeEnd.Text);
            _botSettings.Scale = float.Parse(Scale.Text);
            _botSettings.Offset = new Vector3(float.Parse(OffsetX.Text), float.Parse(OffsetY.Text),
                float.Parse(OffsetZ.Text));

            _botSettings.SaveFrequency = 100;
            
            try
            {
                if (_itemsToSpawn == null)
                {
                    ShowErrorPage("Either no map has been selected or the data is bad");
                    Log.Error("Selected map is null, select a map first");
                    return;
                }

                var saveAmount = int.Parse(SaveItemCount.Text);
                Log.Information("-----STARTING BOT-----");
                await Bot.StartBot(_itemsToSpawn, _botSettings, _botState);
                Log.Information("-----STOPPING BOT-----");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                ShowErrorPage(exception);
                throw;
            }
        }
    
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
             var process =  Process.GetCurrentProcess();
             process.Close();
        }
        /* private async void ResumeBot_OnClick(object sender, RoutedEventArgs e)
         {
             
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
                         
        }*/

        private void ShowErrorPage(Exception exception)
        {
            Error error = new Error(exception);
            error.ErrorTextBox.Text = exception.Message + Environment.NewLine + exception.StackTrace;
            error.Show();
        }

        private void ShowErrorPage(string exception)
        {
            Error error = new Error(exception);
            error.Show();
        }

        /* private async void LoadBlender_OnClick(object sender, RoutedEventArgs e)
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
 
             
 
             try
             {
                 
                 
                 var saveAmmount = int.Parse(SaveItemCount.Text);
                 await Bot.StartBot(items, itemStart: int.Parse(ItemRangeStart.Text),
                     itemEnd: int.Parse(ItemRangeEnd.Text), saveAmount: saveAmmount);
             }
             catch (Exception exception)
             {
                 ShowErrorPage(exception);
                 Log.Fatal("Exception: {ExceptionMessage} , StackTrace: {Trace}", exception.Message,
                     exception.StackTrace);
             }
         }
         */

        private static readonly DevUI DevWindow = new DevUI();

        private void EnterDev_OnClick(object sender, RoutedEventArgs e)
        {
            DevWindow.Show();
        }


        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Utils.HyperlinkRequestNavigate(sender, e);
        }
    }
}