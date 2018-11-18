using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ModuleSystem.ModelHandlers
{
    public abstract class ModelHandler
    {
        private static readonly Dictionary<string, ModelHandler> Map;

        static ModelHandler()
        {
            Map = new Dictionary<string, ModelHandler>
            {
                {"Ent", new EntHandler()}
            };
        }

        public abstract void Process(IEntity Entity, AnimatedUpdatableModel Model);
        
        public static ModelHandler Build(string Name)
        {
            return Map[Name];
        }
    }
}