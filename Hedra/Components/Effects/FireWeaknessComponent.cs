using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class FireWeaknessComponent : EntityComponent
    {
        public FireWeaknessComponent(IEntity Entity) : base(Entity)
        {
        }

        public int Power => 6;

        public override void Update()
        {
        }
    }
}