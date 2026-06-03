using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using System.IO.Compression;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using TC1MissionMaker.MissionData;
using TC1MissionMaker.Models;
using TC1MissionMaker.ModFiles;

namespace TC1MissionMaker;

internal class Mod
{
    private readonly string[] _missionFiles;

    private readonly List<ModFile> _modFiles = [];

    private Dictionary<string, Stream> _packagedFiles = [];

    private readonly string _missionFolder, _internalModName;

    private readonly Dictionary<string, Dictionary<string, string[]>> _wizards;

    private readonly bool _debug;

    
    public Mod(string[] files, string folder, bool debug)
    {
        _missionFiles = files;
        _missionFolder = folder;
        _internalModName = Path.GetFileName(folder);
        _wizards = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string[]>>>(File.ReadAllText(Path.Combine("Assets", "wizards.json")));
        _debug = debug;
    }

    public void Create()
    {
        //Metadata for PitCrew
        MetadataFile metadata = new MetadataFile(_internalModName, "ToolByFTIW", "Custom missions!", _internalModName + ".mdata");
        _modFiles.Add(metadata);

        //add missions archive inside metadata
        metadata.AddFile(_internalModName + "_data");

        //Stores a lot of overall settings regarding the mission such as but not limited to spec restriction and time of day it takes place.
        string fileName =  GenerateFileName("spawner");
        MissionSpawner spawnerFile = new MissionSpawner(fileName, _missionFolder, _internalModName);
        metadata.AddFile(fileName);
        _modFiles.Add(spawnerFile);
        
        //Tells the TCU server to treat these missions as valid and give proper rewards.
        fileName = GenerateFileName("serverdata");
        ServerData serverData = new ServerData(fileName);
        metadata.AddFile(fileName);
        _modFiles.Add(serverData);
        
        //stores the mission icons seen on the map.
        fileName = GenerateFileName("tags");
        TagData tagData = new TagData(fileName);
        metadata.AddFile(fileName);
        _modFiles.Add(tagData);

        //stores the actual strings along with their id's
        fileName = GenerateFileName("strings");
        StringData stringData = new StringData(fileName);
        metadata.AddFile(fileName);
        _modFiles.Add(stringData);

        //stores the id's and what localization bundle they are tied to (always 13)
        fileName = GenerateFileName("table");
        StringTableData stringTableData = new StringTableData(fileName);
        metadata.AddFile(fileName);
        _modFiles.Add(stringTableData);

        //stores bot spawns, takedown health of cars, and police arrest triggers.
        fileName = GenerateFileName("entities");
        EntitiesData entitiesData = new EntitiesData(fileName);
        metadata.AddFile(fileName);
        _modFiles.Add(entitiesData);

        //TODO unknown
        fileName = GenerateFileName("files");
        ZoneData zoneData = new ZoneData(fileName);
        metadata.AddFile(fileName);
        _modFiles.Add(zoneData);

        int count = 1;
        HashSet<string> existingIds = [];

        foreach (string missionFile in _missionFiles)
        {
            XElement? root = null;
            try
            {
                root = XDocument.Load(missionFile).Root;
            }
            catch (XmlException e)
            {
                Logger.Error($"{missionFile} is invalid: {e.Message}");
                continue;
            }
            
            MissionInfo mission = new MissionInfo();
            bool init = mission.Init(root, _internalModName, existingIds, count, _missionFiles.Length);
            if (!init)
                continue;

            existingIds.Add(mission.InternalName);
            zoneData.AddFile(mission);
            
            mission.GenerateStringData(root, stringData, stringTableData);
            tagData.GenerateTag(root, mission);
            spawnerFile.AddMissionToSpawner(root, mission);

            serverData.AddMission(root, mission);
            Mission missionBin = (Mission)Activator.CreateInstance(mission.Type.MissionFile, root, mission, entitiesData, _wizards);
            missionBin.Parse();

            //Get output of mission and store it
            BinaryObjectFile bof = new BinaryObjectFile();
            bof.Root = missionBin.GetRoot();
            Stream data = new MemoryStream();
            bof.Serialize(data);
            data.Position = 0;

            //Save the individual missions if in debug mode.
            if (_debug)
            {
                FileUtil.CheckAndCreateFolder("DEBUG");
                FileStream stream = File.OpenWrite(Path.Combine("DEBUG", $"Mission_{mission.InternalName}.entities.bin"));
                data.CopyTo(stream);
                stream.Flush();
                stream.Close();
            }

            _packagedFiles.TryAdd(mission.Path, data);

            count++;
        }

        foreach (string key in spawnerFile.GetImages().Keys)
        {
            _packagedFiles.TryAdd(key, spawnerFile.GetImages()[key]);
        }
    }

    public void Package()
    {
        if (_packagedFiles.Count == 0)
            return;

        string path = Path.Combine(Directory.GetParent(_missionFolder).FullName, _internalModName + ".zip");

        //Delete old zip to prevent overwrites
        //File.Delete(path);

        //pack every file up
        Stream fatStream = new MemoryStream();
        Stream datStream = new MemoryStream();
        BigFileUtil.RepackBigFileStream(_packagedFiles, datStream, fatStream, "ToolByFTIW");

        _packagedFiles.Clear();
        _packagedFiles.Add(_internalModName + "_data.fat", fatStream);
        _packagedFiles.Add(_internalModName + "_data.dat", datStream);

        FileUtil.PackageModFiles(path, _modFiles.ToArray(), _packagedFiles);

        Logger.Info($"Mod outputted at {path}", ConsoleColor.Green);
        Console.ResetColor();
    }
    
    private string GenerateFileName(string extension)
    {
        return _internalModName + "_" + extension + ".xml";
    }
}