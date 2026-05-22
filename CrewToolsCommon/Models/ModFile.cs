using System.Xml.Linq;

namespace CrewToolsCommon.Models
{
    public abstract class ModFile
    {
        private readonly Dictionary<string, HashSet<XElement>> Adds = [];
        private readonly Dictionary<string, HashSet<XElement>> Edits = [];

        private readonly string OutputFile;
     
        private readonly XDocument Doc;

        public ModFile(string mergingFile, string outputFile, XDocument doc = null)
        {
            OutputFile = outputFile;
            Doc = doc;

            //used if we want to implement a base file first with data.
            if (doc == null)
            {
                Doc = new XDocument(new XElement("root"));
                Doc.Root.SetAttributeValue("file", mergingFile);
            }
        }

        public XDocument GetDocument()
        {
            return Doc;
        }

        public string GetOutput()
        {
            return OutputFile;
        }

        public void InsertAddCommand(string depth, XElement element)
        {
            Adds.TryAdd(depth, []);
            Adds[depth].Add(element);
        }

        public void InsertEditCommand(string depth, string fieldName, string fieldValue)
        {
            Edits.TryAdd(depth, []);

            //dont make another edit with the exact same name
            if (Edits[depth].Where(field => field.Attribute("name").Value == fieldName).Any())
                return;

            Edits[depth].Add(XMLUtil.GenerateField("name", fieldName, fieldValue));
        }

        public virtual void Serialize(Stream stream)
        {
            foreach (string depth in Edits.Keys)
            {
                XElement editCommand = new XElement("edit");
                editCommand.SetAttributeValue("depth", depth);
                editCommand.Add(Edits[depth]);
                Doc.Root.Add(editCommand);
            }

            foreach (string depth in Adds.Keys)
            {
                XElement addCommand = new XElement("add");
                addCommand.SetAttributeValue("depth", depth);
                addCommand.Add(Adds[depth]);
                Doc.Root.Add(addCommand);
            }

            Doc.Save(stream);
        }
    }
}
