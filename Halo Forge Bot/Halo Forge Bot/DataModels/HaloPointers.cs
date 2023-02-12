using System;
using System.Text.Json.Serialization;

namespace Halo_Forge_Bot.Utilities;

public class HaloPointers
{
    public static string GameVersion = "6.10023.16112.0"; //dec 8 2022

    public readonly string
        RootBrowserHover =
            "HaloInfinite.exe+0x482B178,0xB8,0x950"; //todo find the pointer that tracks both root and sub hover

    public static readonly string SetScaleItemArray = "HaloInfinite.exe+4632F08,0x0";
    public static readonly string SetSetPositionItemArray = "HaloInfinite.exe+4905198,0x10,0x0";
    public static readonly string FolderHover = "HaloInfinite.exe+42BF168,0x48,0x68";

    public static readonly string ItemCount = "HaloInfinite.exe+4905198,0x78";
    public static readonly string ItemCountScaleArray = "HaloInfinite.exe+4905198,0x78";
    public static readonly string SelectedItemText = "HaloInfinite.exe+42F0B08,C8,390,774";
    
    /// <summary>
    /// the black selector when navigating menus. This is its index in the ui. Working for both the item browser and the edit properties tab
    /// </summary>
    public static readonly string GlobalHover = "HaloInfinite.exe+42F0B30,0xBB0,0x950"; 
    public static readonly string SubBrowserHover = "HaloInfinite.exe+0x42CCE70,0xBB0,0x950";
    public static readonly string TopBrowserHover = "HaloInfinite.exe+0x49DDD10,0x1A8"; 
    public static readonly string BrowserScroll = "HaloInfinite.exe+0x482B108,0xB8,0x934";
    public static readonly string EditPropertyBool = "HaloInfinite.exe+0x42F01C0,0x160";
    /// <summary>
    /// this is the edit property input textbox string
    /// </summary>
    public static readonly string EditNameBox = "HaloInfinite.exe+0x42F0320,0xB98,0x774";
    public static readonly string UiMenuVisible = "HaloInfinite.exe+4282728,0x0,0x28";


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