using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Player;

namespace Hedra.Engine.ModuleSystem
{
    public class HumanoidModelTemplater
    {
        private static Dictionary<string, HumanoidModelTemplate> _modelTemplates;

        public HumanoidModelTemplater(Dictionary<string, HumanoidModelTemplate> TemplatesDict)
        {
            _modelTemplates = TemplatesDict;
        }

        public HumanoidModelTemplate this[string Key] => _modelTemplates[Key.ToLowerInvariant()];
        public HumanoidModelTemplate this[HumanType Key] => _modelTemplates[Key.ToString().ToLowerInvariant()];
        public HumanoidModelTemplate this[Class Key] => _modelTemplates[Key.ToString().ToLowerInvariant()];
    }
}
