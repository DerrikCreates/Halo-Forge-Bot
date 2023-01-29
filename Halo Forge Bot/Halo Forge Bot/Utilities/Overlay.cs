using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bond;
using BondReader.Schemas.Generic;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using Halo_Forge_Bot.GameUI;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Halo_Forge_Bot.Utilities;

public static class Overlay
{
    private static StickyWindow window;
    private static Font _font;
    private static Font _largeFont;


    public static void Setup()
    {
        var gfx = new Graphics()
        {
            MeasureFPS = true,
        };

        window = new StickyWindow(ForgeUI.SetHaloProcess().MainWindowHandle, gfx)
        {
            FPS = 60,
            IsTopmost = true,
            IsVisible = true
        };

        // window = new GraphicsWindow(0, 0, 800, 800, gfx)
        // {
        //     FPS = 5,
        //     IsTopmost = true,
        //     IsVisible = true
        // };

        window.DestroyGraphics += _window_DestroyGraphics;
        window.DrawGraphics += _window_DrawGraphics;
        window.SetupGraphics += _window_SetupGraphics;


        window.Create();
        window.Join();
    }


    public static void _window_SetupGraphics(object sender, SetupGraphicsEventArgs e)
    {
        var gfx = e.Graphics;
        _font = gfx.CreateFont("Arial", 12);
        _largeFont = gfx.CreateFont("Arial", 24);
    }

    private static int i = 0;

    private static string ReverseString(string s)
    {
        var array = s.ToCharArray();
        Array.Reverse(array);
        return new string(array);
    }

    private static async void _window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
    {
        var addr = ReverseString("D1 AB 7E 90"); //90 7E AB D1

        //7FF7C86C0000 + 6486 706A
        var gfx = e.Graphics;


        gfx.BeginScene();
        gfx.ClearScene();


        gfx.DrawTextWithBackground(_largeFont, 12, gfx.CreateSolidBrush(0, 255, 0),
            gfx.CreateSolidBrush(0, 0, 0), 50, 50,
            $"Version: {Bot.Version}");


        gfx.DrawTextWithBackground(_largeFont, 16, gfx.CreateSolidBrush(255, 0, 0),
            gfx.CreateSolidBrush(0, 0, 0), 50, 100,
            $"Item Count: {MemoryHelper.GetItemCount()}");


        gfx.DrawTextWithBackground(_largeFont, 12, gfx.CreateSolidBrush(255, 0, 0),
            gfx.CreateSolidBrush(0, 0, 0), 50, 150,
            $"Potentially Failed Item Count: {Bot.FailedItems}");
        gfx.DrawTextWithBackground(_largeFont, 12, gfx.CreateSolidBrush(255, 0, 0),
            gfx.CreateSolidBrush(0, 0, 0), 50, 200,
            Bot.posLogString);
    }

    private static void _window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
    {
    }


    public static T fromBytes<T>(byte[] arr) where T : struct
    {
        T str = new();

        int size = Marshal.SizeOf(str);
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, str.GetType());
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return str;
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct ForgeItems // This is the item array that sets rot pos but NOT scale
{
    [FieldOffset(0x10)] public readonly IntPtr ItemArrayStart;
    [FieldOffset(0x18)] public readonly IntPtr LastItemSpawnedEnd;
    [FieldOffset(0x20)] public readonly IntPtr ItemArrayMaxAllocated;
    [FieldOffset(0x78)] public readonly int ItemCount;
}