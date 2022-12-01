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


        while (await ClipboardService.GetTextAsync() != data)
        {
            await ClipboardService.SetTextAsync(data);
        }


        while (MemoryHelper.GetEditBoxText() != data)
        {
            await Task.Delay(25);
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
            await Task.Delay(25);

            var toType = data.ToCharArray();
            foreach (var c in toType)
            {
                SendKeys.SendWait(c.ToString());
                await Task.Delay(10);
            }

            //Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            await Task.Delay(100);
        }

        await NavigationHelper.CloseEditUI();
    }

    /// <summary>
    /// Handles setting the forge item's position
    /// </summary>
    /// <param name="position"></param>
    public static async Task SetPositionProperty(Vector3 position, ForgeUIObjectModeEnum itemObjectMode)
    {
        await SetProperty(position.X.ToString("F3"), ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Forward, itemObjectMode));
        await SetProperty(position.Y.ToString("F3"), ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Horizontal, itemObjectMode));
        await SetProperty(position.Z.ToString("F3"), ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Vertical, itemObjectMode));
    }

    /// <summary>
    /// Handles setting the forge item's scale
    /// </summary>
    /// <param name="scale"></param>
    public static async Task SetScaleProperty(Vector3 scale, ForgeUIObjectModeEnum itemObjectMode)
    {
        var realScale = MemoryHelper.GetSelectedScale() * scale;
        
        await SetProperty(realScale.X.ToString("F3"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.SizeX, itemObjectMode));
        
        await SetProperty(realScale.Y.ToString("F3"),
            ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.SizeY, itemObjectMode));
        
        await SetProperty(realScale.Z.ToString("F3"),
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
            await SetProperty(newRot.Z.ToString("F3"),
                ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Yaw, itemObjectMode));
            await SetProperty((-newRot.Y).ToString("F3"),
                ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Pitch, itemObjectMode));
            await SetProperty((newRot.X).ToString("F3"),
                ObjectPropertiesOptions.GetPropertyIndex(ObjectPropertyName.Roll, itemObjectMode));
            await Task.Delay(50);
        }
        else
        {
            await SetProperty(rotation.Z.ToString("F3"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Roll]);
            await SetProperty(rotation.X.ToString("F3"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Pitch]);
            await SetProperty(rotation.Y.ToString("F3"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Yaw]);
            await Task.Delay(50);
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