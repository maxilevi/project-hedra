using Hedra.AISystem;
using Hedra.Engine.Generation;
using Hedra.EntitySystem;
using Hedra.Game;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class DungeonDualAIComponent : DualAIComponent
    {
        private readonly Dungeon0 _structure;
        public DungeonDualAIComponent(IEntity Parent, IComponent<IEntity> AI1, IComponent<IEntity> AI2, CollidableStructure Structure) : base(Parent, AI1, AI2)
        {
            _structure = (Dungeon0)Structure.WorldObject;
        }
            
        public override void Update()
        {
            base.Update();
            if(Parent.Distance(_structure.BuildingTrigger.Position) > 16) return;
            if(_structure.BuildingTrigger.IsInside(GameManager.Player))
                SwitchOne();
            else
                SwitchTwo();
        }
    }
}