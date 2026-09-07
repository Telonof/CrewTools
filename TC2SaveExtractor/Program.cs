using System.Buffers.Binary;
using Gibbed.Dunia2.BinaryObjectInfo;
using Gibbed.Dunia2.FileFormats;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using CrewToolsCommon.Utilities;
using Gibbed.ProjectData;

namespace TC2SaveExtractor;

internal class Program
{
    public static void Main(string[] args)
    {
        Logger.Clean();
        Logger.Banner("==========================================");
        Logger.BannerHighlight("The Crew 2 Save Extractor/Repacker by", "FTIW");
        Logger.BannerHighlight("Using modified Dunia 2 tools from", "Gibbed");
        Logger.BannerHighlight("Field and Object hashes provided by", "Guki");
        Logger.Banner("==========================================");

        //test for .net
        if (args.Length > 1)
        {
            Logger.Info("Program is working correctly.");
            return;
        }

        if (args.Length == 0 && OperatingSystem.IsWindows())
        {
            Logger.Info($"Usage: {Path.GetFileName(Environment.ProcessPath)} <input xml/bin>");
            Logger.Error("Close this window and drag and drop your bin/xml onto this program instead.", true);
            Wait();
            return;
        }

        if (args.Length == 0 && OperatingSystem.IsLinux())
        {
            Logger.Error("Run this program in the terminal and enter your bin/xml as an argument.", true);
            return;
        }

        if (!File.Exists(args[0]))
        {
            Logger.Error("File not found.", true);
            return;
        }

        //Load data needed for binary object names
        Manager manager = Manager.Load();
        HashFinder.Load(manager);
        string projectPath = Manager.Load().ActiveProject.ListsPath;
        var infoManager = InfoManager.Load(projectPath);

        //compressed -> xml
        if (Path.GetExtension(args[0]) == ".bin")
        {
            Logger.Info("Saving to save.xml...");
            ExtractSave(args[0], infoManager);
            Logger.WriteAndFlush();
            Wait();
            return;
        }

        //xml -> compressed
        if (Path.GetExtension(args[0]) == ".xml")
        {
            Logger.Info("Saving to main-save.bin...");
            ImportSave(args[0], infoManager);
            Logger.WriteAndFlush();
            Wait();
            return;
        }

        Logger.Error("Not a bin or xml.", true);
        Wait();
    }

    private static void ExtractSave(string path, InfoManager infoManager)
    {
        BinaryObjectFile bof = new BinaryObjectFile();

        //Skip first 8 bytes since they are not part of the nbCF.
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        fs.Seek(8, SeekOrigin.Begin);
        bof.Deserialize(fs);

        var objectFileDef = infoManager.GetObjectFileDefinition(Path.GetFileNameWithoutExtension(path));

        //Save to XML
        Exporting.Export(objectFileDef, "save.xml", infoManager, bof);
        fs.Dispose();

        Logger.Info("Finished!");
    }

    private static void ImportSave(string path, InfoManager manager)
    {
        //Load custom uuid and name
        Dictionary<string, string>? data = LoadJSON();

        if (data == null)
            return;

        string uuid = data["uuid"];
        if (!Guid.TryParse(uuid, out _))
        {
            Logger.Error("Bad UUID, did you set it inside data.json?");
            return;
        }

        //Convert uuid and name inside save to that of player.
        XDocument doc = XDocument.Load(path);
        var uuidHash = doc.Descendants("field").FirstOrDefault(f => (string)f.Attribute("name") == "SaveOwnerId");
        uuidHash.Value = ConvertToHexString(uuid);

        //Convert xml to binary
        var import = new Importing(manager);
        BinaryObjectFile bof = new BinaryObjectFile();
        bof.Root = import.Import(null, Path.GetFileNameWithoutExtension(path),
            doc.CreateNavigator().SelectSingleNode("/object"));

        //Write bof into stream
        MemoryStream dataStream = new MemoryStream();
        FileStream finalStream = File.Create("main-save.bin");
        bof.Serialize(dataStream);

        //Get bytes and size
        byte[] dataBytes = dataStream.ToArray();
        int size = dataBytes.Length;
        //We dispose here because we need them in bytes to crc hash the file.
        dataStream.Dispose();

        //Header info
        //Grab the crc32 of the bytes as well as size and convert to big endian.
        uint crc = CRC32.Hash(dataBytes, 0, size);
        byte[] crcArray = BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(crc));
        byte[] sizeArray = BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(size));

        //Write everything together. Format: [4 byte big endian int32 size][4 byte big endian int32 crc32 of data segment][data segment]
        finalStream.Write(sizeArray);
        finalStream.Write(crcArray);
        finalStream.Write(dataBytes);
        finalStream.Close();

        Logger.Info("Finished!");
    }

    private static Dictionary<string, string>? LoadJSON()
    {
        string path = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "data.json");

        if (!File.Exists(path))
        {
            Logger.Error("data.json not found, did you delete it?");
            return null;
        }

        string json = File.ReadAllText(path);

        //If invalid json, simply don't use it.
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch (JsonException e)
        {
            Logger.Error(e.Message);
            Wait();
            return null;
        }
    }

    private static string ConvertToHexString(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        Array.Resize(ref bytes, bytes.Length + 1);
        return BitConverter.ToString(bytes).Replace("-", "");
    }

    private static void Wait()
    {
        Logger.Info("Press any key to exit.");
        Console.ReadKey();
    }
}