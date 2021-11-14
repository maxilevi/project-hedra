using System;
using System.Collections.Generic;
using Hedra;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Sound;
using NUnit.Framework;

namespace HedraTests.Game
{
    [TestFixture]
    [Ignore("HumanoidBalanceTest is not useful for detecting bugs, it only causes errors")]
    public class HumanoidBalanceTest : BaseTest
    {
        private static bool _loaded;
        private int RandomLevel => Utils.Rng.Next(0, Humanoid.MaxLevel) + 1;
        private readonly HumanoidBalanceSheet _sheet = new HumanoidBalanceSheet();
        private Humanoid _human;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _human = new Humanoid();
            _human.RandomFactor = Humanoid.NewRandomFactor();
            _human.Model = new HumanoidModel(_human, new HumanoidModelTemplate
            {
                Path = string.Empty,
                Scale = 0
            });
        }

        [Test]
        public void TestHumanoidXpProgression()
        {
            TestWithinRange(_sheet.HumanoidXp, () => _human.MaxXP, 1, ClassDesign.None, 1, 50, 98);
        }

        [TestCaseSource(nameof(TestClasses))]
        public void TestHumanoidAttackDamageProgression(ClassDesign Design)
        {
            TestWithinRange(_sheet.BaseHumanoidDamage, () => _human.DamageEquation, 1, Design);
        }
        
        [TestCaseSource(nameof(RandomWeapons))]
        public void TestHumanoidAttackSpeedWithRandomWeapons(Item TestItem)
        {
            _human.MainWeapon = TestItem;
            TestContext.WriteLine($"Using item {TestItem.Name}");
            TestWithinRange(_sheet.HumanoidAttackSpeed, () => _human.AttackSpeed, 1, ClassDesign.None);
        }
        
        [TestCaseSource(nameof(BadWeapons))]
        public void TestHumanoidAttackDamageProgressionWithBadWeapons(Item TestItem)
        {
            _human.MainWeapon = TestItem;
            TestContext.WriteLine($"Using item {TestItem.Name}");
            TestWithinRange(_sheet.HumanoidDamageWithCommonWeapons, () => _human.UnRandomizedDamageEquation, 1, ClassDesign.None);
        }
        
        [TestCaseSource(nameof(NormalWeapons))]
        public void TestHumanoidAttackDamageProgressionWithNormalWeapons(Item TestItem)
        {
            _human.MainWeapon = TestItem;
            TestContext.WriteLine($"Using item {TestItem.Name}");
            TestWithinRange(_sheet.HumanoidDamageWithNormalWeapons, () => _human.UnRandomizedDamageEquation, 1, ClassDesign.None);
        }
        
        [TestCaseSource(nameof(GoodWeapons))]
        public void TestHumanoidAttackDamageProgressionWithGoodWeapons(Item TestItem)
        {
            _human.MainWeapon = TestItem;
            TestContext.WriteLine($"Using item {TestItem.Name}");
            TestWithinRange(_sheet.HumanoidDamageWithBestWeapons, () => _human.UnRandomizedDamageEquation, 1, ClassDesign.None);
        }

        [TestCaseSource(nameof(TestClasses))]
        public void TestHumanoidHealthProgression(ClassDesign Design)
        {
            TestWithinRange(_sheet.HumanoidHealth, () => _human.MaxHealth, .0f, Design);
            TestWithinRange(_sheet.HumanoidHealth, () => _human.MaxHealth, .5f, Design);
            TestWithinRange(_sheet.HumanoidHealth, () => _human.MaxHealth, 1f, Design);
        }

        [TestCaseSource(nameof(TestClasses))]
        public void TestHumanoidManaProgression(ClassDesign Design)
        {
            TestWithinRange(_sheet.HumanoidMana, () => _human.MaxMana, .0f, Design);
            TestWithinRange(_sheet.HumanoidMana, () => _human.MaxMana, .5f, Design);
            TestWithinRange(_sheet.HumanoidMana, () => _human.MaxMana, 1f, Design);
        }
        
        [TestCaseSource(nameof(TestClasses))]
        public void TestHumanoidAttackResistance(ClassDesign Design)
        {
            TestWithinRange(_sheet.HumanoidResistance, () => _human.AttackResistance, 1, Design);
        }
        
        [TestCaseSource(nameof(TestClasses))]
        public void TestHumanoidSpeed(ClassDesign Design)
        {
            TestWithinRange(_sheet.HumanoidSpeed, () => _human.Speed, 1, Design);
        }
        
        [TestCaseSource(nameof(TestClasses))]
        public void TestHumanoidStamina(ClassDesign Design)
        {
            TestWithinRange(_sheet.HumanoidStamina, () => _human.Stamina, 1, Design);
        }

        private void TestWithinRange(BalanceEntry Entry, Func<float> Lambda, float RandomFactor, ClassDesign Design)
        {
            TestWithinRange(Entry, Lambda, RandomFactor, Design, 1, 50, 99);
        }

        private void TestWithinRange(BalanceEntry Entry, Func<float> Lambda, float RandomFactor,
            ClassDesign Class, params int[] Levels)
        {
            TestContext.WriteLine($"RandomFactor = {RandomFactor}");
            _human.RandomFactor = RandomFactor;
            var humanClass = Class;
            _human.Class = humanClass;
            _human.Level = Levels[0];
            var val = Lambda();
            var msg =
                $"{humanClass.Name} with level '{_human.Level}' should have {Entry.Min1} < {val} < {Entry.Max1}";
            Assert.GreaterOrEqual(Entry.Max1, val, msg);
            Assert.LessOrEqual(Entry.Min1, val, msg);
            TestContext.WriteLine(msg);

            _human.Level = Levels[1];
            val = Lambda();
            msg =
                $"{humanClass.Name} with level '{_human.Level}' should have {Entry.Min50} < {val} < {Entry.Max50}";
            Assert.GreaterOrEqual(Entry.Max50, val, msg);
            Assert.LessOrEqual(Entry.Min50, val, msg);
            TestContext.WriteLine(msg);

            _human.Level = Levels[2];
            val = Lambda();
            msg =
                $"{humanClass.Name} with level '{_human.Level}' should have {Entry.Min99} < {val} < {Entry.Max99}";
            Assert.GreaterOrEqual(Entry.Max99, val, msg);
            Assert.LessOrEqual(Entry.Min99, val, msg);
            TestContext.WriteLine(msg);            
        }

        /* Called via reflection by NUnit */
        private static IEnumerable<ClassDesign> TestClasses()
        {
            var classes = new []
            {
                typeof(WarriorDesign),
                typeof(MageDesign),
                typeof(ArcherDesign),
                typeof(RogueDesign)
            };
            for (var i = 0; i < classes.Length; i++)
            {
                yield return ClassDesign.FromType(classes[i]);
            }
        }
        
        /* Called via reflection by NUnit */
        private static IEnumerable<Item> BadWeapons()
        {
            LoadItems();
            for (var i = 0; i < 3; i++)
            {
                yield return GrabWeapon(new ItemPoolSettings(ItemTier.Common)
                {
                    RandomizeTier = true,
                    EquipmentType = EquipmentType.Sword.ToString()
                });
            }
        }
        
        /* Called via reflection by NUnit */
        private static IEnumerable<Item> NormalWeapons()
        {
            LoadItems();
            for (var i = 0; i < 3; i++)
            {
                yield return GrabWeapon(new ItemPoolSettings(ItemTier.Unique)
                {
                    RandomizeTier = true,
                    EquipmentType = EquipmentType.Sword.ToString()
                });
            }
        }
        
        /* Called via reflection by NUnit */
        private static IEnumerable<Item> GoodWeapons()
        {
            LoadItems();
            for (var i = 0; i < 3; i++)
            {
                yield return GrabWeapon(new ItemPoolSettings(ItemTier.Divine)
                {
                    RandomizeTier = true,
                    EquipmentType = EquipmentType.Sword.ToString()
                });
            }
        }
        
        /* Called via reflection by NUnit */
        private static IEnumerable<Item> RandomWeapons()
        {
            LoadItems();
            for (var i = 0; i < 3; i++)
            {
                yield return ItemPool.Grab(new ItemPoolSettings(ItemTier.Divine));
            }
        }

        private static void LoadItems()
        {
            AssetManager.Provider = new DummyAssetProvider();
            SoundPlayer.Provider = new DummySoundProvider();
            if (!_loaded)
            {
                _loaded = true;
                HedraContent.Register();
            }
            ItemLoader.LoadModules(GameLoader.AppPath);
        }
        
        private static Item GrabWeapon(ItemPoolSettings Settings)
        {
            Item item = null;
            while (item == null || !item.IsWeapon)
            {
                Log.WriteLine("Grabbing item from ItemPool");
                item = ItemPool.Grab(Settings);
            }
            return item;
        }
    }
}