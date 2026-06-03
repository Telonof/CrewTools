namespace TC1MissionMaker.Models;

internal sealed class GameplayZone
{
    //just an id for TagData to know where it is.
    public readonly string Id;

    //Index used for server to know and properly reward a player for that region in faction missions.
    public readonly int Index;

    //A string id via existing strings in-game for hudLocation if none is specified by the user.
    public readonly int DescriptionId;

    private GameplayZone(string id, int index, int descriptionId)
    {
        Id = id;
        Index = index;
        DescriptionId = descriptionId;
    }

    public static readonly GameplayZone Midwest = new GameplayZone("3CAF1B0400000020", 1, 3147);
    public static readonly GameplayZone EastCoast = new GameplayZone("AAC71B0400000020", 2, 3148);
    public static readonly GameplayZone South = new GameplayZone("AACE1B0400000020", 3, 3149);
    public static readonly GameplayZone MountainStates = new GameplayZone("ABCE1B0400000020", 4, 3150);
    public static readonly GameplayZone WestCoast = new GameplayZone("DC3F1C0400000020", 5, 3151);

    public static GameplayZone FromString(string zone)
    {
        switch (zone)
        {
            case "midwest":
                return Midwest;
            case "east_coast":
                return EastCoast;
            case "the_south":
                return South;
            case "mountain_states":
                return MountainStates;
            case "west_coast":
                return WestCoast;
            default:
                return Midwest;
        }
    }

}
