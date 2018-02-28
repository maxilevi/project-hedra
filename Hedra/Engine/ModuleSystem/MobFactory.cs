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

        public Entity Build(MobType Type, int Seed)
        {
            return this.Build(Type.ToString(), Seed);
        }

        public Entity Build(string Type, int Seed)
        {
            var mob = new Entity();
            var rng = new Random(Seed);

            mob.Type = _factories[Type.ToLowerInvariant()].Name;

            var barComponent = new HealthBarComponent(mob)
            {
                DistanceFromBase = (Math.Abs(mob.DefaultBox.Min.Y) + Math.Abs(mob.DefaultBox.Max.Y)) * .75f
            };
            mob.AddComponent(barComponent);

            int levelN = rng.Next(0, 10);
            if (levelN <= 4) mob.Level = 1;
            else if (levelN > 4 && levelN <= 7) mob.Level = 2;
            else if (levelN > 7 && levelN <= 9) mob.Level = 3;

            barComponent.FontColor = mob.Level == 1 ? Color.White : mob.Level == 2 ? Color.Gold : Color.Red;
            mob.Level = (int) (LocalPlayer.Instance.Level * .2f + mob.Level);

            _factories[Type.ToLowerInvariant()].Apply(mob);

            var ai = mob.SearchComponent<AIComponent>();
            if (ai == null) throw new ArgumentException("No AIComponent has been set");

            var dmg = mob.SearchComponent<DamageComponent>();
            if (dmg == null) throw new ArgumentException("No DamageComponent has been set");

            mob.AttackDamage *= Math.Max(mob.Level * .35f, 1);

            dmg.XpToGive = Math.Max(1, (float) Math.Round(dmg.XpToGive * mob.Level * .75f));
            dmg.OnDamageEvent += delegate(DamageEventArgs Args)
            {
                if (Args.Damager != null)
                {
                    ai.OldLogic = ai.AILogic;
                    ai.AILogic = () => ai.Attack(Args.Damager, true);
                    ai.FollowTimer.Reset();
                }
            };

            mob.MaxHealth += mob.MaxHealth * mob.Level * .45f;
            mob.Health = mob.MaxHealth;
            

            return mob;
        }
    }
}
