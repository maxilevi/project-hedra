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
        [Test]
        public void TestUsingInteractableStructureWorks()
        {
            var player = new PlayerMock();
            var structure = new InteractableStructureMock();
            structure.Position = new Vector3(300, 0, 500);
            GameManager.Player = player;
            var originalLevel = player.Level;
            player.Position = new Vector3(295, 0, 500);
            player.CameraMock.LookingDirection = (structure.Position - player.Position).NormalizedFast();
            
            structure.Update();
            Assert.AreEqual("[E] Here is a mock string", player.MessageMock.LastMessage);
            EventProvider.SimulateKeyDown(structure.Key);
            structure.Update();
           
            Assert.Greater(player.Level, originalLevel);
            var newLevel = player.Level;
            player.MessageMock.Reset();
            
            structure.Update();
            EventProvider.SimulateKeyDown(structure.Key);
            structure.Update();
            
            Assert.Null(player.MessageMock.LastMessage);
            Assert.AreEqual(newLevel, player.Level);
        }
        
        [Test]
        public void TestSelectingStructure()
        {
            var player = new PlayerMock();
            var structure = new InteractableStructureMock();
            structure.Position = new Vector3(300, 0, 500);
            GameManager.Player = player;
            player.Mana = -10;
            player.Position = new Vector3(295, 0, 500);
            player.CameraMock.LookingDirection = (structure.Position - player.Position).NormalizedFast();
            
            structure.Update();
            Assert.AreEqual(10, player.Mana);
            
            player.Position = new Vector3(0, 0, 0);
            
            structure.Update();
            Assert.AreEqual(0, player.Mana);
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
        public const int StructureInteractionRadius = 25;
        public override string Message => "Here is a mock string";
        public override int InteractDistance => StructureInteractionRadius;

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