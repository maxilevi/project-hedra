using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class MarketBuilder : Builder<MarketParameters>
    {
        public MarketBuilder(CollidableStructure Structure) : base(Structure)
        {
        }

        public override bool Place(MarketParameters Parameters, VillageCache Cache)
        {
            var work = CreateGroundwork(Parameters.Position, Parameters.Size, BlockType.StonePath);
            return PushGroundwork(work);
        }

        public override BuildingOutput Paint(MarketParameters Parameters, BuildingOutput Input)
        {
            return Input;
        }

        public override BuildingOutput Build(MarketParameters Parameters, DesignTemplate Design, VillageCache Cache,
            Random Rng, Vector3 VillageCenter)
        {
            var marketDist = 3.5f + Rng.NextFloat() * .75f + 0.2f;
            var marketCount = 8 + Rng.Next(0, 4);
            return DoBuildMarket(Parameters.Position, Rng, marketDist, marketCount);
        }

        public static BuildingOutput DoBuildMarket(Vector3 Position, Random Rng, float Distance, int Count)
        {
            var marketModels = new List<VertexData>();
            var marketShapes = new List<CollisionShape>();
            var originalPosition = Position;
            var transMatrix = Matrix4x4.CreateScale(4f) * Matrix4x4.CreateTranslation(originalPosition);
            for (var i = 0; i < Count; i++)
            {
                var market0 = VillageCache.Market.Market0_Clone.ToVertexData().Clone();
                var extraShelf = Rng.Next(0, 4) != 0;
                if (extraShelf) market0 += VillageCache.Market.Market1_Clone.ToVertexData().Clone();
                market0.Transform(Matrix4x4.CreateRotationY(90 * Mathf.Radian));
                market0.Translate(Vector3.UnitZ * Distance * Chunk.BlockSize);
                market0.Transform(Matrix4x4.CreateRotationY(360 / Count * i * Mathf.Radian));
                market0.Color(AssetManager.ColorCode1, MarketColor(Rng));

                var shapes = VillageCache.Market.MarketShapes_Clone.DeepClone();
                if (extraShelf) shapes.Add((CollisionShape)VillageCache.Market.ExtraShelf_Clone[0].Clone());

                for (var j = 0; j < shapes.Count; j++)
                {
                    shapes[j].Transform(Matrix4x4.CreateRotationY(90 * Mathf.Radian));
                    shapes[j].Transform(Matrix4x4.CreateTranslation(Vector3.UnitZ * Distance * Chunk.BlockSize));
                    shapes[j].Transform(Matrix4x4.CreateRotationY(360 / Count * i * Mathf.Radian));
                    shapes[j].Transform(transMatrix);
                }

                var basketCount = Rng.Next(0, 6);
                if (basketCount == 0) basketCount = 2;
                else if (basketCount == 1 || basketCount == 2) basketCount = 3;
                else if (basketCount == 3 || basketCount == 4 || basketCount == 5) basketCount = 4;

                var shelfShapes = VillageCache.Market.ShelfShapes_Clones[basketCount].DeepClone();
                var shelfModel = VillageCache.Market.ShelfModels_Clones[basketCount].ToVertexData().Clone();

                shelfModel.Transform(Matrix4x4.CreateRotationY(90 * Mathf.Radian));
                shelfModel.Translate(Vector3.UnitZ * Distance * Chunk.BlockSize);
                shelfModel.Transform(Matrix4x4.CreateRotationY(360 / Count * i * Mathf.Radian));
                shelfModel.Color(AssetManager.ColorCode1, Colors.BerryColor(Rng));

                for (var j = 0; j < shelfShapes.Count; j++)
                {
                    shelfShapes[j].Transform(Matrix4x4.CreateRotationY(90 * Mathf.Radian));
                    shelfShapes[j].Transform(Matrix4x4.CreateTranslation(Vector3.UnitZ * Distance * Chunk.BlockSize));
                    shelfShapes[j].Transform(Matrix4x4.CreateRotationY(360 / Count * i * Mathf.Radian));
                    shelfShapes[j].Transform(transMatrix);
                }

                market0 += shelfModel;
                shapes.AddRange(shelfShapes);

                if (extraShelf)
                {
                    basketCount = Rng.Next(0, 6);
                    if (basketCount == 0) basketCount = -1;
                    else if (basketCount == 1 || basketCount == 2) basketCount = 5;
                    else if (basketCount == 3 || basketCount == 4 || basketCount == 5) basketCount = 6;

                    if (basketCount != -1)
                    {
                        shelfShapes = VillageCache.Market.ShelfShapes_Clones[basketCount].DeepClone();
                        shelfModel = VillageCache.Market.ShelfModels_Clones[basketCount].ToVertexData().Clone();

                        shelfModel.Transform(Matrix4x4.CreateRotationY(90 * Mathf.Radian));
                        shelfModel.Translate(Vector3.UnitZ * Distance * Chunk.BlockSize);
                        shelfModel.Transform(Matrix4x4.CreateRotationY(360 / Count * i * Mathf.Radian));
                        shelfModel.Color(AssetManager.ColorCode1, Colors.BerryColor(Rng));

                        for (var j = 0; j < shelfShapes.Count; j++)
                        {
                            shelfShapes[j].Transform(Matrix4x4.CreateRotationY(90 * Mathf.Radian));
                            shelfShapes[j]
                                .Transform(Matrix4x4.CreateTranslation(Vector3.UnitZ * Distance * Chunk.BlockSize));
                            shelfShapes[j].Transform(Matrix4x4.CreateRotationY(360 / Count * i * Mathf.Radian));
                            shelfShapes[j].Transform(transMatrix);
                        }
                    }

                    market0 += shelfModel;
                    shapes.AddRange(shelfShapes);
                }

                market0.Transform(transMatrix);
                marketModels.Add(market0);
                marketShapes.AddRange(shapes.ToArray());
            }

            return new BuildingOutput
            {
                Models = marketModels,
                Shapes = marketShapes,
                BuildAsInstance = false
            };
        }

        public override void Polish(MarketParameters Parameters, VillageRoot Root, Random Rng)
        {
            base.Polish(Parameters, Root, Rng);
            var originalPosition = Parameters.Position + Physics.HeightAtPosition(Parameters.Position) * Vector3.UnitY;
            for (var i = 0; i < 4; ++i)
                switch (i)
                {
                    case 0:
                        SpawnHumanoid(HumanType.Merchant, originalPosition - Vector3.UnitZ * 40f);
                        break;
                    case 1:
                        SpawnHumanoid(HumanType.Merchant, originalPosition + Vector3.UnitZ * 40f);
                        break;
                    case 2:
                        if (Rng.Next(0, 2) == 1)
                            SpawnVillager(originalPosition - Vector3.UnitX * 40f, Rng);
                        if (Rng.Next(0, 2) == 1)
                            SpawnVillager(originalPosition - Vector3.UnitX * 50f, Rng);
                        break;
                    case 3:
                        if (Rng.Next(0, 2) == 1)
                            SpawnVillager(originalPosition + Vector3.UnitX * 40f, Rng);
                        if (Rng.Next(0, 2) == 1)
                            SpawnVillager(originalPosition - Vector3.UnitX * 50f, Rng);
                        break;
                }
        }

        private static Vector4 MarketColor(Random Rng)
        {
            switch (Rng.Next(0, 6))
            {
                case 0: return Colors.FromHtml("#BF4B42");
                case 1: return Colors.FromHtml("#FF6380");
                case 2: return Colors.FromHtml("#AA3D98");
                case 3: return Colors.FromHtml("#379B95");
                case 4: return Colors.FromHtml("#FFAD5A");
            }

            return Colors.Red;
        }
    }
}