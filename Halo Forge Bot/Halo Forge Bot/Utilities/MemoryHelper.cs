using System;
using System.Threading;
using Memory;
using Serilog;

namespace Halo_Forge_Bot.Utilities;

public static class MemoryHelper
{
    public static readonly string
        RootBrowserHover =
            "HaloInfinite.exe+0x482B178,0xB8,0x950"; //todo find the pointer that tracks both root and sub hover

    public static readonly string _GlobalHover = "HaloInfinite.exe+0x42CCE70,0xBA0,0x950";
    public static readonly string _SubBrowserHover = "HaloInfinite.exe+0x42CCE70,0xBB0,0x950";
    public static readonly string _TopBrowserHover = "HaloInfinite.exe+0x49FD8A0,0x1A8";
    public static readonly string _BrowserScroll = "HaloInfinite.exe+0x482B108,0xB8,0x934";
    
    public static readonly string _EditPropertyBool = "HaloInfinite.exe+0x00000482B1F8,0xB5";

    public static readonly string _EditNameBox = "HaloInfinite.exe+0x0000042CC760,0xBA0,0x774";

    private static readonly int
        transformOffset = 0x128; //"HaloInfinite.exe+0x42CCE48,0x80 + transformOffset - Bottom Transform UI 

    private static readonly string _transformPointer = "HaloInfinite.exe+0x42CCE48,0x80";

    public static readonly string
        _xPositionUI = $"{_transformPointer},{transformOffset:X}";

    public static readonly string
        _yPositionUI = $"{_transformPointer},{(transformOffset + 4):X}";

    public static readonly string
        _zPositionUI = $"{_transformPointer},{(transformOffset + 8):X}";

    public static readonly string
        _zRotationUI = $"{_transformPointer},{(transformOffset + 12):X}";

    public static readonly string
        _yRotationUI = $"{_transformPointer},{(transformOffset + 16):X}";

    public static readonly string
        _xRotationUI = $"{_transformPointer},{(transformOffset + 20):X}";

    public static readonly string
        _xScaleUI = $"{_transformPointer},{(transformOffset + 24):X}";

    public static readonly string
        _yScaleUI =
            $"{_transformPointer},0x80,{(transformOffset + 28):X}";

    public static readonly string
        _zScaleUI =
            $"HaloInfinite.exe+0000042CCE48,0x80,{(transformOffset + 32):X}";

    public static readonly Mem Memory = new();

    public static T ReadMemory<T>(string address)
    {
        Log.Debug("Reading Memory of type: {Type} at address {Address}", typeof(T).ToString(), address);
        return Memory.ReadMemory<T>(address);
    }

    public static void WriteMemory<T>(string address, T data)
    {
        Log.Debug("Writing Memory to address: {Address} , Data: {Data}", address, data);
        if (data == null)
        {
            throw new NullReferenceException(
                $"Data provided to WriteMemory is null, DataType: {data.GetType().FullName}");
        }

        string memoryType = "";
        switch (data.GetType().Name)
        {
            case "Int32":
                memoryType = "int";
                break;
            case "Byte":
                memoryType = "byte";
                break;
            case "Int16":
                memoryType = "2byte";
                break;
            case "Byte[]":
                memoryType = "bytes";
                break;
            case "Single":
                memoryType = "float";
                break;
            case "String":
                memoryType = "string";
                break;
            case "Double":
                memoryType = "double";
                break;
            case "Int64":
                memoryType = "long";
                break;
            default:
                throw new NotSupportedException($"Type:{data.GetType().FullName}, is not supported");
                break;
        }

        Memory.WriteMemory(address, memoryType, data.ToString());
    }

    public static int GetGlobalHover()
    {
        return Memory.ReadMemory<int>(_GlobalHover);
        
    }

    public static string GetEditBoxText()
    {
        Memory.ReadString(_EditNameBox, length: 80, zeroTerminated: false);
        var s = MemoryHelper.Memory.ReadString(_EditNameBox, length: 80, zeroTerminated: false);
        var sep = new[] { "\0\0", };
        var str = s.Split(sep, StringSplitOptions.TrimEntries)[0];
        str = str.Replace("\0", "");
        return str;
    }

    public static decimal GetXPosition()
    {
        return Memory.ReadMemory<decimal>(_xPositionUI);
    }

    public static void FreezeGlobalHover(int data)
    {
        Memory.FreezeValue(_GlobalHover, "int", data.ToString());
    }

    public static void UnFreezeGlobalHover()
    {
        Memory.UnfreezeValue(_GlobalHover);
    }

    public static void SetGlobalHover(int data)
    {
        WriteMemory(_GlobalHover, data);
    }

    public static int GetEditMenuState()
    {
        var i = ReadMemory<byte>(_EditPropertyBool);
        Log.Debug("EditMenuState: {State}", i);
        return i;
    }

    public static int GetBrowserHover()
    {
        Log.Verbose("Getting browser hover");
        return ReadMemory<int>(RootBrowserHover);
    }

    public static int GetSubBrowserHover()
    {
        Log.Verbose("Getting sub browser hover");
        return ReadMemory<int>(_SubBrowserHover);
    }

    public static void SetBrowserHover(int data)
    {
        Log.Debug("Setting Browser Hover with Data: {Data}", data);
        WriteMemory(RootBrowserHover, data);
    }

    public static void SetSubBrowserHover(int data)
    {
        Log.Debug("Setting Browser Hover with Data: {Data}", data);
        WriteMemory(_SubBrowserHover, data);
    }

    public static int GetTopBrowserHover()
    {
        return ReadMemory<int>(_TopBrowserHover);
    }

    public static int GetBrowserScroll()
    {
        return ReadMemory<int>(_BrowserScroll);
    }

    public static void SetBrowserScroll(int data)
    {
        WriteMemory(_BrowserScroll, data);
    }
}