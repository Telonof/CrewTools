using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;
using System.Buffers.Binary;
using System.Xml.Linq;
using TC1MissionMaker.Models;


namespace TC1MissionMaker.ModFiles;

internal class ServerData : ModFile
{
    public ServerData(string outputFile) : base("server_missions", outputFile, null)
    {
    }

    public void AddMission(XElement root, MissionInfo mission)
    {
        if (mission.Hidden)
            return;

        XElement serverMission = new XElement(mission.Class.RewardType);
        serverMission.SetAttributeValue("ID", $"0x{ConversionUtil.ULongToHex(BinaryPrimitives.ReverseEndianness(mission.Id))}");
        serverMission.SetAttributeValue("Name", mission.InternalName);

        if (mission.Class.RewardType.Equals("faction"))
            serverMission.SetAttributeValue("GameplayZone", mission.Zone.Index);

        serverMission.SetAttributeValue("DelockLevel", XMLUtil.GrabIntOrDefault(root, "minimumLevel", 1));
        serverMission.SetAttributeValue("MaxLevel", XMLUtil.GrabIntOrDefault(root, "missionLevel", 20));
        serverMission.SetAttributeValue("RewardValuesID", $"0x{ConversionUtil.ULongToHex(BinaryPrimitives.ReverseEndianness(mission.RewardId))}");

        if (mission.Points.Length > 0)
        {
            serverMission.SetAttributeValue("ObjectiveBronze", mission.Points[0].ToString());
            serverMission.SetAttributeValue("ObjectiveSilver", mission.Points[1].ToString());
            serverMission.SetAttributeValue("ObjectiveGold", mission.Points[2].ToString());
            serverMission.SetAttributeValue("ObjectivePlatinum", mission.Points[3].ToString());
        }

        GetDocument().Root.Add(serverMission);
    }
}