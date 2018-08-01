﻿using System;
using System.Collections;
using System.Linq;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class VillageBuilder
    {
        private readonly VillageRoot _root;
        private readonly HouseBuilder _houseBuilder;
        private readonly FarmBuilder _farmBuilder;
        private readonly BlacksmithBuilder _blacksmithBuilder;
        private readonly StableBuilder _stableBuilder;
        private readonly WellBuilder _wellBuilder;
        private readonly MarketBuilder _marketBuilder;
        private readonly PlacementDesigner _designer;
        private readonly Random _rng;

        public VillageBuilder(VillageRoot Root, Random Rng)
        {
            _houseBuilder = new HouseBuilder();
            _farmBuilder = new FarmBuilder();
            _blacksmithBuilder = new BlacksmithBuilder();
            _stableBuilder = new StableBuilder();
            _wellBuilder = new WellBuilder();
            _marketBuilder = new MarketBuilder();
            _root = Root;
            _rng = Rng;
            _designer = new PlacementDesigner(_root, new VillageConfiguration(), Rng);
        }

        public PlacementDesign DesignVillage()
        {
            return _designer.CreateDesign();
        }
        
        public void PlaceGroundwork(PlacementDesign Design)
        {
            Design.Markets = LoopStructures(Design.Markets, _marketBuilder, _wellBuilder);
            Design.Houses = LoopStructures(Design.Houses, _houseBuilder);
            Design.Blacksmith = LoopStructures(Design.Blacksmith, _blacksmithBuilder);
            Design.Farms = LoopStructures(Design.Farms, _farmBuilder);
            Design.Stables = LoopStructures(Design.Stables, _stableBuilder);
            _designer.BuildPaths(Design);
        }

        private T[] LoopStructures<T>(T[] Parameters, params Builder<T>[] Builders) where T : IBuildingParameters
        {
            var list = Parameters.ToList();
            for (var i = 0; i < Parameters.Length; i++)
            {
                for(var j = 0; j < Builders.Length; j++)
                {
                    if (!Builders[j].Place(Parameters[i], _root.Cache))
                    {
                        list.Remove(Parameters[i]);
                        break;
                    }
                }
            }
            return list.ToArray();
        }

        public void Build(PlacementDesign Design)
        {
            var parameters = new IBuildingParameters[][]
            {
                Design.Houses,
                Design.Farms,
                Design.Blacksmith,
                Design.Stables,
                Design.Markets,
                Design.Markets
            };
            var builders = new object[] { _houseBuilder, _farmBuilder, _blacksmithBuilder, _stableBuilder, _wellBuilder, _marketBuilder};
            for (var i = 0; i < builders.Length; i++)
            {
                for (var j = 0; j < parameters[i].Length; j++)
                {
                    var build = builders[i].GetType().GetMethod("Build");
                    var output = (BuildingOutput) build.Invoke(builders[i], new object[] { parameters[i][j], _root.Cache, _rng, Design.Position });

                    var paint = builders[i].GetType().GetMethod("Paint");
                    var finalOutput = (BuildingOutput) paint.Invoke(builders[i], new object[] { parameters[i][j], output });
                    CoroutineManager.StartCoroutine(PlaceCoroutine, parameters[i][j].Position, finalOutput);

                    var polish = builders[i].GetType().GetMethod("Polish");
                    polish.Invoke(builders[i], new object[] { parameters[i][j] });
                }
            }
        }

        private IEnumerator PlaceCoroutine(object[] Arguments)
        {
            var position = (Vector3) Arguments[0];
            var buildingOutput = (BuildingOutput) Arguments[1];
            var model = buildingOutput.Model;
            var shapes = buildingOutput.Shapes;
            
            var underChunk = World.GetChunkAt(position);
            var currentSeed = World.Seed;
            while(underChunk == null || !underChunk.BuildedWithStructures)
            {
                if(World.Seed != currentSeed) yield break;
                underChunk = World.GetChunkAt(position);
                yield return null;
            }
            var height = Physics.HeightAtPosition(position);
            var transMatrix = Matrix4.CreateTranslation(Vector3.UnitY * height);
            model.Transform(transMatrix);
            shapes.ForEach(S => S.Transform(transMatrix));
            underChunk.Blocked = true;
            underChunk.AddStaticElement(model);
            underChunk.AddCollisionShape(shapes.ToArray());
        }
    }
}