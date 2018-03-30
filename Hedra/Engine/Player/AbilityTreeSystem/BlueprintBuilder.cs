using System;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public static class BlueprintBuilder
    {
        public static AbilityTreeBlueprint Build(Class Type)
        {
            switch (Type)
            {
                case Class.Archer:
                    return new ArcherAbilityTreeBlueprint();

                case Class.Rogue:
                    return new RogueAbilityTreeBlueprint();

                case Class.Warrior:
                    return new WarriorAbilityTreeBlueprint();            
                
                default:
                    throw new ArgumentException($"Blueprint for class {Type.ToString().ToUpperInvariant()} could not be found.");
            }
        }
    }
}
