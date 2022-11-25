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
    private static Dictionary<ObjectId, UIItem> UIItems = new();

    private static void LoadItemData()
    {
        var separators = new[] { ':', '>' };
        string path = Utils.ExePath + "/RawData/ForgeObjects.txt";
        var lines = File.ReadAllLines(path);

        string lastSubFolder = "";
        string lastCat = "";
        int subFolderIndex = 0;
        int catIndex = 0;
        for (int i = 0; i < lines.Count(); i++)
        {
            var itemData = new UIItem();

            var data = lines[i].Split(separators);

            itemData.Category = data[0].Trim();
            itemData.SubCategory = data[1].Trim();
            itemData.ItemName = data[2].Trim();
            itemData.ItemId = (ObjectId)int.Parse(data[3].Trim());

            if (lastCat != itemData.Category)
            {
                catIndex++;
            }

            if (lastSubFolder != itemData.SubCategory)
            {
                subFolderIndex++;
            }

            // itemData.= subFolderIndex + catIndex;
            UIItems.Add(itemData.ItemId, itemData);
            lastSubFolder = itemData.SubCategory;
        }
    }

    private static int CalcScroll(int uiSize, int cursorLocation)
    {
        if (uiSize < 13)
        {
            return 0;
        }

        int diff = cursorLocation - uiSize;
        if (diff < 13)
        {
            return uiSize + 13;
        }

        return cursorLocation;
    }

    public static async Task DevTesting()
    {
        // LoadItemData();
        UIData();
        var map = BondHelper.ProcessFile<BondSchema>($"{Utils.ExePath}/Temp/SnowMap.mvar");

        Dictionary<ObjectId, List<ItemSchema>> items = new();
        foreach (var itemSchema in map.Items)
        {
            var id = (ObjectId)itemSchema.ItemId.Int;


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

                await Task.Delay(10);
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

                await Task.Delay(10);
            }

            await Task.Delay(200);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN); // enter the sub cat
            await Task.Delay(200);


            while (MemoryHelper.GetGlobalHover() != mapitem.ObjectOrder - 1) // hover item
            {
                Input.PressKey(VirtualKeyCode.VK_S);
                await Task.Delay(10);
            }


            foreach (var itemSchema in item.Value)
            {
                if (saveCount == 5)
                {
                    Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                    saveCount = 0;
                }

                saveCount++;
                await Task.Delay(200);
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN); // spawn the item?
                await Task.Delay(200);

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
                    await Task.Delay(50);
                }


                while (MemoryHelper.GetTopBrowserHover() != 1)
                {
                    await Task.Delay(50);
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_E);
                    await Task.Delay(5);
                }

                var defaultScale = MemoryHelper.GetSelectedScale();

                var xScale = itemSchema.SettingsContainer.Scale.First().ScaleContainer.X;
                var yScale = itemSchema.SettingsContainer.Scale.First().ScaleContainer.Y;
                var zScale = itemSchema.SettingsContainer.Scale.First().ScaleContainer.Z;

                var itemScale = new Vector3(xScale, yScale, zScale);

                var realScale = (itemScale * defaultScale); //Vector3.Multiply(itemScale, defaultScale) * 10;

                await SetProp(realScale.X.ToString(), StaticByDefault.Layout[PropertyName.SizeX]);
                await SetProp(realScale.Y.ToString(), StaticByDefault.Layout[PropertyName.SizeY]);
                await SetProp(realScale.Z.ToString(), StaticByDefault.Layout[PropertyName.SizeZ]);


                var xPos = itemSchema.Position.X * 10;
                var yPos = itemSchema.Position.Y * 10;
                var zPos = itemSchema.Position.Z * 10;

                await SetProp(xPos.ToString(), StaticByDefault.Layout[PropertyName.Forward]);
                await SetProp(yPos.ToString(), StaticByDefault.Layout[PropertyName.Horizontal]);
                await SetProp(zPos.ToString(), StaticByDefault.Layout[PropertyName.Vertical]);

                var xForward = itemSchema.Forward.X;
                var yForward = itemSchema.Forward.Y;
                var zForward = itemSchema.Forward.Z;

                var xUp = itemSchema.Up.X;
                var yUp = itemSchema.Up.Y;
                var zUp = itemSchema.Up.Z;

                var rotation =
                    Utils.DidFishSaveTheDay(new Vector3(xForward, yForward, xForward), new Vector3(xUp, yUp, zUp));


                await SetProp(rotation.X.ToString(), StaticByDefault.Layout[PropertyName.Roll]);
                await SetProp(rotation.X.ToString(), StaticByDefault.Layout[PropertyName.Pitch], VirtualKeyCode.VK_W);
                await SetProp(rotation.Y.ToString(), StaticByDefault.Layout[PropertyName.Yaw], VirtualKeyCode.VK_W);

                await Task.Delay(50);

                while (MemoryHelper.GetTopBrowserHover() != 0)
                {
                    Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_Q);
                    await Task.Delay(10);
                }


                itemCountID++;
            }

            await Task.Delay(75);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.BACK);
            await Task.Delay(100);

            while (MemoryHelper.GetGlobalHover() != mapitem.ParentFolder.ParentCategory.CategoryOrder - 1)
            {
                Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
                await Task.Delay(10);
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
        while (MemoryHelper.GetGlobalHover() != index)
        {
            Input.Simulate.Keyboard.KeyPress(key);
            await Task.Delay(15);
        }


        //          var selectedPosition = MemoryHelper.GetSelectedPosition();

        while (MemoryHelper.GetEditMenuState() == 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(50);
        }

        var clipData = "";
        while (await ClipboardService.GetTextAsync() != data)
        {
            await ClipboardService.SetTextAsync(data);
        }


        while (MemoryHelper.GetEditBoxText() != data)
        {
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.BACK);
            await Task.Delay(150);
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            await Task.Delay(50);
        }

        while (MemoryHelper.GetEditMenuState() != 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(50);
        }
    }

    private static async Task<string> CollectElement(int elementId, int menu = 1)
    {
        while (MemoryHelper.GetTopBrowserHover() != menu)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_Q);
            //Input.PressKey(VirtualKeyCode.VK_Q);
        }

        MemoryHelper.FreezeGlobalHover(elementId);
        while (MemoryHelper.GetGlobalHover() != elementId)
        {
            await Task.Delay(10);
        }

        MemoryHelper.UnFreezeGlobalHover();

        while (MemoryHelper.GetEditMenuState() == 0)
        {
            //Thread.Sleep(50);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            Thread.Sleep(100);
        }

        var a = MemoryHelper.GetEditBoxText();
        float value;

        while (float.TryParse(a, out value) == false)
        {
            a = MemoryHelper.GetEditBoxText();
        }

        //double check
        if (float.TryParse(MemoryHelper.GetEditBoxText(), out float valueCheck) == true)
        {
            if (Math.Abs(value - valueCheck) < 0.01 == false)
            {
                Log.Error("Value collected was not correct");
            }
        }


        while (MemoryHelper.GetEditMenuState() != 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        }

        await ClipboardService.SetTextAsync("");
        Thread.Sleep(350);
        return a;
    }

    private static async void SetRotM(Vector3 pos)
    {
        pos.X = MathF.Round(pos.X, 5);
        pos.Y = MathF.Round(pos.Y, 5);
        pos.Z = MathF.Round(pos.Z, 5);

        // might be correct. todo when you wake up finish testing all axis and object rotation
        await SetAxis(pos.Z, StaticByDefault.Layout[PropertyName.Roll]); // zyx, yzx,yxz,
        Thread.Sleep(1000);
        await SetAxis(pos.X, StaticByDefault.Layout[PropertyName.Pitch]);
        Thread.Sleep(1000);
        await SetAxis(pos.Y, StaticByDefault.Layout[PropertyName.Yaw]);
    }

    private static async Task SetPosM(Vector3 pos)
    {
        await SetAxis(pos.X, StaticByDefault.Layout[PropertyName.Forward]);
        Thread.Sleep(1000);
        await SetAxis(pos.Y, StaticByDefault.Layout[PropertyName.Horizontal]);
        Thread.Sleep(1000);
        await SetAxis(pos.Z, StaticByDefault.Layout[PropertyName.Vertical]);
    }

    private static async Task SetScaleM(Vector3 pos)
    {
        await SetAxis(pos.X, StaticByDefault.Layout[PropertyName.SizeX]);
        Thread.Sleep(1000);
        await SetAxis(pos.Y, StaticByDefault.Layout[PropertyName.SizeY]);
        Thread.Sleep(1000);
        await SetAxis(pos.Z, StaticByDefault.Layout[PropertyName.SizeZ]);
    }

    private static float GetAxis(int uiIndex)
    {
        Log.Information("Get Axis- UIIndex:{UIIndex}", uiIndex);


        MemoryHelper.Memory.WriteMemory(HaloPointers.SubBrowserHover, "int", uiIndex.ToString());
        MemoryHelper.Memory.WriteMemory(HaloPointers.BrowserScroll, "int", "0");
        Thread.Sleep(50);
        Input.PressKey(VirtualKeyCode.RETURN);
        Thread.Sleep(200);
        Input.PressKey(VirtualKeyCode.VK_C, 20, VirtualKeyCode.CONTROL);
        Thread.Sleep(25);
        var text = ClipboardService.GetText();
        Thread.Sleep(50);
        Input.PressKey(VirtualKeyCode.ESCAPE);
        Thread.Sleep(50);
        if (text is not null)
        {
            var axisText = text.ToCharArray().ToList();

            //clean up axis data
            var num = text.Split(".");
            var right = num[1].Substring(0, 2);

            var final = $"{num[0]}.{right}";

            var number = float.Parse(final);
            return number;
        }

        Log.Information("The Clipboard Text is null");
        return -1;
    }

    private static async Task SetAxis(float pos, int uiIndex, int sleep = 100)
    {
        Log.Information("Set Axis with Value: {value} , UIIndex:{UIIndex}", pos, uiIndex);
        pos = MathF.Round(pos, 5);

        MemoryHelper.Memory.WriteMemory(HaloPointers.SubBrowserHover, "int", uiIndex.ToString());
        Thread.Sleep(sleep);
        MemoryHelper.Memory.WriteMemory(HaloPointers.BrowserScroll, "int", "0");
        Thread.Sleep(300);
        Input.PressKey(VirtualKeyCode.RETURN);
        Thread.Sleep(300);
        ClipboardService.SetText(pos.ToString());
        await Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.BACK);
        await Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);
        Thread.Sleep(sleep);


        Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        Thread.Sleep(sleep);
    }


    public static List<ForgeUIFolder?> folders = new();


    public static void SetupFolders() //todo use lite db and not josh's bs :)
    {
        foreach (var cat in ForgeObjectBrowser.Categories)
        {
            foreach (var folder in cat.Value.CategoryFolders)
            {
                folders.Add(folder.Value);
            }

            folders.Add(null);
        }
    }


    private static async Task NavigateToRandomItem()
    {
        Array values = Enum.GetValues(typeof(ObjectId));
        Random random = new Random();

        for (int i = 0; i < 200; i++)
        {
            var randomObject = (ObjectId)values.GetValue
                (random.Next(values.Length));

            await NavigateToItem(randomObject);
        }
    }


    public static void UIData()
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
        var data = File.ReadAllLines(strWorkPath + "/RawData/ForgeObjects.txt");

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

    private static int tempImageCount = 0;

    public static async Task NavigateToItem(ObjectId id)
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);

        var item = GetItemByID(id);
        // todo make a proper db to store the data. 
        // todo make the bot collect the default scale / static/dynamic state and save it if its not there

        var catOrder = item.ParentFolder.ParentCategory.CategoryOrder;
        var subOrder = item.ParentFolder.FolderOffset;
        var itemOrder = item.ObjectOrder;

        for (int j = 0; j < catOrder - 1; j++) // travel to the correct cat
        {
            await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S, keySleep: 25);
        }

        await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN, keySleep: 25); //enter cat

        for (int j = 0; j < subOrder; j++) //travel to correct folder
        {
            await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S, keySleep: 25);
        }

        await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN, keySleep: 25); // enter folder
        for (int j = 0; j < itemOrder - 1; j++) // travel to requested object
        {
            await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S, keySleep: 25);
        }

        var selectedItemImage = PixelReader.ScreenshotArea(ForgeUI.ForgeMenu);
        selectedItemImage.Save(strWorkPath +
                               $"/images/{item.ObjectName}-{catOrder}-{subOrder}-{itemOrder}---{tempImageCount}.png",
            ImageFormat.Png);
        Thread.Sleep(100);
        // Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN, keySleep: 125);
        // Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_R, keySleep: 25);

        tempImageCount++;


        // we should now be reset
    }

    public static async Task UnNavigateToItem(ObjectId id)
    {
        var item = GetItemByID(id);

        var catOrder = item.ParentFolder.ParentCategory.CategoryOrder;
        var subOrder = item.ParentFolder.FolderOffset;

        // reset the cursor to the start for next item

        await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.BACK, keySleep: 25);


        for (int j = 0; j < subOrder; j++)
        {
            await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W, keySleep: 25);
        }

        await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN, keySleep: 25);

        for (int j = 0; j < catOrder - 1; j++)
        {
            await Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W, keySleep: 25);
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