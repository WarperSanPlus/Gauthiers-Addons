namespace Gauthier
{
    public class Addons
    {
        private static readonly Random rdm = new();

        public static int GetRandomInt(int max, int min = 0) => rdm.Next(min, max);

        public static int GetRandomInt(int seed, int max, int min = 0) => new Random(seed).Next(min, max);
    }
}