using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using BondReader.Schemas.Items;
using InfiniteForgeConstants.Forge_UI.Object_Properties;
using Memory;
using Microsoft.VisualBasic.Logging;
using TextCopy;
using WindowsInput.Native;
using Serilog;
using Log = Serilog.Log;

namespace Halo_Forge_Bot.Utilities;

public static class PropertyHelper
{
    /// <summary>
    /// Returns the data inside a property field
    /// </summary>
    /// <param name="index"> The index in the UI that this property is located </param>
    public static async Task<string> GetProperty(int index)
    {
        await NavigationHelper.OpenEditUI(index);
        var ret = MemoryHelper.GetEditBoxText();
        await NavigationHelper.CloseEditUI(true);
        return ret;
    }

    /// <summary>
    /// Handles setting a specific property field
    /// </summary>
    /// <param name="data"> The data to put into the field </param>
    /// <param name="index"> The index in the UI that this property is located </param>
    private static async Task SetProperty(string data, int index)
    {
        Log.Information("Setting property at Index:{Index} with value: {Value}", index, data);
        if (data == "") // not sure if its possible to ever be "" but just in case.
        {
            data = "0";
        }

        //Setup data to type
        var optimizedLength = data.Length;

        //Optimize buffer to remove unnecessary 0s
        if (data.Length > 4)
        {
            if (data[^3] == '.' && data[^1] == '0' && data != "0")
            {
                //Data is number, check if can optimize last digits
                if (data[^2] == '0')
                {
                    optimizedLength -= 3;
                }
                else
                {
                    optimizedLength -= 1;
                }
            }
        }

        var buffer = data[..optimizedLength].ToCharArray();
        var optimizedData = new string(buffer);

        await NavigationHelper.OpenEditUI(index);

        while (MemoryHelper.GetEditBoxText() != optimizedData)
        {
            await Task.Delay(25);
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);

            foreach (var c in buffer)
            {
                await Task.Delay(25);
                SendKeys.SendWait(c.ToString());
            }

            await Task.Delay(25);
            await Input.HandlePause();
        }

        await NavigationHelper.CloseEditUI();
    }

    /// <summary>
    /// Handles setting the forge item's position
    /// </summary>
    /// <param name="position"></param>
    public static async Task SetPositionProperty(Vector3 position, ForgeUIObjectModeEnum itemObjectMode)
    {
        await SetProperty(Math.Round(position.X, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Forward, itemObjectMode));
        await SetProperty(Math.Round(position.Y, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Horizontal, itemObjectMode));
        await SetProperty(Math.Round(position.Z, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Vertical, itemObjectMode));
    }
    
    public static async Task SetForwardProperty(float value, ForgeUIObjectModeEnum itemObjectMode)
    {
        await SetProperty(Math.Round(value, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Forward, itemObjectMode));
        
    }

    /// <summary>
    /// Handles setting the forge item's scale
    /// </summary>
    /// <param name="scale"></param>
    public static async Task SetScaleProperty(Vector3 scale, ForgeUIObjectModeEnum itemObjectMode)
    {
        if (itemObjectMode is ForgeUIObjectModeEnum.DYNAMIC or ForgeUIObjectModeEnum.DYNAMIC_FIRST
            or ForgeUIObjectModeEnum.DYNAMIC_FIRST_VARIANT)
            return;

        var realScale = MemoryHelper.GetSelectedScale() * scale;

        await SetProperty(Math.Round(realScale.X, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.SizeX, itemObjectMode));

        await SetProperty(Math.Round(realScale.Y, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.SizeY, itemObjectMode));

        await SetProperty(Math.Round(realScale.Z, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.SizeZ, itemObjectMode));
    }

    /// <summary>
    /// Handles setting the forge item's rotation
    /// </summary>
    /// <param name="rotation"></param>
    /// <param name="isBlender"> Changes how rotation is handled </param>
    public static async Task SetRotationProperty(Vector3 rotation, ForgeUIObjectModeEnum itemObjectMode)
    {
        await SetProperty(Math.Round(rotation.Z, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Roll, itemObjectMode));

        await SetProperty(Math.Round(rotation.X, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Pitch, itemObjectMode));

        await SetProperty(Math.Round(rotation.Y, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Yaw, itemObjectMode));
    }

    public static async Task SetBlenderRotationProperty(Vector3 rotation, ForgeUIObjectModeEnum itemObjectMode)
    {
        //todo make sure all axis are within the halo infinite YPR range -180/180 -90/90 this should fix most of the rotation flipping issues
        var rot = rotation; // Utils.ToDegree(Utils.ToEulerAnglesZ(Utils.ToQuaternionZ(rotation)));

        await SetProperty(Math.Round(rot.X, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Roll, itemObjectMode));

        await SetProperty(Math.Round(-rot.Y, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Pitch, itemObjectMode));

        await SetProperty(Math.Round(rot.Z, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Yaw, itemObjectMode));

        await SetProperty(Math.Round(rot.X, 2).ToString("F2"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Roll, itemObjectMode));
    }

    /// <summary>
    /// Sets all of the properties of a forge item given the supplied itemSchema
    /// </summary>
    /// <param name="itemSchema"> The forge item data </param>
    /// <param name="isBlender"> Changes how rotation is handled </param>
    public static async Task SetMainProperties(ForgeItem itemSchema, ForgeUIObjectModeEnum itemObjectMode,
        bool isBlender = false)
    {
        await NavigationHelper.OpenUI(NavigationHelper.ContentBrowserTabs.ObjectProperties);

        await SetScaleProperty(new Vector3(itemSchema.ScaleX, itemSchema.ScaleY, itemSchema.ScaleZ), itemObjectMode);

        await SetPositionProperty(new Vector3(itemSchema.PositionX * 10, itemSchema.PositionY * 10,
            itemSchema.PositionZ * 10), itemObjectMode);


        if (isBlender)
        {
            var rot = Utils.ToDegree(new Vector3(itemSchema.RotationX, itemSchema.RotationY, itemSchema.RotationZ));
            await SetBlenderRotationProperty(rot, itemObjectMode);
        }
        else
        {
            await SetRotationProperty(Utils.DidFishSaveTheDay(
                new Vector3(itemSchema.ForwardX, itemSchema.ForwardY, itemSchema.ForwardZ),
                new Vector3(itemSchema.UpX, itemSchema.UpY, itemSchema.UpZ)), itemObjectMode);
        }
    }

    public static async Task SetPropertiesFromMemory(ForgeItem itemSchema, int itemIndex)
    {
        MemoryHelper.SetItemPosition(itemIndex,
            new Vector3(itemSchema.PositionX, itemSchema.PositionY, itemSchema.PositionZ));

        MemoryHelper.SetItemRotation(itemIndex,
            new Vector3(itemSchema.ForwardX, itemSchema.ForwardY, itemSchema.ForwardZ),
            new Vector3(itemSchema.UpX, itemSchema.UpY, itemSchema.UpZ));

        MemoryHelper.SetItemScale(itemIndex, new Vector3(itemSchema.ScaleX, itemSchema.ScaleY, itemSchema.ScaleZ));
    }
}