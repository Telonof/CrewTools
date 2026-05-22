namespace TC1StickerTool
{
    public sealed class Category
    {
        public readonly string Name;

        public readonly string Index;

        public readonly string ServerDepth;

        public readonly string FatherArchetypeID;

        public readonly string StickerType;

        private Category(string name, string index, string serverDepth, string fatherArchetypeID, string stickerType)
        {
            Name = name;
            Index = index;
            ServerDepth = serverDepth;
            FatherArchetypeID = fatherArchetypeID;
            StickerType = stickerType;
        }

        public static readonly Category ANIMALS = new Category("Animals", "48738407", "437:0", "5163850700000000", "A4C176EF");
        public static readonly Category URBAN = new Category("Urban", "45738407", "473:0", "A963850700000000", "78429DB7");
        public static readonly Category UBISOFT = new Category("Ubisoft", "4F699207", "513:0", "F468920700000000", "68585163");
        public static readonly Category FLAGS = new Category("Flags", "4B738407", "450:0", "5E63850700000000", "80FA0117");
        public static readonly Category FLAMING = new Category("Flaming", "43738407", "431:0", "6D63850700000000", "A7F29671");
        public static readonly Category NATURE = new Category("Nature", "47738407", "455:0", "7B63850700000000", "69ECE519");
        public static readonly Category RACING = new Category("Racing", "44738407", "425:0", "8563850700000000", "0D9457D3");
        public static readonly Category SKULLS = new Category("Skulls", "49738407", "462:0", "C290850700000000", "F12B5BF4");
        public static readonly Category SURF = new Category("Surf", "4C738407", "467:0", "9D63850700000000", "086651AE");
        public static readonly Category TRIBAL = new Category("Tribal", "46738407", "441:0", "A163850700000000", "A05794AD");
        public static readonly Category VINTAGE = new Category("Vintage", "4A738407", "479:0", "AF63850700000000", "C3FB1BD8");
        public static readonly Category SEASONAL = new Category("Christmas", "1E619207", "509:0", "1C61920700000000", "3580DEF9");

        private static readonly Dictionary<string, Category> CatMap = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase)
        {
            { "tribal", TRIBAL },
            { "vintage", VINTAGE },
            { "seasonal", SEASONAL },
            { "animals", ANIMALS },
            { "urban", URBAN },
            { "ubisoft", UBISOFT },
            { "flags", FLAGS },
            { "flaming", FLAMING },
            { "nature", NATURE },
            { "racing", RACING },
            { "skulls", SKULLS },
            { "surf", SURF },
        };

        public static Category FromString(string input)
        {
            if (CatMap.TryGetValue(input, out var category))
                return category;

           return ANIMALS;
        }
    }
}
