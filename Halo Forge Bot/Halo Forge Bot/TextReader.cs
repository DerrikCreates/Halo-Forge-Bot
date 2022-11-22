using System.Drawing;
using Tesseract;

namespace Halo_Forge_Bot;

public class TextReader
{
    public static void ReadArea(Bitmap image)
    {
        using var engine = new TesseractEngine(@"./tes/best", "eng", EngineMode.Default);
        using var page = engine.Process(image);
        var v = page.GetText();
    }
}