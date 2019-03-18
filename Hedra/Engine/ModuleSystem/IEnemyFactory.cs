using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.ModuleSystem
{
    public interface IEnemyFactory
    {
        string Name { get; set; }
        float MaxHealth { get; }
        float AttackDamage { get; }

        void Apply(Entity Mob, bool NormalizeValues = true);

        void Polish(Entity Mob);
    }
}