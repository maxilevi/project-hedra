using Hedra.Engine.CraftingSystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.Player.Networking
{
    public class NetworkPlayer : Humanoid, IPlayer
    {
        public ICamera View => throw new System.NotImplementedException();
        public ChunkLoader Loader => throw new System.NotImplementedException();
        public UserInterface UI => throw new System.NotImplementedException();
        public IToolbar Toolbar => throw new System.NotImplementedException();
        public IAbilityTree AbilityTree => throw new System.NotImplementedException();
        public RealmHandler Realms => throw new System.NotImplementedException();
        public PetManager Pet => throw new System.NotImplementedException();
        public Chat Chat => throw new System.NotImplementedException();
        public Minimap Minimap => throw new System.NotImplementedException();
        public TradeInventory Trade => throw new System.NotImplementedException();
        public CollisionGroup[] NearCollisions => throw new System.NotImplementedException();
        public CraftingInventory Crafting => throw new System.NotImplementedException();
        public QuestInventory Questing => throw new System.NotImplementedException();
        public IStructureAware StructureAware => throw new System.NotImplementedException();
        public bool InterfaceOpened => throw new System.NotImplementedException();
        public void Respawn() => throw new System.NotImplementedException();
        public void Reset() => throw new System.NotImplementedException();
        public void HideInterfaces() => throw new System.NotImplementedException();
        public bool Enabled
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }
    }
}