using CrewToolsCommon;
using Gibbed.Dunia2.FileFormats;
using System.Xml.Linq;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker.MissionData
{
    /**Isolates settings regarding missions that can have police. **/
    internal class PoliceMission : Mission
    {
        public PoliceMission(XElement missionData, MissionInfo mission, EntitiesData entitesData, Dictionary<string, Dictionary<string, string[]>> wizards) : base(missionData, mission, entitesData, wizards)
        {
        }

        public override void GenerateText()
        {
            throw new NotImplementedException();
        }

        public override bool ParseInstantStart(bool instant)
        {
            throw new NotImplementedException();
        }

        public override void ParseTTOG(string entity)
        {
            throw new NotImplementedException();
        }

        public override void MissionSpecific()
        {
            ParseArrestConditions();
        }

        protected void OverridePoliceCar(string id, string atomPosition, string carType)
        {
            XElement? cars = _missionData.Element("policeCars");

            string car = XMLUtil.GrabIDHex(cars, carType);
            if (string.IsNullOrWhiteSpace(car))
                return;

            ulong entityId = _entitiesData.GetCarIfFound(car);
            if (entityId == 0)
            {
                entityId = _mission.GenerateId();
                _entitiesData.AddBot(entityId, car, "4B43840700000000");
            }

            GenerateFullEntityElement("3D549BD4", id, atomPosition, BitConverter.GetBytes(entityId));
        }

        protected ulong GenerateTrigger(XElement triggerInfo)
        {
            BinaryObject trigger = AddChildDirectly(_rootNode, "Entity");
            ulong id = _mission.GenerateId();
            AddField(trigger, "ID", BitConverter.GetBytes(id));
            AddField(trigger, "FatherArchetypeID", BitConverter.GetBytes((ulong)106429084));
            AddField(trigger, "SpawnPolicy", new byte[4]);
            AddField(trigger, "Angles", new byte[16]);
            GeneratePositionData(triggerInfo, "position", trigger, "WorldPosition");

            //Thanks IVT
            trigger = AddChildDirectly(trigger, "Components");
            trigger = AddChildDirectly(trigger, "CITEntityComponent_VolumeTrigger");
            trigger = AddChildDirectly(trigger, "Shapes");
            trigger = AddChildDirectly(trigger, "ShapesElement");
            trigger = AddChildDirectly(trigger, "ShapesValue");

            AddField(trigger, "hid_DTCTH_ClassName", Convert.FromHexString("7CE8EBF3"));
            AddField(trigger, "Length", BitConverter.GetBytes(XMLUtil.GrabFloatOrDefault(triggerInfo, "length", 10, true)));
            AddField(trigger, "Width", BitConverter.GetBytes(XMLUtil.GrabFloatOrDefault(triggerInfo, "width", 10, true)));
            AddField(trigger, "Depth", BitConverter.GetBytes(XMLUtil.GrabFloatOrDefault(triggerInfo, "depth", 10, true)));
            AddField(trigger, "Rotation", new byte[16]);

            return id;
        }

        protected void ParseArrestConditions()
        {
            XElement conditions = _missionData.Element("arrestConditions");
            if (conditions == null)
                return;

            //what is the max speed the player can be where the game will consider them "being arrested" (the countdown).
            float maxSpeedForArrest = XMLUtil.GrabFloatOrDefault(conditions, "maxSpeedForArrest", 30, true);

            //How long does the player have in the arrest countdown
            float arrestTimer = XMLUtil.GrabFloatOrDefault(conditions, "arrestTimer", 2.5f, true);

            //How close can the player be to be considered "getting arrested"
            float maxDistanceForArrest = XMLUtil.GrabFloatOrDefault(conditions, "maxDistanceForArrest", 12.5f, true);

            //How many police need to be in the area to be considered "getting arrested"?
            int minPoliceArrest = XMLUtil.GrabIntOrDefault(conditions, "minPoliceArrest", 1, true);

            //How close can the player be from a roadblock to be considered "getting arrested"
            float arrestDistanceFromRoadBlock = XMLUtil.GrabFloatOrDefault(conditions, "arrestDistanceFromRoadBlock", 10, true);

            ulong id = _mission.GenerateId();
            _entitiesData.AddArrestConditions(id, maxSpeedForArrest, arrestTimer, maxDistanceForArrest, minPoliceArrest, arrestDistanceFromRoadBlock);

            //link arrest params to settings
            GenerateEntityElement("3D549BD4", "2C7D35A3", BitConverter.GetBytes(id));
        }

        /// <summary>
        /// <param name="data">
        /// 00 = class name for endLink class<br></br>
        /// 01 = id for endLink class<br></br>
        /// 02 = class name for startLink class<br></br>
        /// 03 = id for startLink class<br></br>
        /// 04 = id for start trigger<br></br>
        /// 05 = atom position for start trigger<br></br>
        /// 06 = id for end trigger<br></br>
        /// 07 = atom position for end trigger
        /// </param>
        /// </summary>
        protected void ParsePoliceTriggers(string[] data)
        {
            //trigger to summon the cops
            XElement trigger = _missionData.Element("policeStartTrigger");
            if (trigger == null)
                return;

            ulong id = GenerateTrigger(trigger);
            byte[][] ids = [BitConverter.GetBytes(id)];
            GenerateFullEntityListElement("D1FB81EA", data[4], data[5], ids);

            //set police to true
            GenerateBoolElement("6F5CD0E3", true);

            //add link data
            BinaryObject setting = AddChildDirectly(_atomEntNode, "ListMissionAtomEntElement");
            setting = AddChildDirectly(setting, "ListMissionAtomEntValue");
            AddField(setting, "hid_DTCTH_ClassName", Convert.FromHexString(data[0]));
            AddField(setting, "ID", Convert.FromHexString(data[1]));
            setting = AddChildDirectly(setting, "EndLinkID");
            setting = AddChildDirectly(setting, "EndLinkIDElement");
            AddField(setting, "EndLinkIDValue", BitConverter.GetBytes(2147483657));

            setting = AddChildDirectly(_atomEntNode, "ListMissionAtomEntElement");
            setting = AddChildDirectly(setting, "ListMissionAtomEntValue");
            AddField(setting, "hid_DTCTH_ClassName", Convert.FromHexString(data[2]));
            AddField(setting, "ID", Convert.FromHexString(data[3]));
            setting = AddChildDirectly(setting, "StartLinkID");
            setting = AddChildDirectly(setting, "StartLinkIDElement");
            AddField(setting, "StartLinkIDValue", BitConverter.GetBytes(2147483657));

            GenerateLinkElement(2147483657, BitConverter.ToUInt32(Convert.FromHexString(data[3])));

            //trigger to call them off
            trigger = _missionData.Element("policeStopTrigger");
            if (trigger == null)
                return;

            id = GenerateTrigger(trigger);
            ids = [BitConverter.GetBytes(id)];
            GenerateFullEntityListElement("D1FB81EA", data[6], data[7], ids);
        }
    }
}
