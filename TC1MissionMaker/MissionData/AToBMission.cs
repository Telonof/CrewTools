using CrewToolsCommon;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    internal class AToBMission : PoliceMission
    {
        public AToBMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            int id = _mission.Strings[StringType.ObjectiveReminder];
            if (id == -1)
                return;

            GenerateAtomElement("FAF4ACD1", "A273673B", BitConverter.GetBytes(id), "String");
            GenerateAtomElement("D975D698", "04260000", "00789BC500C0C7C4", BitConverter.GetBytes(id));
        }

        public override bool ParseInstantStart(bool instant)
        {
            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateEntityElement("3D549BD4", "C7871B87", Convert.FromHexString(entity));
            GenerateFullBoolElement("05250000", "002010C600801445", false);
        }

        public override void MissionSpecific()
        {
            base.MissionSpecific();

            byte[][] posData = GenerateCheckpointEntity("565D460300000000", "312466AB", false, "0109870700000000");
            GenerateRewardMovie(posData);
            GenerateSpawnpointEntity(true);
            ParsePoliceTriggers(["A772B217", "1F000000", "4AF89AEB", "2B240000", "BF230000", "008003C50040BB45", "C2230000", "0000FFC400C0D045"]);

            //timer
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "time", 0, true);
            GenerateFloatElement("7B37A6CF", GetTimeMod(time));

            //enable timer
            GenerateBoolElement("34C31B74", time > 0);

            //health
            bool health = XMLUtil.GrabBoolOrDefault(_missionData, "health", false);
            GenerateBoolElement("7E1E081D", health);

            //fail the mission on health 0
            if (health)
                GenerateFullBoolElement("01260000", "0000A3440000A7C4", true);
            else
                RemoveField(_scriptingNode, "MissionStopLifeRegen");

            //unknown, but needed for proper spawn point
            GenerateFullEntityElement("5EF477AA", "24250000", "002003C600802345", BitConverter.GetBytes(_spawnpointId));

            //cop stuff

            //Are they cops or enemies (gang members)
            bool gang = XMLUtil.GrabBoolOrDefault(_missionData, "gang", false);
            GenerateFullBoolElement("41240000", "006020C500900046", gang);

            //how many cops will spawn on trigger (they still spawn perodically after)
            int value = XMLUtil.GrabIntOrDefault(_missionData, "initCopCount", 1, true);
            GenerateFullFloatElement("E2240000", "00A0114500A8D845", (float)value);

            //police stars
            value = XMLUtil.GrabIntOrDefault(_missionData, "stars", 1, true);
            GenerateFullFloatElement("DF230000", "0000424400C0C045", (float)value);

            //override police bots
            OverridePoliceCar("37240000", "000001C500F40246", "normal");
            OverridePoliceCar("36240000", "0000F8C400740146", "fast");
            OverridePoliceCar("35240000", "0000ECC400340046", "offroad");
            OverridePoliceCar("34240000", "0000DCC400740046", "roadblock");

            //gang ids. You can only have police or gang so we don't need different carTypes.
            OverridePoliceCar("3C240000", "00D003C500B8F345", "normal");
            OverridePoliceCar("3B240000", "00A0FDC400B8F045", "fast");
            OverridePoliceCar("3A240000", "00A0F1C40038EE45", "offroad");
            OverridePoliceCar("39240000", "00A0E1C400B8EE45", "roadblock");
        }
    }
}
