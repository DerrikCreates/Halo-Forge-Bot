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
using Newtonsoft.Json.Linq;
using Serilog;
using TextCopy;
using WindowsInput.Native;
using Clipboard = TextCopy.Clipboard;


namespace Halo_Forge_Bot;

public static class Bot
{
    public static int travelSleep = 10;

    public static int WhenToSave = 15;
    //todo extract all NON BOT LOGIC for start bot, it should only be for starting / ending the bot
    public static async Task StartBot(List<ForgeItem> map, int itemStart = 0, int itemEnd = 0,
        bool resumeFromLast = false , bool isBlender = false)
    {
        //todo create a class for both blender and .mvar files, maybe use the blender file json
        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess()
            .Id); // todo add checks to the ui to stop the starting of the bot without halo being open / crash detection

        int startIndex = itemStart;
        Dictionary<ObjectId, List<MapItem>> items = new();

        // LoadItemData();
        BuildUILayout();

        if (resumeFromLast)
        {
            var recoveryValues = GetRecoveryFiles();
            if (recoveryValues == null)
            {
                throw new Exception("No recovery data files found.");
            }

            items = recoveryValues.Item2;
            startIndex = recoveryValues.Item1;
        }
        else
        {
            var splitItemList = new List<ForgeItem>(); // item list of the items to process
            var tempArray =
                map.OrderBy(item => item.ItemId)
                    .ToList(); // temp to an array to i know now for sure its in the correct order. might be unnecessary 
            if (itemEnd == 0)
            {
                itemEnd = tempArray.Count();
            }

            int index = 0;


            for (int i = itemStart; i < itemEnd; i++) // extracting the requested items from the map. 
            {
                splitItemList.Add(tempArray[i]);
            }

            foreach (var itemSchema in splitItemList)
            {
                var id = (ObjectId)itemSchema.ItemId;

                if (itemSchema.IsStatic == false)
                {
                    //todo have a better way to detect if an item is default static / dynamic. the bot will currently break if we try and spawn a dynamic by default item

                    Log.Warning(
                        "Item with id: {ItemID} is dynamic, we currently only support static items, skipping this item",
                        id);
                    continue;
                }

                var mapItem = new MapItem(index++, itemSchema);
                mapItem.item = itemSchema;
                if (items.ContainsKey(id)) // collect similar items into lists to reduce the bots ui traveling 
                {
                    items[id].Add(mapItem);
                    continue;
                }

                items.Add(id, new List<MapItem>());

                items[id].Add(mapItem);
            }

            WriteObjectRecoveryFile(items);
        }

        ForgeUI.SetHaloProcess();
        int itemCountID = 0;
        int saveCount = 0;

        foreach (var item in items)
        {
            /*
            //todo extract all the data processing and the bot logic from each other
            // while
            //     (MemoryHelper.GetGlobalHover() !=
            //      0) // reset the cursor to the top of the current menu (in most cases the object browser)
            // {
            //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
            //     await Task.Delay(travelSleep);
            // }

            await NavigationHelper.ReturnToTop();

            await Task.Delay(200);

            // while (MemoryHelper.GetTopBrowserHover() != 0) // set item browser to active menu
            // {
            //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_E);
            //     await Task.Delay(travelSleep);
            // }
            
            // set item browser to active menu
            await NavigationHelper.MoveToTab(0);

            var currentObjectId = item.Key;

            ForgeObjectBrowser.FindItem(currentObjectId, out ForgeUIObject? mapitem); // gets the items ui location data

            if (mapitem == null)
            {
                Log.Warning("Skipping null item, MapId: {id}, Name: {name} ", Enum.GetName(currentObjectId));
                continue;
            }

            //navigate to item with memory checks
            // while (MemoryHelper.GetGlobalHover() !=
            //        mapitem.ParentFolder.ParentCategory.CategoryOrder - 1) //Set cursor to correct cat
            // {
            //     Log.Debug("Move To Cat currentHover:{Hover} , Required Hover: {Reqired}",
            //         MemoryHelper.GetGlobalHover(), mapitem.ParentFolder.ParentCategory.CategoryOrder - 1);
            //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_S);
            //
            //     await Task.Delay(travelSleep);
            // }

            await NavigationHelper.NavigateVertical(mapitem.ParentFolder.ParentCategory.CategoryOrder);

            await Task.Delay(200);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN); // open the cat
            await Task.Delay(200);


            // while (MemoryHelper.GetGlobalHover() !=
            //        mapitem.ParentFolder.ParentCategory.CategoryOrder +
            //        mapitem.ParentFolder.FolderOffset - 1) // move cursor to sub cat
            // {
            //     Log.Debug("Move To subCat currentHover:{Hover} , Required Hover: {Reqired}",
            //         MemoryHelper.GetGlobalHover(),
            //         mapitem.ParentFolder.ParentCategory.CategoryOrder +
            //         mapitem.ParentFolder.FolderOffset - 1);
            //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_S);
            //
            //     await Task.Delay(travelSleep);
            // }

            await NavigationHelper.NavigateVertical(mapitem.ParentFolder.ParentCategory.CategoryOrder +
                                                    mapitem.ParentFolder.FolderOffset);

            await Task.Delay(200);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN); // enter the sub cat
            await Task.Delay(200);


            // while (MemoryHelper.GetGlobalHover() != mapitem.ObjectOrder - 1) // hover item
            // {
            //     Input.PressKey(VirtualKeyCode.VK_S);
            //     await Task.Delay(travelSleep);
            // }

            await NavigationHelper.NavigateVertical(mapitem.ObjectOrder);
            */
            
            ForgeUIObject _forgeObject;
            ForgeObjectBrowser.FindItem(item.Key, out _forgeObject);

            foreach (var mapItem in item.Value) // the start of the item spawning loop
            {
                if (mapItem.UniqueId < startIndex && resumeFromLast)
                {
                    continue;
                }

                WriteObjectRecoveryIndexToFile(mapItem.UniqueId);

                saveCount++;
                await Task.Delay(200);
                
                await NavigationHelper.SpawnItem(_forgeObject);
                
                // while (MemoryHelper.GetMenusVisible() == 1)
                // {
                //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN); // spawn the item?
                //     await Task.Delay(200);
                // }

                await Task.Delay(200);

                if (saveCount == WhenToSave)
                {
                    //todo add a save count setting to the ui
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

                await PropertyHelper.SetMainProperties(mapItem.item,isBlender);
                itemCountID++;
            }
            /*
            await Task.Delay(75);
           // Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.BACK);
            await Task.Delay(100);
            
            // while (MemoryHelper.GetGlobalHover() != mapitem.ParentFolder.ParentCategory.CategoryOrder - 1)
           
            while (MemoryHelper.GetGlobalHover() != _forgeObject.ParentFolder.ParentCategory.CategoryOrder - 1)
            {
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
                await Task.Delay(travelSleep);
            }

            await Task.Delay(100);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(500);
            // Input.Simulate.Keyboard

            //MemoryHelper.SetGlobalHover(UIItems[currentObjectId].index);
            //  MemoryHelper.SetBrowserScroll(UIItems[currentObjectId].index);
            */
        }
    }

    private static bool UILayoutBuilt = false;
    /// <summary>
    /// This needs changing to work with items that seem to be getting removed (like the last few antennas)
    /// </summary>
    public static void BuildUILayout()
    {
        if (UILayoutBuilt) return;
        UILayoutBuilt = true;
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

    private static Tuple<int, Dictionary<ObjectId, List<MapItem>>> GetRecoveryFiles()
    {
        Tuple<int, Dictionary<ObjectId, List<MapItem>>> recoveryObject =
            new Tuple<int, Dictionary<ObjectId, List<MapItem>>>(0, new Dictionary<ObjectId, List<MapItem>>());

        JsonSerializerSettings s = new JsonSerializerSettings();
        s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

        if (!File.Exists(Utils.ExePath + "/recovery/currentObjectRecoveryIndex.json"))
        {
            WriteObjectRecoveryIndexToFile(20);
        }

        using (StreamReader file = File.OpenText(Utils.ExePath + "/recovery/currentObjectRecoveryIndex.json"))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
            var index = JToken.ReadFrom(reader).Value<int>();
            recoveryObject =
                new Tuple<int, Dictionary<ObjectId, List<MapItem>>>(index, new Dictionary<ObjectId, List<MapItem>>());
        }


        if (File.Exists(Utils.ExePath + "/recovery/ObjectRecoveryData.json"))
        {
            // read JSON directly from a file
            using (StreamReader file = File.OpenText(Utils.ExePath + "/recovery/ObjectRecoveryData.json"))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                var items = (JObject)JToken.ReadFrom(reader);
                recoveryObject = new Tuple<int, Dictionary<ObjectId, List<MapItem>>>(recoveryObject.Item1,
                    items.ToObject<Dictionary<ObjectId, List<MapItem>>>());
            }
        }

        return recoveryObject;
    }

    private static void WriteObjectRecoveryIndexToFile(int index)
    {
        JsonSerializerSettings s = new JsonSerializerSettings();
        s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        Directory.CreateDirectory(Utils.ExePath + "/json/");

        var a = JsonConvert.SerializeObject(index, s);
        File.WriteAllText(Utils.ExePath + "/recovery/currentObjectRecoveryIndex.json", a);
    }

    private static void WriteObjectRecoveryFile(Dictionary<ObjectId, List<DataModels.MapItem>> items)
    {
        JsonSerializerSettings s = new JsonSerializerSettings();
        s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        Directory.CreateDirectory(Utils.ExePath + "/recovery/");

        var a = JsonConvert.SerializeObject(items, s);
        File.WriteAllText(Utils.ExePath + "/recovery/ObjectRecoveryData.json", a);
    }
}