using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using BondReader.Schemas.Items;
using InfiniteForgeConstants.Forge_UI.Object_Properties;
using TextCopy;
using WindowsInput.Native;

namespace Halo_Forge_Bot.Utilities;

public static class PropertyHelper
{
    private static async Task SetProperty(string data, int index, VirtualKeyCode key = VirtualKeyCode.VK_S)
    {
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
            await Task.Delay(50);
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);

            /*
            var toType = data.ToCharArray();
            foreach (var c in toType)
            {
                SendKeys.SendWait(c.ToString());
                await Task.Delay(33);
            }
            */
            Input.Simulate.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            await Task.Delay(50);
        }

        while (MemoryHelper.GetEditMenuState() != 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(50);
        }
    }

    public static async Task SetMainProperties(ItemSchema itemSchema)
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

        var xScale = MathF.Round(itemSchema.SettingsContainer.Scale.First().ScaleContainer.X, 3);
        var yScale = MathF.Round(itemSchema.SettingsContainer.Scale.First().ScaleContainer.Y, 3);
        var zScale = MathF.Round(itemSchema.SettingsContainer.Scale.First().ScaleContainer.Z, 3);

        var itemScale = new Vector3(xScale, yScale, zScale);

        var realScale = (itemScale * defaultScale); //Vector3.Multiply(itemScale, defaultScale) * 10;

        await SetProperty(realScale.X.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeX]);
        await SetProperty(realScale.Y.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeY]);
        await SetProperty(realScale.Z.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.SizeZ]);

        var xPos = MathF.Round(itemSchema.Position.X * 10, 3);
        var yPos = MathF.Round(itemSchema.Position.Y * 10, 3);
        var zPos = MathF.Round(itemSchema.Position.Z * 10, 3);

        await SetProperty(xPos.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Forward]);
        await SetProperty(yPos.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Horizontal]);
        await SetProperty(zPos.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Vertical]);

        var yForward = MathF.Round(itemSchema.Forward.Y, 3);
        var zForward = MathF.Round(itemSchema.Forward.Z, 3);
        var xForward = MathF.Round(itemSchema.Forward.X, 3);

        var yUp = MathF.Round(itemSchema.Up.Y, 3);
        var xUp = MathF.Round(itemSchema.Up.X, 3);
        var zUp = MathF.Round(itemSchema.Up.Z, 3);

        var rotation =
            Utils.DidFishSaveTheDay(new Vector3(xForward, yForward, zForward), new Vector3(xUp, yUp, zUp));


        await SetProperty(rotation.Z.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Roll]);
        await SetProperty(rotation.X.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Pitch], VirtualKeyCode.VK_W);
        await SetProperty(rotation.Y.ToString(), ObjectPropertiesOptions.StaticByDefault[ObjectPropertyName.Yaw], VirtualKeyCode.VK_W);
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