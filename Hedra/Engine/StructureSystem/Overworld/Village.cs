using System;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

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

        public void AddHumanoid(IHumanoid Human)
        {
            if(_humans.Contains(Human))
                throw new ArgumentException($"This humanoid has already been added to the list.");
            _humans.Add(Human);
        }
        
        public void AddMob(IEntity Mob)
        {
            if(_mobs.Contains(Mob))
                throw new ArgumentException($"This entity has already been added to the list.");
            _mobs.Add(Mob);
        }

        public override void Dispose()
        {
            base.Dispose();
            for (var i = 0; i < _humans.Count; i++)
            {
                _humans[i].Dispose();
            }
            for (var i = 0; i < _mobs.Count; i++)
            {
                _mobs[i].Dispose();
            }
        }
    }
}