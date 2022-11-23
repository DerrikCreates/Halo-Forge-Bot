using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BondReader;
using BondReader.Schemas;
using BondReader.Schemas.Items;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using InfiniteForgeConstants.ObjectSettings;
using Newtonsoft.Json;
using Serilog;
using TextCopy;
using WindowsInput.Native;


namespace Halo_Forge_Bot;

public static class Bot
{


   




    

    private static async void SetRotM(Vector3 pos)
    {
        pos.X = MathF.Round(pos.X, 5);
        pos.Y = MathF.Round(pos.Y, 5);
        pos.Z = MathF.Round(pos.Z, 5);

        // might be correct. todo when you wake up finish testing all axis and object rotation
        await SetAxis(pos.Z, UIIndex["rotZ"]); // zyx, yzx,yxz,
        Thread.Sleep(1000);
        await SetAxis(pos.X, UIIndex["rotY"]);
        Thread.Sleep(1000);
        await SetAxis(pos.Y, UIIndex["rotX"]);
    }

    private static async Task SetPosM(Vector3 pos)
    {
        await SetAxis(pos.X, UIIndex["posX"]);
        Thread.Sleep(1000);
        await SetAxis(pos.Y, UIIndex["posY"]);
        Thread.Sleep(1000);
        await SetAxis(pos.Z, UIIndex["posZ"]);
    }

    private static async Task SetScaleM(Vector3 pos)
    {
        await SetAxis(pos.X, UIIndex["sizeX"]);
        Thread.Sleep(1000);
        await SetAxis(pos.Y, UIIndex["sizeY"]);
        Thread.Sleep(1000);
        await SetAxis(pos.Z, UIIndex["sizeZ"]);
    }

    private static float GetAxis(int uiIndex)
    {
        Log.Information("Get Axis- UIIndex:{UIIndex}", uiIndex);


        MemoryHelper.Memory.WriteMemory(MemoryHelper.SubBrowserHover, "int", uiIndex.ToString());
        MemoryHelper.Memory.WriteMemory(MemoryHelper.ScrollBar, "int", "0");
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

        MemoryHelper.Memory.WriteMemory(MemoryHelper.SubBrowserHover, "int", uiIndex.ToString());
        Thread.Sleep(sleep);
        MemoryHelper.Memory.WriteMemory(MemoryHelper.ScrollBar, "int", "0");
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

    static Dictionary<string, int> UIIndex = new();

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

    public static async Task DevTesting()
    {
        UIData();

        ForgeUI.SetHaloProcess();

        MemoryHelper.Memory.OpenProcess(ForgeUI.HaloProcess.Id, out string failReason);


        UIIndex = new();
        // ForgeUI.SetHaloProcess();


        UIIndex.Add("sizeX", 4);
        UIIndex.Add("sizeY", 5);
        UIIndex.Add("sizeZ", 6);


        UIIndex.Add("posX", 10);
        UIIndex.Add("posY", 11);
        UIIndex.Add("posZ", 12);

        UIIndex.Add("rotX", 14);
        UIIndex.Add("rotY", 15);
        UIIndex.Add("rotZ", 16);

        BondSchema bond = BondHelper.ProcessFile<BondSchema>($"{Utils.ExePath}/SnowMap.mvar");

        Dictionary<ObjectId, List<ItemSchema>> Items = new Dictionary<ObjectId, List<ItemSchema>>();
        SetupFolders();
        int skipCounter = 0;
        foreach (var item in bond.Items)
        {
            if (!Items.ContainsKey((ObjectId)item.ItemId.Int))
            {
                Items.Add((ObjectId)item.ItemId.Int, new List<ItemSchema>());
            }

            Items[(ObjectId)item.ItemId.Int].Add(item);
        }

        bool lazy = false;
        foreach (var id in Items)
        {
            // await NavigateToItem(id.Key); // takes us the the item we want to spawn
            var currentItem = GetItemByID(id.Key);
            skipCounter++;
            if (lazy == false)
            {
                lazy = true;
                //await UnNavigateToItem(id.Key);
                continue;
            }

            int saveCounter = 0;
            foreach (var item in id.Value)
            {
                if (Input.EXIT)
                {
                    return;
                }
                else
                {
                    lazy = true;
                }


                //click to spawn prim cube
                // MoveMouseTo(110, 70); // to object menu and click

                // mem.WriteMemory(MemoryHelper.RootBrowserHover, "int", "4");

                while (MemoryHelper.Memory.ReadInt(MemoryHelper.TopBrowserHover) != 0)
                {
                    Input.PressKey(VirtualKeyCode.VK_E);
                }


                var index = folders.IndexOf(currentItem.ParentFolder) + 3;
                if (index == -1)
                {
                    Log.Error("Could not find index for {Item}", currentItem.ObjectName);
                }

                Thread.Sleep(200);
                MemoryHelper.Memory.WriteMemory(MemoryHelper.RootBrowserHover, "int", index.ToString());
                Thread.Sleep(200);
                MemoryHelper.Memory.WriteMemory(MemoryHelper.ScrollBar, "int", index.ToString());

                Thread.Sleep(200);
                Input.PressKey(VirtualKeyCode.RETURN);
                Thread.Sleep(200);

                MemoryHelper.Memory.WriteMemory(MemoryHelper.SubBrowserHover, "int",
                    (currentItem.ObjectOrder - 1).ToString());
                MemoryHelper.Memory.WriteMemory(MemoryHelper.ScrollBar, "int",
                    (currentItem.ObjectOrder - 1).ToString());
                Thread.Sleep(200);
                Input.PressKey(VirtualKeyCode.RETURN);
                Thread.Sleep(200);
                saveCounter++;
                if (saveCounter == 5)
                {
                    saveCounter = 0;
                    Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                    Thread.Sleep(300);
                }

                Thread.Sleep(200);
                Input.PressKey(VirtualKeyCode.VK_R);
                Thread.Sleep(200);
                while (MemoryHelper.Memory.ReadInt(MemoryHelper.TopBrowserHover) != 1)
                {
                    Input.PressKey(VirtualKeyCode.VK_E);
                }

                // after spawning check if we have already collected the default scale

                // if not then collect it and save it to the dic


                if (currentItem.DefaultScale == Vector3.Zero)
                {
                    //Start collecting scale
                    //currentItem.DefaultObjectMode.
                    //ForgeUI.GetDefaultObjectMode()


                    var x = GetAxis(UIIndex["sizeX"]);
                    var y = GetAxis(UIIndex["sizeY"]);
                    var z = GetAxis(UIIndex["sizeZ"]);

                    currentItem.DefaultScale = new Vector3(x, y, z);

                    Utils.SaveJson(ForgeObjectBrowser.Categories, "ObjectBrowser");
                }

                if (currentItem.DefaultObjectMode == ForgeUIObjectModeEnum.NONE)
                {
                    // collect the default object mode and save it to the dict
                    currentItem.DefaultObjectMode = ForgeUI.GetDefaultObjectMode();
                }


                MemoryHelper.Memory.WriteMemory(MemoryHelper.ScrollBar, "int", "0");
                Thread.Sleep(10);
                Vector3 realScale =
                    Vector3.Multiply(new Vector3(item.SettingsContainer.Scale.First().ScaleContainer.X,
                        item.SettingsContainer.Scale.First().ScaleContainer.Y,
                        item.SettingsContainer.Scale.First().ScaleContainer.Z), currentItem.DefaultScale);

                await SetScaleM(realScale);

                var position = Vector3.Multiply(10, new Vector3(item.Position.X, item.Position.Y, item.Position.Z));
                position.X = MathF.Round(position.X, 5);
                position.Y = MathF.Round(position.Y, 5);
                position.Z = MathF.Round(position.Z, 5);
                await SetPosM(position);

                var forward = new Vector3(item.Forward.X, item.Forward.Y, item.Forward.Z);
                var up = new Vector3(item.Up.X, item.Up.Y, item.Up.Z);

                var r = Utils.DidFishSaveTheDay(forward, up);

                SetRotM(r);
            }
        }

        return;

        /*
        int editMenuSleep = 1;
        Thread.Sleep(4000);
        foreach (var item in bond.ItemList)
        {
            if (item.ItemId == 1759788903)
            {
                var forward = new Vector3(item.ForwardX, -item.ForwardY, item.ForwardZ);
                var up = new Vector3(item.UpX, -item.UpY, item.UpZ);

                var rotation = Utils.DirectionToEuler(forward, up);
                var position = Vector3.Multiply(10, new Vector3(item.PositionX, item.PositionY, item.PositionZ));

                // Starting at the correct object this enter spawns the item;
                Log.Information("----Starting New Item----");
                Thread.Sleep(editMenuSleep);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
                Thread.Sleep(editMenuSleep);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_R);
                Thread.Sleep(editMenuSleep);


                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_E);

                Log.Information("Scrolling to the top");
                for (int i = 0; i < 25; i++)
                {
                    Input.Simulate.Mouse.VerticalScroll(1);
                    Thread.Sleep(4);
                }

                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_Z);

                Log.Information("Traveling to size vector");
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);

                Log.Information("Collecting Scale Vector");
                var scaleStrings = new string[3] { "4", "4", "4" }; // await CollectVector3();
                // Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                //  Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                //   Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);

                //   Vector3 defaultScale = new Vector3(float.Parse(scaleStrings[0].Substring(0, 3)),
                //        float.Parse(scaleStrings[1].Substring(0, 3)), float.Parse(scaleStrings[2].Substring(0, 3)));
                //      Log.Information("X:{defaultScaleX},Y:{defaultScaleY},Z{defaultScaleZ}", defaultScale.X, defaultScale.Y,
                //           defaultScale.Z);

                Vector3 scale = new Vector3(item.ScaleX, item.ScaleY, item.ScaleZ);
                //scale *= 10;

                Vector3 realScale = Vector3.Multiply(scale, new Vector3(4, 4, 4));

                Log.Information("Reseting at X scale for writing");


                // Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W);
                //  Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W);


                SetVector3(realScale); // Setting the scale vector


                /* SetSelectedScale(scale.X);
                 Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                 SetSelectedScale(scale.Y);
                 Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                 SetSelectedScale(scale.Z);
                 */
/*
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);


        SetVector3(position);
        /* Log.Information("We are now hovering X/Forward");
         // We are now hovering X/Forward
         Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
         Thread.Sleep(300); // Sleeping for the edit menu to show
         ClipboardService.SetText(position.X.ToString());
         Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);

         Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
         Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);

         Log.Information("We are now hovering Y/Horizontal");
         // We are now hovering Y/Horizontal
         Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
         Thread.Sleep(300); // Sleeping for the edit menu to show
         ClipboardService.SetText(position.Y.ToString());
         Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);

         Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
         Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);

         Log.Information(" We are now hovering Z/Vertical");
         // We are now hovering Z/Vertical
         Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
         Thread.Sleep(300); // Sleeping for the edit menu to show
         ClipboardService.SetText(position.Z.ToString());
         Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);

         Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
         


        Log.Information("Traveling to the roll input");
        //Traveling to the roll input
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        // Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        //Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        //Input.PressKey(VirtualKeyCode.RETURN);
        Thread.Sleep(50);
        // Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W);

        SetVector3(new Vector3(rotation.Degrees.Z, rotation.Degrees.X, rotation.Degrees.Y));


        //todo create method for the process of settings and reading vector3's in the ui
        Log.Information("Setting the Roll");
        // Setting the Roll
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300); // Sleeping for the edit menu to show
        ClipboardService.SetText(rotation.Degrees.X.ToString());
        Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);

        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W);

        Log.Information("Setting the Pitch");
        // Setting the Pitch
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300); // Sleeping for the edit menu to show
        ClipboardService.SetText(rotation.Degrees.X.ToString());
        Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);

        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W);

        Log.Information("Setting the Yaw");
        // Setting the Yaw
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300); // Sleeping for the edit menu to show
        ClipboardService.SetText(rotation.Degrees.X.ToString());
        Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);

        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);


        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_Q);
        Log.Information("----End of item----");
    }
    
}

return;
*/

//77 443 - if static by default
//77 296 - dynamic only
//77 331 - not static by default


// ForgeUI.SetHaloProcess();

//  TraverseFolder(ForgeObjectBrowser.Categories["Decals"]);

// var json = JsonConvert.SerializeObject(ForgeObjectBrowser.Categories);
// File.WriteAllText(json, "Z://josh/itemNames.json");
// CollectScale();

/* 
        for (int i = 0; i < 40; i++)
        {
            Input.Simulate.Mouse.VerticalScroll(1);
            await Task.Delay(5);
        }
        
int delay = 4;
while (true)
{
    for (int i = 0; i < 50; i++)
    {
       // Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_S);
       Input.PressKey(VirtualKeyCode.VK_S,delay);
       // await Task.Delay(delay);
    }

    await Task.Delay(1000);

    for (int i = 0; i < 50; i++)
    {
       // Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
       Input.PressKey(VirtualKeyCode.VK_W,delay);
       // await Task.Delay(delay);
    }

    await Task.Delay(1000);
    */

        var settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        var json = JsonConvert.SerializeObject(ForgeObjectBrowser.Categories, settings);
        File.WriteAllText("Z:/josh/godhelpme.json", json);
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


    public static void GetUIData()
    {
        foreach (var category in ForgeObjectBrowser.Categories)
        {
            if (Input.EXIT)
            {
                return;
            }

            Input.PressKey(VirtualKeyCode.VK_Z);
            Input.PressKey(VirtualKeyCode.VK_Z);

            for (int i = 0; i < category.Value.CategoryOrder - 1; i++) // travel to category
            {
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                Thread.Sleep(50);
            }

            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);

            //the category folder is open
            foreach (var folder in category.Value.CategoryFolders)
            {
                //start moving though the folder
                if (Input.EXIT)
                {
                    return;
                }

                for (int i = 0; i < folder.Value.FolderOffset; i++)
                {
                    // Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                    //  Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
                }
            }
        }
    }

    public static void UIData()
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
        var data = File.ReadAllLines(strWorkPath + "/ForgeObjects.txt");

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
            var itemNameName = FixCapatial(itemName[x].ToLower());

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


            ForgeObjectBrowser.Categories[rootFolderName].CategoryFolders[subFolderName]
                .AddItem(itemNameName, Enum.Parse<ObjectId>(itemID[x]));
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


    private static void SetSelectedScale(float axis)
    {
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300);

        //todo make all clipboard actions use setdata with retry times
        //  IDataObject dataObject = new DataObject();
        //   dataObject.SetData(ConvertToDataFormats(TextDataFormat.UnicodeText), false, axis.ToString());
        ClipboardService.SetText(axis.ToString());
        // Clipboard.SetDataObject(dataObject, false, 5, 10);


        Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);

        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300);
    }
}