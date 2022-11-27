using System;
using BondReader.Schemas.Items;

namespace Halo_Forge_Bot.DataModels
{
    public class MapItem
    {
        public int UniqueId { get; init; }
        public ItemSchema Schema { get; init; }

        public MapItem(int id, ItemSchema schema)
        {
            UniqueId = id;
            Schema = schema;
        }
    }
}