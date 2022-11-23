using System.Collections.Generic;
using Newtonsoft.Json;

namespace Halo_Forge_Bot.DataModels;

//todo this is temp, import the blender addon tools
public class BlenderMap 
{
    [JsonProperty("mapId")] 
    public int MapId { get; set; }
    [JsonProperty("itemList")] 
    public List<BlenderItem> ItemList { get; set; }
}

