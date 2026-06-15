using CrewToolsCommon;
using CrewToolsCommon.Models;
using System.Xml.Linq;
using TCUSaveCarExtractor.Models;

namespace TCUSaveCarExtractor.ModFiles;

internal class AICarFile : ModFile
{
    public AICarFile(string outputFile) : base("entity/generated/archetypes.entities.bin", outputFile, null)
    {
    }

    public void AddAllCars(HashSet<Car> cars)
    {
        foreach (Car car in cars)
        {
            AddCustomCar(car);
        }
    }

    private void AddCustomCar(Car car)
    {
        string newCarID = car.TemplateID.Substring(0, car.TemplateID.Length - 9) + "800000000";

        XElement bodySection = XMLUtil.GenerateObject("name", "BodySection");
        bodySection.Add(GeneratePuzzlePiece("FrontBumperSlot", car.FrontBumperID));
        bodySection.Add(GeneratePuzzlePiece("RearBumperSlot", car.RearBumperID));
        bodySection.Add(GeneratePuzzlePiece("SideMirrorSlot", car.SideMirrorID));
        bodySection.Add(GeneratePuzzlePiece("SkirtsSlot", car.SkirtsID));
        bodySection.Add(GeneratePuzzlePiece("RearWingSlot", car.RearWingID));
        bodySection.Add(GeneratePuzzlePiece("MotorHoodSlot", car.HoodID));
        bodySection.Add(GeneratePuzzlePiece("LicensePlateSlot", car.LicensePlateID));
        bodySection.Add(GeneratePuzzlePiece("FrontFenderSlot", car.FrontFenderID));
        bodySection.Add(GeneratePuzzlePiece("RearFenderSlot", car.RearFenderID));
        bodySection.Add(GeneratePuzzlePiece("RimStyleSlot", car.RimsID));

        XElement userSection = XMLUtil.GenerateObject("name", "UserDatasSection");
        userSection.Add(GeneratePuzzlePiece("CarColorSlot", car.ColorID));
        userSection.Add(GeneratePuzzlePiece("StickersSlot", car.StickerID));
        userSection.Add(GeneratePuzzlePiece("AvatarHelmetSlot", car.AvatarHelmet));
        userSection.Add(GeneratePuzzlePiece("AvatarTopSlot", car.AvatarTopID));
        userSection.Add(GeneratePuzzlePiece("AvatarBottomSlot", car.AvatarBottomID));

        XElement carDataSection = XMLUtil.GenerateObject("name", "CarDatasSection");
        carDataSection.Add(GeneratePuzzlePiece("InteriorSlot", car.InteriorID));

        //bike
        XElement bikeSection = XMLUtil.GenerateObject("name", "BikeSection");
        bikeSection.Add(GeneratePuzzlePiece("SwingArmSlot", car.SwingArmID));
        bikeSection.Add(GeneratePuzzlePiece("FrontLightSlot", car.FrontLightID));
        bikeSection.Add(GeneratePuzzlePiece("ExhaustSlot", car.ExhaustID));
        bikeSection.Add(GeneratePuzzlePiece("SideMirrorSlot", car.SideMirrorBikeID));
        bikeSection.Add(GeneratePuzzlePiece("ForkSlot", car.ForkID));
        bikeSection.Add(GeneratePuzzlePiece("RearLightSlot", car.RearLightID));
        bikeSection.Add(GeneratePuzzlePiece("FairingSlot", car.FairingID));
        bikeSection.Add(GeneratePuzzlePiece("FrontFenderSlot", car.FrontFenderBikeID));
        bikeSection.Add(GeneratePuzzlePiece("SeatSlot", car.BikeSeatID));
        bikeSection.Add(GeneratePuzzlePiece("RimStyleSlot", car.RimStyleBikeID));

        XElement puzzleRoot = XMLUtil.GenerateObject("name", "Puzzle");
        puzzleRoot.Add(bodySection);
        puzzleRoot.Add(userSection);
        puzzleRoot.Add(carDataSection);
        puzzleRoot.Add(bikeSection);

        XElement carRoot = XMLUtil.GenerateObject("name", "Entity");
        carRoot.Add(XMLUtil.GenerateField("name", "ID", newCarID));
        carRoot.Add(XMLUtil.GenerateField("name", "FatherArchetypeID", car.TemplateID));
        carRoot.Add(puzzleRoot);

        carRoot.Descendants("none").Remove();

        InsertAddCommand("root", carRoot);
    }

    private XElement? GeneratePuzzlePiece(string pieceName, string pieceID)
    {
        if (string.IsNullOrWhiteSpace(pieceID))
            return new XElement("none");

        if (pieceID == "FFFFFFFFFFFFFFFF")
            return new XElement("none");

        XElement pieceObject = XMLUtil.GenerateObject("name", "ContainedItemsListElement");
        pieceObject.Add(XMLUtil.GenerateField("name", "ContainedItemsListValue", pieceID));

        XElement pieceList = XMLUtil.GenerateObject("name", "ContainedItemsList");
        
        if (pieceName.Contains("FenderSlot"))
            pieceList.Add(XMLUtil.GenerateField("name", "FenderSplashGuard", "01000000"));
        
        pieceList.Add(XMLUtil.GenerateField("name", "CheckIds", "00"));
        pieceList.Add(pieceObject);

        XElement piece = XMLUtil.GenerateObject("name", pieceName);
        piece.Add(pieceList);
        return piece;
    }
}
