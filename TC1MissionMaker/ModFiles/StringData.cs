using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Text;
using System.Xml.Linq;


namespace TC1MissionMaker.ModFiles;

public class StringData : ModFile
{
    private readonly HashSet<int> _ids = [];


    public StringData(string outputFile) : base("localization", outputFile, null)
    {
    }

    public int GenerateString(string input, bool precision = false)
    {
        if (string.IsNullOrWhiteSpace(input))
            return -1;

        int id = Hash(input, precision);
        if (_ids.Contains(id))
            return id;

        XElement obj = XMLUtil.GenerateObject("name", "_");
        obj.Add(XMLUtil.GenerateField("name", "id", ConversionUtil.IntToHex(id)));
        obj.Add(XMLUtil.GenerateField("name", "_", Convert.ToHexString(ConvertToLocalizationString(input))));

        InsertAddCommand("root", obj);
        _ids.Add(id);
        return id;
    }

    private int Hash(string input, bool precision)
    {
        uint hash = CRC32.Hash(input);

        int signedHash = (int)hash;

        if (signedHash >= -1 && signedHash < 41000)
            signedHash += 41001;

        if (!precision)
            return signedHash;

        //for some reason objective reminders are more strict and require snapping
        //the id to certain points after it exceeds 16 million.
        float f = signedHash;
        byte[] bytes = BitConverter.GetBytes(f);

        return (int)BitConverter.ToSingle(bytes);
    }

    //game stores strings with 00's between each character.
    private byte[] ConvertToLocalizationString(string input)
    {
        byte[] str = Encoding.UTF8.GetBytes(input);
        byte[] locStr = new byte[(str.Length * 2) + 2];
        for (int i = 0; i < str.Length; i++)
        {
            locStr[i * 2] = str[i];
            locStr[(i * 2) + 1] = 0;
        }

        return locStr;
    }
}