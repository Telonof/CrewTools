namespace CrewToolsCommon
{
    public class RandomUtil
    {
        private static readonly Random Random = new Random(DateTimeOffset.UtcNow.Millisecond);

        public static int BetweenInc(int a, int b)
        {
            return Random.Next(a, b + 1);
        }

        public static int Between(int a, int b)
        {
            return Random.Next(a, b);
        }
    }
}
