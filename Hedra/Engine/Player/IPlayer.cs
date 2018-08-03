using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player
{
    public interface IPlayer : ISearchable, IHumanoid
    {
        IMessageDispatcher MessageDispatcher { get; }
        ICamera View { get; }
        ChunkLoader Loader { get; }
        UserInterface UI { get; set; }
        PlayerInventory Inventory { get; }
        EntitySpawner Spawner { get; }
        Toolbar Toolbar { get; }
        QuestLog QuestLog { get; }
        AbilityTreeSystem.AbilityTree AbilityTree { get; }
        PetManager Pet { get; }
        Chat Chat { get; }
        Minimap Minimap { get; }
        Map Map { get; }
        TradeInventory Trade { get; }
        HangGlider Glider { get; }
        Vector3 Position { get; set; }
        ICollidable[] NearCollisions { get; }
        bool Enabled { get; set; }
        void Respawn();
        void Load();
        void Unload();
        void Reset();
    }
}
