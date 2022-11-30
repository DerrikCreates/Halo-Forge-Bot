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
    private static async Task SetProperty(string data, int index)
    {
        Log.Information("Setting property at Index:{Index} with value: {Value}", index, data);
        if (data == "") // not sure if its possible to ever be "" but just in case.
        {
            data = "0";
        }

        await NavigationHelper.OpenEditUI(index);
        // while (MemoryHelper.GetGlobalHover() != index)
        // {
        //     Input.Simulate.Keyboard.KeyPress(key);
        //     //MemoryHelper.SetGlobalHover(index);
        //     await Task.Delay(Bot.travelSleep);
        // }


        //          var selectedPosition = MemoryHelper.GetSelectedPosition();

        // while (MemoryHelper.GetEditMenuState() == 0)
        // {
        //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        //     await Task.Delay(200);
        // }


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
        // while (MemoryHelper.GetEditMenuState() != 0)
        // {
        //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        //     await Task.Delay(100);
        // }
    }

    public static async Task SetMainProperties(ForgeItem itemSchema, bool isBlender = false)
    {
        await NavigationHelper.OpenUI(NavigationHelper.ContentBrowserTabs.ObjectProperties);
        // while (MemoryHelper.GetMenusVisible() == 0)
        // {
        //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_R);
        //     await Task.Delay(60);
        // }

        // while (MemoryHelper.GetTopBrowserHover() != 1)
        // {
        //     await Task.Delay(60);
        //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_E);
        //     await Task.Delay(60);
        // }

        var defaultScale = MemoryHelper.GetSelectedScale();

        var yScale = itemSchema.ScaleY;
        var xScale = itemSchema.ScaleX;
        var zScale = itemSchema.ScaleZ;

        var itemScale = new Vector3(xScale, yScale, zScale);

        var realScale = (itemScale * defaultScale); //Vector3.Multiply(itemScale, defaultScale) * 10;

        await SetProperty(realScale.X.ToString("F3"),
            ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeX]);
        await SetProperty(realScale.Y.ToString("F3"),
            ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeY]);
        await SetProperty(realScale.Z.ToString("F3"),
            ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeZ]);

        var xPos = itemSchema.PositionX * 10;
        var yPos = itemSchema.PositionY * 10;
        var zPos = itemSchema.PositionZ * 10;

        await SetProperty(xPos.ToString("F3"), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Forward]);
        await SetProperty(yPos.ToString("F3"), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Horizontal]);
        await SetProperty(zPos.ToString("F3"), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Vertical]);

        var yForward = itemSchema.ForwardY;
        var zForward = itemSchema.ForwardZ;
        var xForward = itemSchema.ForwardX;

        var yUp = itemSchema.UpY;
        var xUp = itemSchema.UpX;
        var zUp = itemSchema.UpZ;

        var rotation =
            Utils.DidFishSaveTheDay(new Vector3(xForward, yForward, zForward), new Vector3(xUp, yUp, zUp));


        if (isBlender)
        {
            var blenderRotation = new Vector3(itemSchema.RotationX, itemSchema.RotationY, itemSchema.RotationZ);

            blenderRotation = Utils.ToDegree(blenderRotation);


            await SetProperty(blenderRotation.Z.ToString("F3"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Roll]);
            await SetProperty(blenderRotation.X.ToString("F3"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Pitch]);
            await SetProperty(blenderRotation.Y.ToString("F3"),
                ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Yaw]);
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


        /*
    while (MemoryHelper.GetGlobalHover() != ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeX])
    {
        Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
    }
    */

        // while (MemoryHelper.GetTopBrowserHover() != 0)
        // {
        //     Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_Q);
        //     await Task.Delay(33);
        // }
    }
}