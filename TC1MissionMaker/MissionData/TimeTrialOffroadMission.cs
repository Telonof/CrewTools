using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;


namespace TC1MissionMaker.MissionData
{
    internal class TimeTrialOffroadMission : Mission
    {
        /* This is an unused mission type in the game. It's the exact same as time trial missions but replace checkpoints with crates to break. */
        public TimeTrialOffroadMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            
        }

        public override void MissionSpecific()
        {
            GenerateSpawnpointEntity();
            ParseCrates();

            //time when mission starts
            float initialTime = XMLUtil.GrabFloatOrDefault(_missionData, "initialTime", 10, true);
            GenerateFullFloatElement("B0000000", "00008144000018C3", initialTime);

            //time given each checkpoint clear
            initialTime = XMLUtil.GrabFloatOrDefault(_missionData, "timePerCrate", 10, true);
            GenerateFloatElement("7B37A6CF", initialTime);
        }

        public override bool ParseInstantStart(bool instant)
        {
            return true;
        }

        public override void ParseTTOG(string entity)
        {
            throw new NotImplementedException();
        }

        private void ParseCrates()
        {
            XElement crateElement = _missionData.Element("crates");

            var crates = crateElement?.Elements("crate");
            if (crates == null || !crates.Any())
                return;

            int count = crates.Count();
            byte[][] crateIds = new byte[count][];

            for (int i = 0; i < count; i++)
            {
                ulong id = _mission.GenerateId();
                XElement crate = crates.ElementAt(i);

                BinaryObject obj = AddChildDirectly(_rootNode, "Entity");
                AddField(obj, "ID", BitConverter.GetBytes(id));
                AddField(obj, "FatherArchetypeID", Convert.FromHexString("7BFB160500000000"));
                AddField(obj, "SpawnPolicy", new byte[4]);
                AddField(obj, "Angles", ConversionUtil.FloatsToByteArray(XMLUtil.GrabAngles(crate)));
                GeneratePositionData(crate, "position", obj, "WorldPosition");

                crateIds[i] = BitConverter.GetBytes(id);
            }

            //crate listings
            GenerateFullEntityListElement("D1FB81EA", "D4000000", "000058430000D344", crateIds , "49C6DF20");
        }
    }
}
