namespace TC1MissionMaker.Models;

internal sealed class MissionDuration
{
    //Shows on the hud how long the mission takes, only 3 types exist, short, medium, long.
    public readonly int HudDuration;

    //A default id on what to give the player upon completeion if none specified by user.
    public readonly ulong RewardId;

    private MissionDuration(int hudDuration, ulong rewardId)
    {
        HudDuration = hudDuration;
        RewardId = rewardId;
    }

    public static readonly MissionDuration VeryShort = new MissionDuration(0, 127057550);
    public static readonly MissionDuration Short = new MissionDuration(0, 127057535);
    public static readonly MissionDuration Medium = new MissionDuration(1, 127057536);
    public static readonly MissionDuration Long = new MissionDuration(2, 127057537);
    public static readonly MissionDuration VeryLong = new MissionDuration(2, 127057538);

    public static MissionDuration FromString(string duration)
    {
        switch (duration)
        {
            case "veryshort":
                return VeryShort;
            case "short":
                return Short;
            case "medium":
                return Medium;
            case "long":
                return Long;
            case "verylong":
                return VeryLong;
            default:
                return Short;
        }
    }
}
