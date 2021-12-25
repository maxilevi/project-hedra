/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 16/06/2017
 * Time: 02:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    ///     Description of Chest.
    /// </summary>
    public sealed class Chest : AnimableInteractableStructure
    {
        private static readonly CollisionShape DefaultShape;

        public Chest(Vector3 Position, Item ItemSpecification) : base(Position, Vector3.One)
        {
            this.ItemSpecification = ItemSpecification;
        }

        public Item ItemSpecification { get; set; }
        public override string Message => Translations.Get("interact_chest");
        public override int InteractDistance => 16;

        protected override string ModelPath =>
            Season.IsChristmas ? "Assets/Chr/ChristmasChestIdle.dae" : "Assets/Chr/ChestIdle.dae";

        protected override string IdleAnimationPath => "Assets/Chr/ChestIdle.dae";
        protected override string UseAnimationPath => "Assets/Chr/ChestOpen.dae";
        protected override string ColliderPath => "Assets/Env/Chest.ply";
        protected override Vector3 ColliderOffset => -Vector3.UnitX * 1.5f;
        protected override float AnimationSpeed => 4.0f;
        protected override bool EnableLegacyTerrainHeightMode => true;
        protected override Vector3 ModelScale => Vector3.One * 2.5f;
        public event OnItemCollect OnPickup;

        protected override void OnUse(IHumanoid Humanoid)
        {
            var worldItem = World.DropItem(ItemSpecification, Position);
            worldItem.Position = new Vector3(worldItem.Position.X, worldItem.Position.Y + .75f * Scale.Y,
                worldItem.Position.Z);
            worldItem.OnPickup += delegate(IPlayer Player) { OnPickup?.Invoke(Player); };
        }
    }
}