using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.API;
using Hedra.Engine.ClassSystem.Templates;
using Hedra.Engine.Core;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.ClassSystem
{
    public class AbilityTreeLoader : ModuleLoader<AbilityTreeLoader, AbilityTreeTemplate>
    {
        private readonly Dictionary<string, uint> _icons = new Dictionary<string, uint>();
        
        public AbilityTreeBlueprint this[string Key]
        {
            get
            {
                var template = Templates[Key.ToLowerInvariant()];
                if (!_icons.ContainsKey(Key))
                    _icons.Add(Key, Graphics2D.LoadFromAssets(template.Icon));
                
                return new AbilityTreeBlueprint(template)
                {
                    Icon = _icons[Key]
                };
            }
        }

        public string[] Names => Templates.Keys.ToArray();
        
        protected override string FolderPrefix => "SkillTrees";
    }
}