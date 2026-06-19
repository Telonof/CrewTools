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

        public override void MissionSpecific()
        {
            GenerateSpawnpointEntity();
            GenerateRewardMovie(null);
            ParseScoring();

            //timer
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "time", 120, true);
            GenerateFullFloatElement("C65B0000", "0000FDC400001543", time);

            //how much time someone has before car selection is up.
            float carSelectTime = XMLUtil.GrabFloatOrDefault(_missionData, "carSelectTime", 20, true);
            GenerateFullFloatElement("22B80000", "0040F3C500806745", carSelectTime);

            //disable car select screen completely
            bool disableCarSelect = XMLUtil.GrabBoolOrDefault(_missionData, "disableCarSelectScreen");
            if (disableCarSelect)
                GenerateBoolElement("81EAE5AF", disableCarSelect);
        }

        private void ParseScoring()
        {
            XElement? scoreNode = _missionData.Element("scoring");

            if (scoreNode == null)
                return;

            //max multiplier
            GenerateFullFloatElement("ADB80000", "000083C40000BD44", XMLUtil.GrabIntOrDefault(scoreNode, "maxMultiplier", 99, true));

            //how many points are given each second
            GenerateFullFloatElement("AE5C0000", "008003C50000B944", XMLUtil.GrabFloatOrDefault(scoreNode, "pointsPerSecond", 100, true));

            //how long does it take until the next multiplier is given
            GenerateFullFloatElement("ABB80000", "0000A3C40000BD44", XMLUtil.GrabFloatOrDefault(scoreNode, "multiplierWait", 0.8f, true));

            //how long should the game wait until it banks the points.
            GenerateFullFloatElement("D5B70000", "0000FBC40000C744", XMLUtil.GrabFloatOrDefault(scoreNode, "multiplierHoldTime", 1f, true));

            //how much does the multiplier go up by? (Example: 50 means it goes from x1 to x51 instead of x2)
            GenerateFullFloatElement("ACB80000", "000093C40000BD44", XMLUtil.GrabFloatOrDefault(scoreNode, "multiplierIncrease", 1f, true));
        }
    }
}
