using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Threading;
using BondReader;
using BondReader.Schemas;
using BondReader.Schemas.Items;
using Halo_Forge_Bot.config;
using Halo_Forge_Bot.DataModels;
using Halo_Forge_Bot.GameUI;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using InfiniteForgeConstants.Forge_UI.Object_Properties;
using InfiniteForgeConstants.ObjectSettings;
using Memory;
using Newtonsoft.Json;
using Serilog;
using TextCopy;
using WindowsInput.Native;
using Clipboard = TextCopy.Clipboard;


namespace Halo_Forge_Bot;

public static class Bot
{
    public static async Task StartBot(BondSchema map, int itemStart = 0, int itemEnd = 0)
    {
        int currentSkipCount = 0;
        // LoadItemData();
        BuildUILayout();

        var splitItemList = map.Items.ToList();
        if (itemEnd == 0)
        {
            itemEnd = splitItemList.Count;
        }

        splitItemList = splitItemList.GetRange(itemStart, itemEnd - itemStart);
        Dictionary<ObjectId, List<ItemSchema>> items = new();
        foreach (var itemSchema in splitItemList)
        {
            var id = (ObjectId)itemSchema.ItemId.Int;

            if (itemSchema.StaticDynamicFlagUnknown != 21)
            {
                Log.Warning(
                    "Item with id: {ItemID} is dynamic, we currently only support static items, skipping this item",
                    id);
                continue;
            }

            if (items.ContainsKey(id))
            {
                items[id].Add(itemSchema);
                continue;
            }

            items.Add(id, new List<ItemSchema>());
            items[id].Add(itemSchema);
        }

        ForgeUI.SetHaloProcess();
        int itemCountID = 0;
        int saveCount = 0;

        foreach (var item in items)
        {
            while (MemoryHelper.GetGlobalHover() != 0)
            {
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
            }

            await Task.Delay(200);

            while (MemoryHelper.GetTopBrowserHover() != 0) // set item browser to active menu
            {
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_E);
                await Task.Delay(10);
            }

            var currentObjectId = item.Key;

            ForgeObjectBrowser.FindItem(currentObjectId, out ForgeUIObject? mapitem);

            if (mapitem == null)
            {
                Log.Warning("Skipping null item, MapId: {id}, Name: {name} ", Enum.GetName(currentObjectId));
                continue;
            }
            //navigate to item with memory checks

            while (MemoryHelper.GetGlobalHover() !=
                   mapitem.ParentFolder.ParentCategory.CategoryOrder - 1) //Set cursor to correct cat
            {
                Log.Debug("Move To Cat currentHover:{Hover} , Required Hover: {Reqired}",
                    MemoryHelper.GetGlobalHover(), mapitem.ParentFolder.ParentCategory.CategoryOrder - 1);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_S);

                await Task.Delay(33);
            }

            await Task.Delay(200);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN); // open the cat
            await Task.Delay(200);


            while (MemoryHelper.GetGlobalHover() !=
                   mapitem.ParentFolder.ParentCategory.CategoryOrder +
                   mapitem.ParentFolder.FolderOffset - 1) // move cursor to sub cat
            {
                Log.Debug("Move To subCat currentHover:{Hover} , Required Hover: {Reqired}",
                    MemoryHelper.GetGlobalHover(),
                    mapitem.ParentFolder.ParentCategory.CategoryOrder +
                    mapitem.ParentFolder.FolderOffset - 1);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_S);

                await Task.Delay(33);
            }

            await Task.Delay(200);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN); // enter the sub cat
            await Task.Delay(200);


            while (MemoryHelper.GetGlobalHover() != mapitem.ObjectOrder - 1) // hover item
            {
                Input.PressKey(VirtualKeyCode.VK_S);
                await Task.Delay(33);
            }

            
            foreach (var itemSchema in item.Value)
            {
                /*

                if (itemCountID >= itemToStopAt)
                {
                    Log.Information("--STOPPING THE BOT-- ITEM COUNT LIMIT REACHED");
                    return;
                }

*/
                saveCount++;
                await Task.Delay(200);
                while (MemoryHelper.GetMenusVisible() == 1)
                {
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN); // spawn the item?
                    await Task.Delay(200);
                }

                await Task.Delay(200);

                if (saveCount == 10)
                {
                    await Task.Delay(100);
                    Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                    saveCount = 0;
                    await Task.Delay(100);
                }

                /*
                while (MemoryHelper.GetEditMenuState() == 0)
                {
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.F2);
                }

                await Task.Delay(75);

                var newName = $"{Enum.GetName((ObjectId)itemSchema.ItemId.Int)}--{itemCountID}--";
                await ClipboardService.SetTextAsync(newName);
                while (await ClipboardService.GetTextAsync() != newName)
                {
                    await ClipboardService.SetTextAsync(newName);
                }

                await Task.Delay(10);
                while (MemoryHelper.GetEditBoxText() != newName)
                {
                    Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
                    await Task.Delay(10);
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.BACK);
                    await Task.Delay(10);
                    Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                    await Task.Delay(10);
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                }


                while (MemoryHelper.GetMenusVisible() != 0)
                {
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    await Task.Delay(10);
                }
                */

                await PropertyHelper.SetMainProperties(itemSchema);


                itemCountID++;
            }

            await Task.Delay(75);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.BACK);
            await Task.Delay(100);

            while (MemoryHelper.GetGlobalHover() != mapitem.ParentFolder.ParentCategory.CategoryOrder - 1)
            {
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
                await Task.Delay(33);
            }

            await Task.Delay(100);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(500);
            // Input.Simulate.Keyboard

            //MemoryHelper.SetGlobalHover(UIItems[currentObjectId].index);
            //  MemoryHelper.SetBrowserScroll(UIItems[currentObjectId].index);
        }
    }

    public static void BuildUILayout()
    {
        //Your cursed stuff is in here if you still need it, manually fixed the part of the file.
        //ConformForgeObjects.BuildUiLayout();
        
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
        var data = File.ReadAllLines(strWorkPath + "/config/ForgeObjects.txt");

        foreach (var line in data)
        {
            var objectData = line.Split(":");
            if (objectData[0] == "Z_NULL" || objectData[0] == "Z_UNUSED") continue;
            
            ForgeObjectBrowser.AddItem(ConformForgeObjects.FixCapital(objectData[0].ToLower()),
                ConformForgeObjects.FixCapital(objectData[1].ToLower()),
                Enum.GetName((ObjectId)int.Parse(objectData[3])) ?? objectData[2], Enum.Parse<ObjectId>(objectData[3]));
        }
    }


    public static ForgeUIObject? GetItemByID(ObjectId id)
    {
        foreach (var category in ForgeObjectBrowser.Categories)
        {
            foreach (var subFolder in category.Value.CategoryFolders)
            {
                foreach (var obj in subFolder.Value.FolderObjects)
                {
                    if (obj.Value.ObjectId == id)
                    {
                        return obj.Value;
                    }
                }
            }
        }

        return null;
    }
}