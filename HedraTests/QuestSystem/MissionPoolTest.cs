using System;
using Hedra.Engine.Player;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Mission;
using NUnit.Framework;
using System.Numerics;

namespace HedraTests.QuestSystem
{
    [TestFixture]
    public class MissionPoolTest
    {

        [Test]
        public void TestTiersArePrioritizedWithinTier()
        {
            MissionPool.Load(new []
            {
                new DummyMissionDesign(QuestTier.Easy),
                new DummyMissionDesign(QuestTier.Medium),
                new DummyMissionDesign(QuestTier.Hard)
            });
            Assert.That(MissionPool.Random(Vector3.Zero, QuestTier.Hard).Tier, Is.EqualTo(QuestTier.Hard));
            Assert.That(MissionPool.Random(Vector3.Zero, QuestTier.Medium).Tier, Is.EqualTo(QuestTier.Medium));
            Assert.That(MissionPool.Random(Vector3.Zero, QuestTier.Easy).Tier, Is.EqualTo(QuestTier.Easy));
        }

        [Test]
        public void TestsGivesAnyWhenEmpty()
        {
            MissionPool.Load(new []
            {
                new DummyMissionDesign(QuestTier.Easy)
            });
            Assert.That(MissionPool.Random(Vector3.Zero, QuestTier.Hard).Tier, Is.EqualTo(QuestTier.Easy));
            Assert.That(MissionPool.Random(Vector3.Zero, QuestTier.Medium).Tier, Is.EqualTo(QuestTier.Easy));
        }

        [Test]
        public void TestMissionPoolFailsWhenEmpty()
        {
            MissionPool.Load(new IMissionDesign[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => MissionPool.Random(Vector3.Zero));
        }
        
        private class DummyMissionDesign : IMissionDesign
        {
            public DummyMissionDesign(QuestTier Tier)
            {
                this.Tier = Tier;
            }
            
            public MissionObject Build(Vector3 Position, IHumanoid Giver, IPlayer Owner)
            {
                return null;
            }

            public bool CanGive(Vector3 Position) => true;

            public QuestTier Tier { get; private set; }
            public QuestHint Hint { get; private set; }
            public string Name => "DUMMY";
            public bool IsStoryline => false;
            public bool CanSave => false;
        }
    }
}