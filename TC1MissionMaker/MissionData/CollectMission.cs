using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;


namespace TC1MissionMaker.MissionData
{
    internal class CollectMission : PoliceMission
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
            base.MissionSpecific();

            GenerateSpawnpointEntity(true);
            GenerateRewardMovie(null);
            ParseCrates();
            ParsePoliceTriggers(["A772B217", "40010000", "4AF89AEB", "B90B0000", "120B0000", "003084C500004D45", "B30B0000", "00C085C500807745"]);

            //timer
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "time", 60, true);
            GenerateFullFloatElement("27010000", "0068C9C500009C42", GetTimeMod(time));

            //unknown
            GenerateFullEntityElement("5EF477AA", "0F0D0000", "000034C500807FC4", BitConverter.GetBytes(_spawnpointId));

            //cop stuff

            //Are they cops or enemies (gang members)
            bool gang = XMLUtil.GrabBoolOrDefault(_missionData, "gang", false);
            GenerateFullBoolElement("F00B0000", "004088C500C0BC45", gang);

            //how many cops will spawn on trigger (they still spawn perodically after)
            int value = XMLUtil.GrabIntOrDefault(_missionData, "initCopCount", 1, true);
            GenerateFullFloatElement("A80C0000", "0000184300189845", (float)value);

            //police stars
            value = XMLUtil.GrabIntOrDefault(_missionData, "stars", 1, true);
            GenerateFullFloatElement("750B0000", "0000A7C400408845", (float)value);

            //override police bots
            OverridePoliceCar("DB0B0000", "002071C50088C145", "normal");
            OverridePoliceCar("DC0B0000", "00206CC50088BE45", "fast");
            OverridePoliceCar("DD0B0000", "002066C50008BC45", "offroad");

            //gang ids. You can only have police or gang so we don't need different carTypes.
            OverridePoliceCar("D30B0000", "00F073C50058AF45", "normal");
            OverridePoliceCar("D40B0000", "00F06EC50058AC45", "fast");
            OverridePoliceCar("D50B0000", "00F068C500D8A945", "offroad");
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
