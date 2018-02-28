using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public HumanoidTemplate this[Class Key] => _humanTemplates[Key.ToString().ToLowerInvariant()];
    }
}
