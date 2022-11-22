using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace Halo_Forge_Bot;

public static class Utils
{
    private static string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
    public static string ExePath = Path.GetDirectoryName(dllPath);

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

    public static Vector3 DidFishSaveTheDay(Vector3 forward, Vector3 up)
    {
        forward = new Vector3(forward.Y, forward.Z, forward.X); // working???
        up = new Vector3(up.Y, up.Z, up.X);
        var quat = LookRotation(forward, up);
        var final = QuaternionToYXZ(quat);


        Vector3 to180(Vector3 v)
        {
            if (v.X > 180)
            {
                v.X -= 360;
            }

            if (v.X < -180)
            {
                v.X += 360;
            }


            if (v.Y > 180)
            {
                v.Y -= 360;
            }

            if (v.Y < -180)
            {
                v.Y += 360;
            }


            if (v.Z > 180)
            {
                v.Z -= 360;
            }

            if (v.Z < -180)
            {
                v.Z += 360;
            }

            return v;
        }

        return to180(final);
    }

    public static Quaternion LookRotation(Vector3 forward, Vector3 up)
    {
        forward = Vector3.Normalize(forward);

        Vector3 vector = Vector3.Normalize(forward);
        Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
        Vector3 vector3 = Vector3.Cross(vector, vector2);
        var m00 = vector2.X;
        var m01 = vector2.Y;
        var m02 = vector2.Z;
        var m10 = vector3.X;
        var m11 = vector3.Y;
        var m12 = vector3.Z;
        var m20 = vector.X;
        var m21 = vector.Y;
        var m22 = vector.Z;


        float num8 = (m00 + m11) + m22;
        var quaternion = new Quaternion();
        if (num8 > 0f)
        {
            var num = (float)Math.Sqrt(num8 + 1f);
            quaternion.W = num * 0.5f;
            num = 0.5f / num;
            quaternion.X = (m12 - m21) * num;
            quaternion.Y = (m20 - m02) * num;
            quaternion.Z = (m01 - m10) * num;
            return quaternion;
        }

        if ((m00 >= m11) && (m00 >= m22))
        {
            var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
            var num4 = 0.5f / num7;
            quaternion.X = 0.5f * num7;
            quaternion.Y = (m01 + m10) * num4;
            quaternion.Z = (m02 + m20) * num4;
            quaternion.W = (m12 - m21) * num4;
            return quaternion;
        }

        if (m11 > m22)
        {
            var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
            var num3 = 0.5f / num6;
            quaternion.X = (m10 + m01) * num3;
            quaternion.Y = 0.5f * num6;
            quaternion.X = (m21 + m12) * num3;
            quaternion.W = (m20 - m02) * num3;
            return quaternion;
        }

        var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
        var num2 = 0.5f / num5;
        quaternion.X = (m20 + m02) * num2;
        quaternion.Y = (m21 + m12) * num2;
        quaternion.Z = 0.5f * num5;
        quaternion.W = (m01 - m10) * num2;
        return quaternion;
    }

    public static Vector3 QuaternionToYXZ(Quaternion quaternion)
    {
        float[] te = new float[16];
        var x = quaternion.X;
        var y = quaternion.Y;
        var z = quaternion.Z;
        var w = quaternion.W;
        var x2 = x + x;
        var y2 = y + y;
        var z2 = z + z;
        var xx = x * x2;
        var xy = x * y2;
        var xz = x * z2;
        var yy = y * y2;
        var yz = y * z2;
        var zz = z * z2;
        var wx = w * x2;
        var wy = w * y2;
        var wz = w * z2;

        var sx = 1;
        var sy = 1;
        var sz = 1;

        te[0] = (1 - (yy + zz)) * sx;
        te[1] = (xy + wz) * sx;
        te[2] = (xz - wy) * sx;
        te[3] = 0;

        te[4] = (xy - wz) * sy;
        te[5] = (1 - (xx + zz)) * sy;
        te[6] = (yz + wx) * sy;
        te[7] = 0;

        te[8] = (xz + wy) * sz;
        te[9] = (yz - wx) * sz;
        te[10] = (1 - (xx + yy)) * sz;
        te[11] = 0;

        te[12] = 0;
        te[13] = 0;
        te[14] = 0;
        te[15] = 1;

        var m11 = te[0];
        var m12 = te[4];
        var m13 = te[8];
        var m21 = te[1];
        var m22 = te[5];
        var m23 = te[9];
        var m31 = te[2];
        var m32 = te[6];
        var m33 = te[10];

        Vector3 output = new Vector3(); // { x: 0, y: 0, z: 0 };

        output.X = MathF.Asin(Math.Clamp(m23, -1, 1));

        if (Math.Abs(m23) < 0.9999999)
        {
            output.Y = MathF.Atan2(m13, m33);
            output.Z = MathF.Atan2(m21, m22);
        }
        else
        {
            output.Y = MathF.Atan2(-m31, m11);
            output.Z = 0;
        }

        // Convert from radians to degrees
        output.X *= 180 / MathF.PI;
        output.Y *= 180 / MathF.PI;
        output.Z *= 180 / MathF.PI;

        return output;
    }

    public static (Vector3 Radians, Vector3 Degrees) DirectionToEuler(Vector3 forward, Vector3 up)
    {
        if (forward == Vector3.Zero && up == Vector3.Zero || forward == up)
            return (Vector3.Zero, Vector3.Zero);

        var right = Vector3.Cross(forward, up);
        var z = MathF.Round(MathF.Atan2(forward.Y, forward.X), 5);

        var y = MathF.Round(MathF.Atan2(forward.Z, MathF.Sqrt(forward.X * forward.X + forward.Y * forward.Y)), 5);

        if (y > MathF.PI / 2)
        {
            y = MathF.PI - y;
        }

        if (y < -MathF.PI / 2)
        {
            y = -MathF.PI - y;
        }

        var x = -MathF.Round(MathF.Atan2(right.Z, up.Z), 5);

        return (new Vector3(x, y, z), new Vector3(x, y, z) * (180 / MathF.PI));
    }

    public static void SaveJson(object objectToSave, string filename)
    {
        JsonSerializerSettings jss = new JsonSerializerSettings();
        jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        var json = JsonConvert.SerializeObject(objectToSave, jss);
        File.WriteAllText($"{ExePath}/{filename}.json", json);
    }
}