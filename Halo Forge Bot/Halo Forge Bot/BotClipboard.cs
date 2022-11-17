using System.Data;
using System.Threading;
using System.Windows;
using WindowsInput.Native;

namespace Halo_Forge_Bot;

public static class BotClipboard
{
    public static bool GetClipboardChange(out string clipboardText)
    {
        var current = Clipboard.GetText();
        Input.PressKey(VirtualKeyCode.VK_C, mod: VirtualKeyCode.CONTROL);
        Thread.Sleep(100);
        clipboardText = Clipboard.GetText();

        if (clipboardText == current)
            throw new EvaluateException($"Clipboard has not changed from previous reading {clipboardText}");
            
        return true;
    }
}