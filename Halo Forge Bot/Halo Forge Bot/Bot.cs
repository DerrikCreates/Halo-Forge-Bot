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
using WindowsInput.Native;


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
                        CollectScale();
                        break;

                    case ForgeUIObjectModeEnum.DYNAMIC:
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        CollectScale();
                        break;

                    case ForgeUIObjectModeEnum.DYNAMIC_FIRST:
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
                        CollectScale();
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

    public static Vector3 CollectScale()
    {
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300);
        Vector3 scale = new Vector3();
        BotClipboard.GetClipboardChange(out string clipboard);
        scale.X = float.Parse(clipboard);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.ESCAPE);
        //
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300);
        BotClipboard.GetClipboardChange(out clipboard);
        scale.Y = float.Parse(clipboard);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.ESCAPE);

        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
        Thread.Sleep(300);
        BotClipboard.GetClipboardChange(out clipboard);
        scale.Z = float.Parse(clipboard);
        Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.ESCAPE);

        return scale;
    }

    public static void DevTesting()
    {
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
        var json = JsonConvert.SerializeObject(ForgeObjectBrowser.Categories,settings);
        File.WriteAllText("Z:/josh/godhelpme.json",json );
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