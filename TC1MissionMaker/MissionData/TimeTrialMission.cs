using CrewToolsCommon;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    internal class TimeTrialMission : PoliceMission
    {
        public TimeTrialMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            int id = _mission.Strings[StringType.ObjectiveReminder];
            if (id == -1)
                return;

            GenerateAtomElement("D975D698", "A273673B", BitConverter.GetBytes(id));
            GenerateAtomElement("FAF4ACD1", "97080000", "00409BC500007EC4", BitConverter.GetBytes(id), "FFFFFFFF", "String");
        }

        public override bool ParseInstantStart(bool instant)
        {
            if (instant)
            {
                GenerateFullBoolElement("CD000000", "0068E6C500403A44", instant);
                GenerateFullBoolElement("980A0000", "00C0E6C50000DD44", false);
                GenerateEntityElement("3D549BD4", "C7871B87", Convert.FromHexString("FFFFFFFFFFFFFFFF"));
            }

            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateEntityElement("3D549BD4", "C7871B87", Convert.FromHexString(entity));
            GenerateFullBoolElement("980A0000", "00C0E6C50000DD44", false);
            GenerateFullBoolElement("CD000000", "0068E6C500403A44", true);
        }

        public override void MissionSpecific()
        {
            base.MissionSpecific();

            byte[][] posData = GenerateCheckpointEntity("DDFF3C0100000000");
            GenerateRewardMovie(posData);
            GenerateSpawnpointEntity();
            ParsePoliceTriggers(["9C3DB6AF", "30090000", "D38F084C", "89090000", "21090000", "00E0A04400A88445", "1F090000", "0000BB4400C0A145"]);

            //dont start timer until ttog cutscene ends
            GenerateFullFloatElement("190A0000", "00802EC500005AC4", 0f);
            //modifier for checkpoint time (should always be 0, -1 means minus 1 second per checkpoint)
            GenerateFullFloatElement("2E0A0000", "0000E5C4000060C2", 0f);

            //time when mission starts
            float initialTime = XMLUtil.GrabFloatOrDefault(_missionData, "initialTime", 10, true);
            GenerateFullFloatElement("0A000000", "008040C50000D4C3", initialTime);

            //time given each checkpoint clear
            initialTime = XMLUtil.GrabFloatOrDefault(_missionData, "timePerCheckpoint", 10, true);
            GenerateFloatElement("7B37A6CF", initialTime);

            //police stars
            int value = XMLUtil.GrabIntOrDefault(_missionData, "stars", 1, true);
            GenerateFullFloatElement("83090000", "00B07E4500608A45", (float)value);

            //how many cops will spawn on trigger (they still spawn perodically after)
            value = XMLUtil.GrabIntOrDefault(_missionData, "initCopCount", 1, true);
            GenerateFullFloatElement("7C0A0000", "000898450050B945", (float)value);

            //actual police trigger enable
            GenerateFullBoolElement("820A0000", "0000F04200C09045", true);

            //override spawn params of police
            string spawnParamEntity = XMLUtil.GrabIDHex(_missionData, "policeSpawnParams");
            if (!string.IsNullOrWhiteSpace(spawnParamEntity))
                GenerateFullEntityElement("3D549BD4", "DD0A0000", "0040D0C50060DAC4", Convert.FromHexString(spawnParamEntity));

            //Are they cops or enemies (gang members)
            bool gang = XMLUtil.GrabBoolOrDefault(_missionData, "gang", false);
            GenerateFullBoolElement("F2090000", "0028FFC500E0CCC4", gang);

            //override police bots
            OverridePoliceCar("F8090000", "0078EFC500C0B9C4", "normal");
            OverridePoliceCar("F9090000", "00F8ECC500C0C5C4", "fast");
            OverridePoliceCar("FA090000", "00F8E9C500C0CFC4", "offroad");

            //gang ids. You can only have police or gang so we don't need different carTypes.
            OverridePoliceCar("F3090000", "00E0F0C5004001C5", "normal");
            OverridePoliceCar("F4090000", "0060EEC5004007C5", "fast");
            OverridePoliceCar("F5090000", "0060EBC500400CC5", "offroad");

            //override behavior
            spawnParamEntity = XMLUtil.GrabIDHex(_missionData, "policeBehavior");
            if (!string.IsNullOrWhiteSpace(spawnParamEntity))
            {
                GenerateFullEntityElement("3D549BD4", "8B0B0000", "00C0E6C50000C1C4", Convert.FromHexString(spawnParamEntity));
                GenerateFullEntityElement("3D549BD4", "8D0B0000", "0040E8C5008001C5", Convert.FromHexString(spawnParamEntity));
            }

            //unknown
            GenerateFullBoolElement("1E0B0000", "0000F1C400800145", true);
        }
    }
}
