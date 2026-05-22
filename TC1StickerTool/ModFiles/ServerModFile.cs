using CrewToolsCommon;
using CrewToolsCommon.Models;
using System.Xml.Linq;

namespace TC1StickerTool.ModFiles
{
    internal class ServerModFile : ModFile
    {
        public ServerModFile(string outputFile, XDocument doc = null) : base("server", outputFile, doc)
        {
        }

        public void AddShopItem(string id, Category cat)
        {
            XElement sObj = XMLUtil.GenerateObject("name", "StickersElement");
            sObj.Add(XMLUtil.GenerateField("name", "StickersValue", id));
            InsertAddCommand(cat.ServerDepth, sObj);
        }
    }
}
