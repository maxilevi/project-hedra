using System.Runtime.InteropServices;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using HedraTests.Player;
using Moq;
using NUnit.Framework;

namespace HedraTests.EntitySystem
{
    [TestFixture]
    public class DamageComponentTest : BaseTest
    {
        private DamageComponent _damageComponent;
        private IEntity _entity;
        private bool _isStatic;
        private PhysicsComponent _physics;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _physics = new PhysicsComponent(null);
            var entityMock = new Mock<IEntity>();
            entityMock.Setup(E => E.Physics).Returns( () => _physics);
            entityMock.SetupProperty(E => E.Health);
            entityMock.SetupProperty(E => E.IsDead);
            entityMock.Setup(E => E.IsStatic).Returns( () => _isStatic);
            _entity = entityMock.Object;
            _entity.Health = 100;
            GameManager.Player = new PlayerMock();
            _damageComponent = new DamageComponent(_entity);
        }
        
        [Test]
        public void TestNoExpIsGivenIfTargetInvalid()
        {
            _entity.IsDead = true;
            _damageComponent.Damage(10, null, out var xp, false);
            Assert.AreEqual(0, xp);
        }
        
        [Test]
        public void TestHasBeenAttacked()
        {
            Assert.False(_damageComponent.HasBeenAttacked);
            _damageComponent.Damage(10, null, out var xp, false);
            Assert.True(_damageComponent.HasBeenAttacked);
        }
        
        [Test]
        public void TestDamageBillboardIsCreated()
        {
            _damageComponent.Damage(10, null, out var xp, true);
            Assert.AreEqual(1, _damageComponent.DamageLabels.Count);
        }
        
        public void TestDamageBillboardHasTheCorrectColors()
        {

        }
        
        public void TestEntityIsPushedWhenDamaged()
        {
            
        }
        
        public void TestItemIsDropedWhenKilled()
        {
            var wasDroped = false;
            var dropMock = new Mock<DropComponent>(_entity);
            dropMock.Setup(D => D.Drop()).Callback( () => wasDroped = true );
            var drop = dropMock.Object;
            _entity.AddComponent(drop);
            _entity.Health = 5;
            _damageComponent.Damage(10, null, out var xp, false);
            Assert.True(wasDroped);
        }
        
        [Test]
        public void TestDamageEventIsLaunched()
        {
            var wasDamaged = false;
            _damageComponent.OnDamageEvent += delegate
            {
                wasDamaged = true;
                
            };
            _damageComponent.Damage(10, null, out var xp, false);
            Assert.True(wasDamaged);
        }
        
        public void TestAttackTintIsAdded()
        {
            
        }

        public void TestStaticEntitesDontShowAnyLabels()
        {

        }
        
        public void TestSoundIsPlayedAfterDamage()
        {
            
        }
        
        [Test]
        public void TestEntityIsKilled()
        {
            _entity.Health = 5;
            _damageComponent.Damage(10, null, out var xp, false);
            Assert.True(_entity.IsDead);
        }
        
        public void TestDisposeCorotuineIsLaunched()
        {
            
        }
        
        [Test]
        public void TestExpIsGivenAfterKillingTarget()
        {
            _entity.Health = 5;
            _damageComponent.XpToGive = 10;
            _damageComponent.Damage(10, null, out var xp, false);
            Assert.AreEqual(10, xp);
        }
        
        [Test]
        public void TestEntityCanBeDamaged()
        {
            AssertWasDamaged(10, 10, out var xp);
        }
        
        [Test]
        public void TestImmuneWorksAsExpected()
        {
            _damageComponent.Immune = true;
            AssertWasDamaged(10, 0, out var xp);
        }

        private void AssertWasDamaged(float Damage, float Delta, out float XP)
        {
            var prevHealth = _entity.Health;
            _damageComponent.Damage(Damage, null, out XP, false);
            Assert.AreEqual(prevHealth - Delta, _entity.Health);
        }
    }
}