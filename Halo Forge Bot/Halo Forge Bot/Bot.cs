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
    public static int WhenToSave = 3;
    //todo extract all NON BOT LOGIC for start bot, it should only be for starting / ending the bot

    public static int ConesToSpawn;
    public static bool next = false; //todo change this dog shit


    public static async Task StartBot(List<ForgeItem> map, int saveAmmount, int itemStart = 0, int itemEnd = 0,
        bool resumeFromLast = false, bool isBlender = false)
    {
        //todo create a class for both blender and .mvar files, maybe use the blender file json
        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess()
            .Id); // todo add checks to the ui to stop the starting of the bot without halo being open / crash detection

        Task.Run(Overlay.Setup);
        int startIndex = itemStart;
        Dictionary<ObjectId, List<MapItem>> items = new();

        // LoadItemData();
        BuildUILayout();
        BotSetup(map, out startIndex, itemStart, itemEnd, resumeFromLast, ref items);

        // Dictionary<ObjectId, List<MapItem>> temp = new();
        // temp.Add(ObjectId.PRIMITIVE_BLOCK, items[ObjectId.PRIMITIVE_BLOCK]);


        await BotLoop(items, startIndex, resumeFromLast, isBlender, saveAmmount);

        /*
        await NavigationHelper.MoveToTab(NavigationHelper.ContentBrowserTabs.Folders);
        Input.PressKey(VirtualKeyCode.VK_S, 50);
        Input.PressKey(VirtualKeyCode.VK_F, 50);
        Input.PressKey(VirtualKeyCode.VK_S, 50);
        for (int i = 0; i < MemoryHelper.GetItemCount(); i++)
        {
            Input.PressKey(VirtualKeyCode.VK_S, 25);
            Input.PressKey(VirtualKeyCode.VK_F, 25);
            await Task.Delay(25);
            Input.PressKey(VirtualKeyCode.VK_F, 25);
        }
        */
    }

    private static void BotSetup(List<ForgeItem> map, out int startIndex, int itemStart, int itemEnd,
        bool resumeFromLast, ref Dictionary<ObjectId, List<MapItem>> items)
    {
        startIndex = 0;
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
    }

    private static void SetDataMemory(MapItem item, int i)
    {
        var current = item;
        MemoryHelper.SetItemPosition(i, new Vector3(current.item.PositionX,
            current.item.PositionY,
            current.item.PositionZ));

        MemoryHelper.SetItemScale(i, new Vector3(current.item.ScaleX,
            current.item.ScaleY,
            current.item.ScaleZ));


        var forward = new Vector3(current.item.ForwardX, current.item.ForwardY, current.item.ForwardZ);
        var up = new Vector3(current.item.UpX, current.item.UpY, current.item.UpZ);


        MemoryHelper.SetItemRotation(i, forward, up);
    }

    private static async Task BotLoop(Dictionary<ObjectId, List<MapItem>> items, int startIndex, bool resumeFromLast,
        bool isBlender, int saveAmmount)
    {
        //todo check if the scale array always has the same number of items as the item array
        var sorted = items.OrderByDescending(x => x.Value.Count).ToList();


        ForgeUI.SetHaloProcess();

        int saveCount = saveAmmount;

        int itemIndex = 0;
        foreach (var item in sorted)
        {
            int itemCountID = MemoryHelper.GetItemCount();
            ForgeUIObject _forgeObject;
            ForgeObjectBrowser.FindItem(item.Key, out _forgeObject);

            //WriteObjectRecoveryIndexToFile(mapItem.UniqueId);


            await Task.Delay(200);

            if (item.Key == ObjectId.PLAYER_SCALE_OBJECT)
            {
                continue;
            }

            int spawnCounter = 0;

            foreach (var mapItem in item.Value)
            {
                saveCount++;
                await NavigationHelper.SpawnItem(_forgeObject);
                await Task.Delay(10);

                // if (saveCount == WhenToSave) //todo fix saving
                // {
                //     
                //     Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                //     await Task.Delay(1000);
                //     saveCount = 0;
                // }

                SetDataMemory(mapItem, itemCountID);

                await Task.Delay(50);
                await NavigationHelper.MoveToTab(NavigationHelper.ContentBrowserTabs.ObjectProperties);
                //Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                //  Thread.Sleep(10);
                // Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_D);
                await NavigationHelper.NavigateVertical(
                    ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Forward,
                        ForgeUIObjectModeEnum.STATIC_FIRST));
                await Task.Delay(10);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                await Task.Delay(10);
                await PropertyHelper.SetForwardProperty(mapItem.item.PositionX * 10,
                    ForgeUIObjectModeEnum.STATIC_FIRST);


                spawnCounter++;
                itemCountID++;


                // if (saveCount == WhenToSave)
                // {
                //     // await NavigationHelper.CloseUI();
                //     await Task.Delay(100);
                //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_R);
                //     await Task.Delay(100);
                //     //todo add a save count setting to the ui
                //     Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                //     Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                //     saveCount = 0;
                //     await Task.Delay(100);
                // }
            }


            await Task.Delay(200);


            //await PropertyHelper.SetMainProperties(mapItem.item, _forgeObject.DefaultObjectMode == ForgeUIObjectModeEnum.NONE ? ForgeUIObjectModeEnum.STATIC_FIRST : _forgeObject.DefaultObjectMode, isBlender);
            //  await PropertyHelper.SetPropertiesFromMemory(mapItem.item, itemCountID);


            await Input.HandlePause();
        }
    }

    /// <summary>
    /// Ensures the UI layout is only built once
    /// </summary>
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
                Enum.GetName((ObjectId)int.Parse(objectData[3])) ?? objectData[2], Enum.Parse<ObjectId>(objectData[3]),
                ForgeUIObjectModeEnum.STATIC_FIRST);
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