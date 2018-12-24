using Hedra.Engine.CraftingSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.BoatSystem;
using Hedra.Engine.Player.CraftingSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.Player
{
    public interface IPlayer : ISearchable, IHumanoid
    {
        IMessageDispatcher MessageDispatcher { get; }
        ICamera View { get; }
        ChunkLoader Loader { get; }
        UserInterface UI { get; set; }
        IPlayerInventory Inventory { get; }
        EntitySpawner Spawner { get; }
        IToolbar Toolbar { get; }
        QuestInterface QuestInterface { get; }
        IAbilityTree AbilityTree { get; }
        PetManager Pet { get; }
        Chat Chat { get; }
        Minimap Minimap { get; }
        TradeInventory Trade { get; }
        Vector3 Position { get; set; }
        CollisionGroup[] NearCollisions { get; }
        CraftingInventory Crafting { get; }
        bool InterfaceOpened { get; }
        bool Enabled { get; set; }
        void Respawn();
        void Reset();
        void HideInterfaces();
    }
}
