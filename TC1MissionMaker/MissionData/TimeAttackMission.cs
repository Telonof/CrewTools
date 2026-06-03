using CrewToolsCommon;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    internal class TimeAttackMission : Mission
    {
        public TimeAttackMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            int id = _mission.Strings[StringType.ObjectiveReminder];
            if (id == -1)
                return;

            GenerateAtomElement("D975D698", "A273673B", BitConverter.GetBytes(id));
        }

        public override bool ParseInstantStart(bool instant)
        {
            if (instant)
                GenerateFullEntityElement("3D549BD4", "375B0000", "0048D4C500C02544", Convert.FromHexString("FFFFFFFFFFFFFFFF"));

            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateFullEntityElement("3D549BD4", "375B0000", "0048D4C500C02544", Convert.FromHexString(entity));
        }

        public override void MissionSpecific()
        {
            byte[][] posData = GenerateCheckpointEntity("DDFF3C0100000000");
            GenerateRewardMovie(posData);
            GenerateSpawnpointEntity();

            //how much time someone has before car selection is up.
            float carSelectTime = XMLUtil.GrabFloatOrDefault(_missionData, "carSelectTime", 20, true);
            GenerateFullFloatElement("6AB80000", "0098F3C5000005C3", carSelectTime);
        }
    }
}
