using System.Text;

namespace CrewToolsCommon.Utilities
{
    public class ConversionUtil
    {
        public static string FloatsToHex(float[] coords)
        {
            return Convert.ToHexString(FloatsToByteArray(coords));
        }

        //coords in this game are xyzw (w always unused, z is up and down)
        //angles are pich roll yaw w (w is always unused)
        public static byte[] FloatsToByteArray(float[] coords)
        {
            byte[] bytes = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(coords[0]), 0, bytes, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(coords[1]), 0, bytes, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(coords[2]), 0, bytes, 8, 4);
            return bytes;
        }

        public static string FloatToHex(float value)
        {
            return Convert.ToHexString(BitConverter.GetBytes(value));
        }

        public static string StringToHex(string input)
        {
            return Convert.ToHexString(StringToByteArray(input));
        }

        public static byte[] StringToByteArray(string input)
        {
            byte[] str = Encoding.UTF8.GetBytes(input);
            byte[] strNullTerm = new byte[str.Length + 1];
            Buffer.BlockCopy(str, 0, strNullTerm, 0, str.Length);
            return strNullTerm;
        }

        public static string ULongToHex(ulong value)
        {
            return Convert.ToHexString(BitConverter.GetBytes(value));
        }

        public static string BoolToHex(bool value)
        {
            return Convert.ToHexString(BitConverter.GetBytes(value));
        }

        public static string IntToHex(int value)
        {
            return Convert.ToHexString(BitConverter.GetBytes(value));
        }
    }
}
