using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class VillageAssembler
    {
        private readonly VillageRoot _root;
        private readonly HouseBuilder _houseBuilder;
        private readonly NeighbourhoodWellBuilder _neighbourHoodWellBuilder;
        private readonly FarmBuilder _farmBuilder;
        private readonly BlacksmithBuilder _blacksmithBuilder;
        private readonly StableBuilder _stableBuilder;
        private readonly MarketWellBuilder _marketWellBuilder;
        private readonly MarketBuilder _marketBuilder;
        private readonly IPlacementDesigner _designer;
        private readonly Random _rng;
        private readonly CollidableStructure _structure;

        public VillageAssembler(CollidableStructure Structure, VillageRoot Root, Random Rng)
        {
            _structure = Structure;
            _houseBuilder = new HouseBuilder(_structure);
            _farmBuilder = new FarmBuilder(_structure);
            _blacksmithBuilder = new BlacksmithBuilder(_structure);
            _stableBuilder = new StableBuilder(_structure);
            _marketWellBuilder = new MarketWellBuilder(_structure);
            _neighbourHoodWellBuilder = new NeighbourhoodWellBuilder(_structure);
            _marketBuilder = new MarketBuilder(_structure);
            _root = Root;
            _rng = Rng;
            Size = VillageDesign.MaxVillageSize;//Rng.Next(VillageDesign.MinVillageSize, VillageDesign.MaxVillageSize);
            _designer = new GridPlacementDesigner(_root, new VillageConfiguration
            {
                Size = Size
            }, Rng);
        }

        public int Size { get; }

        public PlacementDesign DesignVillage()
        {
            return _designer.CreateDesign();
        }
        
        public void PlaceGroundwork(PlacementDesign Design)
        {
            Design.Markets = LoopStructures(Design.Markets, _marketBuilder, _marketWellBuilder);
            Design.Houses = LoopStructures(Design.Houses, _houseBuilder, _neighbourHoodWellBuilder);
            Design.Blacksmith = LoopStructures(Design.Blacksmith, _blacksmithBuilder);
            Design.Farms = LoopStructures(Design.Farms, _farmBuilder);
            Design.Stables = LoopStructures(Design.Stables, _stableBuilder);
            _designer.FinishPlacements(_structure, Design);
        }

        private List<T> LoopStructures<T>(IList<T> Parameters, params Builder<T>[] Builders) where T : IBuildingParameters
        {
            var list = Parameters.ToList();
            for (var i = 0; i < Parameters.Count; i++)
            {
                for(var j = 0; j < Builders.Length; j++)
                {
                    if (IsUnderwater(Parameters[i].Position) || !Builders[j].Place(Parameters[i], _root.Cache))
                    {
                        list.Remove(Parameters[i]);
                        break;
                    }
                }
            }
            return list;
        }

        private bool IsUnderwater(Vector3 Position)
        {
            return World.BiomePool.GetRegion(Position).Generation.GetHeight(Position.X, Position.Z, null, out _) < BiomePool.SeaLevel
                && !_structure.Mountain.Collides(Position.Xz);
        }

        public void Build(PlacementDesign Design, CollidableStructure Structure)
        {
            var parameters = new IBuildingParameters[][]
            {
                Design.Houses.ToArray(),
                Design.Farms.ToArray(),
                Design.Blacksmith.ToArray(),
                Design.Stables.ToArray(),
                Design.Markets.ToArray(),
                Design.Markets.ToArray()
            };
            var radius = 0f;
            var builders = new object[] { _houseBuilder, _farmBuilder, _blacksmithBuilder, _stableBuilder, _marketWellBuilder, _marketBuilder};
            for (var i = 0; i < builders.Length; i++)
            {
                for (var j = 0; j < parameters[i].Length; j++)
                {
                    var build = builders[i].GetType().GetMethod(nameof(Builder<IBuildingParameters>.Build));
                    var output = (BuildingOutput) build.Invoke(builders[i], new object[] { parameters[i][j], parameters[i][j].Design, _root.Cache, _rng, Design.Position });

                    var paint = builders[i].GetType().GetMethod(nameof(Builder<IBuildingParameters>.Paint));
                    var finalOutput = (BuildingOutput) paint.Invoke(builders[i], new object[] { parameters[i][j], output });
                    CoroutineManager.StartCoroutine(PlaceCoroutine, parameters[i][j].Position, finalOutput.Compress(), Structure);

                    var polish = builders[i].GetType().GetMethod(nameof(Builder<IBuildingParameters>.Polish));
                    polish.Invoke(builders[i], new object[] { parameters[i][j], _rng });
                }
            }
        }

        private IEnumerator PlaceCoroutine(object[] Arguments)
        {
            var position = (Vector3) Arguments[0];
            var buildingOutput = (CompressedBuildingOutput) Arguments[1];
            var structure = (CollidableStructure) Arguments[2];

            var waiter = new WaitForChunk(position)
            {
                DisposeCondition = () => structure.Disposed || buildingOutput.IsEmpty,
                OnDispose = () => buildingOutput.Dispose()
            };
            while (waiter.MoveNext()) yield return null;            
            if (waiter.Disposed) yield break;

            var placer = buildingOutput.Place(position);
            while (placer.MoveNext()) yield return null;
            structure.AddInstance(buildingOutput.Instances.ToArray());
            structure.AddStaticElement(buildingOutput.Models.Select(C => C.ToVertexData()).ToArray());
            structure.AddCollisionShape(buildingOutput.Shapes.ToArray());
            structure.WorldObject.AddChildren(buildingOutput.Structures.ToArray());
        }
    }
}