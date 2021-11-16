using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Village : BaseStructure
    {
        private readonly List<IHumanoid> _humans;
        private readonly List<IEntity> _mobs;

        public Village(Vector3 Position) : base(Position)
        {
            _humans = new List<IHumanoid>();
            _mobs = new List<IEntity>();
        }

        public VillageGraph Graph { get; set; }
        public CollidableStructure Structure { get; set; }
    }
}