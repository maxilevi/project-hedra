using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.ModelHandlers
{
    public class GhostHandler : ModelHandler
    {
        private static readonly GhostFactory Factory;

        static GhostHandler()
        {
            Factory = new GhostFactory();
        }
        
        public override void Process(IEntity Entity, AnimatedUpdatableModel Model)
        {
            Factory.Build(Model);
        }
    }
}