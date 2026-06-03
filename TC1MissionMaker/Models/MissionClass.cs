namespace TC1MissionMaker.Models;

//Determines wether mission is story/faction/cau.
internal sealed class MissionClass
{

    //Despite its name it decides how the game treats the mission. 4 means scale the mission with the player.
    public readonly int OnlineMissionType;
    
    //string to put inside ServerData to tell the server what class misison this is.
    public readonly string RewardType;

    //How the mission icon looks on the map
    public readonly int TagType;

    private MissionClass(int onlineMissionType, string rewardType, int tagType)
    {
        OnlineMissionType = onlineMissionType;
        RewardType = rewardType;
        TagType = tagType;
    }
    
    public static readonly MissionClass Story = new(3, "story", 1);
    public static readonly MissionClass StoryChained = new(3, "story", 43);
    public static readonly MissionClass Faction = new(4, "faction", 41);
    public static readonly MissionClass FakeFaction = new(3, "faction", 41);
    public static readonly MissionClass Cau = new(12, "cau", 26);
    
    public static MissionClass FromString(string mClass)
    {
        switch (mClass)
        {
            case "story":
                return Story;
            case "chained_story":
                return StoryChained;
            case "faction":
                return Faction;
            case "fake_faction":
                return FakeFaction;
            case "cau":
                return Cau;
            default:
                return Faction;
        }
    }
}