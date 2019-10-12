using System;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Engine.WorldBuilding;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class MarketWellBuilder : Builder<MarketParameters>
    {
        public MarketWellBuilder(CollidableStructure Structure) : base(Structure)
        {
        }
        
        public override bool Place(MarketParameters Parameters, VillageCache Cache)
        {
            return PlaceGroundwork(Parameters.Position, Parameters.WellSize);
        }

        public override void Polish(MarketParameters Parameters, VillageRoot Root, Random Rng)
        {
            base.Polish(Parameters, Root, Rng);
            var offset = (Physics.HeightAtPosition(Parameters.Position) + 4) * Vector3.UnitY;
            DecorationsPlacer.PlaceWhenWorldReady(Parameters.Position + offset,
                P => Structure.WorldObject.AddChildren(new Well(P, MarketParameters.MarketSize)),
            () => Structure.Disposed);
        }
    }
}