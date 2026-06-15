namespace TCUSaveCarExtractor.Models;

public record Car
{
    public string ModelID { get; set; }
    public string DressID { get; set; }
    public string TemplateID { get; set; }

    public string FrontBumperID { get; set; }
    public string RearBumperID { get; set; }
    public string SkirtsID { get; set; }
    public string SideMirrorID { get; set; }
    public string RearWingID { get; set; }
    public string HoodID { get; set; }
    public string FrontFenderID { get; set; }
    public string RearFenderID { get; set; }
    public string RimsID { get; set; }
    public string LicensePlateID { get; set; }
    public string ColorID { get; set; }
    public string StickerID { get; set; }
    public string InteriorID { get; set; }

    //bike
    public string AvatarHelmet { get; set; }
    public string AvatarTopID { get; set; }
    public string AvatarBottomID { get; set; }
    public string SwingArmID { get; set; }
    public string FrontLightID { get; set; }
    public string ExhaustID { get; set; }
    public string SideMirrorBikeID { get; set; }
    public string ForkID { get; set; }
    public string RearLightID { get; set; }
    public string FrontFenderBikeID { get; set; }
    public string BikeSeatID { get; set; }
    public string FairingID { get; set; }
    public string RimStyleBikeID { get; set; }

}
