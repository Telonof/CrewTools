namespace CrewToolsCommon;

public class HexUtil
{
    public static string ConvertFloatToHexString(float value)
    {
        return Convert.ToHexString(BitConverter.GetBytes(value));
    }
}