using CrewToolsCommon;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    internal class DriftTrialMission : Mission
    {
        public DriftTrialMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
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
            GenerateSpawnpointEntity();
            GenerateRewardMovie(null);

            //timer
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "time", 120, true);
            GenerateFullFloatElement("C65B0000", "0000FDC400001543", time);

            //how much time someone has before car selection is up.
            float carSelectTime = XMLUtil.GrabFloatOrDefault(_missionData, "carSelectTime", 20, true);
            GenerateFullFloatElement("22B80000", "0040F3C500806745", carSelectTime);

            //disable car select screen completely
            bool disableCarSelect = XMLUtil.GrabBoolOrDefault(_missionData, "disableCarSelectScreen");
            GenerateBoolElement("81EAE5AF", disableCarSelect);
        }

        public override bool ParseInstantStart(bool instant)
        {
            if (instant)
                GenerateFullEntityElement("3D549BD4", "375B0000", "00C089C50000AF44", Convert.FromHexString("FFFFFFFFFFFFFFFF"));

            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateFullEntityElement("3D549BD4", "375B0000", "00C089C50000AF44", Convert.FromHexString(entity));
        }
    }
}
