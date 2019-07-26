using System;
using Hedra.API;
using Hedra.Engine.ClassSystem.Templates;
using Hedra.Engine.Core;

namespace Hedra.Engine.ClassSystem
{
    public class ClassLoader : ModuleLoader<Class, ClassLoader, ClassTemplate>
    {        
        public ClassTemplate this[Class Key] => Templates[Key];

        protected override Class ParseKey(string Name)
        {
            return (Class) Enum.Parse(typeof(Class), Name);
        }

        protected override string FolderPrefix => "Classes";
    }
}