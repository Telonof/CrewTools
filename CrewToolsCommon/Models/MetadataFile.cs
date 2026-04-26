using System.Xml.Linq;

namespace CrewToolsCommon.Models
{
    public class MetadataFile
    {
        private readonly string Name, Author, Description, Output;

        private readonly List<string> Files = [];

        public MetadataFile(string name, string author, string description, string output)
        {
            Name = name;
            Author = author;
            Description = description;
            Output = output;
        }

        public void AddFile(string file)
        {
            Files.Add(file);
        }

        public void Serialize()
        {
            XDocument metadataFile = new XDocument(new XElement("metadata"));

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
            File.WriteAllText(Output, metadataFile.ToString());
        }
    }
}
