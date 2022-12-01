using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Halo_Forge_Bot.GameUI;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using InfiniteForgeConstants.Forge_UI.Object_Browser.Folders.Accents;
using InfiniteForgeConstants.Forge_UI.Object_Browser.Folders.Biomes;
using InfiniteForgeConstants.Forge_UI.Object_Browser.Folders.Props;
using InfiniteForgeConstants.Forge_UI.Object_Properties;
using InfiniteForgeConstants.ObjectSettings;
using WindowsInput.Native;

namespace Halo_Forge_Bot.Windows;

public static class Dev
{
    public static async void GetAllObjectTypeData2()
    {        
        Bot.BuildUILayout();
        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess()
            .Id);
        
        ForgeUIObject randomObjectToFind;
        ForgeObjectBrowser.FindItem(ObjectId.TO_GO_CUP_OPEN, out randomObjectToFind);
        await NavigationHelper.NavigateToItem(randomObjectToFind);
        // await Input.KeyPress(VirtualKeyCode.DELETE, 500, 500);
        
        Thread.Sleep(1000);
        await NavigationHelper.HomeObjectBrowserToFolderLevel();
        
        ForgeObjectBrowser.FindItem(ObjectId.RECYCLE_BIN, out randomObjectToFind);
        await NavigationHelper.NavigateToItem(randomObjectToFind);
        // await Input.KeyPress(VirtualKeyCode.DELETE, 500, 500);
        //
        // Thread.Sleep(1000);

        // ForgeObjectBrowser.FindItem(ObjectId.WHEEL_MP, out randomObjectToFind);
        // await NavigationHelper.SpawnItem(randomObjectToFind);
        //
        // ForgeObjectBrowser.FindItem(ObjectId.CABLE_CAP, out randomObjectToFind);
        // await NavigationHelper.SpawnItem(randomObjectToFind);
    }

    public static async void GetAllObjectTypeData()
    {
        Bot.BuildUILayout();
        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess()
            .Id);
        
        Thread.Sleep(1000);
        
        Vector3 currentPosition = (Vector3.One with {X = -1, Y = -1}) * 500;

        var sr = new StreamWriter("./config/AllForgeObjects.txt");

        int skipFolders = 0;
        int skipItems = 0;
        
        foreach (var category in ForgeObjectBrowser.Categories.Values)
        {
            await NavigationHelper.NavigateToCategory(category);
            foreach (var folder in category.CategoryFolders.Values)
            {
                if (skipFolders-- > 0)
                {
                    continue;
                }
                
                await NavigationHelper.NavigateToFolder(folder);
                await NavigationHelper.NavigateVertical(1);
                await NavigationHelper.ReturnToTop();
                
                do
                {
                    while (Input.RequestedPaused)
                    {
                        await Task.Delay(100);
                    }
                    
                    if (skipItems-- > 0)
                    {
                        await Input.KeyPress(VirtualKeyCode.VK_S, 50, 50);
                        await Task.Delay(200);
                        continue;
                    }
                    
                    await Input.KeyPress(VirtualKeyCode.RETURN, 200, 100);
                    await NavigationHelper.OpenUI(NavigationHelper.ContentBrowserTabs.ObjectProperties);
                    await NavigationHelper.ReturnToTop();
                    await Task.Delay(100);
                    
                    await NavigationHelper.ReturnToTop();
                    await Task.Delay(100);
                    
                    await NavigationHelper.ReturnToTop();

                    await Task.Delay(500);
                    
                    var objectMode = ForgeUI.GetDefaultObjectMode(1);
                    var objectName = await PropertyHelper.GetProperty(ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.ObjectName, objectMode));

                    sr.WriteLine($"{category.CategoryName}:{folder.FolderName}:{objectName}:{objectMode}:");
                    await Input.KeyPress(VirtualKeyCode.ESCAPE, 200, 200);
                    sr.Flush();

                    var selectedScale = MemoryHelper.GetSelectedScale();
                    if (selectedScale.X > 100 || selectedScale.Y > 100 || selectedScale.Z > 100)
                    {
                        await PropertyHelper.SetScaleProperty(Vector3.One / 4, objectMode);
                    }
                    else if (selectedScale.X > 50 || selectedScale.Y > 50 || selectedScale.Z > 50)
                    {
                        await PropertyHelper.SetScaleProperty(Vector3.One / 2, objectMode);
                    }

                    await PropertyHelper.SetPositionProperty(currentPosition, objectMode);

                    currentPosition.X += 50;
                    if (currentPosition.X > 1500)
                    {
                        currentPosition.X = -1500;
                        currentPosition.Y += 50;

                        if (currentPosition.Y > 1500)
                        {
                            currentPosition.Z += 50;
                            currentPosition.Y = -1500;
                        }
                    }
                    
                    await NavigationHelper.MoveToTab(NavigationHelper.ContentBrowserTabs.ObjectBrowser);
                    await Input.KeyPress(VirtualKeyCode.VK_S, 50, 50);
                    await Task.Delay(200);
                } 
                while (MemoryHelper.GetGlobalHover() != 0);

                await NavigationHelper.CloseUI();
                await Task.Delay(100);
                Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                await Task.Delay(100);
                Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                await Task.Delay(100);
                Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                await Task.Delay(100);
                await Task.Delay(10000);
            }
        }
        
        sr.Close();
    }
}