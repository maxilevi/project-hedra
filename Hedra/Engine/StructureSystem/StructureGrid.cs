using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public static class StructureGrid
    {
        /* Big structures */
        public const int GraveyardChance = 125;
        public const int GiantTreeChance = 100;
        public const int WaterGiantTreeChance = 500;
        public const int BanditCampChance = 128;
        public const int VillageChance = 300;
        /* Small structures */
        public const int WellChance = 80;
        public const int ObeliskChance = 10;
        public const int CampfireChance = 12;
        public const int TravellingMerchantChance = 20;
        
        public static int Sample(Vector3 Position, bool IsSmall)
        {
            return IsSmall ? SampleSmallObjects(Position) : SampleBigObjects(Position);
        }

        private static int SampleBigObjects(Vector3 Position)
        {
            const float frequency = 0.0005f;
            return BaseSample(Position, frequency);
        }

        private static int SampleSmallObjects(Vector3 Position)
        {
            const float frequency = 0.005f;
            return BaseSample(Position, frequency);
        }

        private static int BaseSample(Vector3 Position, float Frequency)
        {
            var wSeed = World.Seed * 0.0001f;
            return (int) ((float) World.StructureHandler.SeedGenerator.GetValue(Position.X * Frequency + wSeed, Position.Z * Frequency + wSeed) * 100f);
        }
    }
}