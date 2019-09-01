using System.Collections.Generic;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class FishingPost : BaseStructure
    {
        public List<IHumanoid> Fishermans { get; }
        public FishingPost(Vector3 Position) : base(Position)
        {
            Fishermans = new List<IHumanoid>();
        }

        public override void Dispose()
        {
            base.Dispose();
            for (var i = 0; i < Fishermans.Count; ++i)
                Fishermans[i].Dispose();
        }
    }
}