using System.Collections.Generic;

namespace Halo_Forge_Bot.GameUI;

public static class StaticByDefault //todo make this a json file for each type of ui layout there is
{
    public static Dictionary<PropertyName, int> Layout = new()
    {
        { PropertyName.GeneralDropdown, 0 },
        { PropertyName.ObjectName, 1 },
        { PropertyName.ObjectModeDropdown, 2 },
        { PropertyName.ObjectMode, 3 },
        { PropertyName.SizeX, 4 },
        { PropertyName.SizeY, 5 },
        { PropertyName.SizeZ, 6 },
        { PropertyName.Physics, 7 },
        { PropertyName.TransformDropdown, 8 },
        { PropertyName.PositionDropdown, 9 },
        { PropertyName.Forward, 10 },
        { PropertyName.Horizontal, 11 },
        { PropertyName.Vertical, 12 },
        { PropertyName.RotationDropdown, 13 },
        { PropertyName.Yaw, 14 },
        { PropertyName.Pitch, 15 },
        { PropertyName.Roll, 16 },
        { PropertyName.Reset, 17 },
    };
}

public enum PropertyName
{
    GeneralDropdown,
    ObjectName,
    ObjectModeDropdown,
    ObjectMode,
    SizeX,
    SizeY,
    SizeZ,
    Physics,
    TransformDropdown,
    PositionDropdown,
    Forward,
    Horizontal,
    Vertical,
    RotationDropdown,
    Yaw,
    Pitch,
    Roll,
    Reset,
    VisualsDropdown,
    //todo add all object properties ui elements here
}