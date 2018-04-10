using System;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.QuestSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class VillageDesign : StructureDesign
    {
        public static Vector3 StablePosition = -Vector3.UnitX * 160.0f - Vector3.UnitZ * 150.0f;
        public static Vector3 BlacksmithPosition = -Vector3.UnitZ * 30 - Vector3.UnitX * 180f;
        public static Vector3 MarketPosition = -Vector3.UnitY * 2.0f + Vector3.UnitZ * 140.0f;
        public static Vector3 WindmillPosition = -Vector3.UnitY * 2.0f - Vector3.UnitZ * 140.0f;
        public static Vector3 FarmPosition = -Vector3.UnitY * 2.0f - Vector3.UnitZ * 140.0f;
        public override int Radius { get; set; } = 900;

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            Matrix4 marketTransMatrix = Matrix4.CreateScale(5f) * Matrix4.CreateTranslation(MarketPosition);
            Matrix4 farmTransMatrix = Matrix4.CreateScale(8f) * Matrix4.CreateTranslation(FarmPosition);
            Matrix4 windmillTransMatrix = Matrix4.CreateScale(10f) * Matrix4.CreateTranslation(WindmillPosition);
            Matrix4 stableTransMatrix = Matrix4.CreateScale(8f) * Matrix4.CreateTranslation(StablePosition);
            Matrix4 blacksmithTransMatrix = Matrix4.CreateScale(5f) * Matrix4.CreateTranslation(BlacksmithPosition);


            var rng = new Random(World.Seed + 2341243);


            for (int j = 0; j < 3; j++)
            {
                Vector3 houseOffset = Vector3.UnitZ * 60.0f - Vector3.UnitX * 240.0f + Vector3.UnitX * 20f * Chunk.BlockSize;
                Vector3 housePosition = Vector3.UnitZ * j * Chunk.BlockSize * 20f + houseOffset;
                Matrix4 houseTransMatrix = Matrix4.CreateScale(4f) * Matrix4.CreateTranslation(housePosition);
                CoroutineManager.StartCoroutine(VillageGenerator.BuildSingleHouse, new object[] { Position, houseTransMatrix, rng });

            }


            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector3 houseOffset = Vector3.UnitZ * 60.0f + Vector3.UnitX * 360.0f;
                    Vector3 housePosition = houseOffset + Vector3.UnitZ * j * Chunk.BlockSize * 20f;
                    Matrix4 houseTransMatrix = Matrix4.CreateScale(4f) * Matrix4.CreateRotationY(180f * Mathf.Radian) * Matrix4.CreateTranslation(housePosition);
                    CoroutineManager.StartCoroutine(VillageGenerator.BuildSingleHouse, new object[] { Position, houseTransMatrix, rng });

                }
            }


            for (int j = 0; j < 2; j++)
            {
                Vector3 houseOffset = -Vector3.UnitZ * 180.0f + Vector3.UnitX * 140.0f;
                Vector3 housePosition = Vector3.UnitY * .3f + houseOffset + Vector3.UnitZ * j * Chunk.BlockSize * 20f;
                Matrix4 houseTransMatrix = Matrix4.CreateScale(4f) * Matrix4.CreateRotationY(180f * Mathf.Radian) * Matrix4.CreateTranslation(housePosition);
                CoroutineManager.StartCoroutine(VillageGenerator.BuildSingleHouse, new object[] { Position, houseTransMatrix, rng });

            }


            ThreadManager.ExecuteOnMainThread(() => World.QuestManager.SpawnVillager(Position + farmTransMatrix.ExtractTranslation() + Vector3.UnitX * 360.0f, true));
            ThreadManager.ExecuteOnMainThread(() => World.QuestManager.SpawnVillager(Position + farmTransMatrix.ExtractTranslation() + Vector3.UnitX * 220.0f + Vector3.UnitZ * 180f, true));
            ThreadManager.ExecuteOnMainThread(() => World.QuestManager.SpawnVillager(Position + farmTransMatrix.ExtractTranslation() + Vector3.UnitX * 220.0f + Vector3.UnitZ * 400f, true));

            CoroutineManager.StartCoroutine(VillageGenerator.BuildMarket, new object[] { Structure, rng, marketTransMatrix * Matrix4.CreateTranslation(Position + Vector3.UnitY * .75f) });
            CoroutineManager.StartCoroutine(VillageGenerator.BuildCenter, new object[] { rng, null, marketTransMatrix * Matrix4.CreateTranslation(Position) });
            CoroutineManager.StartCoroutine(VillageGenerator.BuildFarms, new object[] { Structure, rng, farmTransMatrix, Position });
            CoroutineManager.StartCoroutine(VillageGenerator.BuildFarms, new object[] { Structure, rng, farmTransMatrix * Matrix4.CreateTranslation(Vector3.UnitX * 360.0f), Position });
            CoroutineManager.StartCoroutine(VillageGenerator.BuildFarms, new object[] { Structure, rng, farmTransMatrix * Matrix4.CreateTranslation(Vector3.UnitX * 220.0f + Vector3.UnitZ * 180), Position });
            CoroutineManager.StartCoroutine(VillageGenerator.BuildFarms, new object[] { Structure, rng, farmTransMatrix * Matrix4.CreateTranslation(Vector3.UnitX * 220.0f + Vector3.UnitZ * 400), Position });
            CoroutineManager.StartCoroutine(VillageGenerator.BuildBlacksmith, new object[] { Structure, blacksmithTransMatrix, Position });
            CoroutineManager.StartCoroutine(VillageGenerator.GenerateWindmill, new object[] { Position, rng, true, windmillTransMatrix });
            CoroutineManager.StartCoroutine(VillageGenerator.GenerateStable, new object[] { Structure, Position, rng, true, stableTransMatrix });
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            Vector3 farmPosition = -Vector3.UnitY * 2.0f - Vector3.UnitZ * 140.0f;
            try
            {

                World.QuestManager.AddVillagePosition(
                    TargetPosition + MarketPosition, 96);
                World.QuestManager.AddVillagePosition(
                    TargetPosition + farmPosition + Vector3.UnitX * 360f, 96);
                World.QuestManager.AddVillagePosition(
                    TargetPosition - Vector3.UnitX * 180.0f - Vector3.UnitZ * 140.0f, 64);
                World.QuestManager.AddVillagePosition(TargetPosition + farmPosition, 96);
                World.QuestManager.AddVillagePosition(
                    TargetPosition + farmPosition + Vector3.UnitX * 220.0f + Vector3.UnitZ * 180, 96);
                World.QuestManager.AddVillagePosition(
                    TargetPosition + farmPosition + Vector3.UnitX * 220.0f + Vector3.UnitZ * 400, 96);
                World.QuestManager.AddVillagePosition(
                    TargetPosition + Vector3.UnitZ * -30 + Vector3.UnitX * -180, 64);

                for (var i = 0; i < 2; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        World.QuestManager.AddVillagePosition(
                            TargetPosition - Vector3.UnitX * 241.0f + Vector3.UnitZ * 60.0f
                            + Vector3.UnitX * i * Chunk.BlockSize * 20f
                            + Vector3.UnitZ * j * Chunk.BlockSize * 20f, 48);
                    }
                }

                for (var i = 0; i < 2; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        World.QuestManager.AddVillagePosition(
                            TargetPosition + Vector3.UnitX * 360.0f + Vector3.UnitZ * 60.0f
                            + Vector3.UnitX * i * Chunk.BlockSize * 20f
                            + Vector3.UnitZ * j * Chunk.BlockSize * 20f, 48);
                    }
                }

                for (var i = 0; i < 2; i++)
                {
                    for (var j = 0; j < 2; j++)
                    {
                        World.QuestManager.AddVillagePosition(
                            TargetPosition + Vector3.UnitX * 140.0f - Vector3.UnitZ * 175.0f
                            + Vector3.UnitX * i * Chunk.BlockSize * 20f
                            + Vector3.UnitZ * j * Chunk.BlockSize * 20f, 48);
                    }
                }
            }
            
            catch (ArgumentException e)
            {
                Log.WriteLine("Couldn't setup town here. " + e);
                return null;
            }

            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            var plateau = new Plateau(TargetPosition, this.Radius, 800, height);
            World.QuestManager.AddPlateau(plateau);
            return new CollidableStructure(this, TargetPosition, plateau);        
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            return BiomeGenerator.PathFormula(ChunkOffset.X, ChunkOffset.Y) > 0 && Rng.Next(0, 55) == 1 && height > 0;
        }
    }
}
