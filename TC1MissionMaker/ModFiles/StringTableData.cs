using System.Xml.Linq;
using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;


namespace TC1MissionMaker.ModFiles;

public class StringTableData : ModFile
{
    private readonly HashSet<int> _addedIds = [];

    public StringTableData(string outputFile) : base("localization/tat.localization.bin", outputFile, null)
    {
    }
    
    public void AddId(int locId)
    {
        if (_addedIds.Contains(locId))
            return;

        XElement obj = XMLUtil.GenerateObject("name", "_");

        obj.Add(XMLUtil.GenerateField("name", "bid", ConversionUtil.IntToHex(13)));
        obj.Add(XMLUtil.GenerateField("name", "id", ConversionUtil.IntToHex(locId)));

        InsertAddCommand("root", obj);
        _addedIds.Add(locId);
    }
}