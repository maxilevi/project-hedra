using System;
using System.Collections.Generic;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem;
using Moq;
using NUnit.Framework;

namespace HedraTests.Player.Skills
{
    [TestFixture]
    public abstract class LearningSkillTestBase<T> : SkillTest<T> where T : LearningSkill, new()
    {
        protected abstract EquipmentType LearnType { get; }
        protected abstract int InventoryPosition { get; }
        private Dictionary<int, List<string>> _results;

        [SetUp]
        public void Setup()
        {
            base.Setup();
            _results = new Dictionary<int, List<string>>();
            void Callback(int Index, string Type)
            {
                if (!_results.ContainsKey(Index))
                    _results.Add(Index, new List<string>());     
                _results[Index].Add(Type);
            }
            var mockInventory = new Mock<IPlayerInventory>();
            mockInventory.Setup(I => I.AddRestriction(It.IsAny<int>(), It.IsAny<string>())).Callback(
                (int T1, string T2) => Callback(T1, T2)
            );
            mockInventory.Setup(I => I.AddRestriction(It.IsAny<int>(), It.IsAny<EquipmentType>())).Callback(
                (int T1, EquipmentType T2) => Callback(T1, T2.ToString())
            );
            Player.Inventory = mockInventory.Object;
        }

        [Test]
        public void TestAbilityIsLearned()
        {
            AssertLearned(false);
            Skill.Level = 1;
            Skill.Update();
            AssertLearned(true);
        }
        
        private void AssertLearned(bool Expected)
        {
            Assert.AreEqual(Expected, _results.ContainsKey(InventoryPosition));
            if(Expected)
                Assert.AreEqual(Expected, _results[InventoryPosition].Contains(LearnType.ToString()));
        }
    }
}