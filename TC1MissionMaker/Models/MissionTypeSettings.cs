namespace TC1MissionMaker.Models;

internal sealed class MissionTypeSettings
{
    //Has the id to set the custom music
    public readonly string[] MusicIds;
    //The id to hook into to force set the time for chained missions.
    public readonly string VarTimeId;

    public MissionTypeSettings(string type)
    {
        switch (type)
        {
            case "RACE":
                VarTimeId = "77020000";
                MusicIds = ["6A180000", "000094C300600C46"];
                break;
            case "TIME_TRIAL":
                VarTimeId = "CC000000";
                MusicIds = ["80080000", "008043C400C05B45"];
                break;
            case "TIME_ATTACK":
                VarTimeId = "E1B70000";
                MusicIds = ["A15B0000", "00402CC500A03F45"];
                break;
            case "ESCAPE":
                VarTimeId = "27110000";
                MusicIds = ["B1100000", "0048BBC50020DC45"];
                break;
            case "A_TO_B":
                VarTimeId = "75010000";
                MusicIds = ["47210000", "0040A6C500802A45"];
                break;
            case "COLLECT":
                VarTimeId = "07010000";
                MusicIds = ["BC0C0000", "00B041C500E0FB44"];
                break;
            case "TAKEDOWN":
                VarTimeId = "A8060000";
                MusicIds = ["A8120000", "00E0A9C500808345"];
                break;
            case "DRIFT_TRIAL":
                VarTimeId = "11B80000"; 
                MusicIds = ["A15B0000", "0080F4C400A04345"];
                break;
            case "DRAG_TRIAL":
                VarTimeId = "5F3F0000";
                MusicIds = ["", ""];
                break;
            default:
                VarTimeId = "77020000";
                MusicIds = [];
                break;
        }
    }
}
