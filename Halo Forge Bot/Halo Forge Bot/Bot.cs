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
using Halo_Forge_Bot.DataModels;
using Halo_Forge_Bot.GameUI;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
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
        BuildUiLayout();

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

                while (MemoryHelper.GetMenusVisible() == 0)
                {
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_R);
                    await Task.Delay(60);
                }


                while (MemoryHelper.GetTopBrowserHover() != 1)
                {
                    await Task.Delay(60);
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_E);
                    await Task.Delay(60);
                }

                var defaultScale = MemoryHelper.GetSelectedScale();

                var xScale = MathF.Round(itemSchema.SettingsContainer.Scale.First().ScaleContainer.X, 3);
                var yScale = MathF.Round(itemSchema.SettingsContainer.Scale.First().ScaleContainer.Y, 3);
                var zScale = MathF.Round(itemSchema.SettingsContainer.Scale.First().ScaleContainer.Z, 3);

                var itemScale = new Vector3(xScale, yScale, zScale);

                var realScale = (itemScale * defaultScale); //Vector3.Multiply(itemScale, defaultScale) * 10;

                await SetProp(realScale.X.ToString(), StaticByDefault.Layout[PropertyName.SizeX]);
                await SetProp(realScale.Y.ToString(), StaticByDefault.Layout[PropertyName.SizeY]);
                await SetProp(realScale.Z.ToString(), StaticByDefault.Layout[PropertyName.SizeZ]);


                var xPos = MathF.Round(itemSchema.Position.X * 10, 3);
                var yPos = MathF.Round(itemSchema.Position.Y * 10, 3);
                var zPos = MathF.Round(itemSchema.Position.Z * 10, 3);

                await SetProp(xPos.ToString(), StaticByDefault.Layout[PropertyName.Forward]);
                await SetProp(yPos.ToString(), StaticByDefault.Layout[PropertyName.Horizontal]);
                await SetProp(zPos.ToString(), StaticByDefault.Layout[PropertyName.Vertical]);

                var yForward = MathF.Round(itemSchema.Forward.Y, 3);
                var zForward = MathF.Round(itemSchema.Forward.Z, 3);
                var xForward = MathF.Round(itemSchema.Forward.X, 3);

                var yUp = MathF.Round(itemSchema.Up.Y, 3);
                var xUp = MathF.Round(itemSchema.Up.X, 3);
                var zUp = MathF.Round(itemSchema.Up.Z, 3);

                var rotation =
                    Utils.DidFishSaveTheDay(new Vector3(xForward, yForward, zForward), new Vector3(xUp, yUp, zUp));


                await SetProp(rotation.Z.ToString(), StaticByDefault.Layout[PropertyName.Roll]);
                await SetProp(rotation.X.ToString(), StaticByDefault.Layout[PropertyName.Pitch], VirtualKeyCode.VK_W);
                await SetProp(rotation.Y.ToString(), StaticByDefault.Layout[PropertyName.Yaw], VirtualKeyCode.VK_W);
                await Task.Delay(50);


                while (MemoryHelper.GetGlobalHover() != StaticByDefault.Layout[PropertyName.SizeX])
                {
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
                }

                while (MemoryHelper.GetTopBrowserHover() != 0)
                {
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_Q);
                    await Task.Delay(33);
                }


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

    private static async Task SetProp(string data, int index, VirtualKeyCode key = VirtualKeyCode.VK_S)
    {
        if (data == "")
        {
            data = "0";
        }

        while (MemoryHelper.GetGlobalHover() != index)
        {
            Input.Simulate.Keyboard.KeyPress(key);
            //MemoryHelper.SetGlobalHover(index);
            await Task.Delay(33);
        }


        //          var selectedPosition = MemoryHelper.GetSelectedPosition();

        while (MemoryHelper.GetEditMenuState() == 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(100);
        }


        while (await ClipboardService.GetTextAsync() != data)
        {
            await ClipboardService.SetTextAsync(data);
        }


        while (MemoryHelper.GetEditBoxText() != data)
        {
            await Task.Delay(50);
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);

            /*
            var toType = data.ToCharArray();
            foreach (var c in toType)
            {
                SendKeys.SendWait(c.ToString());
                await Task.Delay(33);
            }
            */
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            await Task.Delay(50);
        }

        while (MemoryHelper.GetEditMenuState() != 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(50);
        }
    }


    public static void BuildUiLayout()
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
        var data = File.ReadAllLines(strWorkPath + "/config/ForgeObjects.txt");

        List<string> rootFolder = new();
        List<string> subFolder = new();
        List<string> itemName = new();
        List<string> itemID = new();
        foreach (var line in data)
        {
            var sections = line.Split(new char[] { '>', ':' });
            rootFolder.Add(sections[0].Trim());
            subFolder.Add(sections[1].Trim());
            itemName.Add(sections[2].Trim());
            itemID.Add(sections[3].Trim());
        }


        string lastRoot = rootFolder[0];
        string lastSub = subFolder[0];

        // This is the most cursed thing ive ever wrote. I just wanted finished
        for (int x = 0; x < itemName.Count; x++)
        {
            var rootFolderName = FixCapatial(rootFolder[x].ToLower());
            var subFolderName = FixCapatial(subFolder[x].ToLower());
            var itemNameName = Enum.GetName((ObjectId)int.Parse(itemID[x]));


            subFolderName = subFolderName.Replace("Mp", "MP");
            subFolderName = subFolderName.Replace(" ", "_");
            subFolderName = subFolderName.Replace("Bazzar", "Bazaar");
            subFolderName = subFolderName.Replace("Missles", "Missiles");
            subFolderName = subFolderName.Replace("Unsc", "UNSC");
            subFolderName = subFolderName.Replace("-", "");
            subFolderName = subFolderName.Replace("__", "_");
            rootFolderName = rootFolderName.Replace("Fx", "FX");
            subFolderName = subFolderName.Replace("_/", "");
            rootFolderName = rootFolderName.Replace(" ", "_");

            if (rootFolderName == "Halo_Design_Set")
            {
                subFolderName = subFolderName.Replace("Crate", "Crates");
            }
            // Handle the first letter in the string.

            if (rootFolderName == "Z_null")
            {
                break;
            }

            if (itemNameName == null)
            {
            }

            ForgeObjectBrowser.Categories[rootFolderName].CategoryFolders[subFolderName]
                .AddItem(itemNameName ??= itemName[x], Enum.Parse<ObjectId>(itemID[x]));
        }

        string FixCapatial(string value)
        {
            var array = value.ToCharArray();
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }

            for (int x = 1; x < array.Length; x++)
            {
                if (array[x - 1] == ' ')
                {
                    if (char.IsLower(array[x]))
                    {
                        array[x] = char.ToUpper(array[x]);
                    }
                }
            }

            return new string(array);
        }

        JsonSerializerSettings s = new JsonSerializerSettings();
        s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        var cp = ForgeObjectBrowser.Categories["Accents"].CategoryFolders["City_Props"];
        var cpMP = ForgeObjectBrowser.Categories["Accents"].CategoryFolders["City_Props_MP"];

        ForgeObjectBrowser.Categories["Accents"].CategoryFolders["City_Props"] = cpMP;
        ForgeObjectBrowser.Categories["Accents"].CategoryFolders["City_Props_MP"] = cp;
        var a = JsonConvert.SerializeObject(ForgeObjectBrowser.Categories, s);
        // File.WriteAllText("z:/josh/ItemData.json", a);
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