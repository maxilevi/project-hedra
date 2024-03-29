using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Game;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Game;
using HedraTests.Player;
using NUnit.Framework;
using System.Numerics;
using Hedra;
using Hedra.Numerics;

namespace HedraTests.WorldBuilding
{
    [TestFixture]
    public class InteractableStructureTest : BaseTest
    {
        private InteractableStructureMock _structure;
        private PlayerMock _player;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _player = new PlayerMock();
            _structure = new InteractableStructureMock();
            _structure.Position = new Vector3(300, 0, 500);
            GameManager.Player = _player;
            _player.Position = new Vector3(295, 0, 500);
            _player.CameraMock.LookingDirection = (_structure.Position - _player.Position).NormalizedFast();
            _player.CanInteract = true;
        }
        
        [Test]
        public void TestUsingInteractableStructureWorks()
        {
            var originalLevel = _player.Level;
            _structure.Update();
            Assert.AreEqual("[E] Here is a mock string", _player.MessageMock.LastMessage);
            EventProvider.SimulateKeyDown(_structure.Key);
            _structure.Update();
           
            Assert.Greater(_player.Level, originalLevel);
            var newLevel = _player.Level;
            _player.MessageMock.Reset();
            
            _structure.Update();
            EventProvider.SimulateKeyDown(_structure.Key);
            _structure.Update();
            
            Assert.Null(_player.MessageMock.LastMessage);
            Assert.AreEqual(newLevel, _player.Level);
        }
        
        [Test]
        public void TestStructureIsNotDisposedWhenSet()
        {
            _structure.SetDisposeAfterUse(false);
            _structure.Update();
            EventProvider.SimulateKeyDown(_structure.Key);
            _structure.Update();
            Assert.True(_structure.Interacted);
            Assert.False(_structure.Disposed);
        }
        
        [Test]
        public void TestStructureIsDisposedWhenSet()
        {
            _structure.SetDisposeAfterUse(true);
            _structure.Update();
            EventProvider.SimulateKeyDown(_structure.Key);
            _structure.Update();
            Assert.True(_structure.Interacted);
            Assert.True(_structure.Disposed);
        }
        
        [Test]
        public void TestCanInteractIsRespected()
        {
            var originalLevel = _player.Level;
            _structure.SetCanInteract(false);
            _structure.Update();
            EventProvider.SimulateKeyDown(_structure.Key);
            _structure.Update();
           
            Assert.AreEqual(_player.Level, originalLevel);
        }
        
        [Test]
        public void TestSelectingStructure()
        {
            _player.Mana = -10;
            _structure.DoUpdate();
            Assert.AreEqual(10, _player.Mana);
            
            _player.Position = new Vector3(0, 0, 0);
            
            _structure.DoUpdate();
            Assert.AreEqual(0, _player.Mana);
        }

        [Test]
        public void TestInteractionDistance()
        {    
            var structPosition = new Vector3(300, 0 ,300);
            var playerPosition = new Vector3(270, 0, 300);
            var orientation = (structPosition - playerPosition).NormalizedFast();
            for (var i = 0; i < 10; i++)
            {
                var inRange = (structPosition - playerPosition).Xz().LengthSquared() <
                           InteractableStructureMock.StructureInteractionRadius *
                           InteractableStructureMock.StructureInteractionRadius;
                Assert.AreEqual(inRange, CreateInteraction(structPosition, playerPosition, orientation));
            }     
        }
        
        [Test]
        public void TestInteractionAngleDistance()
        {
            var structPosition = new Vector3(300, 0 ,300);
            var playerPosition = new Vector3(305, 0 ,300);
            var originalOrientation = (structPosition - playerPosition).NormalizedFast();
            for (var i = 0; i < 360; i += 15)
            {
                var orientation = Vector3.TransformNormal(originalOrientation, Matrix4x4.CreateRotationY(i * Mathf.Radian).Inverted().Transposed());
                var inRange = Vector2.Dot((structPosition - playerPosition).Xz().NormalizedFast(),
                                  orientation.Xz().NormalizedFast());
                Assert.AreEqual(inRange > .9f, CreateInteraction(structPosition, playerPosition, orientation));
            }
        }

        private bool CreateInteraction(Vector3 StructPosition, Vector3 PlayerPosition, Vector3 PlayerLookAt)
        {
            var player = new PlayerMock {CanInteract = true};
            GameManager.Player = player;
            var structure = new InteractableStructureMock
            {
                Position = StructPosition
            };
            
            player.Position = PlayerPosition;
            player.CameraMock.LookingDirection = PlayerLookAt;
            
            structure.DoUpdate();
            EventProvider.SimulateKeyDown(structure.Key);
            structure.DoUpdate();

            return structure.Interacted;
        }
    }

    class InteractableStructureMock : InteractableStructure
    {
        private bool _disposeAfterUse;
        private bool _canInteract;
        protected override float InteractionAngle => .9f;
        public const int StructureInteractionRadius = 25;
        public override string Message => "Here is a mock string";
        public override int InteractDistance => StructureInteractionRadius;
        protected override bool CanInteract => _canInteract;
        protected override bool DisposeAfterUse => _disposeAfterUse;

        public InteractableStructureMock() : base(Vector3.Zero)
        {
            _disposeAfterUse = base.DisposeAfterUse;
            _canInteract = base.CanInteract;
        }
        
        public void SetDisposeAfterUse(bool Value)
        {
            _disposeAfterUse = Value;
        }
        
        public void SetCanInteract(bool Value)
        {
            _canInteract = Value;
        }

        public void DoUpdate()
        {
            base.DoUpdate(Time.DeltaTime);
        }

        public void Update()
        {
            base.Update(Time.DeltaTime);
        }

        protected override void Interact(IHumanoid Humanoid)
        {
            Humanoid.Level += 20;
        }
        
        protected override void OnSelected(IHumanoid Humanoid)
        {
            base.OnSelected(Humanoid);
            Humanoid.Mana = 10;
        }
        
        protected override void OnDeselected(IHumanoid Humanoid)
        {
            base.OnDeselected(Humanoid);
            Humanoid.Mana = 0;
        }
    }
}