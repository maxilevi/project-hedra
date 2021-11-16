using System.Numerics;
using Hedra.Engine.Generation.ChunkSystem;

namespace Hedra.Engine.Generation
{
    public class ChunkAutomatons
    {
        private const int chunkBorder = 2;
        private const int offset = chunkBorder / 2;
        private const int Width = Chunk.BoundsX + chunkBorder;
        private const int Height = Chunk.BoundsY;
        private readonly Chunk _parent;

        public ChunkAutomatons(Chunk Parent)
        {
            _parent = Parent;
        }

        public bool Populated { get; private set; }

        private AutomatonCell[][][] CreateTypeArray()
        {
            var types = new AutomatonCell[Width][][];
            for (var x = 0; x < Width; ++x)
            {
                types[x] = new AutomatonCell[Height][];
                for (var y = 0; y < Height; ++y)
                {
                    types[x][y] = new AutomatonCell[Width];
                    for (var z = 0; z < Width; ++z)
                    {
                        var type = GetBlockAt(x - offset, y, z - offset);
                        types[x][y][z] = new AutomatonCell
                        {
                            Type = type,
                            Occupancy = type == BlockType.Water ? 1 : 0
                        };
                    }
                }
            }

            return types;
        }

        public void Update()
        {
            return;
            Populated = true;
            //if (!_parent.HasWater) return;
            var types = CreateTypeArray();
            while (UpdateAutomatons(types))
            {
            }

            SetTypes(types);
        }

        private void SetTypes(AutomatonCell[][][] Types)
        {
            for (var x = 0; x < Width; ++x)
            for (var y = 0; y < Height; ++y)
            for (var z = 0; z < Width; ++z)
                if (Types[x][y][z].Type == BlockType.Water)
                    SetBlockAt(x - offset, y, z - offset, Types[x][y][z].Type);
        }

        private bool UpdateAutomatons(AutomatonCell[][][] Automata)
        {
            var changed = false;
            for (var x = 0; x < Width; x++)
            for (var z = 0; z < Width; z++)
            for (var y = 0; y < Height; ++y)
                if (Automata[x][y][z].Type == BlockType.Water)
                    changed |= Automatons.Water(Automata, x, y, z);
            return changed;
        }

        private void SetBlockAt(int x, int y, int z, BlockType Type)
        {
            GetNeighbourChunk(x, z).SetBlockAt(Modulo(x), y, Modulo(z), Type);
        }

        private Chunk GetNeighbourChunk(int x, int z)
        {
            if (x >= 0 && z >= 0 && x < Chunk.BoundsX && z < Chunk.BoundsZ) return _parent;
            var space = World.ToChunkSpace(_parent.Position + new Vector3(x, 0, z) * Chunk.BlockSize);
            return World.GetChunkByOffset(space);
        }

        private BlockType GetBlockAt(int x, int y, int z)
        {
            return GetNeighbourChunk(x, z).GetBlockAt(Modulo(x), y, Modulo(z)).Type;
        }

        private static int Modulo(int Index)
        {
            const int bounds = Chunk.BoundsX;
            return (Index % bounds + bounds) % bounds;
        }
    }
}