using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;
using System.Xml.Linq;
using TC1MissionMaker.Models;


namespace TC1MissionMaker.ModFiles;

internal class TagData : ModFile
{
    public TagData(string outputFile) : base("project/road66/generated/tagdb.bin", outputFile, null)
    {
    }

    public void GenerateTag(XElement root, MissionInfo mission)
    {
        XElement tagObj = XMLUtil.GenerateObject("name", "TagSpawnersElement");
        XElement tagElement = XMLUtil.GenerateObject("name", "TagSpawnersValue");

        tagElement.Add(XMLUtil.GenerateField("hash", "B8B29C27", "434954546167537061776E657200"));
        tagElement.Add(XMLUtil.GenerateField("name", "hid_DTCTH_ClassName", "BCD33AE4"));
        tagElement.Add(XMLUtil.GenerateField("name", "State", "00"));
        tagElement.Add(XMLUtil.GenerateField("name", "SourceEntityID", ConversionUtil.ULongToHex(mission.Id)));
        tagElement.Add(XMLUtil.GenerateField("name", "SourceEntityArchetypeID", mission.Type.FatherSpawnerId));
        tagElement.Add(XMLUtil.GenerateField("name", "GameplayZoneEntityID", mission.Zone.Id));

        //These don't seem to be important
        tagElement.Add(XMLUtil.GenerateField("name", "DataTowerZoneEntityID", "FFFFFFFFFFFFFFFF"));
        tagElement.Add(XMLUtil.GenerateField("name", "LifeZoneArchetypeID", "FFFFFFFFFFFFFFFF"));
        tagElement.Add(XMLUtil.GenerateField("name", "LifeZoneGroupArchetypeID", "FFFFFFFFFFFFFFFF"));
        tagElement.Add(XMLUtil.GenerateField("name", "LifeZoneNameTextID", "FFFFFFFF"));

        tagElement.Add(XMLUtil.GenerateField("name", "WorldPosition", ParsePosition(root)));

        tagElement.Add(XMLUtil.GenerateField("name", "Angles", "00000000000000800000000000000000"));
        tagElement.Add(XMLUtil.GenerateField("name", "Name", ConversionUtil.StringToHex(mission.InternalName)));
        tagElement.Add(XMLUtil.GenerateField("name", "NameTextID", ConversionUtil.IntToHex(mission.Strings[StringType.Name])));
        tagElement.Add(XMLUtil.GenerateField("name", "DescriptionTextID", ConversionUtil.IntToHex(mission.Strings[StringType.Description])));
        tagElement.Add(XMLUtil.GenerateField("name", "LifeZoneGroupNameTextID", ConversionUtil.IntToHex(mission.Strings[StringType.HudLocation])));

        tagElement.Add(XMLUtil.GenerateField("name", "Type", ConversionUtil.IntToHex(mission.Class.TagType)));
        tagElement.Add(XMLUtil.GenerateField("name", "SubType", ConversionUtil.IntToHex(mission.Type.SubType)));
        tagElement.Add(XMLUtil.GenerateField("hash", "49BC0501", "00"));
        tagElement.Add(XMLUtil.GenerateField("name", "WorldLayer", "FFFFFFFF"));

        //Source location
        XElement obj = XMLUtil.GenerateObject("name", "SourceEntityLocation");
        obj.Add(XMLUtil.GenerateField("hash", "B8B29C27", "434954456E746974794C6F636174696F6E5A6F6E6500"));
        obj.Add(XMLUtil.GenerateField("name", "hid_DTCTH_ClassName", "45241666"));

        XElement zone = XMLUtil.GenerateObject("name", "ZoneID");
        zone.Add(XMLUtil.GenerateField("hash", "2ABD43F2", ConversionUtil.StringToHex($"Mission_{mission.InternalName}")));
        obj.Add(zone);

        tagElement.Add(obj);

        tagObj.Add(tagElement);
        InsertAddCommand("0", tagObj);
    }

    private string ParsePosition(XElement root)
    {
        float[] coords = XMLUtil.GrabCoords(root, "missionLocation", false);

        if (coords.Length == 0)
            coords = XMLUtil.GrabCoords(root, "spawnPosition");

        if (coords.Length == 0)
            throw new FormatException("No missionLocation or spawnPosition found.");  

        return ConversionUtil.FloatsToHex(coords);
    }
}