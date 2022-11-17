using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using ManagedWinapi.Hooks;
using Serilog;
using WindowsInput;
using WindowsInput.Native;

namespace Halo_Forge_Bot;

public static class Input
{
    public static InputSimulator Simulate = new InputSimulator();
    public static LowLevelMouseHook MouseHook = new LowLevelMouseHook();
    public static LowLevelKeyboardHook KeyboardHook = new LowLevelKeyboardHook();
    
    public static bool InputActive = false;

    public static void InitInput()
    {
        
        InputActive = true;
        if (!MouseHook.Hooked) MouseHook.StartHook();
        if (!KeyboardHook.Hooked) KeyboardHook.StartHook();
        KeyboardHook.KeyIntercepted +=
            (int msg, int code, int scanCode, int flags, int time, IntPtr info, ref bool handled) =>
            {
                if (code == (int)VirtualKeyCode.VK_0)
                {
                    MouseHook.Dispose();
                    KeyboardHook.Dispose();
                    
                    throw new Exception($"Implement proper exit here");
                }
            };
    }
    
    
    /// <summary>
    /// Modifier key defaults to control
    /// </summary>
    /// <param name="area"> Area on the screen to monitor</param>
    /// <param name="key"> main key to press</param>
    /// <param name="mod"> Controls what secondary key should be pressed </param>
    /// <param name="keySleep">the time to sleep in between each key action</param>
    public static void PressWithMonitor(Rectangle area, VirtualKeyCode key,
        VirtualKeyCode mod = VirtualKeyCode.NONAME, int keySleep = 50)
    {
        if (mod == VirtualKeyCode.NONAME)
        {
            Log.Information(
                "PressWithMonitor Starting -- Pressing {Key} With Monitor, Sleep:{KeySleep}"
                , key, keySleep);
        }
        else
        {
            Log.Information(
                "PressWithMonitor Starting -- Pressing {Key} With Monitor, Sleep:{KeySleep}, ModKey:{ModKey} "
                , key, keySleep, mod);
        }

        bool hasChanged = false;
        var task = Task.Run((() => PixelReader.WatchForChange(ref hasChanged, area, 1000, 10)));

        while (task.Status != TaskStatus.Running)
        {
            Thread.Sleep(1);
        }


        PressKey(key, keySleep, mod);
        while (hasChanged == false)
        {
        }
    }

    /// <summary>
    /// Press any supplied key and insure Infinite doesn't eat it
    /// </summary>
    /// <param name="key"> The VirtualKeyCode to press </param>
    /// <param name="sleep"> The delay between any key actions </param>
    /// <param name="mod"> Controls what secondary key should be pressed </param>
    public static void PressKey(VirtualKeyCode key, int sleep = 50, VirtualKeyCode mod = VirtualKeyCode.NONAME)
    {
        if (mod != VirtualKeyCode.NONAME)
        {
            Thread.Sleep(sleep);
            Input.Simulate.Keyboard.KeyDown(key);
            Thread.Sleep(sleep);
            Input.Simulate.Keyboard.KeyUp(key);
            Thread.Sleep(sleep);
            Log.Information("Pressing {Key} Sleep:{KeySleep}, Modkey: None"
                , key, sleep);
            return;
        }

        Log.Information("Pressing {Key} with modifier {mod} Sleep:{KeySleep}"
            , key, mod, sleep);
        Thread.Sleep(sleep);
        Input.Simulate.Keyboard.KeyDown(mod);
        Thread.Sleep(sleep);
        Input.Simulate.Keyboard.KeyDown(key);
        Thread.Sleep(sleep);
        Input.Simulate.Keyboard.KeyUp(key);
        Thread.Sleep(sleep);
        Input.Simulate.Keyboard.KeyUp(mod);
        Thread.Sleep(sleep);
    }

    public static void PressMultipleTimes(int count, VirtualKeyCode key, Rectangle rectangle, int delay = 10,
        int timeout = 1000)
    {
        for (int i = 0; i < count; i++)
        {
            bool hasChanged = false;
            Task.Run(() => { PixelReader.WatchForChange(ref hasChanged, rectangle, 1000, delay); });

            PressKey(key);

            while (hasChanged == false) //todo make this not a bool check use tasls
            {
                // Console.WriteLine("waiting");
            }

            Thread.Sleep(50);
        }
    }
}