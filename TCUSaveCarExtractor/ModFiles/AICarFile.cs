using CrewToolsCommon;
using CrewToolsCommon.Models;
using Gibbed.Dunia2.FileFormats;
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
        string newCarID = (CRC64.Hash($"{GetOutput()}{car.TemplateID}") & 0xFFFFFFFFFFFFFF00).ToString("X16");

        XElement motorSection = XMLUtil.GenerateObject("name", "MotorSection");
        motorSection.Add(GeneratePuzzlePiece("MotorCoreSlot", car.PhysIds[0]));
        motorSection.Add(GeneratePuzzlePiece("CarburationSlot", car.PhysIds[1]));
        motorSection.Add(GeneratePuzzlePiece("IgnitionAndElectronicSlot", car.PhysIds[2]));
        motorSection.Add(GeneratePuzzlePiece("ExhaustAndAirFilterSlot", car.PhysIds[3]));
        motorSection.Add(GeneratePuzzlePiece("TurboSlot", car.PhysIds[4]));
        motorSection.Add(GeneratePuzzlePiece("GearBoxSlot", car.PhysIds[5]));

        XElement chassisSection = XMLUtil.GenerateObject("name", "ChassisSection");
        chassisSection.Add(GeneratePuzzlePiece("TyresSlot", car.PhysIds[6]));
        chassisSection.Add(GeneratePuzzlePiece("BrakesSlot", car.PhysIds[7]));
        chassisSection.Add(GeneratePuzzlePiece("SuspensionSlot", car.PhysIds[8]));
        chassisSection.Add(GeneratePuzzlePiece("AllegmentSlot", car.PhysIds[9]));
        chassisSection.Add(GeneratePuzzlePiece("AntiRollSlot", car.PhysIds[10]));

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
        userSection.Add(GeneratePuzzlePiece("CarColor2Slot", car.Color2ID));
        userSection.Add(GeneratePuzzlePiece("StickersSlot", car.StickerID));
        userSection.Add(GeneratePuzzlePiece("AvatarHelmetSlot", car.AvatarHelmet));
        userSection.Add(GeneratePuzzlePiece("AvatarTopSlot", car.AvatarTopID));
        userSection.Add(GeneratePuzzlePiece("AvatarBottomSlot", car.AvatarBottomID));

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
        puzzleRoot.Add(motorSection);
        puzzleRoot.Add(chassisSection);
        puzzleRoot.Add(bodySection);
        puzzleRoot.Add(userSection);
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
        pieceList.Add(pieceObject);

        XElement piece = XMLUtil.GenerateObject("name", pieceName);
        if (pieceName.Contains("FenderSlot"))
            piece.Add(XMLUtil.GenerateField("name", "FenderSplashGuard", "01000000"));

        piece.Add(XMLUtil.GenerateField("name", "CheckIds", "00"));
        piece.Add(pieceList);
        return piece;
    }
}
