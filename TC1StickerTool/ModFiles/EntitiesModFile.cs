using CrewToolsCommon;
using CrewToolsCommon.Models;
using System.Xml.Linq;

namespace TC1StickerTool.ModFiles
{
    internal class EntitiesModFile : ModFile
    {
        public EntitiesModFile(string mergingFile, string outputFile, XDocument doc = null) : base(mergingFile, outputFile, doc)
        {
        }

        public void AddSticker(string id, string lowres, string hires, string icon, Category cat)
        {
            XElement stickerObj = XMLUtil.GenerateObject("name", "Entity");
            stickerObj.Add(XMLUtil.GenerateField("name", "ID", id));
            stickerObj.Add(XMLUtil.GenerateField("name", "FatherArchetypeID", cat.FatherArchetypeID));

            XElement stickerPart = XMLUtil.GenerateObject("name", "StickerPart");
            stickerPart.Add(XMLUtil.GenerateField("name", "hid_DTCTH_ClassName", "FE0BE717"));
            stickerPart.Add(XMLUtil.GenerateField("name", "StickerFileName", lowres));
            stickerPart.Add(XMLUtil.GenerateField("name", "StickerHRFileName", hires));
            stickerPart.Add(XMLUtil.GenerateField("name", "IconPath", icon));
            stickerObj.Add(stickerPart);

            InsertAddCommand("root", stickerObj);
        }
    }
}
