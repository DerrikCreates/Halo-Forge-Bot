using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Halo_Forge_Bot.config;
using Halo_Forge_Bot.DataModels;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using InfiniteForgeConstants.Forge_UI.Object_Properties;
using InfiniteForgeConstants.ObjectSettings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using WindowsInput.Native;

namespace Halo_Forge_Bot.Core;

public static class Bot
{
    public static string Version = "0.2.5n";
    public static string PosLogString = "";


    public static EventHandler<int>? OnItemStart;

    public static EventHandler<int>? OnItemDone;
    //todo extract all NON BOT LOGIC for start bot, it should only be for starting / ending the bot


    public static async Task StartBot(List<ForgeItem> map, BotSettings botSettings, BotState botState)
    {
        OnItemStart += BotTracker.TrackItemSpawnTime;
        OnItemDone += BotTracker.TrackItemSpawnTime;


        var process = ForgeUI.SetHaloProcess();
        if (process == null)
        {
            new Error("Bot cannot find the HaloInfinite.exe process. returning").Show();
            return;
        }

        MemoryHelper.Memory
            .OpenProcess(process
                .Id); // todo add checks to the ui to stop the starting of the bot without halo being open / crash detection

        Task.Run((() => Overlay.Setup(botSettings, botState)));

        Dictionary<ObjectId, List<MapItem>> items = new();
        BuildUiLayout();
        BotSetup(map, botSettings, botState, ref items);
        await BotLoop(items, botSettings, botState);
    }

    private static void BotSetup(List<ForgeItem> map, BotSettings botSettings, BotState botState,
        ref Dictionary<ObjectId, List<MapItem>> items)
    {
        var splitItemList = new List<ForgeItem>(); // item list of the items to process
        var tempArray =
            map.OrderBy(item => item.ItemId)
                .ToList(); // temp to an array to i know now for sure its in the correct order. might be unnecessary 

        if (botSettings.ItemEndIndex == 0)
        {
            botSettings.ItemEndIndex = tempArray.Count();
        }

        int index = 0;

        Log.Information("Creating map with item range of {ItemStartIndex} to {ItemEndIndex}",
            botSettings.ItemStartIndex, botSettings.ItemEndIndex);
        for (int i = botSettings.ItemStartIndex;
             i < botSettings.ItemEndIndex;
             i++) // extracting the requested items from the map. 
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


        /*
         * Create a bounding box off all the map items by looping over and finding the highest x y and z position for each axis
         * then find the lowest for each axis
         *
         * calculate the center of that box by dividing the distance by of the min to the max? of each axis
         *
         * get the bottom center of that box
         * 
         */


        List<MapItem> temp = new();
        foreach (var i in items)
        {
            foreach (var x in items.Values)
            {
                foreach (var y in x)
                {
                    temp.Add(y);
                }
            }
        }


        var Xlow = temp.MinBy(x => x.item.PositionX);
        var Xhigh = temp.MaxBy(x => x.item.PositionX);

        var Ylow = temp.MinBy(x => x.item.PositionY);
        var Yhigh = temp.MaxBy(x => x.item.PositionY);

        var Zlow = temp.MinBy(x => x.item.PositionZ);
        var Zhigh = temp.MaxBy(x => x.item.PositionZ);


        botState.BoundingBox.topLeft = new Vector3(Xhigh.item.PositionX, Ylow.item.PositionY, Zhigh.item.PositionZ) * botSettings.Scale;
        botState.BoundingBox.bottomLeft = new Vector3(Xhigh.item.PositionX, Ylow.item.PositionY, Zlow.item.PositionZ)* botSettings.Scale;
        botState.BoundingBox.bottomRight = new Vector3(Xhigh.item.PositionX, Yhigh.item.PositionY, Zlow.item.PositionZ)* botSettings.Scale;
        botState.BoundingBox.backLeft = new Vector3(Xlow.item.PositionX, Ylow.item.PositionY, Zlow.item.PositionZ)* botSettings.Scale;

        botState.BoundingBox.bottomCenter = (botState.BoundingBox.bottomRight - botState.BoundingBox.bottomLeft) / 2 +
                                            botState.BoundingBox.bottomLeft;
        botState.BoundingBox.bottomCenter.X -=
            (botState.BoundingBox.bottomLeft - botState.BoundingBox.backLeft).Length() / 2;


        // itm = (bottomCenter - itm) 
        // var value =   Yhigh.item.PositionY -  Ylow.item.PositionY;
        //   var yMid = (value / 2) + Ylow.item.PositionY;

        //   value = Zhigh.item.PositionZ - Zlow.item.PositionZ;
        //    var zMid = (value / 2) + Zlow.item.PositionZ;

        //   Vector3 midLeft = new Vector3(h)
    }

    private static void SetDataMemory(MapItem item, int i)
    {
        var current = item;

        var pos = new Vector3(current.item.PositionX, current.item.PositionY, current.item.PositionZ);


        MemoryHelper.SetItemPosition(i, pos);
        MemoryHelper.SetItemScale(i, new Vector3(current.item.ScaleX, current.item.ScaleY, current.item.ScaleZ));


        var forward = new Vector3(current.item.ForwardX, current.item.ForwardY, current.item.ForwardZ);
        var up = new Vector3(current.item.UpX, current.item.UpY, current.item.UpZ);


        MemoryHelper.SetItemRotation(i, forward, up);
    }

    private static async Task BotLoop(Dictionary<ObjectId, List<MapItem>> items, BotSettings settings,
        BotState botState)
    {
        //todo check if the scale array always has the same number of items as the item array
        var sorted = items.OrderByDescending(x => x.Value.Count).ToList();


        ForgeUI.SetHaloProcess();


        foreach (var item in sorted)
        {
            Log.Information("Changing Item Type: {CurrentItemType}", item.Key.ToString());
            int itemCountId = MemoryHelper.GetItemCount();
            ForgeUIObject _forgeObject;
            ForgeObjectBrowser.FindItem(item.Key, out _forgeObject);
            if (_forgeObject == null)
            {
                Log.Error("Could not find the UI data for Forge item: {ForgeObject}, skipping this item",
                    item.Key.ToString());
                continue;
            }
            //WriteObjectRecoveryIndexToFile(mapItem.UniqueId);


            await Task.Delay(200);

            if (item.Key == ObjectId.PLAYER_SCALE_OBJECT)
            {
                continue;
            }


            foreach (var mapItem in item.Value)
            {
                mapItem.item.PositionX *= settings.Scale;
                mapItem.item.PositionY *= settings.Scale;
                mapItem.item.PositionZ *= settings.Scale;

                if (settings.Scale != 1)
                    settings.RecenterMap = true; //todo add a ui check bot to recenter maps
                var basePos = new Vector3(mapItem.item.PositionX, mapItem.item.PositionY, mapItem.item.PositionZ) ;
                var itemOffset = new Vector3(mapItem.item.PositionX, mapItem.item.PositionY, mapItem.item.PositionZ) - botState.BoundingBox.bottomCenter;

                Vector3 newCenter = new Vector3(0, 0, 60);

                Vector3 centerOffset = botState.BoundingBox.bottomCenter - newCenter ;

                Vector3 newPosition = centerOffset - basePos -itemOffset ;
                mapItem.item.PositionX = newPosition.X;
                mapItem.item.PositionY = newPosition.Y;
                mapItem.item.PositionZ = newPosition.Z;
                
                
                mapItem.item.ScaleX *= settings.Scale;
                mapItem.item.ScaleY *= settings.Scale;
                mapItem.item.ScaleZ *= settings.Scale;

                // Start of item spawning
                // there are many redundant memory sets. this is to make sure the data is correct when we force the server update by editing the property
                OnItemStart.Invoke(null, itemCountId);

                SetDataMemory(mapItem, itemCountId);
                await NavigationHelper.SpawnItem(_forgeObject);
                await Task.Delay(250);

                SetDataMemory(mapItem, itemCountId);
                await NavigationHelper.MoveToTab(NavigationHelper.ContentBrowserTabs.ObjectProperties);

                SetDataMemory(mapItem, itemCountId);
                await NavigationHelper.NavigateVertical(
                    ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Forward,
                        ForgeUIObjectModeEnum.STATIC_FIRST));

                SetDataMemory(mapItem, itemCountId);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                await Task.Delay(50);

                SetDataMemory(mapItem, itemCountId);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                await Task.Delay(50);

                SetDataMemory(mapItem, itemCountId);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_A);
                await Task.Delay(50);

                await PropertyHelper.SetForwardProperty(mapItem.item.PositionX * 10,
                    ForgeUIObjectModeEnum.STATIC_FIRST);
                SetDataMemory(mapItem, itemCountId);
                await Task.Delay(50);

                await NavigationHelper.CloseEditUI();
                await Task.Delay(10);

                var currentPos = MemoryHelper.ReadItemPosition(itemCountId);
                var expectedPos = new Vector3(mapItem.item.PositionX, mapItem.item.PositionY, mapItem.item.PositionZ);

                currentPos.X = MathF.Round(currentPos.X, 1);
                currentPos.Y = MathF.Round(currentPos.Y, 1);
                currentPos.Z = MathF.Round(currentPos.Z, 1);

                expectedPos.X = MathF.Round(expectedPos.X, 1);
                expectedPos.Y = MathF.Round(expectedPos.Y, 1);
                expectedPos.Z = MathF.Round(expectedPos.Z, 1);

                Log.Information("Item {ItemID} has been spawned, {ItemCountID} , {ExpectedPosition}, {CurrentPosition}",
                    _forgeObject.ObjectId, itemCountId,
                    new Vector3(mapItem.item.PositionX, mapItem.item.PositionY, mapItem.item.PositionZ),
                    MemoryHelper.ReadItemPosition(itemCountId));
                if ((expectedPos - currentPos).Length() > 0.5)
                {
                    // this item is broken
                    botState.FailedItems++;
                    PosLogString = $"Last Error-- ExpectedPos: {expectedPos}, CurrentPos: {currentPos}";
                    Log.Error(
                        "Item position not correct! current rounded position:{CurrentRoundedPosition} , Expected rounded position {ExpectedRoundedPosition}",
                        currentPos, expectedPos);
                }


               
                OnItemDone.Invoke(null, itemCountId);

                itemCountId++;
            }
        }


        await Task.Delay(200);

        await Input.HandlePause();
    }


    /// <summary>
    /// Ensures the UI layout is only built once
    /// </summary>
    private static bool _uiLayoutBuilt;

    /// <summary>
    /// This needs changing to work with items that seem to be getting removed (like the last few antennas)
    /// </summary>
    public static void BuildUiLayout()
    {
        if (_uiLayoutBuilt) return;
        _uiLayoutBuilt = true;
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