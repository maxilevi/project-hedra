namespace Hedra.Engine.Generation.ChunkSystem
{
    public static class Automatons
    {
        private static bool WaterNeighbourCheck(BlockType[][][] Types, int X, int Y, int Z)
        {
            if (Types[X][Y][Z] == BlockType.Air && !IsSolid(Types[X][Y-1][Z]))
            {
                Types[X][Y][Z] = BlockType.Water;
                return true;
            }
            return false;
        }
        
        public static bool Water(BlockType[][][] Types, int X, int Y, int Z)
        {
            var changed = false;
            if (Y < 2 || X == 0 || Z == 0 || X == Types.Length-1 || Z == Types[0][0].Length-1) return false;

            if (Types[X][Y-1][Z] == BlockType.Air)
            {
                Types[X][Y-1][Z] = BlockType.Water;
                changed = true;
            }
            else if (IsSolid(Types[X][Y-1][Z]))
            {
                changed |= WaterNeighbourCheck(Types, X+1, Y, Z);
                changed |= WaterNeighbourCheck(Types, X, Y, Z+1);
                changed |= WaterNeighbourCheck(Types, X-1, Y, Z);
                changed |= WaterNeighbourCheck(Types, X, Y, Z-1);
            }
            return changed;
        }

        private static bool IsSolid(BlockType Type)
        {
            return Type != BlockType.Water && Type != BlockType.Air;
        }
    }
}