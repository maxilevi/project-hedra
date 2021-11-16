namespace Hedra.Engine.Generation.ChunkSystem
{
    public static class Automatons
    {
        private static bool WaterNeighbourCheck(AutomatonCell[][][] Automatons, int X, int Y, int Z)
        {
            if (Automatons[X][Y][Z].Type == BlockType.Air && !IsSolid(Automatons[X][Y - 1][Z]))
            {
                Automatons[X][Y][Z].Type = BlockType.Water;
                Automatons[X][Y][Z].Occupancy = 1f;
                return true;
            }

            return false;
        }

        private static bool WaterHorizontalCheck(AutomatonCell[][][] Automatons, int X, int Y, int Z, float Occupancy)
        {
            if (Automatons[X][Y][Z].Type == BlockType.Air && IsSolid(Automatons[X][Y - 1][Z]))
            {
                Automatons[X][Y][Z].Type = BlockType.Water;
                Automatons[X][Y][Z].Occupancy = Occupancy - 0.25f;
                return true;
            }

            return false;
        }

        public static bool Water(AutomatonCell[][][] Automatons, int X, int Y, int Z)
        {
            var changed = false;
            if (Y <= 2 || X == 0 || Z == 0 || X == Automatons.Length - 1 || Z == Automatons[0][0].Length - 1)
                return false;

            if (Automatons[X][Y - 1][Z].Type == BlockType.Air)
            {
                Automatons[X][Y - 1][Z].Type = BlockType.Water;
                changed = true;
            }
            else if (IsSolid(Automatons[X][Y - 1][Z]))
            {
                for (var i = 0; i < 2; ++i)
                {
                    changed |= WaterNeighbourCheck(Automatons, X + 1, Y - i, Z);
                    changed |= WaterNeighbourCheck(Automatons, X, Y - i, Z + 1);
                    changed |= WaterNeighbourCheck(Automatons, X - 1, Y - i, Z);
                    changed |= WaterNeighbourCheck(Automatons, X, Y - i, Z - 1);

                    changed |= WaterNeighbourCheck(Automatons, X + 1, Y - i, Z + 1);
                    changed |= WaterNeighbourCheck(Automatons, X - 1, Y - i, Z + 1);
                    changed |= WaterNeighbourCheck(Automatons, X + 1, Y - i, Z - 1);
                    changed |= WaterNeighbourCheck(Automatons, X - 1, Y - i, Z - 1);
                }

                var occupancy = Automatons[X][Y][Z].Occupancy;
                if (occupancy > 0)
                {
                    changed |= WaterHorizontalCheck(Automatons, X + 1, Y, Z, occupancy);
                    changed |= WaterHorizontalCheck(Automatons, X, Y, Z + 1, occupancy);
                    changed |= WaterHorizontalCheck(Automatons, X - 1, Y, Z, occupancy);
                    changed |= WaterHorizontalCheck(Automatons, X, Y, Z - 1, occupancy);

                    changed |= WaterHorizontalCheck(Automatons, X + 1, Y, Z + 1, occupancy);
                    changed |= WaterHorizontalCheck(Automatons, X - 1, Y, Z + 1, occupancy);
                    changed |= WaterHorizontalCheck(Automatons, X + 1, Y, Z - 1, occupancy);
                    changed |= WaterHorizontalCheck(Automatons, X - 1, Y, Z - 1, occupancy);
                }
            }

            return changed;
        }

        private static bool IsSolid(AutomatonCell Cellular)
        {
            return Cellular.Type != BlockType.Water && Cellular.Type != BlockType.Air;
        }
    }

    public struct AutomatonCell
    {
        public BlockType Type;
        public float Occupancy;
    }
}