using System;
using System.Linq;
using System.Threading.Tasks;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using Halo_Forge_Bot.GameUI;

namespace Halo_Forge_Bot.Utilities;

public static class Overlay
{
    private static StickyWindow window;
    private static Font _font;

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
    }

    private static int i = 0;

    private static async void _window_DrawGraphics(object sender, DrawGraphicsEventArgs e)
    {
        var gfx = e.Graphics;


        gfx.ClearScene();
       
        gfx.DrawTextWithBackground(_font, 12, gfx.CreateSolidBrush(255, 0, 0),
            gfx.CreateSolidBrush(0, 0, 0), 50, 50,
            $"ItemCount: {MemoryHelper.GetItemCount()}");

        gfx.DrawTextWithBackground(_font, 12, gfx.CreateSolidBrush(0, 255, 0),
            gfx.CreateSolidBrush(0, 0, 0), 50, 100,
            $"Global Hover: { MemoryHelper.GetGlobalHover().Result }");

        
        //  var ptr = MemoryHelper.Memory.Get64BitCode(HaloPointers.SetScaleItemArray);
        //  ptr += +0xb0;

        // var t = MemoryHelper.Memory.ReadBytes(ptr.ToString("x8"), 4);
        // t = t.Reverse().ToArray();
        /*  var scaleX = BitConverter.ToSingle(t);
  
          var value = MemoryHelper.ReadMemory<float>(ptr.ToUInt64().ToString("x8"));
          gfx.DrawTextWithBackground(_font, 12, gfx.CreateSolidBrush(255, 0, 0),
              gfx.CreateSolidBrush(0, 0, 0), 50, 100,
              $"First X Scale: {scaleX}");
  
          gfx.DrawTextWithBackground(_font, 12, gfx.CreateSolidBrush(255, 0, 0),
              gfx.CreateSolidBrush(0, 0, 0), 50, 150,
              $"X Scale Address: {ptr.ToString("x8")}");
              
              */
    }

    private static void _window_DestroyGraphics(object sender, DestroyGraphicsEventArgs e)
    {
    }
}