using System;
using Hedra.Engine.Core;
using Hedra.ModelHandlers;

namespace Hedra.Engine.ModuleSystem.ModelHandlers
{
    public class ModelHandlerFactory : TypeFactory<ModelHandlerFactory>
    {
        public ModelHandler Build(string Key)
        {
            return (ModelHandler) Activator.CreateInstance(this[Key]);
        }
    }
}