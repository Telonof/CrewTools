using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

using static TC1MissionMaker.Models.MissionType;

namespace TC1MissionMaker.MissionData;

internal abstract class Mission
{
    private const int PROP_LIMIT = 5000;
    //Faction missions aren't always accurate with their times, they seem to be roughly 25-30 seconds off?
    private const int TIME_MOD = 25;

    private readonly Dictionary<string, Dictionary<string, string[]>> _wizards;

    protected readonly XElement _missionData;
    protected readonly MissionInfo _mission;
    protected readonly BinaryObject _rootNode, _scriptingNode, _settingsNode, _atomEntNode, _atomLinkNode, _atomAddNode, _atomVarAddNode;
    protected readonly EntitiesData _entitiesData;

    protected ulong _spawnpointId;

    protected uint _addedId = 2147483648;


    public Mission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards)
    {
        _missionData = missionData;
        _mission = mission;
        _entitiesData = entitesData;
        _wizards = wizards;

        //generate base entity
        _rootNode = GenerateObject("Root");

        BinaryObject child = AddChildDirectly(_rootNode, "Version");
        AddField(child, "EntityCRC", new byte[4]);

        child = AddChildDirectly(_rootNode, "Entity");
        AddField(child, "ID", BitConverter.GetBytes(mission.GrabId(1)));
        AddField(child, "FatherArchetypeID", Convert.FromHexString(mission.Type.FatherArchetypeId));
        AddField(child, "SpawnPolicy", new byte[4]);
        AddField(child, "Angles", new byte[16]);

        GeneratePositionData(_missionData, "spawnPosition", child, "WorldPosition");

        child = AddChildDirectly(child, "Components");
        _scriptingNode = AddChildDirectly(child, "CITEntityComponent_Scripting");
        AddField(_scriptingNode, "CatchupArchetypeID", ParseCatchup());
        //This determines point scaling, set a default of 10 minutes but let people choose their own scale.
        AddField(_scriptingNode, "MaxTime", BitConverter.GetBytes(XMLUtil.GrabIntOrDefault(missionData, "pointScale", 600)));
        AddField(_scriptingNode, "WndScale", BitConverter.GetBytes((uint)34));
        AddField(_scriptingNode, "WndPosition", Convert.FromHexString("00401CC600C03744"));
        AddField(_scriptingNode, "MaxIdAdded", BitConverter.GetBytes((uint)2147483688));

        _settingsNode = AddChildDirectly(_scriptingNode, "ListMissionAtomVariableEnt");
        _atomEntNode = AddChildDirectly(_scriptingNode, "ListMissionAtomEnt");
        _atomLinkNode = AddChildDirectly(_scriptingNode, "ListMissionAtomLinkAdded");
        _atomAddNode = AddChildDirectly(_scriptingNode, "ListMissionAtomAdded");
        _atomVarAddNode = AddChildDirectly(_scriptingNode, "ListMissionAtomVariableAdded");

        AddField(_scriptingNode, "CREDITS", [0x56, 0x47, 0x39, 0x76, 0x62, 0x45, 0x4A, 0x35, 0x52, 0x6C, 0x52, 0x4A, 0x56, 0x77, 0x3D, 0x3D]);

        if (mission.TimeOfDay >= 0)
            EmbedTime();
    }

    public abstract void MissionSpecific();

    /*Each one can generate custom objective reminders but they all do it slightly different enough
    * just to piss me off.*/
    public abstract void GenerateText();

    //If instant start is returned true, do not generate a ttog if the setting exists.
    public abstract bool ParseInstantStart(bool instant);

    //Three two one go/faction intro cutscene
    public abstract void ParseTTOG(string entity);

    public BinaryObject GetRoot()
    {
        return _rootNode;
    }

    public void Parse()
    {
        GenerateMusic();
        ParseNextMission();
        ParseProps();
        ParsePointsForTime();
        ParseFinishCutscene();
        GenerateText();
        MissionSpecific();

        //Traffic
        //Monster needs a hook manually added to it.
        if (!_mission.Type.Equals(MissionType.Monster))
        {
            bool traffic = XMLUtil.GrabBoolOrDefault(_missionData, "traffic", true);
            GenerateBoolElement("AC1FDD9A", traffic);

            //traffic rate
            float trafficRate = XMLUtil.GrabFloatOrDefault(_missionData, "trafficRate", -1);
            if (trafficRate > -1)
                GenerateFloatElement("9D86E46F", trafficRate);
        }

        //damage ratio (higher means hits do more damage)
        float damageRatio = _mission.Type.Equals(MissionType.AToB) ? 0.5f : 0;
        damageRatio = XMLUtil.GrabFloatOrDefault(_missionData, "damageRatio", damageRatio, true);
        //manually setting regen to false crashes the game on finish.
        if (damageRatio > 0)
        {
            AddField(_scriptingNode, "MissionStopLifeRegen", BitConverter.GetBytes(true));
            AddField(_scriptingNode, "MissionDamageRatio", BitConverter.GetBytes(damageRatio));
        }

        //instant start
        bool instantStart = XMLUtil.GrabBoolOrDefault(_missionData, "instantStart", false);
        instantStart = ParseInstantStart(instantStart);

        string ttog = XMLUtil.GrabIDHex(_missionData, "ttog");
        if (!instantStart && !string.IsNullOrWhiteSpace(ttog))
            ParseTTOG(ttog);

        if (_mission.Type.Equals(MissionType.TimeAttack))
            return;

        //loan car
        string loanCar = XMLUtil.GrabIDHex(_missionData, "loanCar");
        if (!string.IsNullOrWhiteSpace(loanCar))
        {
            GenerateEntityElement("3D549BD4", "1ADD3DD4", Convert.FromHexString(loanCar));
            GenerateBoolElement("81EAE5AF", true);
        }
    }

    protected void GenerateRewardMovie(byte[][]? lastPositionData)
    {
        XElement rewardMovieData = _missionData.Element("rewardMovie");

        bool noMovie = XMLUtil.GrabStringOrDefault(rewardMovieData, "position").Equals("none", StringComparison.InvariantCultureIgnoreCase);

        if (noMovie)
            return;

        float[] customCoords = XMLUtil.GrabCoords(rewardMovieData, "position", false);
        float[] customAngles = XMLUtil.GrabAngles(rewardMovieData);

        //We can't automatically add empty coord/yaw data since someone may still want the coord data already provided in lastPositionData.
        if (customCoords.Length != 0)
        {
            lastPositionData[0] = ConversionUtil.FloatsToByteArray(customCoords);
            lastPositionData[1] = ConversionUtil.FloatsToByteArray(customAngles);
        }

        if (lastPositionData == null || lastPositionData.Length == 0)
        {
            lastPositionData = new byte[2][];
            lastPositionData[0] = ConversionUtil.FloatsToByteArray(XMLUtil.GrabCoords(_missionData, "spawnPosition"));
            float yaw = XMLUtil.GrabFloatOrDefault(_missionData, "spawnYaw");
            lastPositionData[1] = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(yaw), 0, lastPositionData[1], 8, 4);
        }

        ulong id = _mission.GenerateId();

        BinaryObject finishNode = AddChildDirectly(_rootNode, "Entity");
        AddField(finishNode, "ID", BitConverter.GetBytes(id));
        AddField(finishNode, "FatherArchetypeID", Convert.FromHexString("6723840700000000"));
        AddField(finishNode, "SpawnPolicy", new byte[4]);
        AddField(finishNode, "Angles", lastPositionData[1]);
        finishNode = AddChildDirectly(finishNode, "WorldPosition");
        AddField(finishNode, "integer", lastPositionData[0]);
        AddField(finishNode, "fraction", new byte[16]);
        AddField(finishNode, "integer_precise", lastPositionData[0]);
        AddField(finishNode, "fraction_precise", new byte[16]);

        //link to settings
        GenerateEntityElement("5EF477AA", "AD5CED4C", BitConverter.GetBytes(id));
    }

    protected float GetTimeMod(float time)
    {
        if (_mission.Class.Equals(MissionClass.Faction) && _mission.CarRestrict != MissionRaceType.ANY)
            return time + TIME_MOD;

        return time;
    }

    protected byte[][] GenerateCheckpointEntity(string checkpointType, string wizardName = "312466AB", bool list = false)
    {
        //Test if checkpoints exist
        XElement checkpoints = _missionData.Element("checkpoints");
        var allCheckpoints = checkpoints?.Elements("checkpoint");
        if (allCheckpoints == null || !allCheckpoints.Any())
            throw new MissingFieldException($"No checkpoints node found in mission {_mission.InternalName}.");

        BinaryObject checkpointNode = AddChildDirectly(_rootNode, "Entity");
        byte[][] lastCheckpointData = new byte[2][];

        string father = "58C1010000000000";
        if (_mission.Type.Equals(MissionType.AToB))
            father = "0109870700000000";

        ulong id = _mission.GenerateId();

        AddField(checkpointNode, "ID", BitConverter.GetBytes(id));
        AddField(checkpointNode, "FatherArchetypeID", Convert.FromHexString(father));
        AddField(checkpointNode, "SpawnPolicy", new byte[4]);
        AddField(checkpointNode, "Angles", new byte[16]);

        //test for laps
        string loop = "FFFFFFFF";
        XElement laps = _missionData.Element("laps");
        int lapCount = XMLUtil.GrabIntOrDefault(_missionData, "laps", 0, true);
        if (_mission.Type.Equals(MissionType.Race) && lapCount > 1)
            loop = "00000000";

        AddField(checkpointNode, "Loop", Convert.FromHexString(loop));

        byte[] checkpointCount = BitConverter.GetBytes((uint)allCheckpoints.Count());
        AddField(checkpointNode, "NumberWaypoint", checkpointCount);
        AddField(checkpointNode, "WaypointStart", new byte[4]);

        ParseCheckpointOptions(checkpointNode);

        GeneratePositionData(_missionData, "spawnPosition", checkpointNode, "WorldPosition");

        BinaryObject waypointList = AddChildDirectly(checkpointNode, "ListWaypointInfo");

        uint currentCheckpoint = 0;
        int totalCheckpoints = allCheckpoints.Count();
        foreach (XElement checkpoint in allCheckpoints)
        {
            byte[] angles = ConversionUtil.FloatsToByteArray(XMLUtil.GrabAngles(checkpoint));

            BinaryObject waypointValue = AddChildDirectly(waypointList, "ListWaypointInfoElement");
            waypointValue = AddChildDirectly(waypointValue, "ListWaypointInfoValue");

            AddField(waypointValue, "Angle", angles);

            AddField(waypointValue, "Snap", BitConverter.GetBytes(XMLUtil.GrabBoolOrDefault(checkpoint, "snap", true)));
            AddField(waypointValue, "OffsetWidth", new byte[4]);
            AddField(waypointValue, "Width", BitConverter.GetBytes(XMLUtil.GrabFloatOrDefault(checkpoint, "width", 10, true)));
            AddField(waypointValue, "ManualDisplayOffsetWidthLeft", new byte[4]);
            AddField(waypointValue, "ManualDisplayOffsetWidthRight", new byte[4]);
            AddField(waypointValue, "Height", BitConverter.GetBytes(XMLUtil.GrabFloatOrDefault(checkpoint, "height", 10, true)));

            //setup checkpoint type
            bool invisible = XMLUtil.GrabBoolOrDefault(checkpoint, "invisible", false);
            if (invisible)
                AddField(waypointValue, "WaypointArchetypeToSpawn", Convert.FromHexString("BE0C020000000000"));
            else
                AddField(waypointValue, "WaypointArchetypeToSpawn", Convert.FromHexString(checkpointType));

            GeneratePositionData(checkpoint, "position", waypointValue, "Position", true);

            lastCheckpointData[0] = ConversionUtil.FloatsToByteArray(XMLUtil.GrabCoords(checkpoint, "position"));
            lastCheckpointData[1] = angles;
            currentCheckpoint++;

            //setup checkpoint unlocks
            List<uint> nexts = [];
            List<uint> unlocks = XMLUtil.GrabIntegers(checkpoint, "unlock", 1);

            //if we have no custom unlocks check if lapped race and add a 0 to loop it at the end, the rest just add one.
            //TODO this needs a clean up
            if (unlocks != null)
                nexts = unlocks;
            else if (currentCheckpoint == totalCheckpoints && loop.Equals("00000000"))
                nexts.Add(0);
            else if (currentCheckpoint < totalCheckpoints)
                nexts.Add(currentCheckpoint);
            else
                continue;

            if (nexts.Count == 0)
                continue;

            BinaryObject next = AddChildDirectly(waypointValue, "ListNextWaypoint");

            foreach (uint unlock in nexts)
            {
                BinaryObject nextElement = AddChildDirectly(next, "ListNextWaypointElement");
                AddField(nextElement, "ListNextWaypointValue", BitConverter.GetBytes(unlock));
            }
        }

        if (list)
        {
            GenerateEntityListElement("D1FB81EA", wizardName, [BitConverter.GetBytes(id)]);
            return lastCheckpointData;
        }

        GenerateEntityElement("5EF477AA", wizardName, BitConverter.GetBytes(id));
        return lastCheckpointData;
    }

    protected void GenerateSpawnpointEntity(bool list = false, bool monster = false)
    {
        BinaryObject spawnpointNode = AddChildDirectly(_rootNode, "Entity");
        XElement? spawnGridData = _missionData.Element("startingGrid");

        _spawnpointId = _mission.GenerateId();
        float yaw = XMLUtil.GrabFloatOrDefault(_missionData, "spawnYaw");
        byte[] angles = new byte[16];
        Buffer.BlockCopy(BitConverter.GetBytes(yaw), 0, angles, 8, 4);

        AddField(spawnpointNode, "ID", BitConverter.GetBytes(_spawnpointId));
        AddField(spawnpointNode, "FatherArchetypeID", BitConverter.GetBytes((ulong)134323));
        AddField(spawnpointNode, "SpawnPolicy", new byte[4]);
        AddField(spawnpointNode, "Angles", angles);
        AddField(spawnpointNode, "Snap", BitConverter.GetBytes(true));
        AddField(spawnpointNode, "IndexPosUseRoadList", BitConverter.GetBytes((uint)5));
        AddField(spawnpointNode, "NumberWaypoint", BitConverter.GetBytes((uint)12));
        AddField(spawnpointNode, "WaypointPerLine", BitConverter.GetBytes(XMLUtil.GrabIntOrDefault(spawnGridData, "maxCarColumns", 2, true)));
        AddField(spawnpointNode, "WidthCompute", BitConverter.GetBytes(XMLUtil.GrabFloatOrDefault(spawnGridData, "widthSpacing", 10, true)));
        AddField(spawnpointNode, "GridLength", BitConverter.GetBytes(XMLUtil.GrabFloatOrDefault(spawnGridData, "lengthSpacing", 8, true)));
        AddField(spawnpointNode, "TimeWaiting", BitConverter.GetBytes(XMLUtil.GrabFloatOrDefault(spawnGridData, "botWaitTime", 6, true)));

        //setup start rank (i.e player is in front when race starts)
        bool startFirst = XMLUtil.GrabBoolOrDefault(spawnGridData, "startFirst", false);
        if (startFirst)
            AddField(spawnpointNode, "PosFromEnd", Convert.FromHexString("00"));

        GeneratePositionData(_missionData, "spawnPosition", spawnpointNode, "WorldPosition");

        if (monster)
        {
            GenerateFullEntityListElement("D1FB81EA", "B4830000", "00806645008037C5", [BitConverter.GetBytes(_spawnpointId)]);
            return;
        }

        if (list)
        {
            GenerateEntityListElement("D1FB81EA", "0976128E", [BitConverter.GetBytes(_spawnpointId)]);
            return;
        }

        GenerateEntityElement("5EF477AA", "0976128E", BitConverter.GetBytes(_spawnpointId));

        if (_mission.Type.Equals(MissionType.Race) || _mission.Type.Equals(MissionType.TimeTrial))
            GenerateEntityElement("5EF477AA", "3F53448C", BitConverter.GetBytes(_spawnpointId));
    }

    protected BinaryObject GenerateObject(string name)
    {
        BinaryObject obj = new BinaryObject();
        obj.NameHash = CRC32.Hash(name);
        return obj;
    }

    protected void AddField(BinaryObject obj, string name, byte[] value)
    {
        obj.Fields.Add(CRC32.Hash(name), value);
    }

    protected void RemoveField(BinaryObject obj, string name)
    {
        obj.Fields.Remove(CRC32.Hash(name));
    }

    protected BinaryObject AddChildDirectly(BinaryObject parent, string name)
    {
        parent.Children.Add(GenerateObject(name));
        return parent.Children.Last();
    }

    protected void GenerateBoolElement(string wizardName, bool value)
    {
        string id = _wizards[wizardName][_mission.Type.Type][0];
        string atomPosition = _wizards[wizardName][_mission.Type.Type][1];

        GenerateFullBoolElement(id, atomPosition, value, wizardName);
    }

    protected void GenerateFullBoolElement(string id, string atomPosition, bool value, string wizardName = "FFFFFFFF")
    {
        GenerateAtomElement("ED505C2F", id, atomPosition, BitConverter.GetBytes(value), wizardName);
    }

    protected void GenerateFloatElement(string wizardName, float value)
    {
        string id = _wizards[wizardName][_mission.Type.Type][0];
        string atomPosition = _wizards[wizardName][_mission.Type.Type][1];

        GenerateFullFloatElement(id, atomPosition, value, wizardName);
    }

    protected void GenerateFullFloatElement(string id, string atomPosition, float value, string wizardName = "FFFFFFFF")
    {
        GenerateAtomElement("DDC4E69A", id, atomPosition, BitConverter.GetBytes(value), wizardName);
    }

    protected void GenerateEntityElement(string className, string wizardName, byte[] value)
    {
        string id = _wizards[wizardName][_mission.Type.Type][0];
        string atomPosition = _wizards[wizardName][_mission.Type.Type][1];

        GenerateFullEntityElement(className, id, atomPosition, value, wizardName);
    }

    protected void GenerateFullEntityElement(string className, string id, string atomPosition, byte[] value, string wizardName = "FFFFFFFF")
    {
        GenerateAtomElement(className, id, atomPosition, value, wizardName, "Entity");
    }

    protected void GenerateEntityListElement(string className, string wizardName, byte[][] values)
    {
        string id = _wizards[wizardName][_mission.Type.Type][0];
        string atomPosition = _wizards[wizardName][_mission.Type.Type][1];
        GenerateFullEntityListElement(className, id, atomPosition, values, wizardName);
    }

    protected void GenerateFullEntityListElement(string className, string id, string atomPosition, byte[][] values, string wizardName = "FFFFFFFF", string listType = "Entities")
    {
        BinaryObject obj = AddChildDirectly(_settingsNode, "ListMissionAtomVariableEntElement");
        obj = AddChildDirectly(obj, "ListMissionAtomVariableEntValue");
        AddField(obj, "hid_DTCTH_ClassName", Convert.FromHexString(className));
        AddField(obj, "ID", Convert.FromHexString(id));
        AddField(obj, "AtomPosition", Convert.FromHexString(atomPosition));
        AddField(obj, "WizardName", Convert.FromHexString(wizardName));
        BinaryObject listObj = AddChildDirectly(obj, listType);
        foreach (byte[] value in values)
        {
            obj = AddChildDirectly(listObj, $"{listType}Element");
            AddField(obj, $"{listType}Value", value);
        }
    }

    protected void GenerateAtomElement(string className, string id, string atomPosition, byte[] value, string wizardName = "FFFFFFFF", string valueName = "Value")
    {
        GenerateFullSetting(_settingsNode, "ListMissionAtomVariableEntElement", "ListMissionAtomVariableEntValue", className, id, atomPosition, value, wizardName, valueName);
    }

    protected void GenerateAtomElement(string className, string wizardName, byte[] value, string valueName = "Value")
    {
        string id = _wizards[wizardName][_mission.Type.Type][0];
        string atomPosition = _wizards[wizardName][_mission.Type.Type][1];

        GenerateAtomElement(className, id, atomPosition, value, wizardName, valueName);
    }

    protected void GeneratePositionData(XElement haystack, string needle, BinaryObject parent, string objName, bool deleteNonPrecise = false)
    {
        byte[] integer_precise = ConversionUtil.FloatsToByteArray(XMLUtil.GrabCoords(haystack, needle));

        BinaryObject worldPos = AddChildDirectly(parent, objName);
        if (!deleteNonPrecise)
        {
            AddField(worldPos, "integer", integer_precise);
            AddField(worldPos, "fraction", new byte[16]);
        }

        AddField(worldPos, "integer_precise", integer_precise);
        AddField(worldPos, "fraction_precise", new byte[16]);
    }

    protected void GenerateLinkElement(uint id, uint targetId)
    {
        BinaryObject linkNode = AddChildDirectly(_atomLinkNode, "ListMissionAtomLinkAddedElement");
        linkNode = AddChildDirectly(linkNode, "ListMissionAtomLinkAddedValue");
        AddField(linkNode, "hid_DTCTH_ClassName", Convert.FromHexString("964F87E6"));
        AddField(linkNode, "ID", BitConverter.GetBytes(id));
        AddField(linkNode, "TargetID", BitConverter.GetBytes(targetId));
    }

    protected void GenerateFullSetting(BinaryObject parent, string elementName, string elementValueName, string className, string id, string atomPosition, byte[] value, string wizardName = "FFFFFFFF", string valueName = "Value")
    {
        BinaryObject setting = AddChildDirectly(parent, elementName);
        setting = AddChildDirectly(setting, elementValueName);
        AddField(setting, "hid_DTCTH_ClassName", Convert.FromHexString(className));
        AddField(setting, "ID", Convert.FromHexString(id));
        AddField(setting, "AtomPosition", Convert.FromHexString(atomPosition));

        if (string.IsNullOrWhiteSpace(wizardName))
            return;

        AddField(setting, "WizardName", Convert.FromHexString(wizardName));
        AddField(setting, valueName, value);
    }

    private void ParseCheckpointOptions(BinaryObject checkpointNode)
    {
        string offroadLine = XMLUtil.GrabStringOrDefault(_missionData, "offroadRacingLine").ToLowerInvariant();
        List<string> racingLines = ["false", "true", "aggressive", "partial"];

        if (racingLines.Contains(offroadLine))
            AddField(checkpointNode, "OffRoad", BitConverter.GetBytes(racingLines.IndexOf(offroadLine)));

        AddField(checkpointNode, "RespectWay", BitConverter.GetBytes(XMLUtil.GrabBoolOrDefault(_missionData, "respectWay", true)));
        AddField(checkpointNode, "UseDirtRoad", BitConverter.GetBytes(XMLUtil.GrabBoolOrDefault(_missionData, "useDirtRoad", true)));
        AddField(checkpointNode, "UseShortcut", BitConverter.GetBytes(XMLUtil.GrabBoolOrDefault(_missionData, "useShortcut", true)));

        //turn off 3d ribbon
        if (_mission.Type.Settings.RacingLineIds == null)
            return;

        bool hideRacingLine = XMLUtil.GrabBoolOrDefault(_missionData, "hideRacingLine");

        //a to b does it differently
        if (!_mission.Type.Equals(MissionType.AToB))
            hideRacingLine = !hideRacingLine;

        GenerateFullBoolElement(_mission.Type.Settings.RacingLineIds[0], _mission.Type.Settings.RacingLineIds[1], hideRacingLine);
    }

    private void GenerateMusic()
    {
        //dont allow hook if hook not found.
        if (_mission.Type.Settings.MusicIds == null)
            return;

        string value = XMLUtil.GrabIDHex(_missionData, "music", false);
        if (string.IsNullOrWhiteSpace(value))
            return;

        GenerateFullEntityElement("3D549BD4", _mission.Type.Settings.MusicIds[0], _mission.Type.Settings.MusicIds[1], Convert.FromHexString(value));
    }

    private void ParseNextMission()
    {
        string nextMission = XMLUtil.GrabStringOrDefault(_missionData, "nextMission");
        if (string.IsNullOrWhiteSpace(nextMission))
            return;

        string next = "Mission_" + nextMission;

        BinaryObject obj = AddChildDirectly(_scriptingNode, "NextZone");
        AddField(obj, "Id", ConversionUtil.StringToByteArray(next));
    }

    //Sets minimum or restricts the amount of points someone can earn through the time portion of missions.
    private void ParsePointsForTime()
    {
        int maxTimePoints = XMLUtil.GrabIntOrDefault(_missionData, "maxTimePoints", -1, true);
        int minTimePoints = XMLUtil.GrabIntOrDefault(_missionData, "minTimePoints", -1, true);
        if (minTimePoints == -1 && maxTimePoints == -1)
            return;

        BinaryObject node = AddChildDirectly(_scriptingNode, "missionScore");

        if (maxTimePoints != -1)
            AddField(node, "MaxTimePoints", BitConverter.GetBytes(maxTimePoints));

        if (minTimePoints != -1)
            AddField(node, "MinTimePoints", BitConverter.GetBytes(minTimePoints));
    }

    private void ParseProps()
    {
        XElement? propsEle = _missionData.Element("props");
        var props = propsEle?.Elements("prop")!;
        if (props == null || !props.Any())
            return;

        if (props.Count() > PROP_LIMIT)
            Logger.Warning($"Prop count exceeds {PROP_LIMIT}, only parsing {PROP_LIMIT}");

        int max = Math.Min(props.Count(), PROP_LIMIT); 
        for (int i = 0; i < max; i++)
        {
            XElement prop = props.ElementAt(i);
            string obj = XMLUtil.GrabIDHex(prop, "object", true);
            if (string.IsNullOrWhiteSpace(obj))
                continue;

            byte[] angles = ConversionUtil.FloatsToByteArray(XMLUtil.GrabAngles(prop));

            ulong id = _mission.GenerateId();

            BinaryObject propObj = AddChildDirectly(_rootNode, "Entity");
            AddField(propObj, "ID", BitConverter.GetBytes(id));
            AddField(propObj, "FatherArchetypeID", Convert.FromHexString(obj));
            AddField(propObj, "SpawnPolicy", [0x06, 0x00, 0x00, 0x00]);
            AddField(propObj, "Angles", angles);
            GeneratePositionData(prop, "position", propObj, "WorldPosition");
        }
    }

    //sets a custom keyframe entity on how ai's handle catching up
    private byte[] ParseCatchup()
    {
        string catchup = XMLUtil.GrabIDHex(_missionData, "catchupArchetype");

        if (string.IsNullOrWhiteSpace(catchup))
            catchup = "50B3940700000000";

        return Convert.FromHexString(catchup);
    }

    private void ParseFinishCutscene()
    {
        XElement rewardMovieData = _missionData.Element("rewardMovie");

        if (_mission.Type.Settings.FinishIds == null)
            return;

        string value = XMLUtil.GrabStringOrDefault(rewardMovieData, "cutscene");
        if (value.Equals("none", StringComparison.InvariantCultureIgnoreCase))
            value = "FFFFFFFFFFFFFFFF";

        if (!string.IsNullOrWhiteSpace(value) && value.Length == 16)
        {
            //hitting the finish line
            GenerateFullEntityElement(_mission.Type.Settings.FinishIds[0], _mission.Type.Settings.FinishIds[1], _mission.Type.Settings.FinishIds[2], Convert.FromHexString(value));
            //camera rotation
            GenerateFullEntityElement(_mission.Type.Settings.FinishIds[3], _mission.Type.Settings.FinishIds[4], _mission.Type.Settings.FinishIds[5], Convert.FromHexString("FFFFFFFFFFFFFFFF"));
        }

        //3-4 finish
        value = XMLUtil.GrabStringOrDefault(rewardMovieData, "rewardView");
        if (value.Equals("none", StringComparison.InvariantCultureIgnoreCase))
            value = "FFFFFFFFFFFFFFFF";

        if (!string.IsNullOrWhiteSpace(value) && value.Length == 16)
            GenerateFullEntityElement(_mission.Type.Settings.FinishIds[6], _mission.Type.Settings.FinishIds[7], _mission.Type.Settings.FinishIds[8], Convert.FromHexString(value));
    }

    //Chained missions do not read any data from mission spawner which contains the data such as vehicle restrict/time of day.
    //So we hook into the time variables directly inside the mission to force set the time.

    //TODO should make this a custom method for it and traffic embed in monster.
    private void EmbedTime()
    {
        uint id = _addedId;

        //atom ent
        BinaryObject setting = AddChildDirectly(_atomEntNode, "ListMissionAtomEntElement");
        setting = AddChildDirectly(setting, "ListMissionAtomEntValue");
        AddField(setting, "hid_DTCTH_ClassName", Convert.FromHexString("4AF89AEB"));
        AddField(setting, "ID", Convert.FromHexString(_mission.Type.Settings.VarTimeId));

        BinaryObject link = AddChildDirectly(setting, "TrueLinkID");
        link = AddChildDirectly(link, "TrueLinkIDElement");
        AddField(link, "TrueLinkIDValue", BitConverter.GetBytes(id++));
        link = AddChildDirectly(setting, "FalseLinkID");
        link = AddChildDirectly(link, "FalseLinkIDElement");
        AddField(link, "FalseLinkIDValue", BitConverter.GetBytes(id++));

        //atom added
        setting = AddChildDirectly(_atomAddNode, "ListMissionAtomAddedElement");
        setting = AddChildDirectly(setting, "ListMissionAtomAddedValue");

        AddField(setting, "hid_DTCTH_ClassName", Convert.FromHexString("C535E217"));
        AddField(setting, "ID", BitConverter.GetBytes(id++));
        AddField(setting, "AtomPosition", Convert.FromHexString("006800C600408344"));
        AddField(setting, "EnableTimeID", BitConverter.GetBytes(id++));
        AddField(setting, "VarTimeID", BitConverter.GetBytes(id++));
        AddField(setting, "VarFadeTimeID", BitConverter.GetBytes(id));

        //reset counter back to 0
        id -= 5;

        setting = AddChildDirectly(setting, "StartLinkID");
        link = AddChildDirectly(setting, "StartLinkIDElement");
        AddField(link, "StartLinkIDValue", BitConverter.GetBytes(id++));
        link = AddChildDirectly(setting, "StartLinkIDElement");
        AddField(link, "StartLinkIDValue", BitConverter.GetBytes(id--));

        //linker
        GenerateLinkElement(id, id + 2);
        GenerateLinkElement(id + 1, id + 2);
        GenerateLinkElement(id + 3, id + 6);
        GenerateLinkElement(id + 4, id + 7);
        GenerateLinkElement(id + 5, id + 8);

        setting = _atomVarAddNode;
        GenerateFullSetting(setting, "ListMissionAtomVariableAddedElement", "ListMissionAtomVariableAddedValue", "ED505C2F", "06000080", "00E000C60000A544", [0x01]);
        GenerateFullSetting(setting, "ListMissionAtomVariableAddedElement", "ListMissionAtomVariableAddedValue", "DDC4E69A", "07000080", "00C0FCC50000A544", BitConverter.GetBytes(_mission.TimeOfDay));
        GenerateFullSetting(setting, "ListMissionAtomVariableAddedElement", "ListMissionAtomVariableAddedValue", "DDC4E69A", "08000080", "00C0F7C50000A544", new byte[4]);

        //We used 9 here, so add 9 to the total.
        _addedId += 9;
    }
}
