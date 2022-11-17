using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Numerics;
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
    
    
    public static (Vector3 Radians, Vector3 Degrees) DirectionToEuler(Vector3 forward, Vector3 up)
    {
        // Thank you Oziwag and Artifice for saving us from understanding what every the fuck you are doing here <3
        // Really if you didn't solve this it would have took me months thanks
        if (forward == Vector3.Zero && up == Vector3.Zero || forward == up)
            return (Vector3.Zero, Vector3.Zero);

        var rollvec = new Vector3(-forward.X,-forward.Y,forward.Z);
        var z = MathF.Atan2(forward.Y, forward.X);
        
        var y = MathF.Atan2(forward.Z, MathF.Sqrt(forward.X * forward.X + forward.Y * forward.Y));
 
        if (y > MathF.PI / 2) { y -= MathF.PI / 2; }
        if (y < -MathF.PI / 2) { y += MathF.PI / 2; }

        var x = MathF.Atan2(Vector3.Dot(forward, Vector3.Cross(rollvec, up)), Vector3.Dot(rollvec, up));

        return (new Vector3(x, y, z), new Vector3(x, y, z) * (180 / MathF.PI));
    }


}
}