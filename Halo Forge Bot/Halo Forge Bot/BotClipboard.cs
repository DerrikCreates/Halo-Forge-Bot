using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using Serilog;
using TextCopy;
using WindowsInput.Native;
using WK.Libraries.SharpClipboardNS;

namespace Halo_Forge_Bot;

public static class BotClipboard
{
    public static SharpClipboard clipboard = new SharpClipboard();
    public static string ClipboardText;
    public static bool scuffedWaitBool = false;

    public static bool GetClipboardChange(out string clipboardTextout, bool clearClipboard = true)
    {
        ClipboardService.SetText("");
        //var oldText = ClipboardService.GetText();
        //todo refactor this to have the rectangle as a param
        Input.PressWithMonitor(ForgeUI.RenameBox, VirtualKeyCode.VK_X,
            VirtualKeyCode.CONTROL); // Input.PressKey(VirtualKeyCode.VK_X, mod: );

        Thread.Sleep(50); // delay to make sure the data is in the clipboard

        var newText = ClipboardService.GetText();


        if ("" == newText)
            throw new EvaluateException($"Clipboard text is empty {ClipboardText}");


        clipboardTextout = newText;
        return true;
    }

    public static void ClipboardChanged(object? sender, SharpClipboard.ClipboardChangedEventArgs e)
    {
        if (e.ContentType == SharpClipboard.ContentTypes.Text)
        {
           // Log.Information("Clipboard Changed old:{LastData} new:{CurrentData}", ClipboardText, )
        }
    }
}