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
    }
}
