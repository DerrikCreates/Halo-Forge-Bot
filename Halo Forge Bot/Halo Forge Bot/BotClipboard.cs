using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using TextCopy;
using WindowsInput.Native;
using WK.Libraries.SharpClipboardNS;
using Clipboard = System.Windows.Forms.Clipboard;

namespace Halo_Forge_Bot;

public static class BotClipboard
{
    public static SharpClipboard Clipboard = new SharpClipboard();
    public static string ClipboardText;


    public static async Task<string> GetClipboardChange(bool clearClipboard = true)
    {
        //todo refactor this to have the rectangle as a param
        Log.Information("GetClipboardChange starting");
        //Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_X, VirtualKeyCode.CONTROL);
        Input.PressKey(VirtualKeyCode.VK_C, mod: VirtualKeyCode.CONTROL);
        ClipboardText = await ClipboardService.GetTextAsync();
        Log.Information("New clipboard text is {ClipboardText} and is type {DataType}");
        Input.PressKey(VirtualKeyCode.BACK);
        // throw new Exception("Clipboard didnt change timeout reached");
        return ClipboardText;
    }

    private static bool testCheck = false;

    public static void ClipboardChanged(object? sender, SharpClipboard.ClipboardChangedEventArgs args)
    {
        Log.Information("SharpClipboard Callback Starting");
        Log.Information($"-------Clipboard Data:{args.Content}:--- Type:" +
                        args.Content.GetType() + " " +
                        args.SourceApplication);


        string clipboard = ClipboardService.GetText();
        ClipboardText = clipboard;

        testCheck = true;
        Log.Information(clipboard + " ClipboardService Lib");
    }
}