using System;
using System.Text.Json.Serialization;

namespace Halo_Forge_Bot.Utilities;

public class HaloPointers
{
    public readonly string
        RootBrowserHover =
            "HaloInfinite.exe+0x482B178,0xB8,0x950"; //todo find the pointer that tracks both root and sub hover

    public readonly string GlobalHover = "HaloInfinite.exe+0x42CCE70,0xBA0,0x950";
    public readonly string SubBrowserHover = "HaloInfinite.exe+0x42CCE70,0xBB0,0x950";
    public readonly string TopBrowserHover = "HaloInfinite.exe+0x49FD8A0,0x1A8";
    public readonly string BrowserScroll = "HaloInfinite.exe+0x482B108,0xB8,0x934";
    public readonly string EditPropertyBool = "HaloInfinite.exe+0x00000482B1F8,0xB5";
    public readonly string EditNameBox = "HaloInfinite.exe+0x0000042CC760,0xBA0,0x774";
    public readonly string UiMenuVisible = "HaloInfinite.exe+00000425EB28,0x0,0x28";

    public static readonly string TransformPointer = "HaloInfinite.exe+0x42CCE48,0x80";
    public readonly string TransformOffset = "0x128";

    [JsonIgnore] private int _transformOffset => Convert.ToInt32(TransformOffset, 16);
    [JsonIgnore] public string XPositionUi => $"{TransformPointer},{_transformOffset:X}";

    [JsonIgnore] public string YPositionUi => $"{TransformPointer},{(_transformOffset + 4):X}";

    [JsonIgnore] public string ZPositionUi => $"{TransformPointer},{(_transformOffset + 8):X}";

    [JsonIgnore] public string ZRotationUi => $"{TransformPointer},{(_transformOffset + 12):X}";

    [JsonIgnore] public string YRotationUi => $"{TransformPointer},{(_transformOffset + 16):X}";

    [JsonIgnore] public string XRotationUi => $"{TransformPointer},{(_transformOffset + 20):X}";

    [JsonIgnore] public string XScaleUi => $"{TransformPointer},{(_transformOffset + 24):X}";

    [JsonIgnore] public string YScaleUi => $"{TransformPointer},{(_transformOffset + 28):X}";

    [JsonIgnore] public string ZScaleUi => $"{TransformPointer},{(_transformOffset + 32):X}";
}