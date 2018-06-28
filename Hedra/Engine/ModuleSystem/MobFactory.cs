using System;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.AISystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.ModuleSystem
{
    internal class MobFactory
    {
        private readonly Dictionary<string, IEnemyFactory> _factories;

        public MobFactory()
        {
            _factories = new Dictionary<string, IEnemyFactory>();
        }

        public void AddFactory(params IEnemyFactory[] Factory)
        {
            foreach (IEnemyFactory factory in Factory)
            {
                _factories.Add(factory.Name.ToLowerInvariant(), factory);
            }   
        }

        public void Empty()
        {
            lock(_factories)
                _factories.Clear();
        }

        public bool ContainsFactory(string Type)
        {
            return _factories.ContainsKey(Type.ToLowerInvariant());
        }

        public Entity Build(MobType Type, int Seed)
        {
            return this.Build(Type.ToString(), Seed);
        }

        public Entity Build(string Type, int Seed)
        {
            var mob = new Entity();
            var rng = new Random(Seed);

            mob.Type = _factories[Type.ToLowerInvariant()].Name;

            var barComponent = new HealthBarComponent(mob);
            mob.AddComponent(barComponent);

            int levelN = rng.Next(0, 10);
            int mobDifficulty = 1;
            if (levelN <= 4) mobDifficulty = 1;
            else if (levelN > 4 && levelN <= 7) mobDifficulty = 2;
            else if (levelN > 7 && levelN <= 9) mobDifficulty = 3;
            var mobDifficultyModifier = Math.Min(1f, mobDifficulty * .75f);

            barComponent.FontColor = mobDifficulty == 1 ? Color.White : mobDifficulty == 3 ? Color.Red : Color.Gold;

            _factories[Type.ToLowerInvariant()].Apply(mob);

            var ai = mob.SearchComponent<BasicAIComponent>();
            if (ai == null) throw new ArgumentException("No AIComponent has been set");

            var dmg = mob.SearchComponent<DamageComponent>();
            if (dmg == null) throw new ArgumentException("No DamageComponent has been set");

            mob.MaxHealth = (GameManager.Player.Level * 1.75f + mob.MaxHealth) * mobDifficultyModifier;
            mob.AttackDamage = mob.AttackDamage * mobDifficultyModifier + GameManager.Player.Level * 0.5f;
            dmg.XpToGive = dmg.XpToGive * mobDifficultyModifier; 
            mob.Health = mob.MaxHealth;
            return mob;
        }
    }
}
