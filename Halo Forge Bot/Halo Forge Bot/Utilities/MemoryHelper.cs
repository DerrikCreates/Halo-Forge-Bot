using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Halo_Forge_Bot.GameUI;
using Memory;
using Newtonsoft.Json;
using Serilog;

namespace Halo_Forge_Bot.Utilities;

public static class MemoryHelper
{
    public static readonly Mem Memory = new();

    public static readonly HaloPointers HaloPointers =
        JsonConvert.DeserializeObject<HaloPointers>(File.ReadAllText("./config/halo_pointers.json")) ??
        throw new NullReferenceException("/config/halo_pointers.json file has an issue");

    public static T ReadMemory<T>(string address)
    {
        // Log.Debug("Reading Memory of type: {Type} at address {Address}", typeof(T).ToString(), address);
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

    /// <summary>
    /// Grabs the current UI vertical index and handles any weird pointer issues
    /// </summary>
    /// <returns></returns>
    public static async Task<int> GetGlobalHover()
    {
        //Todo get better pointer so we don't have to ignore high values
        var ret = Memory.ReadMemory<int>(HaloPointers.GlobalHover);
        while (ret > 10000)
        {
            await Task.Delay(10);
            ret = Memory.ReadMemory<int>(HaloPointers.GlobalHover);
            await Input.HandlePause();
        }

        return ret;
    }

    /// <summary>
    /// Sometimes GlobalHover can be set to 0 before its actually at 0, this checks that the value is actually 0
    /// </summary>
    /// <returns> The actual hover value </returns>
    public static async Task<int> GetGlobalHoverVerbose()
    {
        //If the first value isn't 0 then it hasn't been reset
        var firstResult = await GetGlobalHover();
        if (firstResult != 0) return firstResult;

        //If the first value is 0 then check again after a delay to make sure that it is actually 0
        await Task.Delay(10);
        var ret = await GetGlobalHover();

        while (ret != firstResult)
        {
            firstResult = await GetGlobalHover();
            await Task.Delay(10);
            ret = await GetGlobalHover();
            await Input.HandlePause();
        }

        return ret;
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

        var x = ReadMemory<decimal>(HaloPointers.XPositionUi);
        var y = ReadMemory<decimal>(HaloPointers.YPositionUi);
        var z = ReadMemory<decimal>(HaloPointers.ZPositionUi);

        return new Vector3(Convert.ToSingle(x), Convert.ToSingle(y), (Convert.ToSingle(z)));
    }

    public static Vector3 GetSelectedScale()
    {
        var x = Memory.ReadFloat(HaloPointers.XScaleUi);
        var y = Memory.ReadFloat(HaloPointers.YScaleUi);
        var z = Memory.ReadFloat(HaloPointers.ZScaleUi);

        return new Vector3(x, y, z);
    }

    public static Vector3 GetSelectedRotation()
    {
        var x = ReadMemory<decimal>(HaloPointers.XRotationUi);
        var y = ReadMemory<decimal>(HaloPointers.XRotationUi);
        var z = ReadMemory<decimal>(HaloPointers.XRotationUi);

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

    [Obsolete]
    public static int GetBrowserHover()
    {
        Log.Verbose("Getting browser hover");
        return ReadMemory<int>(HaloPointers.RootBrowserHover);
    }

    [Obsolete]
    public static int GetSubBrowserHover()
    {
        Log.Verbose("Getting sub browser hover");
        return ReadMemory<int>(HaloPointers.SubBrowserHover);
    }

    [Obsolete]
    public static void SetBrowserHover(int data)
    {
        Log.Debug("Setting Browser Hover with Data: {Data}", data);
        WriteMemory(HaloPointers.RootBrowserHover, data);
    }

    [Obsolete]
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
        return ReadMemory<int>(HaloPointers.UiMenuVisible);
    }

    public static int GetItemCount()
    {
        return ReadMemory<int>(HaloPointers.ItemCount);
    }

    /// <summary>
    /// Sets the scale of an item in memory. Does not apply instantly. Need to select and deselect the item changed
    /// </summary>
    /// <param name="scale"></param>
    public static void SetItemScale(int itemIndex, Vector3 scale)
    {
        var ptr = Memory.Get64BitCode(HaloPointers.SetScaleItemArray);


        int itemScaleSize = 0x1E0;


        ptr += itemScaleSize * itemIndex + 0xB0;

        WriteMemory(ptr.ToString("x8"), scale.X);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), scale.Y);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), scale.Z);
    }

    public static void FreezeItemScale(int itemIndex, Vector3 scale)
    {
        var ptr = Memory.Get64BitCode(HaloPointers.SetScaleItemArray);


        int itemScaleSize = 0x1E0;


        ptr += itemScaleSize * itemIndex + 0xB0;

        Memory.FreezeValue(ptr.ToString("x8"), "float", scale.X.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", scale.Y.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", scale.Z.ToString());
    }

    public static void UnFreezeItemScale(int itemIndex)
    {
        var ptr = Memory.Get64BitCode(HaloPointers.SetScaleItemArray);


        int itemScaleSize = 0x1E0;


        ptr += itemScaleSize * itemIndex + 0xB0;

        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
    }

    public static void SetItemPosition(int itemIndex, Vector3 pos)
    {
        var ptr = Memory.Get64BitCode(HaloPointers.SetSetPositionItemArray);

        int itemScaleSize = 0x310;

        ptr += itemScaleSize * itemIndex + 0x8C;

        WriteMemory(ptr.ToString("x8"), pos.X);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), pos.Y);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), pos.Z);
    }


    public static void FreezeItemPosition(int itemIndex, Vector3 pos)
    {
        var ptr = Memory.Get64BitCode(HaloPointers.SetSetPositionItemArray);

        int itemScaleSize = 0x310;

        ptr += itemScaleSize * itemIndex + 0x8C;

        Memory.FreezeValue(ptr.ToString("x8"), "float", pos.X.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", pos.Y.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", pos.Z.ToString());
    }

    public static void UnFreezeItemPosition(int itemIndex)
    {
        var ptr = Memory.Get64BitCode(HaloPointers.SetSetPositionItemArray);

        int itemScaleSize = 0x310;

        ptr += itemScaleSize * itemIndex + 0x8C;

        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
    }


    public static Vector3 ReadItemPosition(int itemIndex)
    {
        var vector = new Vector3();
        var ptr = Memory.Get64BitCode(HaloPointers.SetSetPositionItemArray);

        int itemScaleSize = 0x310;

        ptr += itemScaleSize * itemIndex + 0x8C;

        vector.X = (float)ReadMemory<decimal>(ptr.ToString("x8"));
        ptr += 4;
        vector.Y = (float)ReadMemory<decimal>(ptr.ToString("x8"));
        ptr += 4;
        vector.Z = (float)ReadMemory<decimal>(ptr.ToString("x8"));
        return vector;
    }

    public static void SetItemRotation(int itemIndex, Vector3 forward, Vector3 up)
    {
        forward = Vector3.Normalize(forward);
        up = Vector3.Normalize(up);

        var left = Vector3.Cross(up, forward);
        left = Vector3.Normalize(left);

        var ptr = Memory.Get64BitCode(HaloPointers.SetSetPositionItemArray);

        int itemScaleSize = 0x310;
        ptr += itemScaleSize * itemIndex + 0x68;

        WriteMemory(ptr.ToString("x8"), forward.X);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), forward.Y);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), forward.Z);
        ptr += 4;

        WriteMemory(ptr.ToString("x8"), left.X);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), left.Y);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), left.Z);
        ptr += 4;

        WriteMemory(ptr.ToString("x8"), up.X);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), up.Y);
        ptr += 4;
        WriteMemory(ptr.ToString("x8"), up.Z);
    }

    public static void FreezeItemRotation(int itemIndex, Vector3 forward, Vector3 up)
    {
        forward = Vector3.Normalize(forward);
        up = Vector3.Normalize(up);

        var left = Vector3.Cross(up, forward);
        left = Vector3.Normalize(left);

        var ptr = Memory.Get64BitCode(HaloPointers.SetSetPositionItemArray);

        int itemScaleSize = 0x310;
        ptr += itemScaleSize * itemIndex + 0x68;

        Memory.FreezeValue(ptr.ToString("x8"), "float", forward.X.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", forward.Y.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", forward.Z.ToString());
        ptr += 4;

        Memory.FreezeValue(ptr.ToString("x8"), "float", left.X.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", left.Y.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", left.Z.ToString());
        ptr += 4;

        Memory.FreezeValue(ptr.ToString("x8"), "float", up.X.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", up.Y.ToString());
        ptr += 4;
        Memory.FreezeValue(ptr.ToString("x8"), "float", up.Z.ToString());
    }

    public static void UnFreezeItemRotation(int itemIndex)
    {
       

        var ptr = Memory.Get64BitCode(HaloPointers.SetSetPositionItemArray);

        int itemScaleSize = 0x310;
        ptr += itemScaleSize * itemIndex + 0x68;

        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;

        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;

        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
        ptr += 4;
        Memory.UnfreezeValue(ptr.ToString("x8"));
    }

    public static float GetFolderHover()
    {
        return ReadMemory<short>(HaloPointers.FolderHover);
    }
}