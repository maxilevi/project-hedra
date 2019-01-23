using Hedra.API;
using Hedra.Engine.ClassSystem.Templates;
using Hedra.Engine.Core;

namespace Hedra.Engine.ClassSystem
{
    public class ClassLoader : ModuleLoader<ClassLoader, ClassTemplate>
    {        
        public ClassTemplate this[Class Key] => Templates[Key.ToString().ToLowerInvariant()];

        protected override string FolderPrefix => "Classes";
    }
}