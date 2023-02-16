using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Halo_Forge_Bot.Core;
using Halo_Forge_Bot.DataModels;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using InfiniteForgeConstants.ObjectSettings;
using Newtonsoft.Json;
using WindowsInput.Native;

namespace Halo_Forge_Bot.Windows.UserControls;

public partial class AdvancedBotMenuControl : UserControl
{
    public AdvancedBotMenuControl()
    {
        InitializeComponent();
    }

    private async void CollectItemData_OnClick(object sender, RoutedEventArgs e)
    {
        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess().Id);
        foreach (var category in ForgeObjectBrowser.Categories)
        {
            await NavigationHelper.NavigateToCategory(category.Value);
            foreach (var folder in category.Value.CategoryFolders)
            {
                await NavigationHelper.NavigateToFolder(folder.Value);

                //enters the current folder?
                await Task.Delay(100);
                Input.PressKey(VirtualKeyCode.RETURN);
                await Task.Delay(100);

                var expectedItemHover = 0;
                while (expectedItemHover == await MemoryHelper.GetGlobalHover())
                {
                    //spawn the item?
                    await Task.Delay(100);
                    Input.PressKey(VirtualKeyCode.RETURN);
                    await Task.Delay(100);


                    await NavigationHelper.MoveToTab(NavigationHelper.ContentBrowserTabs.ObjectProperties);
                    await Task.Delay(100);
                    await NavigationHelper.NavigateVertical(1);
                    await Task.Delay(100);
                    bool loop = true;
                    int retryCount = 0;

                    while (loop)
                    {
                        if (await MemoryHelper.GetGlobalHover() == 0)
                        {
                            await CollectData(isStatic: false);
                            loop = false;
                            break;
                        }

                        var id = GetSelectedId();
                        if (id is null)
                        {
                            loop = false;
                            break;
                        }

                        await Task.Delay(200);
                        Input.PressKey(VirtualKeyCode.VK_S);
                        await Task.Delay(200);
                        var currentPos = MemoryHelper.ReadItemPosition(MemoryHelper.GetItemCount() - 1);
                        Input.PressKey(VirtualKeyCode.VK_A);

                        var changedPos = MemoryHelper.ReadItemPosition(MemoryHelper.GetItemCount() - 1);
                        if (Math.Abs(changedPos.X - currentPos.X) > 0.001)
                        {
                            if (Math.Abs(changedPos.Y - currentPos.Y) < 0.001)
                            {
                                if (Math.Abs(changedPos.Z - currentPos.Z) < 0.001)
                                {
                                    // Found the x ui pos

                                    await CollectData(true);
                                    loop = false;
                                    break;
                                }
                            }
                        }


                        await Task.Delay(200);
                        Input.PressKey(VirtualKeyCode.VK_D);
                        await Task.Delay(200);


                        async Task CollectData(bool isStatic)
                        {
                            var ItemName = MemoryHelper.GetSelectedFullName();
                            ItemName = ItemName.Replace(" ", "_");
                            ItemName = ItemName.ToUpper();
                            ObjectId objectId = Enum.Parse<ObjectId>(ItemName);

                            var data = new ForgeObjectData(ItemName, objectId, isStatic,
                                await MemoryHelper.GetGlobalHover(),
                                category.Value.CategoryOrder, folder.Value.FolderOffset,
                                expectedItemHover);
                            ItemDB.AddItem(data);
                        }

                        ObjectId? GetSelectedId()
                        {
                            var ItemName = MemoryHelper.GetSelectedFullName();
                            ItemName = ItemName.Replace(" ", "_");
                            ItemName = ItemName.ToUpper();
                            var valid = Enum.TryParse<ObjectId>(ItemName, out var result);
                            if (valid)
                            {
                                return result;
                            }

                            return null;
                        }
                    }

                    await NavigationHelper.CloseUI();

                    await Task.Delay(100);
                    Input.PressKey(VirtualKeyCode.DELETE);
                    await Task.Delay(100);

                    await NavigationHelper.MoveToTab(NavigationHelper.ContentBrowserTabs.ObjectBrowser);
                    //move to next object in the folder
                    await Task.Delay(100);
                    Input.PressKey(VirtualKeyCode.VK_S);
                    await Task.Delay(100);
                    expectedItemHover++;
                }
            }
        }
    }

    class NameToID
    {
        public int ItemId;
        public string FileName;
    }

    class DataForBlender
    {
        public string? RenderModel;
        public string? ASG;
        public string? Mask;
        public int ObjectId;
        public string IdName;
    }

    string savePath = @"G:\_Projects\_Halo\HaloDesignSet\";
    string itemPath = @"Z:\Halo\ForgeObjectData\";
    private string unpackPath = @"Z:\Halo\Winter Retail Unpack__chore\";


    private void CollectSavedCategoryInfo_OnClick(object sender, RoutedEventArgs e)
    {
        List<DataForBlender> blenderData = new();

        Bot.BuildUiLayout();
        var json = Utils.ExePath + "/Core/FileNamesToID.json";
        var ids = JsonConvert.DeserializeObject<List<NameToID>>(File.ReadAllText(json));
        int renderModelFailCount = 0;
        int textureFailCount = 0;
        int dataFileFailCount = 0;
        int itemCount = 0;
        foreach (var folder in ForgeObjectBrowser.Categories["Halo_Design_Set"].CategoryFolders)
        {
            foreach (var item in folder.Value.FolderObjects)
            {
                if (item.Value.ParentFolder.FolderName.Contains("MP"))
                {
                    continue;
                }
                
                itemCount++;
                var nameToIds = ids.First(x => x.ItemId == (int)item.Value.ObjectId);
                if (nameToIds is null)
                {
                    dataFileFailCount++;
                    break;
                }

                var fileInfo = new FileInfo(savePath + folder.Key + "/" +
                                            Path.GetFileNameWithoutExtension(nameToIds.FileName) + "/" +
                                            nameToIds.FileName);

                Directory.CreateDirectory(fileInfo.Directory.FullName);


                File.Copy(itemPath + nameToIds.FileName, fileInfo.FullName, true);

                File.WriteAllText(fileInfo.Directory + "/ForgeObjectID.txt", nameToIds.ItemId.ToString());

                PowerShell shell = PowerShell.Create();
                shell.Commands.AddCommand($"strings").AddArgument(fileInfo.FullName);
                var output = shell.Invoke();


                List<string> strings = new();
                foreach (var s in output)
                {
                    strings.Add(s.ToString());
                }

                string matBasePath = "/pc__/";
                string modBasePath = "/gen__/";
                string relativePath = "";

                for (int i = 0; i < strings.Count; i++)
                {
                    if (strings[i].StartsWith("__"))
                    {
                        relativePath = strings[i - 1];
                        break;
                    }
                }

                var modelPathInfo = new FileInfo(unpackPath + modBasePath + relativePath);
                var materialPathInfo = new FileInfo(unpackPath + matBasePath + relativePath);
                var materialDirectory = materialPathInfo.Directory;
                var modelDirectory = modelPathInfo.Directory;


                if (!modelDirectory.Exists)
                {
                    renderModelFailCount++;
                    continue;
                }

                var modelPaths = modelDirectory.GetFiles("*", SearchOption.AllDirectories);
                foreach (var model in modelPaths)
                {
                    foreach (var file in model.Directory.GetFiles("*", searchOption: SearchOption.AllDirectories))
                    {
                        File.Copy(file.FullName, $"{fileInfo.Directory}/{Path.GetFileName(file.FullName)}", true);
                    }
                }

                if (!materialDirectory.Exists)
                {
                    textureFailCount++;
                    continue;
                }


                var bitmapFiles = materialDirectory.GetFiles("*", SearchOption.AllDirectories);

                foreach (var bitmap in bitmapFiles)
                {
                    foreach (var file in bitmap.Directory.GetFiles("*", searchOption: SearchOption.AllDirectories))
                    {
                        File.Copy(file.FullName, $"{fileInfo.Directory}/{Path.GetFileName(file.FullName)}", true);
                    }
                }
                // Process process = new();
                // ProcessStartInfo startInfo = new()
                // {
                //     WindowStyle = ProcessWindowStyle.Normal,
                //     FileName = "cmd.exe", 
                //     Arguments = $"strings \"{fileInfo.FullName}\"",
                //     RedirectStandardOutput = true,
                //     RedirectStandardError = true,
                //     UseShellExecute = false
                // };
                // startInfo.WorkingDirectory = "Z:\\";
                // process.StartInfo = startInfo;
                // process.Start();
                // var s = process.StandardOutput.ReadToEnd();
                // var ss = process.StandardError.ReadToEnd();
                // process.WaitForExit();


                var asg = fileInfo.Directory.GetFiles()
                    .FirstOrDefault(x => Path.GetExtension(x.Name) == ".bitmap" && x.Name.Contains("_asg_"));
                var mask = fileInfo.Directory.GetFiles()
                    .FirstOrDefault(x => Path.GetExtension(x.Name) == ".bitmap" && x.Name.Contains("_mask_"));

                var renderModel = fileInfo.Directory.GetFiles()
                    .FirstOrDefault(x => Path.GetExtension(x.Name) == ".render_model");


                blenderData.Add(new DataForBlender()
                {
                    ASG = asg?.FullName,
                    Mask = mask?.FullName,
                    RenderModel = renderModel?.FullName,
                    ObjectId = (int)item.Value.ObjectId,
                    IdName = item.Value.ObjectId.ToString()
                });
            }
        }

        var blendJson = JsonConvert.SerializeObject(blenderData);

        File.WriteAllText(savePath + "BlenderImportData.json", blendJson);
    }
}