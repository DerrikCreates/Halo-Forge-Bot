using System;
using System.Text.Json.Serialization;

namespace Halo_Forge_Bot.Utilities;

public class HaloPointers
{
    public static string GameVersion = "6.10023.18744.0"; //feb 16 2023

    #region RequiredPointers

    //base required pointers as of feb 16 2023

    /// <summary>
    /// the black selector when navigating menus. This is its index in the ui. Working for both the item browser and the edit properties tab
    /// A static pointer is required to get the multiple tab hover to work.
    /// </summary>
    public static readonly string GlobalHover = "HaloInfinite.exe+42FAC70,0xBA0,0x950";

    /// <summary>
    /// This is a bool or 4 byte int showing if the forge ui is open. The escape menu also sets this value = 1 
    /// </summary>
    public static readonly string UiMenuVisible = "HaloInfinite.exe+0x428C890"; //4282728,0x0,0x28";

    /// <summary>
    /// The forge UI top tab. Used to change the tab to object properties / object browser
    /// a 4 byte int ranging from 0-4
    /// </summary>
    public static readonly string TopBrowserHover = "HaloInfinite.exe+0x49E7E90,0x1A8";

    /// <summary>
    /// the item array that allows for setting the scale of forge items
    /// </summary>
    public static readonly string SetScaleItemArray = "HaloInfinite.exe+0x463D0C8,0x0";

    /// <summary>
    /// the root pointer to the set position array. this array exists at the same level as the static item count
    /// </summary>
    public static readonly string SetScaleItemRoot = "HaloInfinite.exe+490F338";

    /// <summary>
    /// this is the edit property input textbox string
    /// </summary>
    public static readonly string EditNameBox = "HaloInfinite.exe+0x42FA560,0xB98,0x774";
    /// <summary>
    /// bool showing if the edit box windows is open. needed for typing properties
    /// </summary>
    public static readonly string EditPropertyBool = "HaloInfinite.exe+42FC030";


    #endregion


    //463 D108

    /// <summary>
    /// this is the item array that allows for position to be set. The correct address ONLY moves the transform widget to the set location
    /// </summary>
    public static string SetSetPositionItemArray => $"{SetScaleItemRoot},0x10,0x0";


    /// <summary>
    /// this is the static item count of the map. used to help set static item positions
    /// </summary>
    public static string ItemCount => $"{SetScaleItemRoot},0x78";
    //463 D000

    public readonly string
        RootBrowserHover =
            "HaloInfinite.exe+0x482B178,0xB8,0x950"; //todo find the pointer that tracks both root and sub hover


    public static readonly string FolderHover = "HaloInfinite.exe+42BF168,0x48,0x68";


    public static readonly string ItemCountScaleArray = "HaloInfinite.exe+4905198,0x78";
    public static readonly string SelectedItemText = "HaloInfinite.exe+42F0B08,C8,390,774";


    public static readonly string SubBrowserHover = "HaloInfinite.exe+0x42CCE70,0xBB0,0x950";

    public static readonly string BrowserScroll = "HaloInfinite.exe+0x482B108,0xB8,0x934";


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