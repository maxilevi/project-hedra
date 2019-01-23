using System;
using Hedra.API;
using Hedra.Engine.ClassSystem.Templates;
using Hedra.Engine.Core;
using Hedra.Engine.Player.AbilityTreeSystem;

namespace Hedra.Engine.ClassSystem
{
    public class AbilityTreeLoader : ModuleLoader<AbilityTreeLoader, AbilityTreeTemplate>
    {        
        public AbilityTreeBlueprint this[string Key]
        {
            get
            {
                if (Key == "ArcherTree")
                    return new ArcherAbilityTreeBlueprint();
                if (Key == "RogueTree")
                    return new RogueAbilityTreeBlueprint();
                if (Key == "MageTree")
                    return new MageAbilityTreeBlueprint();
                if (Key == "WarriorTree")
                    return new WarriorAbilityTreeBlueprint();
                /* Temporary workaround until the subclass system is defined. */
                throw new NotImplementedException();
                //return null;//Templates[Key.ToLowerInvariant()];
            }
        }

        protected override string FolderPrefix => "Classes/Trees";
    }
}