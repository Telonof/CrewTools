using CrewToolsCommon;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;


namespace TC1MissionMaker.MissionData
{
    internal class EscapeMission : PoliceMission
    {
        public EscapeMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            int id = _mission.Strings[StringType.ObjectiveReminder];
            if (id == -1)
                return;

            GenerateAtomElement("D975D698", "68120000", "0000DFC40000A3C4", BitConverter.GetBytes(id));
        }

        public override bool ParseInstantStart(bool instant)
        {
            return true;
        }

        public override void ParseTTOG(string entity)
        {
            throw new NotImplementedException();
        }

        public override void MissionSpecific()
        {
            base.MissionSpecific();

            GenerateSpawnpointEntity(true);
            GenerateRewardMovie(null);

            //stars at the start
            int value = XMLUtil.GrabIntOrDefault(_missionData, "stars", 1, true);
            GenerateFullFloatElement("4B000000", "008026C5000008C3", (float)value);

            //how many cops will spawn at the start (they still spawn perodically after)
            value = XMLUtil.GrabIntOrDefault(_missionData, "initCopCount", 1, true);
            GenerateFullFloatElement("6A110000", "0000EFC40000BCC3", (float)value);

            //Are they cops or enemies (gang members)
            bool gang = XMLUtil.GrabBoolOrDefault(_missionData, "gang", false);
            GenerateFullBoolElement("2F110000", "00802FC500006A44", gang);

            //unknown
            GenerateFullEntityElement("5EF477AA", "68110000", "008000C50000B4C3", BitConverter.GetBytes(_spawnpointId));

            //override spawn params of police
            string spawnParamEntity = XMLUtil.GrabIDHex(_missionData, "policeSpawnParams");
            if (!string.IsNullOrWhiteSpace(spawnParamEntity))
                GenerateFullEntityElement("3D549BD4", "A8110000", "00C0F4C500804645", Convert.FromHexString(spawnParamEntity));

            //Would of been nice if I had wrote down unknown or what these do.
            GenerateFullBoolElement("35120000", "009817C600505645", false);
            GenerateFullFloatElement("A6000000", "0040E8C500008944", 50);

            //override police bots
            OverridePoliceCar("19110000", "00E005C600805845", "normal");
            OverridePoliceCar("3A110000", "00A004C600805245", "fast");
            OverridePoliceCar("3D120000", "006002C600805645", "superFast");
            OverridePoliceCar("38110000", "002003C600804D45", "offroad");

            //gang ids. You can only have police or gang so we don't need different carTypes.
            OverridePoliceCar("40110000", "009406C600203445", "normal");
            OverridePoliceCar("3F110000", "005405C600202E45", "fast");
            OverridePoliceCar("3B120000", "00F001C600B03345", "superFast");
            OverridePoliceCar("3E110000", "00D403C600202945", "offroad");

            //override behavior
            spawnParamEntity = XMLUtil.GrabIDHex(_missionData, "policeBehavior");
            if (!string.IsNullOrWhiteSpace(spawnParamEntity))
            {
                GenerateFullEntityElement("3D549BD4", "B2120000", "0058FAC500F05B45", Convert.FromHexString(spawnParamEntity));
                GenerateFullEntityElement("3D549BD4", "B0120000", "00C0FBC500803445", Convert.FromHexString(spawnParamEntity));
            }
        }
    }
}
