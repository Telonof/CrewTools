using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    internal class MonsterMission : Mission
    {

        private const int MAX_TARGETS = 2048;

        private readonly Dictionary<Category, CategoryInfo> _categories;


        public MonsterMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
            _categories = [];

            _categories.Add(Category.BRONZE, new CategoryInfo()
            {
                FatherID = "3B62930700000000",
                ListID = "62200000",
                ListAtomPosition = "0000B04200005244"
            });

            _categories.Add(Category.SILVER, new CategoryInfo()
            {
                FatherID = "3F62930700000000",
                ListID = "60200000",
                ListAtomPosition = "0000DC4300007644"
            });

            _categories.Add(Category.GOLD, new CategoryInfo()
            {
                FatherID = "E6B3930700000000",
                ListID = "5E200000",
                ListAtomPosition = "00004A4400006244"
            });

            _categories.Add(Category.PLATINUM, new CategoryInfo()
            {
                FatherID = "1BDA930700000000",
                ListID = "C2060100",
                ListAtomPosition = "0000994400005A44"
            });
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
                GenerateFullEntityElement("3D549BD4", "39830000", "008009C500004244", Convert.FromHexString("FFFFFFFFFFFFFFFF"));

            return instant;
        }

        public override void ParseTTOG(string entity)
        {
            GenerateFullEntityElement("3D549BD4", "39830000", "008009C500004244", Convert.FromHexString(entity));
        }

        public override void MissionSpecific()
        {
            GenerateSpawnpointEntity(true, true);
            GenerateFullEntityListElement("D1FB81EA", "E3410000", "0038DC45004881C5", [BitConverter.GetBytes(_spawnpointId)], "993E7E7B");
            GenerateFullEntityListElement("D1FB81EA", "3B830000", "0020A0C500803B44", [BitConverter.GetBytes(_spawnpointId)], "0D0A6296");

            GenerateRewardMovie(null);
            ParseTargets();
            ParsePointRespawn();
            EmbedTraffic();

            //how much time someone has before car selection is up.
            float carSelectTime = XMLUtil.GrabFloatOrDefault(_missionData, "carSelectTime", 20, true);
            GenerateFullFloatElement("22070100", "00B0F1C500005F44", carSelectTime);

            //disable car select screen completely
            bool disableCarSelect = XMLUtil.GrabBoolOrDefault(_missionData, "disableCarSelectScreen");
            if (disableCarSelect)
                GenerateBoolElement("81EAE5AF", disableCarSelect);

            //timer
            float time = XMLUtil.GrabFloatOrDefault(_missionData, "time", 300, true);
            GenerateFullFloatElement("E60D0000", "00D883C500E896C5", time);

            //points
            ulong id = _mission.GenerateId();
            BinaryObject pointEntity = AddChildDirectly(_rootNode, "Entity");
            AddField(pointEntity, "ID", BitConverter.GetBytes(id));
            AddField(pointEntity, "FatherArchetypeID", Convert.FromHexString("C12E940700000000"));
            GenerateFullEntityElement("5EF477AA", "8F070100", "0000C74400007E44", BitConverter.GetBytes(id));

            //how many points are given each second
            GenerateFullFloatElement("6F200000", "0000B4C300005244", XMLUtil.GrabFloatOrDefault(_missionData, "jumpPointsPerSecond", 100, true));
        }

        private void ParsePointRespawn()
        {
            XElement? scoreNode = _missionData.Element("respawnTimes");

            if (scoreNode == null)
                return;

            //bronze
            GenerateFullFloatElement("69200000", "000008C300005244", XMLUtil.GrabFloatOrDefault(scoreNode, "bronze", -1, true));
            //silv
            GenerateFullFloatElement("6B200000", "0000184300006A44", XMLUtil.GrabFloatOrDefault(scoreNode, "silver", -1, true));
            //gold
            GenerateFullFloatElement("6D200000", "0000EC4300005644", XMLUtil.GrabFloatOrDefault(scoreNode, "gold", -1, true));
            //plat
            GenerateFullFloatElement("C0060100", "0000464400007244", XMLUtil.GrabFloatOrDefault(scoreNode, "platinum", -1, true));
        }

        private void ParseTargets()
        {
            XElement crateElement = _missionData.Element("targets");
            Dictionary<Category, List<byte[]>> targets = [];

            var crates = crateElement?.Elements("target");
            if (crates == null || !crates.Any())
                return;

            if (crates.Count() > MAX_TARGETS)
                Logger.Warning($"Target count exceeds {MAX_TARGETS}. Only parsing {MAX_TARGETS} targets.");

            int count = Math.Min(crates.Count(), MAX_TARGETS);

            for (int i = 0; i < count; i++)
            {
                ulong id = _mission.GenerateId();
                XElement crate = crates.ElementAt(i);

                //get category
                string category = XMLUtil.GrabStringOrDefault(crate, "type", false, "BRONZE").ToUpperInvariant();
                Enum.TryParse(category, true, out Category cat);

                BinaryObject obj = AddChildDirectly(_rootNode, "Entity");
                AddField(obj, "ID", BitConverter.GetBytes(id));
                AddField(obj, "FatherArchetypeID", Convert.FromHexString(_categories[cat].FatherID));
                AddField(obj, "SpawnPolicy", new byte[4]);
                AddField(obj, "Angles", ConversionUtil.FloatsToByteArray(XMLUtil.GrabAngles(crate)));
                GeneratePositionData(crate, "position", obj, "WorldPosition");

                targets.TryAdd(cat, []);
                targets[cat].Add(BitConverter.GetBytes(id));
            }

            foreach (Category cat in targets.Keys)
            {
                GenerateFullEntityListElement("D1FB81EA", _categories[cat].ListID, _categories[cat].ListAtomPosition, targets[cat].ToArray());
            }
        }

        private record CategoryInfo
        {
            public string FatherID { get; set; }

            public string ListID { get; set; }

            public string ListAtomPosition { get; set; }
        }

        private enum Category
        {
            BRONZE,
            SILVER,
            GOLD,
            PLATINUM
        }

        //Monster missions don't have the typical traffic node, so we have to make a hook ourselves.
        private void EmbedTraffic()
        {
            uint id = _addedId;

            bool traffic = XMLUtil.GrabBoolOrDefault(_missionData, "traffic", true);
            float trafficRate = XMLUtil.GrabFloatOrDefault(_missionData, "trafficRate", 0.4f, true);

            //atom ent
            BinaryObject setting = AddChildDirectly(_atomEntNode, "ListMissionAtomEntElement");
            setting = AddChildDirectly(setting, "ListMissionAtomEntValue");
            AddField(setting, "hid_DTCTH_ClassName", Convert.FromHexString("344ED289"));
            AddField(setting, "ID", Convert.FromHexString("F80D0000"));

            BinaryObject link = AddChildDirectly(setting, "EndLinkID");
            link = AddChildDirectly(link, "EndLinkIDElement");
            AddField(link, "EndLinkIDValue", BitConverter.GetBytes(id + 3));

            //atom added
            setting = AddChildDirectly(_atomAddNode, "ListMissionAtomAddedElement");
            setting = AddChildDirectly(setting, "ListMissionAtomAddedValue");

            AddField(setting, "hid_DTCTH_ClassName", Convert.FromHexString("6EF39A9B"));
            AddField(setting, "ID", BitConverter.GetBytes(id++));
            AddField(setting, "AtomPosition", Convert.FromHexString("00C8BDC500706245"));
            AddField(setting, "VarEnableID", BitConverter.GetBytes(id++));
            AddField(setting, "VarCountModifierID", BitConverter.GetBytes(id++));

            setting = AddChildDirectly(setting, "StartLinkID");
            link = AddChildDirectly(setting, "StartLinkIDElement");
            AddField(link, "StartLinkIDValue", BitConverter.GetBytes(id));

            id -= 3;

            //linker
            GenerateLinkElement(id + 3, id);
            GenerateLinkElement(id + 1, id + 4);
            GenerateLinkElement(id + 2, id + 5);

            GenerateFullSetting(_atomVarAddNode, "ListMissionAtomVariableAddedElement", "ListMissionAtomVariableAddedValue", "ED505C2F", Convert.ToHexString(BitConverter.GetBytes(id + 4)), "00E000C60000A544", BitConverter.GetBytes(traffic), "AC1FDD9A");
            GenerateFullSetting(_atomVarAddNode, "ListMissionAtomVariableAddedElement", "ListMissionAtomVariableAddedValue", "DDC4E69A", Convert.ToHexString(BitConverter.GetBytes(id + 5)), "00C0FCC50000A544", BitConverter.GetBytes(trafficRate), "9D86E46F");

            _addedId += 6;
        }
    }
}
