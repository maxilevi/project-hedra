using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class FireWeaknessComponent : EntityComponent
    {
        public FireWeaknessComponent(IEntity Entity) : base(Entity)
        {
        }

        public override void Update()
        {
        }

        public int Power => 4;
    }
}