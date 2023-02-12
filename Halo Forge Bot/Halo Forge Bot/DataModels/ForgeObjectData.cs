using System;
using Halo_Forge_Bot.Core;
using InfiniteForgeConstants.ObjectSettings;

namespace Halo_Forge_Bot.DataModels;

public class ForgeObjectData
{
    
    public ForgeObjectData(string name,ObjectId itemId,bool isStatic, int forwardUiHover, int categoryIndex, int categoryFolderIndex, int folderIndex)
    {
        IsStatic = isStatic;
        ItemID = itemId;
        ForwardUiHover = forwardUiHover;
        CategoryIndex = categoryIndex;
        CategoryFolderIndex = categoryFolderIndex;
        FolderIndex = folderIndex;
        Name = name;
    }

    
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    public bool IsStatic { get; set; }
    public int ForwardUiHover { get; set; }
    public ObjectId ItemID { get; set; }
    public int CategoryIndex { get; set; }
    public int CategoryFolderIndex { get; set; }
    public int FolderIndex { get; set; }
}