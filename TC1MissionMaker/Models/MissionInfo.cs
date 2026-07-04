using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Buffers.Binary;
using System.Globalization;
using System.Numerics;
using System.Xml.Linq;
using TC1MissionMaker.MissionData;
using TC1MissionMaker.ModFiles;
using static TC1MissionMaker.Models.MissionType;

namespace TC1MissionMaker.Models;

internal class MissionInfo
{
    //Is the mission hidden on the tags (designed to be chained)
    public bool Hidden { get; set; }
    
    public ulong Id { get; set; }

    public ulong Index { get; private set; } = 0;

    public ulong RewardId { get; set; }

    public float TimeOfDay { get; set; }

    public string InternalName { get; set; }

    public string Path { get; set; }

    public int[] Points { get; set; } 
    
    public MissionType Type { get; set; }

    public MissionClass Class { get; set; }
    
    public MissionDuration Duration { get; set; }
    
    public GameplayZone Zone { get; set; }

    public MissionRaceType CarRestrict { get; set; }
    
    public Dictionary<StringType, int> Strings { get; private set; } = [];



    public bool Init(XElement missionData, string internalModName, HashSet<string> existingIds, int currentCount, int totalMissions)
    {
        string? id = XMLUtil.GrabStringOrDefault(missionData, "internalName", true);
        if (string.IsNullOrWhiteSpace(id))
        {
            Logger.Error($"No internalName found for mission.");
            return false;
        }

        if (existingIds.Contains(id))
        {
            Logger.Error($"A mission with the id {id} already exists.");
            return false;
        }

        InternalName = id;
        Logger.Info($"Parsing {InternalName}: [{currentCount}/{totalMissions}]");

        //Test for valid spawnPosition
        float[] coords = XMLUtil.GrabCoords(missionData, "spawnPosition");
        if (coords.Length == 0)
        {
            Logger.Error($"Mission {id} has an invalid spawnPosition.");
            return false;
        }

        Path = $"project\\road66\\generated\\zones\\Mission_{InternalName}.entities.bin";
        Id = CRC64.Hash($"{internalModName}{id}_id", true) & 0x00FFFFFFFFFF0000;
        Hidden = XMLUtil.GrabBoolOrDefault(missionData, "hidden");
        Class = MissionClass.FromString(XMLUtil.GrabStringOrDefault(missionData, "missionClass").ToLowerInvariant());
        Type = MissionType.FromString(XMLUtil.GrabStringOrDefault(missionData, "missionType").ToLowerInvariant());
        Duration = MissionDuration.FromString(XMLUtil.GrabStringOrDefault(missionData, "duration").ToLowerInvariant());
        Zone = GameplayZone.FromString(XMLUtil.GrabStringOrDefault(missionData, "gameplayZone").ToLowerInvariant());
        Points = GeneratePoints(missionData);

        string vehicleRestrict = XMLUtil.GrabStringOrDefault(missionData, "vehicleRestrict", false, "ANY").ToUpperInvariant();
        if (!Enum.TryParse(vehicleRestrict, true, out MissionRaceType type))
            type = MissionRaceType.ANY;

        CarRestrict = type;

        GenerateRewardID(XMLUtil.GrabIDHex(missionData, "reward"));
        ParseTimeOfDay(missionData);

        return true;
    }

    public void GenerateStringData(XElement missionData, StringData stringData, StringTableData tableData)
    {
        //Name of mission
        int id = stringData.GenerateString(XMLUtil.GrabStringOrDefault(missionData, "name"));
        Strings.Add(StringType.Name, id);
        if (id != -1)
            tableData.AddId(id);

        //Description of mission
        id = stringData.GenerateString(XMLUtil.GrabStringOrDefault(missionData, "description", false, "Made with FTIW's mission tool."));
        Strings.Add(StringType.Description, id);
        tableData.AddId(id);

        //objective reminder when playing the mission
        id = stringData.GenerateString(XMLUtil.GrabStringOrDefault(missionData, "objectiveReminder"), true);
        Strings.Add(StringType.ObjectiveReminder, id);
        if (id != -1)
            tableData.AddId(id);

        //pre mission blurb
        id = stringData.GenerateString(XMLUtil.GrabStringOrDefault(missionData, "preMissionBlurb"));
        if (id == -1)
            id = Type.DescriptionId;
        else
            tableData.AddId(id);

        Strings.Add(StringType.PreMissionBlurb, id);

        //location info bottom right of movie cutscene
        id = stringData.GenerateString(XMLUtil.GrabStringOrDefault(missionData, "hudLocation"));
        if (id == -1)
            id = Zone.DescriptionId;
        else
            tableData.AddId(id);

        Strings.Add(StringType.HudLocation, id);
    }

    public ulong GenerateId()
    {
        //allocate max 65535 entities per mission, we should never need over or even close to this.
        if (Index >= ushort.MaxValue)
            throw new IndexOutOfRangeException($"ID budget for {InternalName} exceeded {ushort.MaxValue}!");

        Index++;
        return Id + Index;
    }

    public ulong GrabId(ulong index)
    {
        return Id + index;
    }

    private void ParseTimeOfDay(XElement missionData)
    {
        string time = XMLUtil.GrabStringOrDefault(missionData, "missionTime", false, "12:00");

        //this is to tell the mission itself to not embed any time since we'll be grabbing the time from freedrive.
        bool freedriveTime = XMLUtil.GrabBoolOrDefault(missionData, "freedriveTime");
        if (freedriveTime)
        {
            TimeOfDay = -1;
            return;
        }

        try
        {
            TimeOfDay = (float)TimeSpan.Parse(time).TotalHours;
        }
        catch (FormatException)
        {
            Logger.Error($"missionTime for {InternalName} was formatted incorrectly, defaulting to 12:00pm");
            TimeOfDay = 12;
        }
    }

    private void GenerateRewardID(string customReward)
    {
        //Grab custom reward if one is setup, otherwise use a default.
        RewardId = Duration.RewardId;

        if (!string.IsNullOrWhiteSpace(customReward))
            RewardId = BinaryPrimitives.ReverseEndianness(Convert.ToUInt64(customReward, 16));
    }

    private int[] GeneratePoints(XElement missionData)
    {
        string? pointsStr = XMLUtil.GrabStringOrDefault(missionData, "medalPoints");
        if (string.IsNullOrWhiteSpace(pointsStr))
            return [];

        string[] split = pointsStr.Split(',');
        if (split.Length != 4)
        {
            Logger.Error("medalPoints requires 4 numbers seperated by commas.");
            return [];
        }

        int[] points = new int[4];

        for (int i = 0; i < 4; i++)
        {
            if (!int.TryParse(split[i], CultureInfo.InvariantCulture, out points[i]))
            {
                Logger.Error($"{split[i]} in medalPoints is not a number.");
                return [];
            }
        }

        return points;
    }
}

internal enum StringType
{
    Name,
    Description,
    PreMissionBlurb,
    ObjectiveReminder,
    HudLocation
}