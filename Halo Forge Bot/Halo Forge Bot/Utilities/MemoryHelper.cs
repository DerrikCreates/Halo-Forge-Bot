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

    public static readonly string SubBrowserHover = "HaloInfinite.exe+0x42CCE70,BB0,950";
    public static readonly string TopBrowserHover = "HaloInfinite.exe+0x49FD8A0,1A8";
    public static readonly string ScrollBar = "HaloInfinite.exe+0x42CCE70,0xBA0,0x934";
    public static readonly string EditPropertyBool = "HaloInfinite.exe+429CBC8,0x158,0x19ED"; // Not a good pointer


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
            case "int32":
                memoryType = "int";
                break;
            case "Byte":
                memoryType = "byte";
                break;
            case "int16":
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
            case "int64":
                memoryType = "long";
                break;
            default:
                throw new NotSupportedException($"Type:{data.GetType().FullName}, is not supported");
                break;
        }

        Memory.WriteMemory(address, memoryType, data.ToString());
    }

    public static int GetEditMenuState()
    {
        return ReadMemory<int>(EditPropertyBool);
    }

    public static int GetBrowserHover()
    {
        Log.Verbose("Getting browser hover");
        return ReadMemory<int>(RootBrowserHover);
    }

    public static int GetSubBrowserHover()
    {
        Log.Verbose("Getting sub browser hover");
        return ReadMemory<int>(SubBrowserHover);
    }

    public static void SetBrowserHover(int data)
    {
        Log.Debug("Setting Browser Hover with Data: {Data}", data);
        WriteMemory(RootBrowserHover, data);
    }

    public static void SetSubBrowserHover(int data)
    {
        Log.Debug("Setting Browser Hover with Data: {Data}", data);
        WriteMemory(SubBrowserHover, data);
    }
}