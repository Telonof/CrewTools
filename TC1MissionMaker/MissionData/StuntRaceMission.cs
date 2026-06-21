using CrewToolsCommon;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    internal class StuntRaceMission : Mission
    {
        public StuntRaceMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
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
                GenerateFullEntityElement("3D549BD4", "375B0000", "00C8BFC500E0A244", Convert.FromHexString("FFFFFFFFFFFFFFFF"));

            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateFullEntityElement("3D549BD4", "375B0000", "00C8BFC500E0A244", Convert.FromHexString(entity));
        }

        public override void MissionSpecific()
        {
            byte[][] posData = GenerateCheckpointEntity("6BF8160000000000");
            GenerateRewardMovie(posData);
            GenerateSpawnpointEntity();
            ParseScoring();
            ParseCustomTimes();

            //lap count
            float fvalue = XMLUtil.GrabFloatOrDefault(_missionData, "laps", 0, true);
            GenerateFloatElement("FEC0A05F", fvalue);

            //how much time someone has before car selection is up.
            float carSelectTime = XMLUtil.GrabFloatOrDefault(_missionData, "carSelectTime", 20, true);
            GenerateFullFloatElement("6AB80000", "0098F3C5000005C3", carSelectTime);

            //points
            ulong id = _mission.GenerateId();
            BinaryObject pointEntity = AddChildDirectly(_rootNode, "Entity");
            AddField(pointEntity, "ID", BitConverter.GetBytes(id));
            AddField(pointEntity, "FatherArchetypeID", Convert.FromHexString("C02E940700000000"));
            GenerateFullEntityElement("5EF477AA", "B6B80000", "0000DDC40000AB44", BitConverter.GetBytes(id));

            //max time
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "pointScale", 300, true);
            GenerateFullFloatElement("A4B80000", "00A0D6C4000096C2", time);
        }
        
        private void ParseScoring()
        {
            XElement? scoreNode = _missionData.Element("scoring");

            if (scoreNode == null)
                return;

            //how many points are given each second
            GenerateFullFloatElement("B8B80000", "00800CC50000AB44", XMLUtil.GrabFloatOrDefault(scoreNode, "jumpPointsPerSecond", 150, true));
            
            //how many points are given each second
            GenerateFullFloatElement("BAB80000", "008005C50000B744", XMLUtil.GrabFloatOrDefault(scoreNode, "wheeliePointsPerSecond", 0, true));
        }

        //List is setup so first item is initial time, then every time sets the time rather than adds onto it.
        private void ParseCustomTimes()
        {
            XElement checkpoints = _missionData.Element("checkpoints");
            var allCheckpoints = checkpoints?.Elements("checkpoint");
            int totalCheckpoints = allCheckpoints.Count();
            byte[][] times = new byte[totalCheckpoints + 1][];
            
            float initialTime = XMLUtil.GrabFloatOrDefault(_missionData, "initialTime", 30, true);
            times[0] = BitConverter.GetBytes(initialTime);
            
            for (int i = 0; i < totalCheckpoints; i++)
            {
                float time = XMLUtil.GrabFloatOrDefault(allCheckpoints.ElementAt(i), "setTime", 30, true);
                times[i + 1] =  BitConverter.GetBytes(time);
            }
            
            GenerateFullEntityListElement("C6D3C9A3", "AEB80000", "0000EDC40000B744", times, "FFFFFFFF", "Values");
        }
    }
}
