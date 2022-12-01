using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using BondReader.Schemas.Items;
using InfiniteForgeConstants.Forge_UI.Object_Properties;
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
        await Task.Delay(25);
        var ret = MemoryHelper.GetEditBoxText();
        await NavigationHelper.CloseEditUI();
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

        await NavigationHelper.OpenEditUI(index);

        while (MemoryHelper.GetEditBoxText() != data)
        {
            await Task.Delay(25);
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
            await Task.Delay(25);

            var toType = data.ToCharArray();
            foreach (var c in toType)
            {
                SendKeys.SendWait(c.ToString());
                await Task.Delay(20);
            }

            await Task.Delay(200);
        }

        await NavigationHelper.CloseEditUI();
    }

    /// <summary>
    /// Handles setting the forge item's position
    /// </summary>
    /// <param name="position"></param>
    public static async Task SetPositionProperty(Vector3 position, ForgeUIObjectModeEnum itemObjectMode)
    {
        await SetProperty(Math.Round(position.X, 2).ToString("F2"), ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Forward, itemObjectMode));
        await SetProperty(Math.Round(position.Y, 2).ToString("F2"), ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Horizontal, itemObjectMode));
        await SetProperty(Math.Round(position.Z, 2).ToString("F2"), ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Vertical, itemObjectMode));
    }

    /// <summary>
    /// Handles setting the forge item's scale
    /// </summary>
    /// <param name="scale"></param>
    public static async Task SetScaleProperty(Vector3 scale, ForgeUIObjectModeEnum itemObjectMode)
    {
        if (itemObjectMode is ForgeUIObjectModeEnum.DYNAMIC or ForgeUIObjectModeEnum.DYNAMIC_FIRST or ForgeUIObjectModeEnum.DYNAMIC_FIRST_VARIANT)
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
    public static async Task SetRotationProperty(Vector3 rotation, ForgeUIObjectModeEnum itemObjectMode, bool isBlender = false)
    {
        if (isBlender)
        {
            var test = Utils.ToQuaternionZ(rotation); //Utils.ToDegree(blenderRotation); //Utils.QuaternionToYXZ(quat);

            var  newRot = Utils.ToEulerAnglesZ(test);
            newRot = Utils.ToDegree(newRot);
            //newRot = Utils.To180(newRot);

            //newRot.Y = Utils.To90(newRot.Y);
            Log.Error("DEBUG ROT: {ROT}",newRot);
            // newRot.Y = -newRot.Y;
            await SetProperty(Math.Round(newRot.Z, 2).ToString("F2"),
                ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Yaw, itemObjectMode));
            await SetProperty(Math.Round(-newRot.Y, 2).ToString("F2"),
                ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Pitch, itemObjectMode));
            await SetProperty(Math.Round(newRot.X, 2).ToString("F2"),
                ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Roll, itemObjectMode));
        }
        else
        {
            await SetProperty(Math.Round(rotation.Z, 2).ToString("F2"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Roll]);
            await SetProperty(Math.Round(rotation.X, 2).ToString("F2"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Pitch]);
            await SetProperty(Math.Round(rotation.Y, 2).ToString("F2"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Yaw]);
        }
    }

    /// <summary>
    /// Sets all of the properties of a forge item given the supplied itemSchema
    /// </summary>
    /// <param name="itemSchema"> The forge item data </param>
    /// <param name="isBlender"> Changes how rotation is handled </param>
    public static async Task SetMainProperties(ForgeItem itemSchema, ForgeUIObjectModeEnum itemObjectMode, bool isBlender = false)
    {
        await NavigationHelper.OpenUI(NavigationHelper.ContentBrowserTabs.ObjectProperties);
        
        await SetScaleProperty(new Vector3(itemSchema.ScaleX, itemSchema.ScaleY, itemSchema.ScaleZ), itemObjectMode);
        
        await SetPositionProperty(new Vector3(itemSchema.PositionX * 10, itemSchema.PositionY * 10,
            itemSchema.PositionZ * 10), itemObjectMode);
        
        await SetRotationProperty(Utils.DidFishSaveTheDay(
            new Vector3(itemSchema.ForwardX, itemSchema.ForwardY, itemSchema.ForwardZ),
            new Vector3(itemSchema.UpX, itemSchema.UpY, itemSchema.UpZ)), itemObjectMode);
    }
}