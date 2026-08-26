using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;


namespace TC1MissionMaker.MissionData
{
    internal class RaceMission : PoliceMission
    {
        //limit of the game
        private const int MAX_BOTS = 7;

        
        public RaceMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            int id = _mission.Strings[StringType.ObjectiveReminder];
            if (id == -1)
                return;

            GenerateAtomElement("D975D698", "A273673B", BitConverter.GetBytes(id));
            GenerateAtomElement("FAF4ACD1", "FB160000", "004089C500C08AC5", BitConverter.GetBytes(id), "FFFFFFFF", "String");
        }

        public override bool ParseInstantStart(bool instant)
        {
            if (instant)
            {
                GenerateFullEntityElement("3D549BD4", "C11A0000", "00C0C0C500802145", Convert.FromHexString("FFFFFFFFFFFFFFFF"));
                GenerateFullBoolElement("8B1A0000", "00C0DCC50000D944", false);
                GenerateFullBoolElement("21030000", "0040D9C500002042", false);
            }

            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateFullEntityElement("3D549BD4", "C11A0000", "00C0C0C500802145", Convert.FromHexString(entity));
            GenerateFullBoolElement("8B1A0000", "00C0DCC50000D944", false);
            GenerateFullBoolElement("21030000", "0040D9C500002042", false);
        }

        public override void MissionSpecific()
        {
            base.MissionSpecific();

            byte[][] posData = GenerateCheckpointEntity("6BF8160000000000");
            GenerateRewardMovie(posData);
            GenerateSpawnpointEntity();
            ParseBots();
            ParsePoliceTriggers(["344ED289", "311A0000", "4AF89AEB", "F9190000", "07190000", "00E8C0C50008DF45", "05190000", "00A8BAC500B0FA45"]);

            //If bots should have a fake vehicle level
            int value = XMLUtil.GrabIntOrDefault(_missionData, "fakeBotLevel", -1);
            if (value != -1)
                AddField(_scriptingNode, "BotTheoricalILevel", BitConverter.GetBytes(value));

            //lap count
            float fvalue = XMLUtil.GrabFloatOrDefault(_missionData, "laps", 0, true);
            GenerateFloatElement("FEC0A05F", fvalue);

            //You must finish 1st or in ?th place.
            fvalue = XMLUtil.GrabFloatOrDefault(_missionData, "finishIn", 1, true);
            GenerateFloatElement("DB474B28", fvalue);

            //disable pedestrian
            bool savedBoolValue = XMLUtil.GrabBoolOrDefault(_missionData, "disablePedestrian");
            GenerateFullBoolElement("B51A0000", "0000FDC400804545", !savedBoolValue);

            //police stars
            value = XMLUtil.GrabIntOrDefault(_missionData, "stars", 1, true);
            GenerateFullFloatElement("68190000", "00F04DC50068F345", (float)value);

            //Are they cops or enemies (gang members)
            savedBoolValue = XMLUtil.GrabBoolOrDefault(_missionData, "gang", false);
            GenerateFullBoolElement("041A0000", "002080C5004C1646", savedBoolValue);

            //how many cops will spawn on trigger (they still spawn perodically after)
            value = XMLUtil.GrabIntOrDefault(_missionData, "initCopCount", 1, true);
            GenerateFullFloatElement("821A0000", "00E00BC500300E46", (float)value);

            //override police bots
            OverridePoliceCar("0A1A0000", "00E060C500B01846", "normal");
            OverridePoliceCar("0B1A0000", "00E05BC500301746", "fast");
            OverridePoliceCar("0C1A0000", "00E055C500F01546", "offroad");
            OverridePoliceCar("0D1A0000", "00E04DC500301646", "roadblock");

            //gang ids. You can only have police or gang so we don't need different carTypes.
            OverridePoliceCar("051A0000", "00B063C500980F46", "normal");
            OverridePoliceCar("061A0000", "00B05EC500180E46", "fast");
            OverridePoliceCar("071A0000", "00B058C500D80C46", "offroad");
            OverridePoliceCar("081A0000", "00B050C500180D46", "roadblock");
        }

        private void ParseBots()
        {
            XElement botConfigs = _missionData.Element("bots");

            var bots = botConfigs?.Elements("bot");
            if (bots == null || !bots.Any())
                return;

            if (bots.Count() > MAX_BOTS)
                Logger.Warning($"Bot count exceeds {MAX_BOTS}. Only parsing {MAX_BOTS} bots.");

            BinaryObject botRoot = AddChildDirectly(_scriptingNode, "ListBotConfig");
            int count = Math.Min(bots.Count(), MAX_BOTS);
            int successfulBotsAdded = 0;

            for (int i = 0; i < count; i++)
            {
                XElement bot = bots.ElementAt(i);

                BinaryObject obj = AddChildDirectly(botRoot, "ListBotConfigElement");
                obj = AddChildDirectly(obj, "ListBotConfigValue");

                //car they drive
                string value = XMLUtil.GrabIDHex(bot, "car", true);
                if (string.IsNullOrWhiteSpace(value))
                {
                    Logger.Error($"No valid car archetype found, skipping.");
                    continue;
                }

                ulong id = _mission.GenerateId();
                _entitiesData.AddBot(id, value);
                AddField(obj, "ArchetypeID", BitConverter.GetBytes(id));

                //archetype for how they act
                value = XMLUtil.GrabIDHex(bot, "behavior", true);
                if (string.IsNullOrWhiteSpace(value))
                {
                    Logger.Error($"No valid car behavior found, skipping.");
                    continue;
                }
                AddField(obj, "AttitudeID", Convert.FromHexString(value));

                //catchup value
                float catchup = XMLUtil.GrabFloatOrDefault(bot, "catchupValue", 1);
                AddField(obj, "CatchupOffset", BitConverter.GetBytes(catchup));

                //name of bot
                value = XMLUtil.GrabStringOrDefault(bot, "name", true, "BOT");
                AddField(obj, "Name", ConversionUtil.StringToByteArray(value));
                successfulBotsAdded++;
            }

            //unknown
            GenerateFullFloatElement("E3030000", "00802145008059C5", successfulBotsAdded);
        }
    }
}
