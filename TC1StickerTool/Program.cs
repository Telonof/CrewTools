using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;
using Gibbed.Dunia2.FileFormats;
using TC1StickerTool.ModFiles;

namespace TC1StickerTool
{
    internal class Program
    {
        private static byte[] hiresXBT = [0x54, 0x42, 0x58, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0xED, 0x69, 0x6D, 0xC9, 0xB1, 0x21, 0x12, 0x88, 0x35, 0xD7, 0x87, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00];
        private static byte[] iconXBT = [0x54, 0x42, 0x58, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x8C, 0x49, 0xEC, 0xED, 0x29, 0xD1, 0xFE, 0x62, 0xD3, 0xB4, 0x1C, 0x75, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00];
        private static byte[] lowresXBT = [0x54, 0x42, 0x58, 0x00, 0x0B, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        public static void Main(string[] args)
        {
            Logger.Clean();

            if (!Directory.Exists("root"))
            {
                Logger.Error("No root folder found in program folder.");
                Logger.WriteAndFlush();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            foreach (string folder in Directory.GetDirectories("root"))
            {
                FolderToMod(folder);
            }

            Logger.WriteAndFlush();
        }

        private static void FolderToMod(string path)
        {
            Dictionary<string, Stream> packagedFiles = [];
            ModFile[] modFiles = new ModFile[4];
            string name = Path.GetFileName(path);

            MetadataFile mdata = new MetadataFile(name, "ToolByFTIW", "Custom stickers!", $"{name}.mdata");
            modFiles[0] = mdata;
            //the dat/fat archive that stores the images.
            mdata.AddFile($"{name}_data");

            EntitiesModFile entitiesModFile = new EntitiesModFile("entity/generated/archetypes.entities.bin", $"{name}_entities.xml");
            modFiles[1] = entitiesModFile;
            mdata.AddFile(entitiesModFile.GetOutput());

            BabDBFile babdb = new BabDBFile("road66database/sticker.babdb", $"{name}_babdb.xml");
            modFiles[2] = babdb;
            mdata.AddFile(babdb.GetOutput());

            ServerModFile sModFile = new ServerModFile($"{name}_server.xml");
            modFiles[3] = sModFile;
            mdata.AddFile(sModFile.GetOutput());

            foreach (string folder in Directory.GetDirectories(path))
            {
                CategoryToFiles(folder, Category.FromString(Path.GetFileName(folder).ToLower()), entitiesModFile, babdb, sModFile, packagedFiles);
            }

            //Generate the archive used to to store the actual images
            Stream fatStream = new MemoryStream();
            Stream datStream = new MemoryStream();
            BigFileUtil.RepackBigFileStream(packagedFiles, datStream, fatStream, "ToolByFTIW");

            packagedFiles.Clear();
            packagedFiles.Add($"{name}_data.fat", fatStream);
            packagedFiles.Add( $"{name}_data.dat", datStream);

            FileUtil.PackageModFiles($"{name}.zip", modFiles, packagedFiles);

            Logger.Info($"Mod outputted at {name}.zip", ConsoleColor.Green);
            Console.ResetColor();
        }

        private static void CategoryToFiles(string path, Category cat, EntitiesModFile entModFile, BabDBFile bdb, ServerModFile serverModFile, Dictionary<string, Stream> packagedFiles)
        {
            Dictionary<string, Dictionary<ImageType, string>> files = [];
            Dictionary<string, ImageType> map = new Dictionary<string, ImageType> {
                {"hires", ImageType.HIRES },
                {"lowres", ImageType.LOWRES },
                {"icon", ImageType.ICON },
            };

            //we need to grab the low, high res, and icon file.
            foreach (string file in Directory.GetFiles(path, "*.dds"))
            {
                string fullName = Path.GetFileNameWithoutExtension(file);
                int nameIndex = fullName.LastIndexOf('_');
                string name = fullName.Substring(0, nameIndex);
                string type = fullName.Substring(nameIndex + 1);
                files.TryAdd(name, []);
                if (!map.TryGetValue(type, out ImageType val))
                {
                    Logger.Error($"Unknown image type {type}");
                    continue;
                }

                string xbtPath = Path.ChangeExtension(file.Substring(5), ".xbt");
                files[name].Add(val, xbtPath);

                //we also need to convert them to XBT
                Stream stream = new MemoryStream();

                byte[] imageData = File.ReadAllBytes(file);
                if (val == ImageType.HIRES)
                    stream.Write(hiresXBT, 0, hiresXBT.Length);
                if (val == ImageType.LOWRES)
                    stream.Write(lowresXBT, 0, lowresXBT.Length);
                if (val == ImageType.ICON)
                    stream.Write(iconXBT, 0, iconXBT.Length);

                stream.Write(imageData, 0, imageData.Length);
                packagedFiles.TryAdd(xbtPath, stream);
            }

            foreach (string key in files.Keys)
            {
                if (files[key].Count != 3)
                {
                    Logger.Error($"Invalid amount of files for {key}, skipping.");

                    foreach (string file in files[key].Values)
                    {
                        packagedFiles.Remove(file);
                    }

                    continue;
                }

                //generate unique id for sticker for reference inside babel db and server shop.
                var str = files[key][ImageType.HIRES].ToLower().Replace("/", "\\");
                str = str.Remove(str.Length - 7);

                string hash = ConversionUtil.ULongToHex(CRC64.Hash(str, true));
                hash = hash.Substring(0, hash.Length - 2) + "00";

                //Generate hashes for each file for the sticker to know where to point in the files in-game.
                ulong hiresNum = CRC64.Hash(files[key][ImageType.HIRES].ToLower().Replace("/", "\\"), true);
                string hiresHash = ConversionUtil.ULongToHex(hiresNum);
                ulong lowresNum = CRC64.Hash(files[key][ImageType.LOWRES].ToLower().Replace("/", "\\"), true);
                string lowresHash = ConversionUtil.ULongToHex(lowresNum);
                ulong iconNum = CRC64.Hash(files[key][ImageType.ICON].ToLower().Replace("/", "\\"), true);
                string iconHash = ConversionUtil.ULongToHex(iconNum);

                entModFile.AddSticker(hash, lowresHash, hiresHash, iconHash, cat);
                bdb.AddDBItem(hash, cat);
                serverModFile.AddShopItem(hash, cat);
            }
        }

        public enum ImageType
        {
            HIRES,
            LOWRES,
            ICON
        }
    }
}