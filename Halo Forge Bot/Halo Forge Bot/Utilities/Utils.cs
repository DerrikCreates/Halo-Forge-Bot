using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using BondReader.Schemas;
using Newtonsoft.Json;
using Serilog;

namespace Halo_Forge_Bot.Utilities;

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

        final = to180(final);


        // final.X = float.IsNaN(final.X) ? 0  : final.X;
        // final.Y = float.IsNaN(final.Y) ? 180  : final.Y;
        //  final.Z = float.IsNaN(final.Z) ? 0 : final.Z;
        return final;
    }

    public static Quaternion LookRotation(Vector3 forward, Vector3 up)
    {
        forward = Vector3.Normalize(forward);
        up = Vector3.Normalize(up); //added for test remove later if not working

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
        var z = MathF.Atan2(forward.Y, forward.X);

        var y = MathF.Atan2(forward.Z, MathF.Sqrt(forward.X * forward.X + forward.Y * forward.Y));

        if (y > MathF.PI / 2)
        {
            y = MathF.PI - y;
        }

        if (y < -MathF.PI / 2)
        {
            y = -MathF.PI - y;
        }

        var x = -MathF.Atan2(right.Z, up.Z);

        return (new Vector3(x, y, z), new Vector3(x, y, z) * (180 / MathF.PI));
    }

    public static void SaveJson(object objectToSave, string filename)
    {
        JsonSerializerSettings jss = new JsonSerializerSettings();
        jss.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        var json = JsonConvert.SerializeObject(objectToSave, jss);
        File.WriteAllText($"{ExePath}/{filename}.json", json);
    }

    public static Random Random = new Random();

    public static T? GetRandomEnum<T>()
    {
        Array values = Enum.GetValues(typeof(T));

        var randomEnum = (T?)values.GetValue(Random.Next(values.Length));

        return randomEnum;
    }

    public static List<ForgeItem> SchemaToItemList(BondSchema map)
    {
        int index = 0;
        var forgeItems = new List<ForgeItem>();
        foreach (var itemSchema in map.Items)
        {
            var forgeItem = new ForgeItem();
            forgeItem.DEBUGSCHEMA = itemSchema;
            forgeItem.IsStatic = itemSchema.StaticDynamicFlagUnknown == 21;

            forgeItem.ItemId = itemSchema.ItemId.Int;


            forgeItem.ForwardX = itemSchema.Forward.X;
            forgeItem.ForwardY = itemSchema.Forward.Y;
            forgeItem.ForwardZ = itemSchema.Forward.Z;

            forgeItem.UpX = itemSchema.Up.X;
            forgeItem.UpY = itemSchema.Up.Y;
            forgeItem.UpZ = itemSchema.Up.Z;

            forgeItem.PositionX = itemSchema.Position.X;
            forgeItem.PositionY = itemSchema.Position.Y;
            forgeItem.PositionZ = itemSchema.Position.Z;

            if (forgeItem.IsStatic)
            {
                forgeItem.ScaleX = itemSchema.SettingsContainer.Scale.First().ScaleContainer.X;
                forgeItem.ScaleY = itemSchema.SettingsContainer.Scale.First().ScaleContainer.Y;
                forgeItem.ScaleZ = itemSchema.SettingsContainer.Scale.First().ScaleContainer.Z;
            }


            if (IsValidNumbers(forgeItem))
            {
                forgeItems.Add(forgeItem);
                continue;
            }

            Log.Warning("NaN detected in item id {ItemId} skipping this item", forgeItem.ItemId);
        }

        return forgeItems;


        bool IsValidNumbers(ForgeItem forgeItem)
        {
            var fields = forgeItem.GetType().GetFields();
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(float))
                {
                    var value = (float)field.GetValue(forgeItem);

                    if (float.IsNaN(value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }


    public static Vector3 DidZSaveTheDay(Vector3 forward, Vector3 up)
    {
        var quat = LookRotation(forward, up);
        return ToEulerAngles(quat);


        Vector3 ToEulerAngles(Quaternion q)
        {
            // Convert X Angle
            float sinr_cosp = (float)(2 * (-q.W * q.X + q.Y * q.Z));
            float cosr_cosp = (float)(1 - 2 * (q.X * q.X + q.Y * q.Y));
            float xAngle = (float)Math.Atan2(sinr_cosp, cosr_cosp) * -1;

            // Convert Y Angle
            float sinp = (float)(2 * (-q.W * q.Y - q.Z * q.X));
            float yAngle;
            if (Math.Abs(sinp) >= 1)
                yAngle = (float)(Math.PI / 2 * Math.Sign(sinp)) * -1;
            else
                yAngle = (float)Math.Asin(sinp) * -1;

            // Convert Z Angle
            float siny_cosp = (float)(2 * (-q.W * q.Z + q.X * q.Y));
            float cosy_cosp = (float)(1 - 2 * (q.Y * q.Y + q.Z * q.Z));
            float zAngle = (float)Math.Atan2(siny_cosp, cosy_cosp) * -1;

            return new Vector3((float)Math.Round(xAngle, 2), (float)Math.Round(yAngle, 2),
                (float)Math.Round(zAngle, 2));
        }

        Vector3 ToDegree(Vector3 r)
        {
            float xDeg = (float)(r.X * 180 / Math.PI);
            float yDeg = (float)(r.Y * 180 / Math.PI);
            float zDeg = (float)(r.Z * 180 / Math.PI);

            return new Vector3(xDeg, yDeg, zDeg);
        }

        Vector3 ToRadian(Vector3 d)
        {
            float xRad = (float)(d.X * Math.PI / 180);
            float yRad = (float)(d.Y * Math.PI / 180);
            float zRad = (float)(d.Z * Math.PI / 180);

            return new Vector3((float)Math.Round(xRad, 2), (float)Math.Round(yRad, 2), (float)Math.Round(zRad, 2));
        }

        Quaternion ToQuaternion(Vector3 e)
        {
            double cy = Math.Cos(e.Z * 0.5);
            double sy = Math.Sin(e.Z * 0.5);
            double cp = Math.Cos(e.Y * 0.5);
            double sp = Math.Sin(e.Y * 0.5);
            double cr = Math.Cos(e.X * 0.5);
            double sr = Math.Sin(e.X * 0.5);

            Quaternion q = new Quaternion
            {
                W = (float)(cr * cp * cy + sr * sp * sy * -1),
                X = (float)(sr * cp * cy - cr * sp * sy * -1),
                Y = (float)(cr * sp * cy + sr * cp * sy * -1),
                Z = (float)(cr * cp * sy - sr * sp * cy * -1)
            };

            return q;
        }
    }
}