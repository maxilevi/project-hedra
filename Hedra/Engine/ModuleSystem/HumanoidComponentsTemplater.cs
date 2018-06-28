using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.ModuleSystem
{
    internal class HumanoidComponentsTemplater
    {
        private static Dictionary<string, HumanoidComponentsTemplate> _componentsTemplates;

        public HumanoidComponentsTemplater(Dictionary<string, HumanoidComponentsTemplate> TemplatesDict)
        {
            _componentsTemplates = TemplatesDict;
        }

        public HumanoidComponentsTemplate this[string Key] => _componentsTemplates[Key.ToLowerInvariant()];
        public HumanoidComponentsTemplate this[HumanType Key] => _componentsTemplates[Key.ToString().ToLowerInvariant()];
        public HumanoidComponentsTemplate this[ClassDesign Key] => _componentsTemplates[Key.Name.ToString().ToLowerInvariant()];
    }
}
