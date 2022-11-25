namespace Halo_Forge_Bot.Utilities;

public static class HaloPointers
{
    public static readonly string
        RootBrowserHover =
            "HaloInfinite.exe+0x482B178,0xB8,0x950"; //todo find the pointer that tracks both root and sub hover

    public const string GlobalHover = "HaloInfinite.exe+0x42CCE70,0xBA0,0x950";
    public const string SubBrowserHover = "HaloInfinite.exe+0x42CCE70,0xBB0,0x950";
    public const string TopBrowserHover = "HaloInfinite.exe+0x49FD8A0,0x1A8";
    public const string BrowserScroll = "HaloInfinite.exe+0x482B108,0xB8,0x934";
    public const string EditPropertyBool = "HaloInfinite.exe+0x00000482B1F8,0xB5";
    public const string EditNameBox = "HaloInfinite.exe+0x0000042CC760,0xBA0,0x774";
    public const string UIMenuVisible = "HaloInfinite.exe+00000425EB28,0x0,0x28";

    private static readonly int
        transformOffset = 0x128; //"HaloInfinite.exe+0x42CCE48,0x80 + transformOffset - Bottom Transform UI 

    private const string _transformPointer = "HaloInfinite.exe+0x42CCE48,0x80";

    public static readonly string
        _xPositionUI = $"{_transformPointer},{transformOffset:X}";

    public static readonly string
        _yPositionUI = $"{_transformPointer},{(transformOffset + 4):X}";

    public static readonly string
        _zPositionUI = $"{_transformPointer},{(transformOffset + 8):X}";

    public static readonly string
        _zRotationUI = $"{_transformPointer},{(transformOffset + 12):X}";

    public static readonly string
        _yRotationUI = $"{_transformPointer},{(transformOffset + 16):X}";

    public static readonly string
        _xRotationUI = $"{_transformPointer},{(transformOffset + 20):X}";

    public static readonly string
        _xScaleUI = $"{_transformPointer},{(transformOffset + 24):X}";

    public static readonly string
        _yScaleUI =
            $"{_transformPointer},{(transformOffset + 28):X}";

    public static readonly string
        _zScaleUI =
            $"{_transformPointer},{(transformOffset + 32):X}";
}