using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;


namespace TC1MissionMaker.MissionData
{
    internal class TakedownMission : Mission
    {
        public TakedownMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            int id = _mission.Strings[StringType.ObjectiveReminder];
            if (id == -1)
                return;

            GenerateAtomElement("FAF4ACD1", "A273673B", BitConverter.GetBytes(id), "String");
            GenerateAtomElement("D975D698", "87160000", "0040BFC500B000C5", BitConverter.GetBytes(id));
        }

        public override bool ParseInstantStart(bool instant)
        {
            return true;
        }

        public override void ParseTTOG(string entity)
        {
            throw new NotImplementedException();
        }

        public override void MissionSpecific()
        {
            byte[][] posData = GenerateCheckpointEntity("BF0C020000000000", "B120B628", true);
            GenerateRewardMovie(posData);
            GenerateSpawnpointEntity(true);
            ParseBotInfo();

            //timer
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "time", 0, true);
            GenerateFloatElement("7B37A6CF", GetTimeMod(time));

            //enable timer
            GenerateBoolElement("34C31B74", time > 0);

            //player speed on spawn
            time = XMLUtil.GrabFloatOrDefault(_missionData, "playerSpawnSpeed", 0);
            GenerateFloatElement("0D7E44B7", time);

            //seems to turn off random health scaling (or at least one of them)
            GenerateFullFloatElement("01140000", "00004EC400804045", 0);
            GenerateFullFloatElement("F6130000", "008028450000DF44", 0);
            GenerateFullFloatElement("F5130000", "00801D450000DF44", 0);

            //unknown
            GenerateFullBoolElement("33160000", "00800DC500807445", true);
            GenerateFullBoolElement("D1120000", "0000A3C4008073C5", false);
        }

        private void ParseBotInfo()
        {
            XElement botInfo = _missionData.Element("botInfo");
            if (botInfo == null)
                return;

            //car bot is driving
            string botCar = XMLUtil.GrabIDHex(botInfo, "car", true);
            if (string.IsNullOrWhiteSpace(botCar))
                return;
               
            ulong id = _mission.GenerateId();
            _entitiesData.AddBot(id, botCar);
            GenerateEntityListElement("40CDDA90", "4771C39B", [BitConverter.GetBytes(id)]);

            //behavior settings of bot
            botCar = XMLUtil.GrabIDHex(botInfo, "behavior");
            if (string.IsNullOrWhiteSpace(botCar))
                return;

            GenerateEntityListElement("40CDDA90", "107B6C93", [Convert.FromHexString(botCar)]);

            //health of bot
            float botHealth = XMLUtil.GrabFloatOrDefault(botInfo, "health", 1, true);
            id = _mission.GenerateId();
            _entitiesData.AddTakedownHealth(id, botHealth);
            GenerateEntityListElement("40CDDA90", "C091FFB9", [BitConverter.GetBytes(id)]);

            //bot speed on spawn
            botHealth = XMLUtil.GrabFloatOrDefault(botInfo, "spawnSpeed", 0);
            GenerateFloatElement("F8DC825F", botHealth);

            //spawn position
            id = _mission.GenerateId();

            BinaryObject obj = AddChildDirectly(_rootNode, "Entity");
            AddField(obj, "ID", BitConverter.GetBytes(id));
            AddField(obj, "FatherArchetypeID", Convert.FromHexString("BD0C020000000000"));
            AddField(obj, "SpawnPolicy", new byte[4]);
            AddField(obj, "Angles", ConversionUtil.FloatsToByteArray(XMLUtil.GrabAngles(botInfo)));
            GeneratePositionData(botInfo, "position", obj, "WorldPosition");
            
            //Add link to spawn in settings
            GenerateEntityListElement("D1FB81EA", "7EE8BB3A", [BitConverter.GetBytes(id)]);
        }
    }
}
