using System;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal class FarmBuilder : Builder<FarmParameters>
    {
        protected override bool LookAtCenter => false;

        public override void Place(FarmParameters Parameters, VillageCache Cache)
        {
            var size = Cache.GrabSize(Parameters.Design.Path);
            var offsets = this.BuildFarmOffsets(size);
            this.PlaceGroundwork(Parameters.Position + offsets[0], size.Xz.LengthFast * .5f);
            this.PlaceGroundwork(Parameters.Position + offsets[1], size.Xz.LengthFast * .5f);
            this.PlaceGroundwork(Parameters.Position + offsets[2], size.Xz.LengthFast * .5f);
            this.PlaceGroundwork(Parameters.Position + offsets[3], size.Xz.LengthFast * .5f);
            if (Parameters.HasWindmill)
            {
                this.PlaceGroundwork(Parameters.Position, Cache.GrabSize(Parameters.WindmillDesign.Path).Xz.LengthFast);
            }
        }

        public override BuildingOutput Paint(FarmParameters Parameters, BuildingOutput Input)
        {
            var regionColor = World.BiomePool.GetRegion(Parameters.Position).Colors;
            Input.Model.Color(AssetManager.ColorCode2, regionColor.GrassColor);
            return base.Paint(Parameters, Input);
        }

        public override BuildingOutput Build(FarmParameters Parameters, VillageCache Cache, Random Rng, Vector3 Center)
        {
            var size = Cache.GrabSize(Parameters.Design.Path);
            var offsets = this.BuildFarmOffsets(size);
            var farm0 = base.Build(Parameters.AlterPosition(offsets[0]), Cache, Rng, Center);
            var farm1 = base.Build(Parameters.AlterPosition(offsets[1]), Cache, Rng, Center);
            var farm2 = base.Build(Parameters.AlterPosition(offsets[2]), Cache, Rng, Center);
            var farm3 = base.Build(Parameters.AlterPosition(offsets[3]), Cache, Rng, Center);
            var windmill = Parameters.HasWindmill ? this.BuildWindmill(Parameters, Cache) : BuildingOutput.Empty;
            return new BuildingOutput
            {
                Model = farm0.Model + farm1.Model + farm2.Model + farm3.Model + windmill.Model,
                Shapes = farm0.Shapes.Concat(farm1.Shapes).Concat(farm2.Shapes).Concat(farm3.Shapes).Concat(windmill.Shapes).ToList()
            };
        }

        private BuildingOutput BuildWindmill(FarmParameters Parameters, VillageCache Cache)
        {
            var transformationMatrix = Matrix4.CreateTranslation(Parameters.Position);
            var model = Cache.GrabModel(Parameters.WindmillDesign.Path);
            model.Transform(transformationMatrix);
            model.Color(AssetManager.ColorCode2, World.BiomePool.GetRegion(Parameters.Position).Colors.WoodColor);
            model.Color(AssetManager.ColorCode3, WindmillWallColor(Parameters.Rng));
            model.Color(AssetManager.ColorCode1, WindmillTopColor(Parameters.Rng));           
            model.GraduateColor(Vector3.UnitY);

            var shapes = Cache.GrabShapes(Parameters.WindmillDesign.Path);
            shapes.ForEach(S => S.Transform(transformationMatrix));
            return new BuildingOutput
            {
                Model = model,
                Shapes = shapes
            };
        }

        private Vector3[] BuildFarmOffsets(Vector3 Size)
        {
            return new Vector3[]
            {
                Vector3.UnitX * Size.X * .75f + Vector3.UnitZ * Size.Z * .75f,
                Vector3.UnitX * Size.X * -.75f + Vector3.UnitZ * Size.Z * .75f,
                Vector3.UnitX * Size.X * .75f + Vector3.UnitZ * Size.Z * -.75f,
                Vector3.UnitX * Size.X * -.75f + Vector3.UnitZ * Size.Z * -.75f
            };
        }
        
        private static Vector4 WindmillTopColor(Random Rng)
        {
            switch (Rng.Next(0, 5))
            {
                case 0: return Colors.FromHtml("#506CCC");
                case 1: return Colors.FromHtml("#e9734e");
                case 2: return Colors.FromHtml("#A8524E");
                case 3: return Colors.FromHtml("#71B3BE");
                case 4: return Colors.FromHtml("#C93E6C");
                default: return new Vector4(0, 0, 0, 1);
            }
        }
        
        private static Vector4 WindmillWallColor(Random Rng)
        {
            switch (Rng.Next(0, 2))
            {
                case 0: return Colors.FromHtml("#d7d3cc");
                case 1: return Colors.FromHtml("#CCCCCC");
                default: return new Vector4(0, 0, 0, 1);
            }
        }
    }
}