using System.Xml.Linq;

namespace CrewToolsCommon
{
    public class XMLUtil
    {
        public static XElement GenerateField(string type, string name, string value)
        {
            XElement field = new XElement("field");
            field.SetAttributeValue(type, name);
            field.SetAttributeValue("type", "BinHex");
            field.SetValue(value);
            return field;
        }

        public static XElement GenerateObject(string type, string name)
        {
            XElement obj = new XElement("object");
            obj.SetAttributeValue(type, name);
            return obj;
        }

        public static XElement GrabField(XElement obj, string type, string value)
        {
            var fields = GrabAllFields(obj, type, value);
            if (fields == null)
                return null;

            return fields.First();
        }

        public static XElement GrabFieldWithValue(XElement obj, string type, string typeValue, string value)
        {
            var fields = GrabAllFieldsWithValue(obj, type, typeValue, value);
            if (fields == null)
                return null;

            return fields.First();
        }

        public static IEnumerable<XElement> GrabAllFields(XElement obj, string type, string value)
        {
            var fields = obj.Descendants("field").Where(field => field.Attribute(type) != null && field.Attribute(type).Value == value);
            if (!fields.Any())
                return null;

            return fields;
        }

        public static IEnumerable<XElement> GrabAllFieldsWithValue(XElement obj, string type, string typeValue, string value)
        {
            var fields = obj.Descendants("field").Where(field => field.Attribute(type) != null && field.Attribute(type).Value == typeValue && field.Value == value);
            if (!fields.Any())
                return null;

            return fields;
        }

        public static XElement GrabObject(XElement obj, string type, string value)
        {
            var objects = GrabAllObjects(obj, type, value);
            if (objects == null)
                return null;

            return objects.First();
        }

        public static IEnumerable<XElement> GrabAllObjects(XElement obj, string type, string value)
        {
            var objects = obj.Descendants("object").Where(field => field.Attribute(type) != null && field.Attribute(type).Value == value);
            if (!objects.Any())
                return null;

            return objects;
        }

        public static string GrabStringOrDefault(XElement haystack, string needle, bool showError = false, string def = "")
        {
            XElement element = haystack.Element(needle);
            if (element == null)
            {
                if (showError) Console.Error.WriteLine($"{needle} not found.");
                return def;
            }

            if (string.IsNullOrWhiteSpace(element.Value))
            {
                if (showError) Console.Error.WriteLine($"{needle} has empty data.");
                return def;
            }
            
            return element.Value;
        }

        public static string GrabIDHex(XElement haystack, string needle, bool showError = false)
        {
            string value = GrabStringOrDefault(haystack, needle, showError);

            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (value.Length != 16)
            {
                Console.Error.WriteLine($"{needle} is not 16 in length.");
                return string.Empty;
            }

            return value;
        }

        public static bool GrabBoolOrDefault(XElement haystack, string needle, bool def = false, bool showError = false)
        {
            string value = GrabStringOrDefault(haystack, needle);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (showError) Console.Error.WriteLine($"Defaulting {needle} to {def}.");
                return def;
            }

            if (!bool.TryParse(value, out bool result))
            {
                if (showError) Console.Error.WriteLine($"{needle} is not a valid boolean, defaulting to {def}.");
                return def;
            }
            
            return result;
        }

        public static int GrabIntOrDefault(XElement haystack, string needle, int def = 0, bool noNegative = false)
        {
            string value = GrabStringOrDefault(haystack, needle);
            if (string.IsNullOrWhiteSpace(value))
                return def;

            if (!int.TryParse(value, out int result))
            {
                Console.Error.WriteLine($"{needle} is not a valid boolean, defaulting to {def}.");
                return def;
            }

            if (noNegative && result < 0)
                return 0;

            return result;
        }

        public static float GrabFloatOrDefault(XElement haystack, string needle, float def = 0, bool noNegative = false)
        {
            string value = GrabStringOrDefault(haystack, needle);
            if (string.IsNullOrWhiteSpace(value))
                return def;

            if (!float.TryParse(value, out float result))
            {
                Console.Error.WriteLine($"{needle} inside {haystack.Name} not a valid float, defaulting to {def}.");
                return def;
            }

            if (noNegative && result < 0)
                return 0;

            return result;
        }

        public static float[] GrabCoords(XElement haystack, string needle)
        {
            string value = GrabStringOrDefault(haystack, needle, true);

            if (string.IsNullOrWhiteSpace(value))
                return [];

            string[] coordStrings = value.Split(",");
            if (coordStrings.Length != 3)
            {
                Console.Error.WriteLine($"{needle} is not formatted with two commas between 3 floats.");
                Environment.Exit(1);
            }

            float[] coords = new float[3];
            for (int i = 0; i < 3; i++)
            {
                if (!float.TryParse(coordStrings[i], out coords[i]))
                {
                    Console.Error.WriteLine($"{coordStrings[i]} is not a valid float.");
                    Environment.Exit(1);
                }
            }

            return coords;
        }

        public static float[] GrabAngles(XElement haystack)
        {
            float[] angles = new float[3];

            angles[0] = GrabFloatOrDefault(haystack, "pitch");
            angles[1] = GrabFloatOrDefault(haystack, "roll", -0);
            angles[2] = GrabFloatOrDefault(haystack, "yaw");

            return angles;
        }
    }
}
