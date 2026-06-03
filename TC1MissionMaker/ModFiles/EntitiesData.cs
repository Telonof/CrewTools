using CrewToolsCommon;
using CrewToolsCommon.Models;
using CrewToolsCommon.Utilities;
using System.Xml.Linq;


namespace TC1MissionMaker.ModFiles;

public class EntitiesData : ModFile
{

    private Dictionary<string, ulong> _addedCars;


    public EntitiesData(string outputFile) : base("entity/generated/archetypes.entities.bin", outputFile, null)
    {
        _addedCars = [];
    }

    public ulong GetCarIfFound(string carId)
    {
        if (_addedCars.TryGetValue(carId, out ulong value))
            return value;

        return 0;
    }

    public void AddBot(ulong id, string car, string father = "2C5ACD0300000000")
    {
        XElement bot = XMLUtil.GenerateObject("name", "Entity");
        bot.Add(XMLUtil.GenerateField("name", "ID", ConversionUtil.ULongToHex(id)));
        bot.Add(XMLUtil.GenerateField("name", "FatherArchetypeID", father));
        bot.Add(XMLUtil.GenerateField("name", "SpawnPolicy", "00000000"));
        bot.Add(XMLUtil.GenerateField("name", "VehicleArchetypeToSpawn", car));
        InsertAddCommand("root", bot);

        if (father != "2C5ACD0300000000")
            _addedCars.Add(car, id);
    }

    public void AddTakedownHealth(ulong id, float health)
    {
        XElement healthEntity = XMLUtil.GenerateObject("name", "Entity");
        healthEntity.Add(XMLUtil.GenerateField("name", "ID", ConversionUtil.ULongToHex(id)));
        healthEntity.Add(XMLUtil.GenerateField("name", "FatherArchetypeID", "DE41840700000000"));
        healthEntity.Add(XMLUtil.GenerateField("name", "MaxHealth", Convert.ToHexString(BitConverter.GetBytes(health))));
        InsertAddCommand("root", healthEntity);
    }

    //blockedVelocity - what speed must they be going for the police to stop arresting.
    //blockedTime - how much time until the countdown is over.
    //blockedDistance - what distance must they be away from the police to stop arresting.
    //minPoliceArrest - how much police need to be nearby to count the arrest?
    //roadblockArrestDistance - how far away can roadblocks count as arrests.
    public void AddArrestConditions(ulong id, float blockedVelocity, float blockedTime, float blockedDistance, int minPoliceArrest, float roadblockArrestDistance)
    {
        XElement arrestCondition = XMLUtil.GenerateObject("name", "Entity");
        arrestCondition.Add(XMLUtil.GenerateField("name", "ID", ConversionUtil.ULongToHex(id)));
        arrestCondition.Add(XMLUtil.GenerateField("name", "FatherArchetypeID", "C817D20300000000"));
        XElement conditions = XMLUtil.GenerateObject("name", "ConditionList");
        XElement conditionsElement = XMLUtil.GenerateObject("name", "ConditionListElement");
        XElement conditionsValue = XMLUtil.GenerateObject("name", "ConditionListValue");
        conditionsValue.Add(XMLUtil.GenerateField("name", "hid_DTCTH_ClassName", "27CBE593"));
        conditionsValue.Add(XMLUtil.GenerateField("name", "BlockedVelocityMax", Convert.ToHexString(BitConverter.GetBytes(blockedVelocity))));
        conditionsValue.Add(XMLUtil.GenerateField("name", "BlockedTimeMin", Convert.ToHexString(BitConverter.GetBytes(blockedTime))));
        conditionsValue.Add(XMLUtil.GenerateField("name", "BlockedDistanceMax", Convert.ToHexString(BitConverter.GetBytes(blockedDistance))));
        conditionsValue.Add(XMLUtil.GenerateField("name", "BlockedMinPoliceCount", Convert.ToHexString(BitConverter.GetBytes(minPoliceArrest))));
        conditionsValue.Add(XMLUtil.GenerateField("name", "RoadBlockArrestDistance", Convert.ToHexString(BitConverter.GetBytes(roadblockArrestDistance))));

        conditionsElement.Add(conditionsValue);
        conditions.Add(conditionsElement);
        arrestCondition.Add(conditions);
        InsertAddCommand("root", arrestCondition);
    }
}