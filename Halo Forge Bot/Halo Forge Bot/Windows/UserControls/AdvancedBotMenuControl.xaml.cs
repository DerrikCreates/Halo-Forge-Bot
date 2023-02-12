using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Halo_Forge_Bot.Core;
using Halo_Forge_Bot.DataModels;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using InfiniteForgeConstants.ObjectSettings;
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
}