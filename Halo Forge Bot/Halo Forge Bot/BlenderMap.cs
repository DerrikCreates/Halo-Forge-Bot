using System.Collections.Generic;
using Newtonsoft.Json;

namespace Halo_Forge_Bot;

public class BlenderMap
{
    [JsonProperty("mapId")] 
    public int MapId { get; set; }
    [JsonProperty("itemList")] 
    public List<BlenderItem> ItemList { get; set; }
}

