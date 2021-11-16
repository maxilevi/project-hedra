using System;
using Hedra.Components;
using Hedra.Crafting;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;

namespace Hedra.Engine.Player.Networking
{
    public class NetworkPlayer : Humanoid, IPlayer
    {
        public event OnRespawnEvent OnRespawn;
        public event OnDeadEvent OnDeath;
        public ICamera View => throw new NotImplementedException();
        public ChunkLoader Loader => throw new NotImplementedException();
        public UserInterface UI => throw new NotImplementedException();
        public IToolbar Toolbar => throw new NotImplementedException();
        public IAbilityTree AbilityTree => throw new NotImplementedException();
        public RealmHandler Realms => throw new NotImplementedException();
        public CompanionHandler Companion => throw new NotImplementedException();
        public Chat Chat => throw new NotImplementedException();
        public Minimap Minimap => throw new NotImplementedException();
        public TradeInventory Trade => throw new NotImplementedException();
        public CollisionGroup[] NearCollisions => throw new NotImplementedException();
        public CraftingInventory Crafting => throw new NotImplementedException();
        public QuestInventory Questing => throw new NotImplementedException();
        public IStructureAware StructureAware => throw new NotImplementedException();
        public bool InterfaceOpened => throw new NotImplementedException();

        public void Respawn()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void HideInterfaces()
        {
            throw new NotImplementedException();
        }

        public bool CanCastSkill => throw new NotImplementedException();

        public void SetSkillPoints(Type Skill, int Points)
        {
            throw new NotImplementedException();
        }

        public T SearchSkill<T>() where T : AbstractBaseSkill
        {
            throw new NotImplementedException();
        }

        public void ShowInventoryFor(Item Bag)
        {
        }

        public bool Enabled
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public Animation AnimationBlending { get; }

        public void ResetModel()
        {
            throw new NotImplementedException();
        }

        public void BlendAnimation(Animation Animation)
        {
            throw new NotImplementedException();
        }

        public bool CaptureMovement { get; set; }

        public void Orientate()
        {
            throw new NotImplementedException();
        }

        public bool InAttackStance { get; set; }
    }
}