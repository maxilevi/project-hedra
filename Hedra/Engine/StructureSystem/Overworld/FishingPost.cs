using System.Collections.Generic;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenToolkit.Mathematics;

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