namespace TC1MissionMaker.Models;

internal sealed class MissionTypeSettings
{
    //Has the id to set the custom music
    public readonly string[] MusicIds;

    //Has the id's to hook into the finish line cutscene.
    //first 3 are when the car hits the finish line. VarMovieSequenceFinishLineID
    //next 3 is the camera rotation around the car VarMovieSequenceWaitingOthersID
    //next 3 is the reward movie cutscene showing you your time. VarMovieSequenceRewardID
    public readonly string[] FinishIds;

    //The id to hook into to force set the time for chained missions.
    public readonly string VarTimeId;

    public MissionTypeSettings(string type)
    {
        switch (type)
        {
            case "RACE":
                VarTimeId = "77020000";
                MusicIds = ["6A180000", "000094C300600C46"];
                FinishIds = ["3D549BD4", "FA040000", "0040E045000036C4",
                             "3D549BD4", "C2040000", "00C0E44500003AC4",
                             "5EF477AA", "C3040000", "00C0ED45000032C4"];
                break;
            case "TIME_TRIAL":
                VarTimeId = "CC000000";
                MusicIds = ["80080000", "008043C400C05B45"];
                FinishIds = ["5EF477AA", "E8000000", "00C0CF450000B544",
                             "5EF477AA", "E6000000", "00C0D4450000B744",
                             "5EF477AA", "E5000000", "0020DC450020C644"];
                break;
            case "TIME_ATTACK":
                VarTimeId = "E1B70000";
                MusicIds = ["A15B0000", "00402CC500A03F45"];
                FinishIds = ["3D549BD4", "CF5B0000", "0080144500A09E44",
                             "3D549BD4", "D15B0000", "00801C4500A09E44",
                             "3D549BD4", "D05B0000", "00802B4500A09C44"];
                break;
            case "ESCAPE":
                VarTimeId = "27110000";
                MusicIds = ["B1100000", "0048BBC50020DC45"];
                FinishIds = ["", "", "", "", "", "", "5EF477AA", "9A020000", "00806D4500002042"];
                break;
            case "A_TO_B":
                VarTimeId = "75010000";
                MusicIds = ["47210000", "0040A6C500802A45"];
                FinishIds = ["5EF477AA", "57080000", "00C0BE450000D744",
                             "5EF477AA", "49080000", "0098C4450000D644",
                             "5EF477AA", "4A080000", "0098CD450000DA44"];
                break;
            case "COLLECT":
                VarTimeId = "07010000";
                MusicIds = ["BC0C0000", "00B041C500E0FB44"];
                FinishIds = ["", "", "", "", "", "", "5EF477AA", "4D010000", "0080264500C05A44"];
                break;
            case "TAKEDOWN":
                VarTimeId = "A8060000";
                MusicIds = ["A8120000", "00E0A9C500808345"];
                FinishIds = ["", "", "", "", "", "", "5EF477AA", "CA070000", "0020054600008443"];
                break;
            case "DRIFT_TRIAL":
                VarTimeId = "11B80000"; 
                MusicIds = ["A15B0000", "0080F4C400A04345"];
                FinishIds = ["3D549BD4", "CF5B0000", "0080464500A0A644",
                             "3D549BD4", "D15B0000", "00804E4500A0A644",
                             "3D549BD4", "D05B0000", "00805D4500A0A444"];
                break;
            case "DRAG_TRIAL":
                VarTimeId = "5F3F0000";
                FinishIds = ["3D549BD4", "FA040000", "0040F74500004843",
                             "3D549BD4", "C2040000", "00C0FB4500006843",
                             "5EF477AA", "C3040000", "00E0024600005843"];
                break;
            case "STUNT_RACE":
                VarTimeId = "49B80000";
                MusicIds = ["A15B0000", "00402CC500A03F45"];
                FinishIds = ["3D549BD4", "CF5B0000", "0080144500A09E44",
                             "3D549BD4", "D15B0000", "00801C4500A09E44",
                             "5EF477AA", "D05B0000", "00802B4500A09C44"];
                break;
            case "MONSTER":
                VarTimeId = "11200000";
                FinishIds = ["3D549BD4", "E0410000", "0038B745004881C5",
                             "3D549BD4", "E2410000", "0038BF4500C880C5",
                             "5EF477AA", "E5410000", "00B8CB4500907EC5"];
                break;
        }
    }
}
