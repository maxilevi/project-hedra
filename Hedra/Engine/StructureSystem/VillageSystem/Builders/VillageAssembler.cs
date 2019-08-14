using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class VillageAssembler
    {
        private readonly VillageRoot _root;
        private readonly HouseBuilder _houseBuilder;
        private readonly FarmBuilder _farmBuilder;
        private readonly BlacksmithBuilder _blacksmithBuilder;
        private readonly StableBuilder _stableBuilder;
        private readonly MarketWellBuilder _marketWellBuilder;
        private readonly MarketBuilder _marketBuilder;
        private readonly GenericBuilderWithNPC _genericBuilder;
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
            _marketBuilder = new MarketBuilder(_structure);
            _genericBuilder = new GenericBuilderWithNPC(_structure);
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
            Design.Houses = LoopStructures(Design.Houses, _houseBuilder);
            Design.Blacksmith = LoopStructures(Design.Blacksmith, _blacksmithBuilder);
            Design.Farms = LoopStructures(Design.Farms, _farmBuilder);
            Design.Stables = LoopStructures(Design.Stables, _stableBuilder);
            Design.Generics = LoopStructures(Design.Generics, _genericBuilder);
            _designer.FinishPlacements(_structure, Design);
        }

        private List<T> LoopStructures<T>(IList<T> Parameters, params Builder<T>[] Builders) where T : IBuildingParameters
        {
            var list = Parameters.ToList();
            for (var i = 0; i < Parameters.Count; i++)
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

            var plateaus = _structure.Plateaux.Concat(new[] {_structure.Mountain}).ToArray();
            plateaus = plateaus.Select(P => P.Clone()).OrderByDescending(P => P.MaxHeight).ToArray();
            for (var i = 0; i < plateaus.Length; ++i)
            {
                plateaus[i].MaxHeight = 
                        World.WorldBuilding.ApplyMultiple(plateaus[i].Position, plateaus[i].MaxHeight, plateaus.Take(i).ToArray());
            }
            for (var i = 0; i < Parameters.Count; i++)
            {
                if(IsUnderwater(Parameters[i].Position, plateaus))
                    list.Remove(Parameters[i]);
            }
            return list;
        }

        private bool IsUnderwater(Vector3 Position, BasePlateau[] Plateaus)
        {
            return World.WorldBuilding.ApplyMultiple(
                   Position.Xz,
                   World.BiomePool.GetRegion(Position).Generation.GetHeight(Position.X, Position.Z, null, out _),
                   Plateaus
            ) < BiomePool.SeaLevel-1;
        }

        public void Build(PlacementDesign Design, CollidableStructure Structure)
        {
            LoopAndBuild(Design, Structure, Design.Markets, _marketBuilder, _marketWellBuilder);
            LoopAndBuild(Design, Structure, Design.Houses, _houseBuilder);
            LoopAndBuild(Design, Structure, Design.Blacksmith, _blacksmithBuilder);
            LoopAndBuild(Design, Structure, Design.Farms, _farmBuilder);
            LoopAndBuild(Design, Structure, Design.Stables, _stableBuilder);
            LoopAndBuild(Design, Structure, Design.Generics, _genericBuilder);
        }

        private void LoopAndBuild<T>(PlacementDesign Design, CollidableStructure Structure, IList<T> Parameters, params Builder<T>[] Builders) where T : IBuildingParameters
        {
            ((Village) Structure.WorldObject).Graph = Design.Graph;
            for (var i = 0; i < Parameters.Count; i++)
            {
                for (var j = 0; j < Builders.Length; j++)
                {
                    var output = Builders[j].Build(Parameters[i], Parameters[i].Design, _root.Cache, _rng, Design.Position);
                    var finalOutput = Builders[j].Paint(Parameters[i], output);

                    var k = j;
                    var o = i;
                    void PolishCallback() => Builders[k].Polish(Parameters[o], _root, _rng);
                    //finalOutput.Models
                    RoutineManager.StartRoutine(PlaceCoroutine, Parameters[i].Position, finalOutput.Compress(),
                        Structure, (Action) PolishCallback);
                }
            }
        }

        private IEnumerator PlaceCoroutine(object[] Arguments)
        {
            var position = (Vector3) Arguments[0];
            var buildingOutput = (CompressedBuildingOutput) Arguments[1];
            var structure = (CollidableStructure) Arguments[2];
            var callback = (Action)Arguments[3];

            var waiter = new WaitForChunk(position)
            {
                DisposeCondition = () => structure.Disposed || buildingOutput.IsEmpty,
                OnDispose = () => buildingOutput.Dispose()
            };
            while (waiter.MoveNext())
            {
                if (waiter.Disposed) yield break;
                yield return null;
            }

            var placer = buildingOutput.Place(position);
            while (placer.MoveNext()) yield return null;
            TaskScheduler.Parallel(() =>
            {
                structure.AddInstance(buildingOutput.Instances.ToArray());
                structure.AddStaticElement(buildingOutput.Models.Select(C => C.ToVertexData()).ToArray());
                structure.AddCollisionShape(buildingOutput.Shapes.ToArray());
                structure.WorldObject.AddChildren(buildingOutput.Structures.ToArray());
                callback();
            });
        }
    }
}