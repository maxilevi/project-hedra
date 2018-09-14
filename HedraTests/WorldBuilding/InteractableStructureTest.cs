using Hedra.Engine;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using HedraTests.Player;
using NUnit.Framework;
using OpenTK;

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
            _structure.Update();
            Assert.AreEqual(10, _player.Mana);
            
            _player.Position = new Vector3(0, 0, 0);
            
            _structure.Update();
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
                var inRange = (structPosition - playerPosition).Xz.LengthSquared <
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
                var orientation = Vector3.TransformNormal(originalOrientation, Matrix4.CreateRotationY(i * Mathf.Radian));
                var inRange = Vector2.Dot((structPosition - playerPosition).Xz.NormalizedFast(),
                                  orientation.Xz.NormalizedFast()) > .9f;
                Assert.AreEqual(inRange, CreateInteraction(structPosition, playerPosition, orientation));
            }
        }

        private bool CreateInteraction(Vector3 StructPosition, Vector3 PlayerPosition, Vector3 PlayerLookAt)
        {
            var player = new PlayerMock();
            GameManager.Player = player;
            var structure = new InteractableStructureMock
            {
                Position = StructPosition
            };
            
            player.Position = PlayerPosition;
            player.CameraMock.LookingDirection = PlayerLookAt;
            
            structure.Update();
            EventProvider.SimulateKeyDown(structure.Key);
            structure.Update();

            return structure.Interacted;
        }
    }

    class InteractableStructureMock : InteractableStructure
    {
        private bool _disposeAfterUse;
        private bool _canInteract;
        public const int StructureInteractionRadius = 25;
        public override string Message => "Here is a mock string";
        public override int InteractDistance => StructureInteractionRadius;
        protected override bool CanInteract => _canInteract;
        protected override bool DisposeAfterUse => _disposeAfterUse;

        public InteractableStructureMock() : base()
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

        protected override void Interact(IPlayer Interactee)
        {
            Interactee.Level += 20;
        }
        
        protected override void OnSelected(IPlayer Interactee)
        {
            base.OnSelected(Interactee);
            Interactee.Mana = 10;
        }
        
        protected override void OnDeselected(IPlayer Interactee)
        {
            base.OnDeselected(Interactee);
            Interactee.Mana = 0;
        }
    }
}