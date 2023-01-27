using System;
using System.Runtime.InteropServices;

namespace Halo_Forge_Bot.DataModels;

[StructLayout(LayoutKind.Explicit)]
public class SetSetPositionItem
{
    [FieldOffset(0x10)] public readonly IntPtr ItemArrayStart;
    [FieldOffset(0x18)] public readonly IntPtr LastItemSpawnedEnd;
    [FieldOffset(0x20)] public readonly IntPtr ItemArrayMaxAllocated;
    [FieldOffset(0x78)] public readonly int ItemCount;
}