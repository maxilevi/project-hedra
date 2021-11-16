using System.Collections.Generic;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;

namespace Hedra.Engine.ModuleSystem
{
    public class HumanoidTemplater
    {
        private static Dictionary<string, HumanoidTemplate> _humanTemplates;

        public HumanoidTemplater(Dictionary<string, HumanoidTemplate> TemplatesDict)
        {
            _humanTemplates = TemplatesDict;
        }

        public HumanoidTemplate this[string Key] => _humanTemplates[Key.ToLowerInvariant()];
        public HumanoidTemplate this[HumanType Key] => _humanTemplates[Key.ToString().ToLowerInvariant()];
        public HumanoidTemplate this[ClassDesign Key] => _humanTemplates[Key.ToString().ToLowerInvariant()];
    }
}