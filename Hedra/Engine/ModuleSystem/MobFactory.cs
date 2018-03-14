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
            float mobDifficulty = 1f;
            if (levelN <= 4) mobDifficulty = 1f;
            else if (levelN > 4 && levelN <= 7) mobDifficulty = 1.5f;
            else if (levelN > 7 && levelN <= 9) mobDifficulty = 2f;

            barComponent.FontColor = mobDifficulty == 1 ? Color.White : mobDifficulty == 2 ? Color.Red : Color.Gold;
            mob.Level = (int) Math.Max(1, LocalPlayer.Instance.Level * .4f * mobDifficulty);

            _factories[Type.ToLowerInvariant()].Apply(mob);

            var ai = mob.SearchComponent<AIComponent>();
            if (ai == null) throw new ArgumentException("No AIComponent has been set");

            var dmg = mob.SearchComponent<DamageComponent>();
            if (dmg == null) throw new ArgumentException("No DamageComponent has been set");

            mob.AttackDamage *= Math.Max(mob.Level * .35f, 1);

            dmg.XpToGive = Math.Max(3f, (float) Math.Round(dmg.XpToGive * mob.Level * .65f));
            dmg.OnDamageEvent += delegate(DamageEventArgs Args)
            {
                if (Args.Damager != null)
                {
                    ai.OldLogic = ai.AILogic;
                    ai.AILogic = () => ai.Attack(Args.Damager, true);
                    ai.FollowTimer.Reset();
                }
            };

            mob.MaxHealth += mob.MaxHealth * mob.Level * .5f;
            mob.Health = mob.MaxHealth;

            return mob;
        }
    }
}
