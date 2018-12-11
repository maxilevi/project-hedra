using System.Collections.Generic;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.ModelHandlers
{
    public abstract class ModelHandler
    {
        public abstract void Process(IEntity Entity, AnimatedUpdatableModel Model);
    }
}