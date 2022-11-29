using BondReader.Schemas.Items;
using Newtonsoft.Json;

namespace Halo_Forge_Bot
{
    public class ForgeItem
    {
        [JsonIgnore] public ItemSchema DEBUGSCHEMA;
        [JsonProperty("positionX")] public float PositionX;
        [JsonProperty("positionY")] public float PositionY;
        [JsonProperty("positionZ")] public float PositionZ;

        [JsonProperty("scaleX")] public float ScaleX;
        [JsonProperty("scaleY")] public float ScaleY;
        [JsonProperty("scaleZ")] public float ScaleZ;

        [JsonProperty("rotationX")] public float RotationX;
        [JsonProperty("rotationY")] public float RotationY;
        [JsonProperty("rotationZ")] public float RotationZ;

        [JsonProperty("forwardX")] public float ForwardX;
        [JsonProperty("forwardY")] public float ForwardY;
        [JsonProperty("forwardZ")] public float ForwardZ;

        [JsonProperty("upX")] public float UpX;
        [JsonProperty("upY")] public float UpY;
        [JsonProperty("upZ")] public float UpZ;

        [JsonProperty("itemId")] public int ItemId;

        [JsonProperty("isStatic")] public bool IsStatic = true;

        [JsonIgnore] public int UniqueId;
        /*
        positionX = None
        positionY = None
        positionZ = None
    
        scaleX = None
        scaleY = None
        scaleZ = None
    
        rotationX = None
        rotationY = None
        rotationZ = None
        
        
        
        forwardX = None
        forwardY = None
        forwardZ = None
        
        upX = None
        upY = None
        upZ = None
        */
    }
}