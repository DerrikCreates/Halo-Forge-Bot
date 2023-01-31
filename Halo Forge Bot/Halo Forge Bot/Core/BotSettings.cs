using System.Numerics;

namespace Halo_Forge_Bot.Core;

public class BotSettings
{
    public int ItemStartIndex, ItemEndIndex;
    public float Scale = 1;
    public Vector3 Offset = Vector3.Zero;
    public int SaveFrequency = 100;
    public bool RecenterMap = false;
}