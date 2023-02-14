using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GameOverlay.Drawing;
using GameOverlay.Windows;
using Halo_Forge_Bot.Core;

namespace Halo_Forge_Bot.Utilities;

public static class Overlay
{
    private static StickyWindow? _window;
    private static Font? _font;
    private static Font? _largeFont;

    private static Graphics? _gfx;

    private static BotState? _botState;
    private static BotSettings? _botSettings;

    public static Task Setup(BotSettings botSettings, BotState botState)
    {
        _botState = botState;
        _botSettings = botSettings;

        _gfx = new Graphics()
        {
            MeasureFPS = true,
        };

        if (ForgeUI.SetHaloProcess() == null)
        {
        }

        var process = ForgeUI.SetHaloProcess();
        if (process == null)
        {
            var errorPage = new Error("Overlay UI cannot find the HaloInfinite.exe Process");
            errorPage.Show();
            return Task.CompletedTask;
        }

        _window = new StickyWindow(process.MainWindowHandle, _gfx)
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

        _window.DestroyGraphics += DestroyGraphics;
        _window.DrawGraphics += DrawGraphics;
        _window.SetupGraphics += SetupGraphics;


        _window.Create();
        _window.Join();

        return Task.CompletedTask;
    }


    private static void SetupGraphics(object? sender, SetupGraphicsEventArgs e)
    {
        var gfx = e.Graphics;
        _font = gfx.CreateFont("Arial", 12);
        _largeFont = gfx.CreateFont("Arial", 24);
    }


    private static void DrawGraphics(object? sender, DrawGraphicsEventArgs e)
    {
        if (_botState == null)
        {
            _gfx.Dispose();
            _window.Dispose();
            _font.Dispose();
            _largeFont.Dispose();
            return;
        }

        var gfx = e.Graphics;


        gfx.BeginScene();
        gfx.ClearScene();


        gfx.DrawTextWithBackground(_largeFont, 12, gfx.CreateSolidBrush(0, 255, 0),
            gfx.CreateSolidBrush(0, 0, 0), 50, 50,
            $"Version: {Bot.Version}");


        gfx.DrawTextWithBackground(_largeFont, 16, gfx.CreateSolidBrush(255, 0, 0),
            gfx.CreateSolidBrush(0, 0, 0), 50, 100,
            $"Item Count: {MemoryHelper.GetItemCount()}");


        
    }

    private static void DestroyGraphics(object? sender, DestroyGraphicsEventArgs e)
    {
        e.Graphics.Dispose();
    }


    
}