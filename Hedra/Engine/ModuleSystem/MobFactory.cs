using System;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.ModuleSystem
{
    public class MobFactory
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
            return _factories.ContainsKey(Type);
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

            barComponent.FontColor = mobDifficulty == 1 ? Color.White : mobDifficulty == 3 ? Color.Red : Color.Gold;

            _factories[Type.ToLowerInvariant()].Apply(mob);

            var ai = mob.SearchComponent<AIComponent>();
            if (ai == null) throw new ArgumentException("No AIComponent has been set");

            var dmg = mob.SearchComponent<DamageComponent>();
            if (dmg == null) throw new ArgumentException("No DamageComponent has been set");

            mob.AttackDamage *= mobDifficulty;

            dmg.XpToGive *= mobDifficulty;
            dmg.OnDamageEvent += delegate(DamageEventArgs Args)
            {
                if (Args.Damager != null)
                {
                    ai.OldLogic = ai.AILogic;
                    ai.AILogic = () => ai.Attack(Args.Damager, true);
                    ai.FollowTimer.Reset();
                }
            };

            mob.MaxHealth *= mobDifficulty;
            mob.Health = mob.MaxHealth;

            return mob;
        }
    }
}
