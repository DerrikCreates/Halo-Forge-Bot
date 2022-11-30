using System;
using System.Collections.Generic;
using System.Threading;
using Halo_Forge_Bot.GameUI;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
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
        ForgeObjectBrowser.FindItem(ObjectId.UNSC_SATELLITE_DISH_PORTABLE, out randomObjectToFind);
        await NavigationHelper.SpawnItem(randomObjectToFind);
        await Input.KeyPress(VirtualKeyCode.DELETE, 500, 500);
        
        Thread.Sleep(1000);
        await NavigationHelper.HomeObjectBrowserToCategoryLevel();
        
        ForgeObjectBrowser.FindItem(ObjectId.RECYCLE_BIN, out randomObjectToFind);
        await NavigationHelper.SpawnItem(randomObjectToFind);
        await Input.KeyPress(VirtualKeyCode.DELETE, 500, 500);
        
        Thread.Sleep(1000);

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

        List<ForgeUIObject> allItems = new List<ForgeUIObject>();

        foreach (var category in ForgeObjectBrowser.Categories.Values)
        {
            foreach (var folder in category.CategoryFolders.Values)
            {
                foreach (var item in folder.FolderObjects.Values)
                {
                    allItems.Add(item);
                }
            }
        }

        Random rnd = new Random();
        while (true)
        {
            await NavigationHelper.SpawnItem(allItems[rnd.Next(allItems.Count)]);
            await Input.KeyPress(VirtualKeyCode.DELETE, 500, 500);
        }
    }
}