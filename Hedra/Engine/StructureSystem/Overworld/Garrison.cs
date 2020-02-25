using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Garrison : BaseStructure
    {
        public Garrison(List<BaseStructure> Children, List<IEntity> Npcs) : base(Children, Npcs)
        {
        }

        public Garrison(Vector3 Position) : base(Position)
        {
        }
    }
}