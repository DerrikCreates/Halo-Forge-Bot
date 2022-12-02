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
    }

    public static async void GetAllObjectTypeData()
    {
        Bot.BuildUILayout();
        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess()
            .Id);
        
        Thread.Sleep(1000);
        
        Vector3 currentPosition = Vector3.One with {X = -1, Y = -1} * 500;

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
                    if (skipItems-- <= 0)
                    {
                        await Input.KeyPress(VirtualKeyCode.RETURN, 100, 50);
                        await NavigationHelper.OpenUI(NavigationHelper.ContentBrowserTabs.ObjectProperties);
                        await NavigationHelper.ReturnToTop();
                        await Task.Delay(100);

                        var objectMode = ForgeUI.GetDefaultObjectMode(1);
                        var objectName = await PropertyHelper.GetProperty(
                            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.ObjectName, objectMode));
                        var selectedScale = MemoryHelper.GetSelectedScale();

                        sr.WriteLine(
                            $"{category.CategoryName}:{folder.FolderName}:{objectName}:{objectMode}:{selectedScale.X},{selectedScale.Y},{selectedScale.Z}:");
                        await Input.KeyPress(VirtualKeyCode.ESCAPE, 100, 50);
                        sr.Flush();
                        
                        //Scale object to be within a 50x50x50 grid
                        if (selectedScale.X > 200 || selectedScale.Y > 200 || selectedScale.Z > 200)
                        {
                            await PropertyHelper.SetScaleProperty(Vector3.One / 8, objectMode);
                        }
                        else if (selectedScale.X > 100 || selectedScale.Y > 100 || selectedScale.Z > 100)
                        {
                            await PropertyHelper.SetScaleProperty(Vector3.One / 4, objectMode);
                        }
                        else if (selectedScale.X > 50 || selectedScale.Y > 50 || selectedScale.Z > 50)
                        {
                            await PropertyHelper.SetScaleProperty(Vector3.One / 2, objectMode);
                        }

                        await PropertyHelper.SetPositionProperty(currentPosition, objectMode);
                        await NavigationHelper.MoveToTab(NavigationHelper.ContentBrowserTabs.ObjectBrowser);
                    }
                    
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

                    await NavigationHelper.NavigateVerticalOneStep(false);
                    await Input.HandlePause();
                } 
                while (await MemoryHelper.GetGlobalHoverVerbose() != 0);
                
                if (skipItems <= 0 && skipFolders <= 0)
                {
                    await NavigationHelper.CloseUI();
                    await Task.Delay(100);
                    Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                    await Task.Delay(100);
                    Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                    await Task.Delay(100);
                    Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                }
            }
        }
        
        sr.Close();
    }
}