using System.Collections.Generic;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class Village : BaseStructure
    {
        public VillageGraph Graph { get; set; }
        private readonly List<IHumanoid> _humans;
        private readonly List<IEntity> _mobs;
        
        public Village(Vector3 Position) : base(Position)
        {
            _humans = new List<IHumanoid>();
            _mobs = new List<IEntity>();
        }

        public void AddHumanoid(IHumanoid Human)
        {
            _humans.Add(Human);
        }
        
        public void AddMob(IEntity Mob)
        {
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