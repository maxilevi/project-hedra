using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal class MarketBuilder : Builder<BuildingParameters>
    {
        public override bool Place(BuildingParameters Parameters, VillageCache Cache)
        {
            return this.PlaceGroundwork(Parameters.Position, 96, BlockType.StonePath);
        }

        public override BuildingOutput Paint(BuildingParameters Parameters, BuildingOutput Input)
        {
            return Input;
        }

        public override BuildingOutput Build(BuildingParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            float marketDist = 1.75f + Rng.NextFloat() * .75f + 1.2f;
            int marketCount = 6 + Rng.Next(0, 4);

            var market = new VertexData();
            var marketShapes = new List<CollisionShape>();
            var originalPosition = Parameters.Position;
            var transMatrix = Matrix4.CreateScale(5f) * Matrix4.CreateTranslation(originalPosition);
            for (var i = 0; i < marketCount; i++)
            {
                if (i == 0)
                    World.WorldBuilding.SpawnHumanoid(HumanType.Merchant, originalPosition - Vector3.UnitZ * 40f);
                else if (i == 1)
                    World.WorldBuilding.SpawnHumanoid(HumanType.Merchant, originalPosition + Vector3.UnitZ * 40f);
                else if (i == 2)
                    World.WorldBuilding.SpawnVillager(originalPosition - Vector3.UnitX * 40f, false);
                else if (i == 3)
                    World.WorldBuilding.SpawnVillager(originalPosition + Vector3.UnitX * 40f, false);


                VertexData market0 = VillageCache.Market.Market0_Clone.Clone();
                bool extraShelf = Rng.Next(0, 4) != 0;
                if (extraShelf) market0 += VillageCache.Market.Market1_Clone.Clone();
                market0.Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
                market0.Translate(Vector3.UnitZ * marketDist * Chunk.BlockSize);
                market0.Transform(Matrix4.CreateRotationY(360 / marketCount * i * Mathf.Radian));
                market0.Color(AssetManager.ColorCode1, MarketColor(Rng));

                List<CollisionShape> shapes = VillageCache.Market.MarketShapes_Clone.DeepClone();
                if (extraShelf) shapes.Add((CollisionShape)VillageCache.Market.ExtraShelf_Clone[0].Clone());

                for (int j = 0; j < shapes.Count; j++)
                {
                    shapes[j].Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
                    shapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
                    shapes[j].Transform(Matrix4.CreateRotationY(360 / marketCount * i * Mathf.Radian));
                    shapes[j].Transform(transMatrix);
                }

                int basketCount = Rng.Next(0, 6);
                if (basketCount == 0) basketCount = 2;
                else if (basketCount == 1 || basketCount == 2) basketCount = 3;
                else if (basketCount == 3 || basketCount == 4 || basketCount == 5) basketCount = 4;

                List<CollisionShape> shelfShapes = VillageCache.Market.ShelfShapes_Clones[basketCount].DeepClone();
                VertexData shelfModel = VillageCache.Market.ShelfModels_Clones[basketCount].Clone();

                shelfModel.Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
                shelfModel.Translate(Vector3.UnitZ * marketDist * Chunk.BlockSize);
                shelfModel.Transform(Matrix4.CreateRotationY(360 / marketCount * i * Mathf.Radian));
                shelfModel.Color(AssetManager.ColorCode1, Colors.BerryColor(Rng));

                for (int j = 0; j < shelfShapes.Count; j++)
                {
                    shelfShapes[j].Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
                    shelfShapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
                    shelfShapes[j].Transform(Matrix4.CreateRotationY(360 / marketCount * i * Mathf.Radian));
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
                        shelfModel = VillageCache.Market.ShelfModels_Clones[basketCount].Clone();

                        shelfModel.Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
                        shelfModel.Translate(Vector3.UnitZ * marketDist * Chunk.BlockSize);
                        shelfModel.Transform(Matrix4.CreateRotationY(360 / marketCount * i * Mathf.Radian));
                        shelfModel.Color(AssetManager.ColorCode1, Colors.BerryColor(Rng));

                        for (int j = 0; j < shelfShapes.Count; j++)
                        {
                            shelfShapes[j].Transform(Matrix4.CreateRotationY(90 * Mathf.Radian));
                            shelfShapes[j].Transform(Matrix4.CreateTranslation(Vector3.UnitZ * marketDist * Chunk.BlockSize));
                            shelfShapes[j].Transform(Matrix4.CreateRotationY(360 / marketCount * i * Mathf.Radian));
                            shelfShapes[j].Transform(transMatrix);
                        }
                    }
                    market0 += shelfModel;
                    shapes.AddRange(shelfShapes);
                }
                market += market0;
                marketShapes.AddRange(shapes.ToArray());
            }
            market.Transform(transMatrix);
            return new BuildingOutput
            {
                Model = market,
                Shapes = marketShapes
            };
        }

        public override void Polish(BuildingParameters Parameters)
        {

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