﻿using System.Runtime.InteropServices;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;
using HedraTests.Player;
using Moq;
using NUnit.Framework;
using OpenTK;

namespace HedraTests.EntitySystem
{
    [TestFixture]
    public class DamageComponentTest : BaseTest
    {
        private DamageComponent _damageComponent;
        private IEntity _entity;
        private bool _isStatic;
        private PhysicsComponent _physics;
        private float _maxHealth;
        private bool _disposed;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _physics = new PhysicsComponent(null);
            _maxHealth = 100;
            var modelMock = new Mock<BaseUpdatableModel>();
            modelMock.SetupProperty(M => M.Tint);
            modelMock.Setup(M => M.BaseBroadphaseBox).Returns(new Box());
            var entityMock = new Mock<IEntity>();
            entityMock.Setup(E => E.Physics).Returns( () => _physics);
            entityMock.SetupProperty(E => E.Health);
            entityMock.Setup(E => E.MaxHealth).Returns(_maxHealth);
            entityMock.SetupProperty(E => E.IsDead);
            entityMock.Setup(E => E.IsStatic).Returns( () => _isStatic);
            entityMock.Setup(E => E.Model).Returns(modelMock.Object);
            entityMock.Setup(E => E.Dispose()).Callback(() => _disposed = true);
            EntityComponent lastComponent = null;
            entityMock.Setup(E => E.AddComponent(It.IsAny<EntityComponent>())).Callback(delegate(EntityComponent Component)
            {
                lastComponent = Component;
            });
            entityMock.Setup(E => E.SearchComponent<DropComponent>()).Returns( () => (DropComponent) lastComponent);
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
        
        [Test]
        public void TestEntityIsPushedWhenDamaged()
        {
            var wasCalled = false;
            var entityMock = new Mock<IEntity>();
            entityMock.SetupAllProperties();
            var threadingMock = new Mock<IPhysicsThreadManager>();
            threadingMock.Setup(P => P.AddCommand(It.IsAny<MoveCommand>())).Callback( () => wasCalled = true);
            Physics.Threading = threadingMock.Object;
            _damageComponent.Damage(10, entityMock.Object, out var xp, true);
            Assert.True(wasCalled);
        }
        
        [Test]
        public void TestItemIsDropedWhenKilled()
        {
            var drop = new DropComponent(_entity)
            {
                DropChance = 0
            };
            _entity.AddComponent(drop);
            _entity.Health = 5;
            _damageComponent.Damage(10, null, out var xp, false);
            Assert.True(drop.Dropped);
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
        
        [Test]
        public void TestAttackTintIsAdded()
        {
            _damageComponent.Damage(10, null, out var xp, true);
            Time.Set(.5f);
            _damageComponent.Update();
            Assert.AreNotEqual(Vector4.Zero, _entity.Model.Tint);
        }

        [Test]
        public void TestStaticEntitesDontShowAnyLabels()
        {
            _isStatic = true;
            _damageComponent.Damage(10, null, out var xp, true);
            Assert.AreEqual(0, _damageComponent.DamageLabels.Count);
        }
        
        [Test]
        public void TestSoundIsPlayedAfterDamage()
        {
            var wasCalled = false;
            var provider = new Mock<ISoundProvider>();
            provider.Setup(P => P.PlaySound(It.IsIn( new []
            {
                SoundType.HitSound,
                SoundType.SlashSound
            }), It.IsAny<Vector3>(), It.IsAny<bool>(), It.IsAny<float>(), It.IsAny<float>())).Callback(delegate
                {
                    wasCalled = true;
                });
            SoundManager.Provider = provider.Object;
            _damageComponent.Damage(10, null, out var xp, true);
            Assert.True(wasCalled);
        }
        
        [Test]
        public void TestEntityIsKilled()
        {
            _entity.Health = 5;
            _damageComponent.Damage(10, null, out var xp, false);
            Assert.True(_entity.IsDead);
        }
        
        [Test]
        public void TestDisposeCoroutineIsLaunched()
        {
            _entity.Health = 5;
            _damageComponent.Damage(10, null, out var xp, false);
            Time.Set(4);
            CoroutineManager.Update();
            CoroutineManager.Update();
            Assert.True(_disposed);
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