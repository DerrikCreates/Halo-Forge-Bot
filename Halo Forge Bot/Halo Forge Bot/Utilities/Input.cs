using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Halo_Forge_Bot.GameUI;
using ManagedWinapi.Hooks;
using Serilog;
using WindowsInput;
using WindowsInput.Native;

namespace Halo_Forge_Bot.Utilities;

public static class Input
{
    public static InputSimulator Simulate = new InputSimulator();

    public static readonly LowLevelMouseHook MouseHook = new LowLevelMouseHook();
    private static readonly Thread EnsureMainControlsThread = new Thread(new ThreadStart(HandleMainControlsThread));
    public static bool InputActive = false;
    public static bool RequestedPaused = false;

    public static void InitInput()
    {
        if (InputActive) return;
        InputActive = true;
        
        EnsureMainControlsThread.Start();
    }

    //Added to remove laggy mouse hook from main input
    public static void InitMouseHook()
    {
        if (!MouseHook.Hooked) MouseHook.StartHook();
    }

    /// <summary>
    /// Ensure you can always exit the bot by using a specific key down event
    /// </summary>
    private static void HandleMainControlsThread()
    {
        // var keyboardHook = new LowLevelKeyboardHook();
        // keyboardHook.StartHook();
        // keyboardHook.KeyIntercepted += InputHook;
        var exit = false;
        var pauseKeyReleased = true;

        while (!exit)
        {
            if (Simulate.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.LEFT))
            {
                exit = true;
                Process.GetCurrentProcess().Kill();
            }

            if (Simulate.InputDeviceState.IsHardwareKeyDown(VirtualKeyCode.LSHIFT))
            {
                if (pauseKeyReleased)
                {
                    pauseKeyReleased = false;
                    RequestedPaused = !RequestedPaused;

                    if (!RequestedPaused)
                    {
                        ForgeUI.SetHaloProcess();
                    }
                }
            }

            if (Simulate.InputDeviceState.IsHardwareKeyUp(VirtualKeyCode.LSHIFT))
                pauseKeyReleased = true;
            
            Thread.Sleep(1);
        }
    }

    /// <summary>
    /// Modifier key defaults to control
    /// </summary>
    /// <param name="area"> Area on the screen to monitor</param>
    /// <param name="key"> main key to press</param>
    /// <param name="mod"> Controls what secondary key should be pressed </param>
    /// <param name="keySleep">the time to sleep in between each key action</param>
    public static async Task<bool> PressWithMonitor(Rectangle area, VirtualKeyCode key,
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
        var task = Task.Run((() => PixelReader.WatchForChange(area, 1000, 10)));

        while (task.Status is TaskStatus.WaitingForActivation or TaskStatus.WaitingToRun)
        {
            Thread.Sleep(1);
        }


        Log.Information("PressWithMonitor - The watch for change task's state is: {TaskStatus}", task.Status);

        PressKey(key, keySleep, mod);
        
        var b = await task;
        if (b == false)
        {
            Log.Error("Press with monitor task did not detect change! Trying 1 more time");
            task = Task.Run((() => PixelReader.WatchForChange(area, 1000, 10)));

            PressKey(key, keySleep, mod);
        }

        var result = await task;
        Log.Error("Press with monitor is retuning with value {Result}", result);
        return result;
    }

    /// <summary>
    /// Press any supplied key and insure Infinite doesn't eat it
    /// </summary>
    /// <param name="key"> The VirtualKeyCode to press </param>
    /// <param name="sleep"> The delay between any key actions </param>
    /// <param name="mod"> Controls what secondary key should be pressed </param>
    public static void PressKey(VirtualKeyCode key, int sleep = 50, VirtualKeyCode mod = VirtualKeyCode.NONAME)

    {
        if (mod == VirtualKeyCode.NONAME)
        {
            Log.Information("Starting to Press {Key} Sleep:{KeySleep}, Modkey: None", key, sleep);
            Thread.Sleep(sleep);
            Input.Simulate.Keyboard.KeyDown(key);
            Thread.Sleep(sleep);
            Input.Simulate.Keyboard.KeyUp(key);
            Thread.Sleep(sleep);
            Log.Information("Pressing ended {Key} Sleep:{KeySleep}, Modkey: None", key, sleep);
            return;
        }

        Log.Information("Pressing {Key} with modifier {mod} Sleep:{KeySleep}", key, mod, sleep);
        //Simulate.Keyboard.ModifiedKeyStroke(mod, key);

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

    public static async void PressMultipleTimes(int count, VirtualKeyCode key, Rectangle rectangle, int delay = 10,
        int timeout = 1000)
    {
        for (int i = 0; i < count; i++)
        {
            bool hasChanged = false;
            Task.Run(() => { PixelReader.WatchForChange(rectangle, 1000, delay); });

            PressKey(key);

            while (hasChanged == false) //todo make this not a bool check use tasls
            {
                // Console.WriteLine("waiting");
            }

            Thread.Sleep(50);
        }
    }

    public static void MoveMouseTo(int x, int y)
    {
        Simulate.Mouse.MoveMouseTo(0, 0);
        Simulate.Mouse.MoveMouseBy(x, y);
    }

    public static void TypeChars(char[] chars)
    {
        foreach (var character in chars)
        {
            PressWithMonitor(ForgeUI.RenameBox, (VirtualKeyCode)character);
            Thread.Sleep(10);
        }
    }

    public static async Task KeyPress(VirtualKeyCode key, int sleepTime, int initialDelay = 0)
    {
        await Task.Delay(initialDelay);
        Simulate.Keyboard.KeyPress(key);
        await Task.Delay(sleepTime);
    }
}