using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Buffers.Binary;
using System.Xml.Linq;
using TC1MissionMaker.Models;

using static TC1MissionMaker.Models.MissionType;

namespace TC1MissionMaker.ModFiles;

internal class MissionSpawner : ModFile
{

    private readonly string _missionFolder, _internalModName;

    private readonly Dictionary<string, Stream> _images = [];

    private readonly byte[] _xbtHeader = [
        0x54, 0x42, 0x58, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x5B, 0x3A, 0x34, 0x0C, 0xB2, 0x84, 0x42, 0xA8, 0xBB, 0x56, 0x7E, 0xA5, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00
    ];


    public MissionSpawner(string outputFile, string missionFolder, string internalModName) : base("project/road66/generated/missionspawners.entities.bin", outputFile, null)
    {
        _missionFolder = missionFolder;
        _internalModName = internalModName;
    }

    public Dictionary<string, Stream> GetImages()
    {
        return _images;
    }

    //Allocate Id 1 to mission id itself always
    public void AddMissionToSpawner(XElement root, MissionInfo mission)
    {
        ulong missionId = mission.GenerateId();

        XElement obj = XMLUtil.GenerateObject("name", "Entity");
        obj.Add(XMLUtil.GenerateField("name", "ID", ConversionUtil.ULongToHex(mission.Id)));
        obj.Add(XMLUtil.GenerateField("name", "FatherArchetypeID", mission.Type.FatherSpawnerId));
        obj.Add(XMLUtil.GenerateField("name", "Angles", "00000000000000800000000000000000"));
        obj.Add(XMLUtil.GenerateField("name", "missionWizardType", ConversionUtil.IntToHex(mission.Type.MissionWizardType)));
        obj.Add(XMLUtil.GenerateField("name", "OnlineMissionType", ConversionUtil.IntToHex(mission.Class.OnlineMissionType)));
        obj.Add(XMLUtil.GenerateField("name", "CanRedoMission", ConversionUtil.BoolToHex(!XMLUtil.GrabBoolOrDefault(root, "oneshot"))));
        obj.Add(XMLUtil.GenerateField("name", "DelockLevel", ConversionUtil.IntToHex(XMLUtil.GrabIntOrDefault(root, "minimumLevel", 1))));
        obj.Add(XMLUtil.GenerateField("name", "MaxLevel", ConversionUtil.IntToHex(XMLUtil.GrabIntOrDefault(root, "missionLevel", 20))));
        obj.Add(XMLUtil.GenerateField("name", "Enable", ConversionUtil.BoolToHex(!mission.Hidden)));
        obj.Add(XMLUtil.GenerateField("name", "Movie", GenerateMovie(root, mission)));
        obj.Add(XMLUtil.GenerateField("name", "WeatherTime", ConversionUtil.FloatToHex(mission.TimeOfDay)));
        obj.Add(XMLUtil.GenerateField("name", "StopWeatherTimeToEnterTime", ConversionUtil.BoolToHex(mission.TimeOfDay < 0)));
        obj.Add(XMLUtil.GenerateField("name", "WeatherTimeEvolve", ConversionUtil.BoolToHex(XMLUtil.GrabBoolOrDefault(root, "timeEvolve"))));
        obj.Add(XMLUtil.GenerateField("name", "ForcedWeather", ParseWeather(root)));
        obj.Add(XMLUtil.GenerateField("name", "Mission", ConversionUtil.ULongToHex(missionId)));
        obj.Add(ParseWorldPos(root));
        obj.Add(ParsePreviousMissions(root));
        obj.Add(ParseZone(mission));
        obj.Add(ParseHudData(root, mission));
        obj.Add(ParseReward(mission));
        obj.Add(ParseComponents(mission));

        InsertAddCommand("root", obj);
    }

    private string ParseWeather(XElement root)
    {
        string id = XMLUtil.GrabIDHex(root, "weather");
        if (string.IsNullOrWhiteSpace(id))
            return "FFFFFFFFFFFFFFFF";

        return id;
    }

    private string GenerateMovie(XElement root, MissionInfo mission)
    {
        XElement movieTag = root.Element("movie");
        if (movieTag == null)
            return "FFFFFFFFFFFFFFFF";

        float[] coords = XMLUtil.GrabCoords(movieTag, "position");
        if (coords.Length == 0)
            return "FFFFFFFFFFFFFFFF";

        string integer_precise = ConversionUtil.FloatsToHex(coords);

        //Handle motion, default is just a static camera existing in the game
        string staticMovement = "cinematics\\02_missions\\generated\\dlc_ivt-wks-000091_20150408_111548_movie_missioncam.bac";
        string customMovement = XMLUtil.GrabStringOrDefault(root, "predefinedMovieMovement");
        if (!string.IsNullOrWhiteSpace(customMovement))
            staticMovement = customMovement;

        string movieFile = ConversionUtil.ULongToHex(CRC64.Hash(staticMovement, true));

        //now the actual movie obj
        string movieID = ConversionUtil.ULongToHex(mission.GenerateId());

        XElement movieObj = XMLUtil.GenerateObject("name", "Entity");
        movieObj.Add(XMLUtil.GenerateField("name", "ID", movieID));
        movieObj.Add(XMLUtil.GenerateField("name", "FatherArchetypeID", "BC0C020000000000"));
        movieObj.Add(XMLUtil.GenerateField("name", "SpawnPolicy", "00000000"));
        movieObj.Add(XMLUtil.GenerateField("name", "Angles", ConversionUtil.FloatsToHex(XMLUtil.GrabAngles(movieTag))));
        movieObj.Add(XMLUtil.GenerateField("name", "MovieFile", movieFile));
        XElement posObj = XMLUtil.GenerateObject("name", "WorldPosition");
        posObj.Add(XMLUtil.GenerateField("name", "integer", integer_precise));
        posObj.Add(XMLUtil.GenerateField("name", "fraction", "00000000000000000000000000000000"));
        posObj.Add(XMLUtil.GenerateField("name", "integer_precise", integer_precise));
        posObj.Add(XMLUtil.GenerateField("name", "fraction_precise", "00000000000000000000000000000000"));
        movieObj.Add(posObj);

        InsertAddCommand("root", movieObj);
        return movieID;
    }

    private XElement ParseWorldPos(XElement root)
    {
        XElement position = XMLUtil.GenerateObject("name", "WorldPosition");

        float[] coords = XMLUtil.GrabCoords(root, "missionLocation", false);

        if (coords.Length == 0)
            coords = XMLUtil.GrabCoords(root, "spawnPosition");

        if (coords.Length == 0)
            throw new FormatException("No missionLocation or spawnPosition found.");

        string integer_precise = ConversionUtil.FloatsToHex(coords);

        position.Add(XMLUtil.GenerateField("name", "integer", integer_precise));
        position.Add(XMLUtil.GenerateField("name", "fraction", "00000000000000000000000000000000"));
        position.Add(XMLUtil.GenerateField("name", "integer_precise", integer_precise));
        position.Add(XMLUtil.GenerateField("name", "fraction_precise", "00000000000000000000000000000000"));

        return position;
    }

    //id is hex strings designed for external missions outside this mod (like ones already in-game)
    //name is designed for internal names from the same mod.
    private XElement ParsePreviousMissions(XElement root)
    {
        XElement missionFileData = root.Element("missionRequirements");
        XElement compiledMissions = XMLUtil.GenerateObject("name", "PrevMissionsNeeded");

        if (missionFileData == null || missionFileData.Elements().Count() == 0)
            return compiledMissions;

        foreach (XElement missionRequirement in missionFileData.Elements())
        {
            string id = "";
            string elementName = missionRequirement.Name.ToString().ToLowerInvariant();

            switch (elementName)
            {
                case "id":
                    if (missionRequirement.Value.Length != 16)
                    {
                        Logger.Error($"{missionRequirement.Value} is not a valid ID.");
                        break;
                    }
                    id = missionRequirement.Value;
                    break;

                case "name":
                    ulong hashedName = CRC64.Hash(_internalModName + missionRequirement.Value) & 0x00FFFFFFFFFF0000;
                    id = ConversionUtil.ULongToHex(hashedName);
                    break;
            }

            if (string.IsNullOrWhiteSpace(id))
                continue;

            XElement prevMissionsElement = XMLUtil.GenerateObject("name", "PrevMissionsNeededElement");
            prevMissionsElement.Add(XMLUtil.GenerateField("name", "PrevMissionsNeededValue", id));
            compiledMissions.Add(prevMissionsElement);
        }

        return compiledMissions;
    }

    private XElement ParseZone(MissionInfo mission)
    {
        XElement zone = XMLUtil.GenerateObject("name", "Zone");
        zone.Add(XMLUtil.GenerateField("name", "Id", ConversionUtil.StringToHex($"Mission_{mission.InternalName}")));
        return zone;
    }

    private XElement ParseHudData(XElement root, MissionInfo mission)
    {
        XElement hud = XMLUtil.GenerateObject("name", "missionHudData");

        XElement image = ParseImgDiaryID(XMLUtil.GrabStringOrDefault(root, "hudImage"));
        if (image != null)
            hud.Add(image);

        //hud difficulty
        List<string> difficulty = ["defaultleft", "easy", "defaultright", "hard", "veryhard", "emptyleft", "emptyright"];
        string difficultyStr = XMLUtil.GrabStringOrDefault(root, "hudInfo").ToLowerInvariant();
        if (difficulty.Contains(difficultyStr))
            hud.Add(XMLUtil.GenerateField("name", "ForcedHudDifficulty", ConversionUtil.IntToHex(difficulty.IndexOf(difficultyStr))));

        //setup hudLocation
        hud.Add(XMLUtil.GenerateField("name", "missionLocation", ConversionUtil.IntToHex(mission.Strings[StringType.HudLocation])));

        hud.Add(XMLUtil.GenerateField("name", "missionType", ConversionUtil.IntToHex((int)mission.CarRestrict)));

        //restrict type of vehicle (car, bike)
        difficulty = ["none", "car", "bike"];
        difficultyStr = XMLUtil.GrabStringOrDefault(root, "vehicleTypeRestrict", false, "none").ToLowerInvariant();
        hud.Add(XMLUtil.GenerateField("name", "missionVehicleType", ConversionUtil.IntToHex(difficulty.IndexOf(difficultyStr))));

        //restriction for police
        difficulty = ["none", "nopolice", "onlypolice"];
        difficultyStr = XMLUtil.GrabStringOrDefault(root, "policeVehicleRestrict", false, "none").ToLowerInvariant();
        hud.Add(XMLUtil.GenerateField("name", "missionPoliceType", ConversionUtil.IntToHex(difficulty.IndexOf(difficultyStr))));

        hud.Add(XMLUtil.GenerateField("name", "missionDuration", ConversionUtil.IntToHex(mission.Duration.HudDuration)));

        //setup missionObjectiveReminder.
        XElement objective = XMLUtil.GenerateObject("name", "missionObjectives");
        XElement objectiveElement = XMLUtil.GenerateObject("name", "missionObjectivesElement");
        objectiveElement.Add(XMLUtil.GenerateField("name", "missionObjectivesValue", ConversionUtil.IntToHex(mission.Strings[StringType.PreMissionBlurb])));
        objective.Add(objectiveElement);
        hud.Add(objective);

        return hud;
    }

    private XElement? ParseImgDiaryID(string image)
    {
        if (string.IsNullOrWhiteSpace(image))
            return null;

        string imagePath = Path.Combine(_missionFolder, image);
        if (!File.Exists(imagePath))
            return null;

        byte[] imageData = File.ReadAllBytes(imagePath);
        MemoryStream stream = new MemoryStream();
        stream.Write(_xbtHeader, 0, _xbtHeader.Length);
        stream.Write(imageData, 0, imageData.Length);

        string path = $"ui\\textures\\{Path.GetFileNameWithoutExtension(imagePath)}.xbt".ToLowerInvariant();
        _images.TryAdd(path, stream);

        string crc = ConversionUtil.ULongToHex(CRC64.Hash(path, true));
        return XMLUtil.GenerateField("name", "ImgDiaryID", crc);
    }

    private XElement ParseReward(MissionInfo mission)
    {
        XElement rewardData = XMLUtil.GenerateObject("name", "rewardData");
        rewardData.Add(XMLUtil.GenerateField("name", "rewardValues", ConversionUtil.ULongToHex(mission.RewardId)));

        //custom medal points for story
        string[] medalTypes = ["bronze", "silver", "gold", "platinum"];
        if (mission.Points.Length == 0)
            return rewardData;

        XElement objectives = XMLUtil.GenerateObject("name", "Objectives");

        for (int i = 0; i < mission.Points.Length; i++)
        {
            string points = ConversionUtil.IntToHex(mission.Points[i]);
            objectives.Add(XMLUtil.GenerateField("name", medalTypes[i], points));
        }
        
        rewardData.Add(objectives);
        return rewardData;
    }

    private XElement ParseComponents(MissionInfo mission)
    {
        XElement component = XMLUtil.GenerateObject("name", "Components");
        XElement tagGenerator = XMLUtil.GenerateObject("name", "CITEntityComponentTagGenerator");
        tagGenerator.Add(XMLUtil.GenerateField("name", "Name", ConversionUtil.StringToHex(mission.InternalName)));
        tagGenerator.Add(XMLUtil.GenerateField("name", "NameTextID", ConversionUtil.IntToHex(mission.Strings[StringType.Name])));
        tagGenerator.Add(XMLUtil.GenerateField("name", "DescriptionTextID", ConversionUtil.IntToHex(mission.Strings[StringType.Description])));
        component.Add(tagGenerator);

        return component;
    }
}
