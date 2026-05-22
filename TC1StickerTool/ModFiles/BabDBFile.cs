using CrewToolsCommon.Models;
using System.Xml.Linq;

namespace TC1StickerTool.ModFiles
{
    internal class BabDBFile : ModFile
    {

        private readonly SortedDictionary<string, XElement> Entries = [];

        public BabDBFile(string mergingFile, string outputFile, XDocument doc = null) : base(mergingFile, outputFile, doc)
        {
        }

        public void AddDBItem(string id, Category category)
        {
            XElement add = new XElement("add");

            add.Add(GenerateColumn("id", id));
            add.Add(GenerateColumn("type", category.StickerType));
            add.Add(GenerateColumn("modelid", "FFFFFFFFFFFFFFFF"));
            XElement pindex = GenerateColumn("packindex", "28");
            pindex.SetAttributeValue("type", "UInt32");
            add.Add(pindex);
            add.Add(GenerateColumn("typeid", category.Index + "00000000"));

            Entries.Add(id, add);
        }

        public override void Serialize(Stream stream)
        {
            foreach (string entry in Entries.Keys)
            {
                GetDocument().Root.Add(Entries[entry]);
            }

            base.Serialize(stream);
        }

        private XElement GenerateColumn(string name, string value)
        {
            XElement column = new XElement(name);
            column.SetValue(value);
            return column;
        }
    }
}
