using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace HedraTests
{
    public class PlayerMock : Humanoid, IPlayer
    {
        public SimpleMessageDispatcherMock MessageMock => MessageDispatcher as SimpleMessageDispatcherMock;
        
        public PlayerMock()
        {
            MessageDispatcher = new SimpleMessageDispatcherMock();
        }

        public Camera View { get; }
        public ChunkLoader Loader { get; }
        public UserInterface UI { get; set; }
        public PlayerInventory Inventory { get; }
        public EntitySpawner Spawner { get; }
        public Toolbar Toolbar { get; }
        public QuestLog QuestLog { get; }
        public AbilityTree AbilityTree { get; }
        public PetManager Pet { get; }
        public Chat Chat { get; }
        public Minimap Minimap { get; }
        public Map Map { get; }
        public TradeInventory Trade { get; }
        public HangGlider Glider { get; }
        public ICollidable[] NearCollisions { get; }
        public bool Enabled { get; set; }
        public void Respawn()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}