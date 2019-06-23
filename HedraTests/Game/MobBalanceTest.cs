using System;
using System.Collections;
using System.Collections.Generic;
using Hedra;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.ModuleSystem;
using Hedra.EntitySystem;
using Hedra.Game;
using HedraTests.Player;
using NUnit.Framework;

namespace HedraTests.Game
{
    [TestFixture]
    public class MobBalanceTest : BaseTest
    {
        private static bool _loaded;
        private readonly MobBalanceSheet _sheet = new MobBalanceSheet();

        [TestCaseSource(nameof(Mobs))]
        public void TestMobXp(IEntity Mob)
        {
            AssertComplies(Mob, _sheet.MobXp, () => Mob.SearchComponent<DamageComponent>().XpToGive);
        }
        
        [TestCaseSource(nameof(Mobs))]
        public void TestMobHealth(IEntity Mob)
        {
            AssertComplies(Mob, _sheet.MobHealth, () => Mob.MaxHealth, 100);
        }

        [TestCaseSource(nameof(Mobs))]
        public void TestMobDamage(IEntity Mob)
        {
            AssertComplies(Mob, _sheet.MobAttackDamage, () => Mob.AttackDamage);
        }

        [TestCaseSource(nameof(Mobs))]
        public void TestMobAttackCooldown(IEntity Mob)
        {
            AssertComplies(Mob, _sheet.MobAttackCooldown, () => Mob.AttackCooldown);
        }

        [TestCaseSource(nameof(Mobs))]
        public void TestMobSpeed(IEntity Mob)
        {
            AssertComplies(Mob, _sheet.MobSpeed, () => Mob.Speed);
        }
        
        [TestCaseSource(nameof(Mobs))]
        public void TestMobLevelIsNotZero(IEntity Mob)
        {
            Assert.Greater(Mob.Level, 0);
            Assert.Pass($"'{Mob.Name}' has level {Mob.Level}.");
        }

        private void AssertComplies(IEntity Mob, UniqueBalanceEntry Entry, Func<float> Lambda, float Base = 0)
        {
            var multiplier = Entry.ScaleWithLevel ? Mob.Level : 1;
            var val = Lambda();
            var max = Entry.Max * multiplier + Base;
            var min = Entry.Min * multiplier + Base;
            var msg =
                $"{Mob.Name} with level '{Mob.Level}' should have {min} < {val} < {max}";
            Assert.GreaterOrEqual(max, val, msg);
            Assert.LessOrEqual(min, val, msg);
            Assert.Pass(msg);
        }

        /* Called via reflection by NUnit */
        private static IEnumerable<IEntity> Mobs()
        {
            BaseTest.MockEngine();
            GameManager.Player = new PlayerMock();
            if (!_loaded)
            {
                HedraContent.Register();
                _loaded = true;
            }
            var factories = MobLoader.LoadModules(GameLoader.AppPath);
            for (var i = 0; i < factories.Length; i++)
            {
                var mob = new SkilledAnimableEntity
                {
                    Type = factories[i].Name
                };
                factories[i].Apply(mob, false);
                yield return mob;
            }
        }     
    }
}