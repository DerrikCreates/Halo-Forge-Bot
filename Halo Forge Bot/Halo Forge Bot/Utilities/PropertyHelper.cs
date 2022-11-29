using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
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
    private static async Task SetProperty(string data, int index, VirtualKeyCode key = VirtualKeyCode.VK_S)
    {
        Log.Information("Setting property at Index:{Index} with value: {Value}", index, data);
        if (data == "") // not sure if its possible to ever be "" but just in case.
        {
            data = "0";
        }

        while (MemoryHelper.GetGlobalHover() != index)
        {
            Input.Simulate.Keyboard.KeyPress(key);
            //MemoryHelper.SetGlobalHover(index);
            await Task.Delay(33);
        }


        //          var selectedPosition = MemoryHelper.GetSelectedPosition();

        while (MemoryHelper.GetEditMenuState() == 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(100);
        }


        while (await ClipboardService.GetTextAsync() != data)
        {
            await ClipboardService.SetTextAsync(data);
        }


        while (MemoryHelper.GetEditBoxText() != data)
        {
            await Task.Delay(25);
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
            await Task.Delay(25);
            /*
            var toType = data.ToCharArray();
            foreach (var c in toType)
            {
                SendKeys.SendWait(c.ToString());
                await Task.Delay(33);
            }
            */
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            await Task.Delay(100);
        }

        while (MemoryHelper.GetEditMenuState() != 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(100);
        }
    }

    public static async Task SetMainProperties(ForgeItem itemSchema)
    {
        while (MemoryHelper.GetMenusVisible() == 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_R);
            await Task.Delay(60);
        }


        while (MemoryHelper.GetTopBrowserHover() != 1)
        {
            await Task.Delay(60);
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_E);
            await Task.Delay(60);
        }

        var defaultScale = MemoryHelper.GetSelectedScale();

        var yScale = MathF.Round(itemSchema.ScaleY, 3);
        var xScale = MathF.Round(itemSchema.ScaleX, 3);
        var zScale = MathF.Round(itemSchema.ScaleZ, 3);

        var itemScale = new Vector3(xScale, yScale, zScale);

        var realScale = (itemScale * defaultScale); //Vector3.Multiply(itemScale, defaultScale) * 10;

        await SetProperty(realScale.X.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeX]);
        await SetProperty(realScale.Y.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeY]);
        await SetProperty(realScale.Z.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeZ]);

        var xPos = MathF.Round(itemSchema.PositionX * 10, 3);
        var yPos = MathF.Round(itemSchema.PositionY * 10, 3);
        var zPos = MathF.Round(itemSchema.PositionZ * 10, 3);

        await SetProperty(xPos.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Forward]);
        await SetProperty(yPos.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Horizontal]);
        await SetProperty(zPos.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Vertical]);

        var yForward = MathF.Round(itemSchema.ForwardY, 3);
        var zForward = MathF.Round(itemSchema.ForwardZ, 3);
        var xForward = MathF.Round(itemSchema.ForwardX, 3);

        var yUp = MathF.Round(itemSchema.UpY, 3);
        var xUp = MathF.Round(itemSchema.UpX, 3);
        var zUp = MathF.Round(itemSchema.UpZ, 3);

        var rotation =
            Utils.DidFishSaveTheDay(new Vector3(xForward, yForward, zForward), new Vector3(xUp, yUp, zUp));


        await SetProperty(rotation.Z.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Roll]);
        await SetProperty(rotation.X.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Pitch],
            VirtualKeyCode.VK_W);
        await SetProperty(rotation.Y.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Yaw],
            VirtualKeyCode.VK_W);
        await Task.Delay(50);


        while (MemoryHelper.GetGlobalHover() != ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeX])
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_W);
        }

        while (MemoryHelper.GetTopBrowserHover() != 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.VK_Q);
            await Task.Delay(33);
        }
    }
}