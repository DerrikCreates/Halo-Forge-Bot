using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BondReader;
using BondReader.Schemas;
using Halo_Forge_Bot.Core;
using Halo_Forge_Bot.Utilities;
using HtmlAgilityPack;
using InfiniteForgeConstants.MapSettings;
using InfiniteForgeConstants.ObjectSettings;

namespace Halo_Forge_Bot.Windows;

public partial class DevUI : Window
{
    public DevUI()
    {
        InitializeComponent();
    }


    private async void DownloadMvarButton_OnClick(object sender, RoutedEventArgs e)
    {
        await Utils.DownloadMvar(FileShareLinkTextBox.Text, "");
    }


    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess().Id);
        var i = MemoryHelper.GetItemCount();


        // MemoryHelper.SetItemScale(0, Vector3.One * 0.1f);
        // MemoryHelper.SetItemScale(1, Vector3.One * 0.1f);
        //  MemoryHelper.SetItemScale(2, Vector3.One * 0.1f);

        var pos = new Vector3(1, 0, 150);
        for (int j = 0; j < 9; j++)
        {
            pos.X += 10;
            MemoryHelper.SetItemPosition(j, pos);
        }


        // var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
        // Error errorWindow = new Error();
        // errorWindow.ErrorTextBox.Text = "TESTSETSTSTESTT THIS IS A TEST";
        // errorWindow.Show();

        // await Task.Run(Bot.DevTesting);
    }


    private void UpdateMem_OnClick(object sender, RoutedEventArgs e)
    {
        MemoryTestUI.Text = MemoryHelper.GetEditBoxText();
    }

    bool keepUpdating = true;


    private Task UpdateMemoryUI(string address, string length)
    {
        if (int.TryParse(length, out int result))
        {
            while (keepUpdating)
            {
                var data = MemoryHelper.Memory.ReadBytes(address, result);
                Dispatcher.Invoke(() => { return DebugMemoryLabel.Text = Convert.ToHexString(data); });

                Thread.Sleep(10);
            }
        }

        return Task.CompletedTask;
    }

    private void OnupdateMemoryUI(string s)
    {
        DebugMemoryLabel.Text = s;
    }

    private void ReadAddressButton_OnClick(object sender, RoutedEventArgs e)
    {
        var address = DebugMemoryAddressTextBox.Text;
        var length = DebugMemoryAddressLengthTextBox.Text;
        Task.Run(() => UpdateMemoryUI(address, length));
    }

    private void AttachToHalo_OnClick(object sender, RoutedEventArgs e)
    {
        MemoryHelper.Memory.OpenProcess(ForgeUI.HaloProcess.Id);
    }

    private void GetHaloObjects_OnClick(object sender, RoutedEventArgs e)
    {
        Dev.GetAllObjectTypeData();
    }

    private void GetHaloObjects2_OnClick(object sender, RoutedEventArgs e)
    {
        Dev.GetAllObjectTypeData2();
    }

    private void ResetState_OnClick(object sender, RoutedEventArgs e)
    {
        NavigationHelper.ResetNavigationState();
    }

    private async void DownloadAllMvars_OnClick(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < 316; i++)
        {
            Task.Run(() => DownloadMvar(i));
            Thread.Sleep(1000);
        }
    }

    private async static Task DownloadMvar(int pageNumber)
    {
        var web = new HtmlWeb();
        HttpClient client = new();
        var url = @$"https://www.halowaypoint.com/halo-infinite/ugc/browse?assetKind=Map&page={pageNumber}";
        var doc = web.Load(url);
        var images = doc.DocumentNode.Descendants("img");

        foreach (var image in images)
        {
            if (image.HasClass("ugc-content-card_contentImage__C_QzH"))
            {
                var mapUrl = image.Attributes["src"].Value;

                mapUrl = mapUrl.Replace("/images/thumbnail.jpg", "/map.mvar");
                var mapName = image.Attributes["alt"].Value;
                mapName = mapName.Substring(7, mapName.Length - 7);


                var request = new HttpRequestMessage(HttpMethod.Get, mapUrl);
                var response = await client.SendAsync(request);
                var map = await response.Content.ReadAsByteArrayAsync();


                await File.WriteAllBytesAsync(
                    $"Z:/maps/HaloInfinite/{MakeValidFileName(HtmlEntity.DeEntitize(mapName))}.mvar", map);
            }
        }
    }

    private static string MakeValidFileName(string name)
    {
        string invalidChars =
            System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }

    private void GenerateStats_OnClick(object sender, RoutedEventArgs e)
    {
        var canvasPath = @"Z:\maps\canvasIds";

        var f = Directory.GetFiles(canvasPath);

        List<string> mapIds = new();
        foreach (var fi in f)
        {
           var m = BondHelper.ProcessFile<BondSchema>(fi);
           FileInfo info = new FileInfo(fi);
           mapIds.Add($"{info.Name}: {m.MapIdContainer.MapId.Int}"); 
           
        }


        
        var joinedNames = mapIds.Aggregate((a, b) => a + ", " + b);
        
        new Error(joinedNames).Show();
        

        int failedItems = 0;
        Dictionary<ObjectId, int> itemCount = new();
        Dictionary<MapId, int> variantCount = new();

        Dictionary<string, int> totalItemCountPerFile = new();
        Dictionary<string, long> fileSize = new();
        var files = Directory.GetFiles(@"Z:\maps\HaloInfinite");

        foreach (var file in files)
        {
            BondSchema? map;
            try
            {
                map = BondHelper.ProcessFile<BondSchema>(file);
            }
            catch (Exception exception)
            {
                failedItems++;
                continue;
            }


            if (variantCount.ContainsKey((MapId)map.MapIdContainer.MapId.Int))
            {
                variantCount[(MapId)map.MapIdContainer.MapId.Int] += 1;
            }
            else
            {
                variantCount.Add((MapId)map.MapIdContainer.MapId.Int, 1);
            }

            totalItemCountPerFile.Add(file, map.Items.Count);

            FileInfo fileInfo = new FileInfo(file);
            fileSize.Add(file, fileInfo.Length);

            foreach (var item in map.Items)
            {
                if (itemCount.ContainsKey((ObjectId)item.ItemId.Int) == false)
                {
                    itemCount.Add((ObjectId)item.ItemId.Int, 1);
                    continue;
                }


                ObjectId id = (ObjectId)item.ItemId.Int;
                itemCount[id] += 1;
            }
        }


        var sortedItemCount = itemCount.OrderByDescending(x => x.Value).ToList();
        var sortedVariantCount = variantCount.OrderByDescending(x => x.Value).ToList();
        //var sortedItemCountPerFile = totalItemCountPerFile.OrderByDescending(x => x.Value).ToList();
        var sortedTotalItemCountPerFile = totalItemCountPerFile.OrderByDescending(x => x.Value).ToList();
        var sortedFileSize = fileSize.OrderByDescending(x => x.Value).ToList();
        List<string> output = new();
        foreach (var item in sortedItemCount)
        {
            output.Add($"{item.Key.ToString()}: {item.Value}");
        }

        File.WriteAllLines(@"Z:\maps\MostUsedItems.txt", output);
        output = new();
        foreach (var vC in sortedVariantCount)
        {
            output.Add($"{vC.Key.ToString()}: {vC.Value} ");
        }

        File.WriteAllLines(@"Z:\maps\MostUsedMap.txt", output);
        output = new();

        foreach (var v in sortedTotalItemCountPerFile)
        {
            output.Add($"{v.Key.ToString()}: {v.Value} ");
        }

        File.WriteAllLines(@"Z:\maps\LargestMapByItems.txt", output);
        output = new();
        foreach (var v in sortedFileSize)
        {
            output.Add($"{v.Key.ToString()}: {v.Value} ");
        }

        File.WriteAllLines(@"Z:\maps\LargestMapByFileSize.txt", output);
        output = new();


        new Error($"There where {failedItems} failed items!").Show();
    }
}