using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hedra.Engine.EntitySystem
{
    public class SpeedBonusComponent : EntityComponent
    {
        public SpeedBonusComponent(Entity Parent, float Speed) : base(Parent)
        {
            
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
