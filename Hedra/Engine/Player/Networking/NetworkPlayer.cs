using System;
using Hedra.Crafting;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.EntitySystem;

namespace Hedra.Engine.Player.Networking
{
    public class NetworkPlayer : Humanoid, IPlayer
    {
        public event OnInteractionEvent Interact;
        public ICamera View => throw new System.NotImplementedException();
        public ChunkLoader Loader => throw new System.NotImplementedException();
        public UserInterface UI => throw new System.NotImplementedException();
        public IToolbar Toolbar => throw new System.NotImplementedException();
        public IAbilityTree AbilityTree => throw new System.NotImplementedException();
        public RealmHandler Realms => throw new System.NotImplementedException();
        public CompanionHandler Companion => throw new System.NotImplementedException();
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
        public bool CanCastSkill => throw new System.NotImplementedException();
        public void SetSkillPoints(Type Skill, int Points)
        {
            throw new NotImplementedException();
        }

        public T SearchSkill<T>() where T : AbstractBaseSkill
        {
            throw new NotImplementedException();
        }

        public bool Enabled
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
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