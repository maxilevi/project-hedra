using System;
using Hedra.Engine.CraftingSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.Player
{   
    public interface IPlayer : IHumanoid
    {
        IMessageDispatcher MessageDispatcher { get; }
        ICamera View { get; }
        ChunkLoader Loader { get; }
        UserInterface UI { get; }
        IPlayerInventory Inventory { get; }
        IToolbar Toolbar { get; }
        IAbilityTree AbilityTree { get; }
        RealmHandler Realms { get; }
        PetManager Pet { get; }
        Chat Chat { get; }
        Minimap Minimap { get; }
        TradeInventory Trade { get; }
        Vector3 Position { get; set; }
        CollisionGroup[] NearCollisions { get; }
        CraftingInventory Crafting { get; }
        QuestInventory Questing { get; }
        IStructureAware StructureAware { get; }
        bool InterfaceOpened { get; }
        bool Enabled { get; set; }
        void Respawn();
        void Reset();
        void HideInterfaces();
    }
}
