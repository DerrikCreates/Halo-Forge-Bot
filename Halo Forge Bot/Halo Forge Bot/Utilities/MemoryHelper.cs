using System;
using System.Numerics;
using System.Threading;
using Memory;
using Serilog;

namespace Halo_Forge_Bot.Utilities;

public static class MemoryHelper
{
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
        return Memory.ReadMemory<int>(HaloPointers.GlobalHover);
    }

    public static string GetEditBoxText()
    {
        Memory.ReadString(HaloPointers.EditNameBox, length: 80, zeroTerminated: false);
        var s = MemoryHelper.Memory.ReadString(HaloPointers.EditNameBox, length: 80, zeroTerminated: false);
        var sep = new[] { "\0\0", };
        var str = s.Split(sep, StringSplitOptions.TrimEntries)[0];
        str = str.Replace("\0", "");
        return str;
    }

    public static Vector3 GetSelectedPosition()
    {
        // return Memory.ReadMemory<decimal>(HaloPointers._xPositionUI);

        var x = ReadMemory<decimal>(HaloPointers._xPositionUI);
        var y = ReadMemory<decimal>(HaloPointers._yPositionUI);
        var z = ReadMemory<decimal>(HaloPointers._zPositionUI);

        return new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), (Convert.ToSingle(z)));
    }

    public static Vector3 GetSelectedScale()
    {
        var x = Memory.ReadFloat(HaloPointers._xScaleUI);
        var y = Memory.ReadFloat(HaloPointers._yScaleUI);
        var z = Memory.ReadFloat(HaloPointers._zScaleUI);

        return new Vector3(x, y, z);
    }

    public static Vector3 GetSelectedRotation()
    {
        var x = ReadMemory<decimal>(HaloPointers._xRotationUI);
        var y = ReadMemory<decimal>(HaloPointers._xRotationUI);
        var z = ReadMemory<decimal>(HaloPointers._xRotationUI);

        return new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), (Convert.ToSingle(z)));
    }

    public static void FreezeScroll(int data)
    {
        Memory.FreezeValue(HaloPointers.BrowserScroll, "int", data.ToString());
    }

    public static void UnFreezeScroll()
    {
        Memory.UnfreezeValue(HaloPointers.BrowserScroll);
    }

    public static void FreezeGlobalHover(int data)
    {
        Memory.FreezeValue(HaloPointers.GlobalHover, "int", data.ToString());
    }

    public static void UnFreezeGlobalHover()
    {
        Memory.UnfreezeValue(HaloPointers.GlobalHover);
    }

    public static void SetGlobalHover(int data)
    {
        WriteMemory(HaloPointers.GlobalHover, data);
    }

    public static int GetEditMenuState()
    {
        var i = ReadMemory<byte>(HaloPointers.EditPropertyBool);
        Log.Debug("EditMenuState: {State}", i);
        return i;
    }

    public static int GetBrowserHover()
    {
        Log.Verbose("Getting browser hover");
        return ReadMemory<int>(HaloPointers.RootBrowserHover);
    }

    public static int GetSubBrowserHover()
    {
        Log.Verbose("Getting sub browser hover");
        return ReadMemory<int>(HaloPointers.SubBrowserHover);
    }

    public static void SetBrowserHover(int data)
    {
        Log.Debug("Setting Browser Hover with Data: {Data}", data);
        WriteMemory(HaloPointers.RootBrowserHover, data);
    }

    public static void SetSubBrowserHover(int data)
    {
        Log.Debug("Setting Browser Hover with Data: {Data}", data);
        WriteMemory(HaloPointers.SubBrowserHover, data);
    }

    public static int GetTopBrowserHover()
    {
        return ReadMemory<int>(HaloPointers.TopBrowserHover);
    }

    public static int GetBrowserScroll()
    {
        return ReadMemory<int>(HaloPointers.BrowserScroll);
    }

    public static void SetBrowserScroll(int data)
    {
        WriteMemory(HaloPointers.BrowserScroll, data);
    }

    public static int GetMenusVisible()
    {
        return ReadMemory<int>(HaloPointers.UIMenuVisible);
    }
}