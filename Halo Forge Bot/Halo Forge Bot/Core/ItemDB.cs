using Halo_Forge_Bot.DataModels;
using Halo_Forge_Bot.Utilities;
using LiteDB;

namespace Halo_Forge_Bot.Core;

public static class ItemDB
{
    private static LiteDatabase _database = new LiteDatabase(Utils.ExePath + "/ForgeItem.db");

    public static void AddItem(ForgeObjectData forgeObjectData)
    {
        _database.GetCollection<ForgeObjectData>().Insert(forgeObjectData);
    }
}