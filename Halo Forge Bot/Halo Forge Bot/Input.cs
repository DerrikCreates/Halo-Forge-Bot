using System;
using System.Threading.Tasks;
using ManagedWinapi.Hooks;
using WindowsInput;
using WindowsInput.Native;


namespace ForgeMacros;

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
                if (code == (int)VirtualKeyCode.ESCAPE)
                {
                    MouseHook.Dispose();
                    KeyboardHook.Dispose();
                    
                    throw new Exception($"Implement proper exit here");
                }
            };
    }

    public static void WaitForInput(VirtualKeyCode key)
    {
        while (!Simulate.InputDeviceState.IsHardwareKeyDown(key))
        {
            Simulate.Keyboard.Sleep(10);
        }
    }
}