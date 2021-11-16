using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.ModelHandlers
{
    public class BeetleHandler : ModelHandler
    {
        public override void Process(IEntity Entity, AnimatedUpdatableModel Model)
        {
            if (string.Equals(Entity.Type, MobType.RangedBeetle.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
                Model.Paint(Colors.FromHtml("#45659E")); /* Blue */
            else if (string.Equals(Entity.Type, MobType.MeleeBeetle.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
                Model.Paint(Colors.FromHtml("#4C9639")); /* Green */
            else if (string.Equals(Entity.Type, MobType.GiantBeetle.ToString(),
                StringComparison.InvariantCultureIgnoreCase))
                Model.Paint(Colors.FromHtml("#F24353")); /* Red */
        }
    }
}