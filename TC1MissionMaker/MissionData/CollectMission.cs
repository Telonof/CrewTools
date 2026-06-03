using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;


namespace TC1MissionMaker.MissionData
{
    internal class CollectMission : Mission
    {
        private const int MAX_CRATES = 1000;


        public CollectMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            int id = _mission.Strings[StringType.ObjectiveReminder];
            if (id == -1)
                return;

            GenerateAtomElement("D975D698", "740D0000", "00E00AC600007EC4", BitConverter.GetBytes(id));
        }

        public override bool ParseInstantStart(bool instant)
        {
            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateEntityElement("3D549BD4", "C7871B87", Convert.FromHexString(entity));
            GenerateFullBoolElement("5F0D0000", "003053C5002012C5", true);
        }

        public override void MissionSpecific()
        {
            GenerateSpawnpointEntity(true);
            GenerateRewardMovie(null);
            ParseCrates();

            //timer
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "time", 60, true);
            GenerateFullFloatElement("27010000", "0068C9C500009C42", GetTimeMod(time));

            //unknown
            GenerateFullEntityElement("5EF477AA", "0F0D0000", "000034C500807FC4", BitConverter.GetBytes(_spawnpointId));
        }

        private void ParseCrates()
        {
            XElement crateElement = _missionData.Element("crates");

            var crates = crateElement?.Elements("crate");
            if (crates == null || !crates.Any())
                return;

            if (crates.Count() > MAX_CRATES)
                Logger.Warning($"Crate count exceeds {MAX_CRATES}. Only parsing {MAX_CRATES} crates.");

            int count = Math.Min(crates.Count(), MAX_CRATES);
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

            //how many they need to destroy
            int crateCount = XMLUtil.GrabIntOrDefault(_missionData, "cratesRequired", count, true);
            GenerateFullFloatElement("C7000000", "0040BBC50000B042", crateCount);

            //crate listings
            GenerateFullEntityListElement("D1FB81EA", "24010000", "00C0A9C500800045", crateIds);
            GenerateFullEntityListElement("D1FB81EA", "1E010000", "00C0A9C50000C344", crateIds);
            GenerateFullEntityListElement("D1FB81EA", "18010000", "0040ABC500000A44", crateIds);
            GenerateFullEntityListElement("D1FB81EA", "14010000", "0018AEC500001043", crateIds);
        }
    }
}
