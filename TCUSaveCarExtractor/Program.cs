using CrewToolsCommon;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using TCUSaveCarExtractor.Models;
using TCUSaveCarExtractor.ModFiles;

namespace TCUSaveCarExtractor;

internal class Program
{
    private static XElement RenderDatabase;
    private static XElement PhysDatabase;


    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Logger.Error($"Usage: {FileUtil.GrabExecName()} <.tcusave file>", true);
            return;
        }

        if (!File.Exists(args[0]))
        {
            Logger.Error($"Error: File {args[0]} not found.", true);
            return;
        }

        string output = Path.Combine(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]));
        AICarFile carFile = new AICarFile(output + "_entities.xml");
        RenderDatabase = XDocument.Load(Path.Combine("Assets", "renderdatabase.xml")).Root.Element("object");
        PhysDatabase = XDocument.Load(Path.Combine("Assets", "physdatabase.xml")).Root.Element("object");

        HashSet<Car> cars = ExtractSaveToPuzzle(args[0]);
        carFile.AddAllCars(cars);

        //serilalize output
        FileStream stream = new FileStream(carFile.GetOutput(), FileMode.Create, FileAccess.Write);
        carFile.Serialize(stream);
        stream.Dispose();

        GenerateMapFile(cars, output);
    }

    private static void GenerateMapFile(HashSet<Car> cars, string output)
    {
        //new id, template id
        Dictionary<string, string> map = [];

        foreach (Car car in cars)
        {
            map[(CRC64.Hash($"{output + "_entities.xml"}{car.TemplateID}") & 0xFFFFFFFFFFFFFF00).ToString("X16")] = car.TemplateID;
        }

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.WriteIndented = true;

        File.WriteAllText(output + "_map.json", JsonSerializer.Serialize(map, options));
    }

    private static HashSet<Car> ExtractSaveToPuzzle(string path)
    {
        HashSet<Car> cars = new HashSet<Car>();
        string jsonData = File.ReadAllText(path);
        var saveFile = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonData);

        if (!saveFile.TryGetValue("Save", out JsonElement saveData))
            return null;

        byte[] data = Convert.FromBase64String(saveData.GetString());

        int index = SearchBytes(data, Encoding.UTF8.GetBytes("20150101-120000"));
        index += 137;

        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(index, SeekOrigin.Begin);

        int carCount = ReadInt32(reader);
        for (int i = 0; i < carCount; i++)
        {
            string carID = ReadID(reader);
            stream.Seek(16, SeekOrigin.Current);
            int dressCount = ReadInt32(reader);
            for (int j = 0; j < dressCount; j++)
            {
                Car car = new Car
                {
                    ModelID = carID,
                    DressID = ReadID(reader),
                    TemplateID = ReadID(reader)
                };

                stream.Seek(19, SeekOrigin.Current);
                for (int phys = 0; phys < 11; phys++)
                {
                    car.PhysIds[phys] = ConvertIndexToPhysID(reader);
                }

                //stream.Seek(41, SeekOrigin.Current);
                car.FrontBumperID = ConvertIndexToIDReversed(reader);
                car.RearBumperID = ConvertIndexToIDReversed(reader);
                car.SkirtsID = ConvertIndexToIDReversed(reader);
                car.SideMirrorID = ConvertIndexToIDReversed(reader);
                car.RearWingID = ConvertIndexToIDReversed(reader);
                car.HoodID = ConvertIndexToIDReversed(reader);
                car.FrontFenderID = ConvertIndexToIDReversed(reader);
                car.RearFenderID = ConvertIndexToIDReversed(reader);
                car.RimsID = ConvertIndexToIDReversed(reader);
                car.LicensePlateID = ConvertIndexToIDReversed(reader);
                car.ColorID = ConvertIndexToIDReversed(reader);
                car.Color2ID = ConvertIndexToIDReversed(reader);
                car.StickerID = ConvertIndexToIDReversed(reader);
                car.InteriorID = ConvertIndexToIDReversed(reader);

                //bike
                car.AvatarHelmet = ConvertIndexToIDReversed(reader);
                car.AvatarTopID = ConvertIndexToIDReversed(reader);
                car.AvatarBottomID = ConvertIndexToIDReversed(reader);
                car.SwingArmID = ConvertIndexToIDReversed(reader);
                car.FrontLightID = ConvertIndexToIDReversed(reader);
                car.ExhaustID = ConvertIndexToIDReversed(reader);
                car.SideMirrorBikeID = ConvertIndexToIDReversed(reader);
                car.ForkID = ConvertIndexToIDReversed(reader);
                car.RearLightID = ConvertIndexToIDReversed(reader);
                car.FrontFenderBikeID = ConvertIndexToIDReversed(reader);
                car.BikeSeatID = ConvertIndexToIDReversed(reader);
                car.FairingID = ConvertIndexToIDReversed(reader);
                car.RimStyleBikeID = ConvertIndexToIDReversed(reader);

                stream.Seek(1313, SeekOrigin.Current);

                cars.Add(car);
            }
        }

        reader.Close();
        stream.Close();
        return cars;
    }

    //thank you stack overflow.
    private static int SearchBytes(byte[] haystack, byte[] needle)
    {
        int len = needle.Length;
        int limit = haystack.Length - len;

        for (int i = 0; i <= limit; i++)
        {
            int k = 0;
            for (; k < len; k++)
            {
                if (needle[k] != haystack[i + k])
                    break;
            }

            if (k == len)
                return i;
        }

        return -1;
    }

    private static int ReadInt32(BinaryReader reader)
    {
        byte[] data = reader.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    private static string ReadID(BinaryReader reader)
    {
        byte[] data = reader.ReadBytes(8);
        Array.Reverse(data);
        return Convert.ToHexString(data);
    }

    private static string ConvertIndexToIDReversed(BinaryReader reader)
    {
        byte[] data = reader.ReadBytes(2);
        Array.Reverse(data);
        return ConvertIndexToID(BitConverter.ToInt16(data));
    }

    private static string ConvertIndexToID(int id)
    {
        if (id == 65535 || id == -1)
            return "FFFFFFFFFFFFFFFF";

        return XMLUtil.GrabField(RenderDatabase.Elements().ElementAt(id), "name", "SourceEntityID").Value;
    }

    private static string ConvertIndexToPhysID(BinaryReader reader)
    {
        byte[] data = reader.ReadBytes(2);
        Array.Reverse(data);
        ushort id = BitConverter.ToUInt16(data);

        if (id == 65535 || id == -1)
            return "FFFFFFFFFFFFFFFF";

        return XMLUtil.GrabField(PhysDatabase.Elements().ElementAt(id), "name", "SourceEntityID").Value;
    }
}