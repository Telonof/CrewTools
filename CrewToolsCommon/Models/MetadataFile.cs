using System.Xml.Linq;

namespace CrewToolsCommon.Models
{
    public class MetadataFile : ModFile
    {
        private readonly string Name, Author, Description;

        private readonly List<string> Files = [];

        public MetadataFile(string name, string author, string description, string output) : base("", output, new XDocument(new XElement("metadata")))
        {
            Name = name;
            Author = author;
            Description = description;
        }

        public void AddFile(string file)
        {
            Files.Add(file);
        }

        public override Stream Serialize()
        {
            XDocument metadataFile = new XDocument(new XElement("metadata"));
            MemoryStream stream = new MemoryStream();

            //author
            XElement english = new XElement("author");
            english.SetValue(Author);
            metadataFile.Root.Add(english);

            //name
            XElement group = new XElement("names");
            english = new XElement("en");
            english.SetValue(Name);
            group.Add(english);
            metadataFile.Root.Add(group);

            //description
            group = new XElement("descriptions");
            english = new XElement("en");
            english.SetValue(Description);
            group.Add(english);
            metadataFile.Root.Add(group);

            group = new XElement("files");
            foreach (string file in Files)
            {
                english = new XElement("file");
                english.SetAttributeValue("priority", "998");
                english.SetAttributeValue("loc", Path.GetFileName(file));
                group.Add(english);
            }

            metadataFile.Root.Add(group);

            metadataFile.Save(stream);
            return stream;
        }
    }
}
