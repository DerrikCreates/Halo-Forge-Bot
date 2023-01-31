using System;
using System.Numerics;

namespace Halo_Forge_Bot.Core;

public class BotState
{
    public string MapName = "";
    public int FailedItems;
    public DateTime StartTime, EndTime;
    public BoundingBox BoundingBox = new();

}

public class BoundingBox
{
    public Vector3 backLeft;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;
    public Vector3 topLeft;
    public Vector3 bottomCenter;
}