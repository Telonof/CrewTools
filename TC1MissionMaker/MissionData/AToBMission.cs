using CrewToolsCommon;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    internal class AToBMission : Mission
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
            byte[][] posData = GenerateCheckpointEntity("565D460300000000");
            GenerateRewardMovie(posData);
            GenerateSpawnpointEntity(true);

            //timer
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "time", 0, true);
            GenerateFloatElement("7B37A6CF", time);

            //enable timer
            GenerateBoolElement("34C31B74", time > 0);

            //health
            bool health = XMLUtil.GrabBoolOrDefault(_missionData, "health", false);
            GenerateBoolElement("7E1E081D", health);

            if (health)
            {
                //car's damage doesn't heal overtime
                AddField(_scriptingNode, "MissionStopLifeRegen", [0x01]);
                //fail the mission on health 0
                GenerateFullBoolElement("01260000", "0000A3440000A7C4", true);
            }

            //damage ratio (higher means hits do more damage)
            time = XMLUtil.GrabFloatOrDefault(_missionData, "damageRatio", 0.5f, true);
            AddField(_scriptingNode, "MissionDamageRatio", BitConverter.GetBytes(time));

            //unknown
            GenerateFullEntityElement("5EF477AA", "24250000", "002003C600802345", BitConverter.GetBytes(_spawnpointId));

        }
    }
}
