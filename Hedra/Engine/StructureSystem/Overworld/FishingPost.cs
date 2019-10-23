using System.Collections.Generic;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class FishingPost : BaseStructure
    {
        public FishingPost(Vector3 Position) : base(Position)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}