using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.ModuleSystem
{
    public interface IEnemyFactory
    {
        string Name { get; set; }
        string DisplayName { get; set; }
        float MaxHealth { get; }
        float AttackDamage { get; }
        float XP { get; }

        void Apply(SkilledAnimableEntity Mob, bool NormalizeValues = true);

        void Polish(Entity Mob);
    }
}