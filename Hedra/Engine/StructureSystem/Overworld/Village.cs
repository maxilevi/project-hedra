using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Village : BaseStructure
    {
        public VillageGraph Graph { get; set; }
        private readonly List<IHumanoid> _humans;
        private readonly List<IEntity> _mobs;
        public CollidableStructure Structure { get; set; }
        
        public Village(Vector3 Position) : base(Position)
        {
            _humans = new List<IHumanoid>();
            _mobs = new List<IEntity>();
        }
    }
}