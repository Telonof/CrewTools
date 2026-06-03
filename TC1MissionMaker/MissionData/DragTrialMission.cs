using CrewToolsCommon;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    internal class DragTrialMission : Mission
    {
        public DragTrialMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            int id = _mission.Strings[StringType.ObjectiveReminder];
            if (id == -1)
                return;

            GenerateAtomElement("D975D698", "A273673B", BitConverter.GetBytes(id));
        }

        public override void MissionSpecific()
        {
            byte[][] posData = GenerateCheckpointEntity("6BF8160000000000");
            GenerateRewardMovie(posData);
            GenerateSpawnpointEntity();

            //how much trials should be set for the mission.
            int runs = XMLUtil.GrabIntOrDefault(_missionData, "runs", 3, true);
            GenerateFullFloatElement("2A3E0000", "000090C200008744", runs);

            //how much time someone has before car selection is up.
            float carSelectTime = XMLUtil.GrabFloatOrDefault(_missionData, "carSelectTime", 20, true);
            GenerateFullFloatElement("833F0000", "0040EBC50000B0C2", carSelectTime);

            //disable car select screen completely
            bool disableCarSelect = XMLUtil.GrabBoolOrDefault(_missionData, "disableCarSelectScreen");
            GenerateBoolElement("81EAE5AF", disableCarSelect);
        }

        public override bool ParseInstantStart(bool instant)
        {
            if (instant)
                GenerateFullEntityElement("3D549BD4", "5C3C0000", "0020FCC400801544", Convert.FromHexString("FFFFFFFFFFFFFFFF"));

            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateFullEntityElement("3D549BD4", "5C3C0000", "0020FCC400801544", Convert.FromHexString(entity));
        }
    }
}
