using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;
using System.Xml.Linq;
using TC1MissionMaker.Models;

namespace TC1MissionMaker.ModFiles
{
    internal class ZoneData : ModFile
    {
        public ZoneData(string outputFile) : base("project/road66/generated/zones/zones.filelist.bin", outputFile, null)
        {
        }

        public void AddFile(MissionInfo mission)
        {
            XElement listItem = XMLUtil.GenerateObject("name", "ListElement");
            listItem.Add(XMLUtil.GenerateField("name", "ListValue", ConversionUtil.StringToHex($"Mission_{mission.InternalName}")));
            InsertAddCommand("0", listItem);
        }
    }
}
