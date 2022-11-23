using Memory;

namespace Halo_Forge_Bot;

public class MemoryHelper
{
    public static string RootBrowserHover = "HaloInfinite.exe+0x482B178,0xB8,0x950"; //todo find the pointer that tracks both root and sub hover
    public static string SubBrowserHover = "HaloInfinite.exe+0x42CCE70,BB0,950";
    public static string TopBrowserHover = "HaloInfinite.exe+0x49FD8A0,1A8";
    public static string ScrollBar = "HaloInfinite.exe+0x42CCE70,0xBA0,0x934";
    public static string EditPropBool = "HaloInfinite.exe+429CBC8,0x158,0x19ED"; // Not a good pointer

    public static Mem Memory = new();
}