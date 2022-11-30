using System;
using BondReader.Schemas.Items;

namespace Halo_Forge_Bot.DataModels
{
    public class MapItem
    {
        public int UniqueId { get; init; }
        public ForgeItem item;
        
        
        public MapItem(int id, ForgeItem item)
        {
            UniqueId = id;
            item = item;
        }
    }
}