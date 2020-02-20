/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/08/2016
 * Time: 08:35 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Components;
using Hedra.Core;
using System.Numerics;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    /// Description of DropComponent.
    /// </summary>
    public class DropComponent : EntityComponent
    {
        public bool Dropped { get; private set; }
        public bool RandomDrop => ItemDrop == null;
        public Item ItemDrop { get; set; }
        private readonly Random _rng;
        private float _dropChance;
        
        public DropComponent(IEntity Parent) : base(Parent)
        {
            _rng = new Random(Unique.RandomSeed());
            Parent.SearchComponent<DamageComponent>().OnDeadEvent += A =>
            {
                Drop(A.Damager);
            };
        }

        private void Drop(IEntity Killer)
        {
            if (!Parent.IsDead || Dropped) return;
            var chance = _rng.NextFloat() * 100f;
            var item = RandomDrop ? ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon)) : ItemDrop;
            chance /= (item != null && item.IsFood && Killer != null ? Killer.Attributes.FoodDropChanceModifier : 1);
            if (chance < DropChance)
            {
                if (item != null)
                {
                    World.DropItem(item, Parent.Position + Vector3.UnitY * 2f + new Vector3(Utils.Rng.NextFloat() * 8f - 4f, 0, Utils.Rng.NextFloat() * 8f - 4f));
                }
            }
            Dropped = true;
        }
        
        public float DropChance
        {
            private get => _dropChance;
            set
            {
                _dropChance = value;
                if(_dropChance > 100 || _dropChance < 0)
                    throw new ArgumentException("Drop chance cannot be less than 0 or more than 100.");
            }
        }
        
        public override void Update(){}
    }
}
