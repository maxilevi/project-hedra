using System;
using System.Collections.Generic;
using System.Drawing;
using Hedra.AISystem;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Localization;

namespace Hedra.Engine.ModuleSystem
{
    public class MobFactory
    {
        private readonly Dictionary<string, IEnemyFactory> _factories;
        private readonly object _lock;

        public MobFactory()
        {
            _factories = new Dictionary<string, IEnemyFactory>();
            _lock = new object();
        }

        public void AddFactory(params IEnemyFactory[] Factory)
        {
            lock (_lock)
            {
                foreach (var factory in Factory)
                {
                    _factories.Add(factory.Name.ToLowerInvariant(), factory);
                }
            }
        }

        public void Empty()
        {
            lock (_lock)
            {
                _factories.Clear();
            }
        }

        public bool ContainsFactory(string Type)
        {
            lock (_lock)
            {
                return _factories.ContainsKey(Type.ToLowerInvariant());
            }
        }

        public IEnemyFactory GetFactory(string Type)
        {
            lock (_lock)
                return _factories[Type.ToLowerInvariant()];
        }

        public SkilledAnimableEntity Build(string Type, int Seed)
        {
            var mob = new SkilledAnimableEntity();

            lock (_lock)
            {
                mob.Type = _factories[Type.ToLowerInvariant()].Name;
                _factories[Type.ToLowerInvariant()].Apply(mob);
            }

            var ai = mob.SearchComponent<BasicAIComponent>();
            if (ai == null) throw new ArgumentException("No AIComponent has been set");

            var dmg = mob.SearchComponent<DamageComponent>();
            if (dmg == null) throw new ArgumentException("No DamageComponent has been set");
            
            var mobDifficulty = GetMobDifficulty(new Random(Seed));
            var mobDifficultyModifier = GetMobDifficultyModifier(mobDifficulty);
            var mobName = Translations.Has(mob.Type.ToLowerInvariant()) ? Translations.Get(mob.Type.ToLowerInvariant()) : mob.Type.AddSpacesToSentence(true);
            if(!Translations.Has(mob.Type.ToLowerInvariant()))
                Log.WriteLine($"Failed to find translation for mob '{mob.Type}', using name as default.");
            
            var barComponent = new HealthBarComponent(
                mob,
                mobName,
                BarTypeFromAIType(ai.Type),
                mobDifficulty == 1 ? Color.White : mobDifficulty == 3 ? Color.Red : Color.Gold
            );
            mob.AddComponent(barComponent);
            
            mob.MaxHealth = mob.MaxHealth * mobDifficultyModifier;
            dmg.XpToGive = dmg.XpToGive * mobDifficultyModifier; 
            mob.Health = mob.MaxHealth;
            return mob;
        }

        public void Polish(Entity Mob)
        {
            lock (_lock)
            {
                _factories[Mob.Type.ToLowerInvariant()].Polish(Mob);
            }
        }

        private static HealthBarType BarTypeFromAIType(AIType AI)
        {
            return AI == AIType.Friendly
                ? HealthBarType.Friendly
                : AI == AIType.Hostile
                    ? HealthBarType.Hostile
                    : HealthBarType.Neutral;
        }
        
        private static int GetMobDifficulty(Random Rng)
        {
            var levelN = Rng.Next(0, 10);
            var mobDifficulty = 1;
            if (levelN <= 4) return 1;
            if (levelN > 4 && levelN <= 7) return 2;
            if (levelN > 7 && levelN <= 9) return 3;
            throw new ArgumentOutOfRangeException($"Rng is not 0 < {levelN} < 10");
        }

        private float GetMobDifficultyModifier(int DifficultyLevel)
        {
            switch (DifficultyLevel)
            {
                case 1:
                    return 1;
                case 2:
                    return 1.25f;
                case 3:
                    return 1.5f;
                default:
                    throw new ArgumentOutOfRangeException($"Mob difficulty level is not 1 <= {DifficultyLevel} <= 2");
            }
        }
    }
}
