using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Halo_Forge_Bot;

public static class Utils
{
    public static Rectangle ConvertRectToRectangle(Rect rect)
    {
        //TODO create a property in the forge ui rect to auto convert;
        Rectangle r = new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        return r;
    }

    public static async Task DownloadMvar(string fileShareLink, string savePath)
    {
        HttpClient client = new HttpClient();
        var msg = client.Send(new HttpRequestMessage(HttpMethod.Get, new Uri(fileShareLink)));

        var page = await msg.Content.ReadAsStringAsync();
        var startIndex = page.IndexOf("https://blobs-infiniteugc.svc.halowaypoint.com/ugcstorage/");
        var endIndex = page.IndexOf("/images/thumbnail.jpg");
        var link = page.Substring(startIndex, endIndex - startIndex);
        var final = link + "/map.mvar";
        var mapMessage = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri(final)));

        var mvarData = await mapMessage.Content.ReadAsByteArrayAsync();

        await File.WriteAllBytesAsync(savePath, mvarData);
    }
}