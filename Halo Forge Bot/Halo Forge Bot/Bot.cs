using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Media;
using ForgeMacros;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using Microsoft.Windows.Themes;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using TextCopy;
using WindowsInput.Native;
using WK.Libraries.SharpClipboardNS;
using Clipboard = System.Windows.Forms.Clipboard;


namespace Halo_Forge_Bot;

public static class Bot
{
    public static void GatherItemStrings()
    {
        ForgeUI.SetHaloProcess();

        foreach (var category in ForgeObjectBrowser.Categories)
        {
            if (category.Value.CategoryName is "Recents" or "Prefabs")
            {
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                continue;
            }

            TraverseFolder(category.Value);
        }

        var json = JsonConvert.SerializeObject(ForgeObjectBrowser.Categories);
        File.WriteAllText(json, "Z://josh/itemNames.json");

        // var p = ForgeUI.SetHaloProcess();
        // NativeHelper.SetActiveWindow(p.MainWindowHandle);
        //NativeHelper.SetForegroundWindow(ForgeUI.HaloProcess.MainWindowHandle);
        // PressWithMonitor(ForgeUI.RenameBox,VirtualKeyCode.VK_X);


        //  PressKey(VirtualKeyCode.VK_X, useMod: true);
        //PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_X, true);
    }

    private static void TraverseFolder(ForgeUICategory category)
    {
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);

        foreach (var folder in category.CategoryFolders)
        {
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);

            while (true)
            {
                // collecting items
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
                Thread.Sleep(300);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.F2);
                Thread.Sleep(300);

                BotClipboard.GetClipboardChange(out var clipboard);

                Log.Information("Clipboard contains: {ClipboardText}", clipboard);
                Thread.Sleep(300);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.ESCAPE);
                Thread.Sleep(300);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_Q); // Q into the object properties

                // COLLECT THE DEFAULT SCALE HERE
                for (int i = 0; i < 50; i++)
                {
                    Input.Simulate.Mouse.VerticalScroll(1);
                    Thread.Sleep(5);
                }

                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_Z);

                Vector3 scale = new Vector3();
                var defaultMode = GetDefaultObjectMode();
                switch (defaultMode)
                {
                    case ForgeUIObjectModeEnum.STATIC_FIRST:
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        CollectVector3();
                        break;

                    case ForgeUIObjectModeEnum.DYNAMIC:
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        CollectVector3();
                        break;

                    case ForgeUIObjectModeEnum.DYNAMIC_FIRST:
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        CollectVector3();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                try
                {
                    Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.DELETE);
                }
                catch (Exception e)
                {
                    Log.Warning("Issue with Last Delete keypress trying again");
                    Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_F);
                    Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.DELETE);
                }


                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_Q);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                try
                {
                    folder.Value.AddItem(clipboard);
                }
                catch (Exception e)
                {
                    //Item already exists inside folder so continue onto next item
                    break;
                }

                folder.Value.FolderObjects[clipboard].DefaultScale = scale;
            }

            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.BACK);
        }

        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.BACK);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
    }

    public static void OpenVectorEditUI()
    {
    }

    public static void SetVector3(Vector3 vector)
    {
        string[] vectorStrings = new string[3];
        vectorStrings[0] = vector.X.ToString();
        vectorStrings[1] = vector.Y.ToString();
        vectorStrings[2] = vector.Z.ToString();
        // Assuming we are hovered the FIRST element of the vector in the ui;
        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(50); // small delay before we start
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN); // Enter Edit UI
            Thread.Sleep(350); // Sleep for window open animation
            ClipboardService.SetText(vectorStrings[i]);
            Thread.Sleep(50); // Sleeping just in case the clipboard is not set yet for some reason?
            Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);
            Thread.Sleep(50); // Sleep Extra so that there is time for the paste to apply in the game
            Input.PressWithMonitor(ForgeUI.ForgeMenu,
                VirtualKeyCode.RETURN); // pressing enter to apply and exit the menu
            Thread.Sleep(350); // waiting to make sure you have exited the edit menu fully
            Input.PressWithMonitor(ForgeUI.ForgeMenu,
                VirtualKeyCode.VK_S); // Move down to the next element of the vector

            Thread.Sleep(50); // small delay after
        }
    }

    public static string[] CollectVector3()
    {
        string[] vectorStrings = new string[3];
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300);
        Vector3 scale = new Vector3();
        Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
        Thread.Sleep(50);
        //BotClipboard.GetClipboardChange(out string clipboard);
        var clipboard = ClipboardService.GetText();
        vectorStrings[0] = clipboard;
        scale.X = float.Parse(clipboard.Substring(0, 3));

        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.ESCAPE);
        Thread.Sleep(300);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300);
        BotClipboard.GetClipboardChange(out clipboard);

        vectorStrings[1] = clipboard;
        scale.Y = float.Parse(clipboard.Substring(0, 3));

        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.ESCAPE);
        Thread.Sleep(300);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300);
        BotClipboard.GetClipboardChange(out clipboard);
        vectorStrings[2] = clipboard;
        scale.Z = float.Parse(clipboard.Substring(0, 3));
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.ESCAPE);
        Thread.Sleep(300);
        return vectorStrings;
    }

    

    public static async Task DevTesting()
    {
        ForgeUI.SetHaloProcess();
        

        return;
        while (true)
        {
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
            Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.ESCAPE);
        }

        return;
        ForgeUI.SetHaloProcess();
        await ClipboardService.SetTextAsync("0");
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        // Thread.Sleep(350);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_V, VirtualKeyCode.CONTROL);
        Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.RETURN);
        // Thread.Sleep(350);
        int count = 0;
        while (true)
        {
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
            //Thread.Sleep(350);
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_X, VirtualKeyCode.CONTROL);
            var readInt = int.Parse(await ClipboardService.GetTextAsync());

            readInt++;

            await ClipboardService.SetTextAsync(readInt.ToString());
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_B, VirtualKeyCode.CONTROL);
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
            // Thread.Sleep(350);
        }

        return;
        BlenderMap d = JsonConvert.DeserializeObject<BlenderMap>(File.ReadAllText("z:/josh/Ascent.DCjson"));

        Thread.Sleep(4000);
        foreach (var item in d.ItemList)
        {
            if (item.ItemId == 1759788903)
            {
                var forward = new Vector3(item.ForwardX, item.ForwardY, item.ForwardZ);
                var up = new Vector3(item.UpX, item.UpY, item.UpZ);

                var rotation = Utils.DirectionToEuler(forward, up);
                var position = Vector3.Multiply(10, new Vector3(item.PositionX, item.PositionY, item.PositionZ));

                // Starting at the correct object this enter spawns the item;
                Log.Information("----Starting New Item----");
                Thread.Sleep(300);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
                Thread.Sleep(300);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_R);
                Thread.Sleep(300);


                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_E);

                Log.Information("Scrolling to the top");
                for (int i = 0; i < 25; i++)
                {
                    Input.Simulate.Mouse.VerticalScroll(1);
                    Thread.Sleep(4);
                }

                Log.Information("Traveling to size vector");
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_Z);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);

                Log.Information("Collecting Scale Vector");
                var scaleStrings = CollectVector3();

                Vector3 defaultScale = new Vector3(float.Parse(scaleStrings[0].Substring(0, 3)),
                    float.Parse(scaleStrings[1].Substring(0, 3)), float.Parse(scaleStrings[2].Substring(0, 3)));


                Vector3 scale = new Vector3(item.ScaleX, item.ScaleY, item.ScaleZ);

                Vector3 realScale = Vector3.Multiply(defaultScale, scale);

                Log.Information("Reseting at X scale for writing");
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_W);


                SetVector3(scale); // Setting the scale vector


                /* SetSelectedScale(scale.X);
                 Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                 SetSelectedScale(scale.Y);
                 Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                 SetSelectedScale(scale.Z);
                 */

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
                 */


                Log.Information("Traveling to the roll input");
                //Traveling to the roll input
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);


                SetVector3(rotation.Degrees);

                /*
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


*/
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_Q);
                Log.Information("----End of item----");
            }
        }

        return;


        //77 443 - if static by default
        //77 296 - dynamic only
        //77 331 - not static by default
        var data = File.ReadAllLines("Z:/josh/ForgeObjects.txt");

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

            ForgeObjectBrowser.Categories[rootFolderName].CategoryFolders[subFolderName].FolderObjects
                .Add(itemNameName, new ForgeUIObject(itemNameName, x));
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

    private static string ConvertToDataFormats(TextDataFormat format)
    {
        switch (format)
        {
            case TextDataFormat.Text:
                return DataFormats.Text;

            case TextDataFormat.UnicodeText:
                return DataFormats.UnicodeText;

            case TextDataFormat.Rtf:
                return DataFormats.Rtf;

            case TextDataFormat.Html:
                return DataFormats.Html;

            case TextDataFormat.CommaSeparatedValue:
                return DataFormats.CommaSeparatedValue;
        }

        return DataFormats.UnicodeText;
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

    public enum ForgeUIObjectModeEnum
    {
        STATIC = 0,
        DYNAMIC = 1,
        STATIC_FIRST = 2,
        DYNAMIC_FIRST = 3
    }

    private static ForgeUIObjectModeEnum GetDefaultObjectMode()
    {
        using (var image = PixelReader.ScreenshotArea(Screen.PrimaryScreen.Bounds))
        {
            var staticByDefault = image.GetPixel(77, 433);
            if (staticByDefault == Color.FromArgb(255, 57, 57, 57))
            {
                return ForgeUIObjectModeEnum.STATIC_FIRST;
                //  Log.Information("static by default");
            }

            var dynamicOnly = image.GetPixel(77, 296);
            if (dynamicOnly == Color.FromArgb(255, 57, 57, 57))
            {
                return ForgeUIObjectModeEnum.DYNAMIC;
                // Log.Information("dynamic only");
            }

            var dynamicDefault = image.GetPixel(77, 331);
            if (dynamicDefault == Color.FromArgb(255, 57, 57, 57))
            {
                return ForgeUIObjectModeEnum.DYNAMIC_FIRST;
            }

            throw new Exception("No Object Mode Detected");
            image.Save("z:/josh/fuckoff.png", ImageFormat.Png);
        }
    }
}