using System;
using Hedra.Crafting;
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
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Components;
using Hedra.Engine.Bullet;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.Player
{
    public delegate void OnInteractionEvent(InteractableStructure Structure);
    
    public delegate void OnRespawnEvent();
    
    public interface IPlayer : IHumanoid, ISkillUser
    {
        event OnDeadEvent OnDeath;
        event OnRespawnEvent OnRespawn;
        IMessageDispatcher MessageDispatcher { get; }
        ICamera View { get; }
        ChunkLoader Loader { get; }
        UserInterface UI { get; }
        IPlayerInventory Inventory { get; }
        IToolbar Toolbar { get; }
        IAbilityTree AbilityTree { get; }
        EquipmentHandler Equipment { get; }
        RealmHandler Realms { get; }
        CompanionHandler Companion { get; }
        Chat Chat { get; }
        Minimap Minimap { get; }
        TradeInventory Trade { get; }
        Vector3 Position { get; set; }
        CollisionGroup[] NearCollisions { get; }
        CraftingInventory Crafting { get; }
        QuestInventory Questing { get; }
        IStructureAware StructureAware { get; }
        void ShowInventoryFor(Item Bag);
        bool InterfaceOpened { get; }
        bool Enabled { get; set; }
        void Respawn();
        void Reset();
        void HideInterfaces();
    }
}
