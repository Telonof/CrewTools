using TC1MissionMaker.MissionData;

namespace TC1MissionMaker.Models;

internal sealed class MissionType
{
    public readonly string FatherArchetypeId;

    public readonly string Type;

    //icon showing it's race/time trial etc after you hover over the mission icon on the map.
    public readonly int SubType;

    //default pre mission blurb is none is specified by the user.
    public readonly int DescriptionId;

    public readonly string FatherSpawnerId;

    public readonly int MissionWizardType;

    public readonly MissionTypeSettings Settings;

    public readonly Type MissionFile;

    private MissionType(string fatherArchetypeId, string type, int subType, int descriptionId, string fatherSpawnerId, int missionWizardType, MissionTypeSettings settings, Type missionFile)
    {
        FatherArchetypeId = fatherArchetypeId;
        Type = type;
        SubType = subType;
        DescriptionId = descriptionId;
        FatherSpawnerId = fatherSpawnerId;
        MissionWizardType = missionWizardType;
        Settings = settings;
        MissionFile = missionFile;
    }

    public static readonly MissionType Race = new MissionType("50C1010000000000", "RACE", 0, 37965,
        "615BF00200000000", 1, new MissionTypeSettings("RACE"), typeof(RaceMission));

    public static readonly MissionType TimeTrial = new MissionType("AA870F0000000000", "TIME_TRIAL", 6,
        6480, "625BF00200000000", 4, new MissionTypeSettings("TIME_TRIAL"), typeof(TimeTrialMission));

    public static readonly MissionType TimeAttack = new MissionType("24AC930700000000", "TIME_ATTACK", 12,
        38749, "552A940700000000", 7, new MissionTypeSettings("TIME_ATTACK"), typeof(TimeAttackMission));

    public static readonly MissionType AToB = new MissionType("83D53A0400000000", "A_TO_B", 1, 6480,
        "D194C20400000000", 8, new MissionTypeSettings("A_TO_B"), typeof(AToBMission));

    public static readonly MissionType Escape = new MissionType("C901640700000000", "ESCAPE", 5, 17996,
        "1145840700000000", 10, new MissionTypeSettings("ESCAPE"), typeof(EscapeMission));

    public static readonly MissionType Collect = new MissionType("D83CA00200000000", "COLLECT", 4, 36944,
        "B68E240300000000", 6, new MissionTypeSettings("COLLECT"), typeof(CollectMission));

    public static readonly MissionType Takedown = new MissionType("8447130000000000", "TAKEDOWN", 3,
        6491, "635CF00200000000", 3, new MissionTypeSettings("TAKEDOWN"), typeof(TakedownMission));

    public static readonly MissionType DriftTrial = new MissionType("A784930700000000", "DRIFT_TRIAL", 10,
        38542, "A529940700000000", 15, new MissionTypeSettings("DRIFT_TRIAL"), typeof(DriftTrialMission));

    public static readonly MissionType DragTrial = new MissionType("3657930700000000", "DRAG_TRIAL", 9,
        38537, "A629940700000000", 16, new MissionTypeSettings("DRAG_TRIAL"), typeof(DragTrialMission));

    public static readonly MissionType StuntRace = new MissionType("8D2E940700000000", "STUNT_RACE", 13,
        39201, "902E940700000000", 20, new MissionTypeSettings("STUNT_RACE"), typeof(StuntRaceMission));

    public static readonly MissionType Monster = new MissionType("CA63930700000000", "MONSTER", 8,
        38532, "A429940700000000", 7, new MissionTypeSettings("MONSTER"), typeof(MonsterMission));

    //unused in the main game, possible to make a mission out of, but extremely basic. Think time trial but crate instead of checkpoint.
    public static readonly MissionType TimeTrialOffroad = new MissionType("8C58D90100000000", "TIME_TRIAL_OFFROAD", 6,
-1, "635BF00200000000", 2, new MissionTypeSettings("TIME_TRIAL_OFFROAD"), typeof(TimeTrialOffroadMission));

    public static MissionType FromString(string type)
    {
        switch (type)
        {
            case "race":
                return Race;
            case "time_trial":
                return TimeTrial;
            case "time_attack":
                return TimeAttack;
            case "a_to_b":
                return AToB;
            case "escape":
                return Escape;
            case "collect":
                return Collect;
            case "takedown":
                return Takedown;
            case "drift_trial":
                return DriftTrial;
            case "drag_trial":
                return DragTrial;
            case "stunt_race":
                return StuntRace;
            case "monster":
                return Monster;
            default:
                return Race;
        }
    }

    public enum MissionRaceType
    {
        FULLSTOCK,
        STREET,
        DIRT,
        CIRCUIT,
        RAID,
        PERF,
        ANY,
        MONSTER,
        DRIFT,
        DRAG
    }
}
