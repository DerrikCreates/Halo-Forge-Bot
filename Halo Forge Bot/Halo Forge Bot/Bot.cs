using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ForgeMacros;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using Newtonsoft.Json;
using Serilog;
using WindowsInput.Native;

namespace Halo_Forge_Bot;

public static class Bot
{
    
    public static void GatherItemStrings()
    {
        ForgeUI.SetHaloProcess();
        var folders = ForgeObjectBrowser.Categories["Accents"].CategoryFolders;
        var foldersCount = ForgeObjectBrowser.Categories["Accents"].CategoryFolders.Count;


        ForgeObjectBrowser.Categories["Accents"].CategoryFolders.Values.ToList();
        foreach (var folder in folders)
        {
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.VK_S);
            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);

            while (true)
            {
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.RETURN);
                Thread.Sleep(300);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.F2);
                Thread.Sleep(300);

                BotClipboard.GetClipboardChange(out var clipboard);

                Log.Information("Clipboard contains: {ClipboardText}", clipboard);
                Thread.Sleep(300);
                Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.ESCAPE);
                Thread.Sleep(300);
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
            }


            Input.PressWithMonitor(ForgeUI.ForgeMenu, VirtualKeyCode.BACK);
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
}