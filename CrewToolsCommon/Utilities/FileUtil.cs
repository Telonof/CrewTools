using CrewToolsCommon.Models;
using System.IO.Compression;

namespace CrewToolsCommon
{
    public class FileUtil
    {
        public static void CheckAndCreateFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void CheckAndDeleteFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static string GrabExecName()
        {
            return Path.GetFileName(Environment.ProcessPath);
        }

        public static void PackageModFiles(string zipPath, ModFile[] modFiles, Dictionary<string, Stream> extraFiles)
        {
            ZipArchive archive = new ZipArchive(File.Open(zipPath, FileMode.Create), ZipArchiveMode.Create);
            foreach (ModFile file in modFiles)
            {
                Stream data = new MemoryStream();
                file.Serialize(data);
                data.Position = 0;

                ZipArchiveEntry item = archive.CreateEntry(file.GetOutput());
                using (Stream entryStream = item.Open())
                    data.CopyTo(entryStream);

                data.Dispose();
            }

            foreach (string key in extraFiles.Keys)
            {
                extraFiles[key].Position = 0;
                ZipArchiveEntry item = archive.CreateEntry(key);
                using (Stream entryStream = item.Open())
                    extraFiles[key].CopyTo(entryStream);

                extraFiles[key].Dispose();
            }

            archive.Dispose();
        }
    }
}
