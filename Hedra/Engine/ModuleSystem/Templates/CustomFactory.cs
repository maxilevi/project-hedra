using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.AISystem;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem.ModelHandlers;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;

namespace Hedra.Engine.ModuleSystem.Templates
{
    public class CustomFactory : IEnemyFactory
    {
        public const float MinXpFactor = .45f;
        public const float MaxXpFactor = .75f;
        private static Dictionary<string, Type> EffectTable { get; }

        public string Name { get; set; }
        public float MaxHealth { get; set; }
        public float AttackDamage { get; set; }
        public float AttackCooldown { get; set; } = 1.5f;
        public float Speed { get; set; }
        public float XP { get; set; }
        public string AIType { get; set; }
        public bool Ridable { get; set; }
        public EffectTemplate[] Effects { get; set; }
        public DropTemplate[] Drops { get; set; }
        public ModelTemplate Model { get; set; }
        public int Level { get; set; }

        static CustomFactory()
        {
            EffectTable = new Dictionary<string, Type>
            {
                {"Fire", typeof(FireComponent)},
                {"Poison", typeof(PoisonousComponent)},
                {"Freeze", typeof(FreezeComponent)},
                {"Bleed", typeof(BleedComponent)},
                {"Slow", typeof(SlowComponent)},
                {"Knock", typeof(KnockComponent)}
            };

            foreach (var pair in EffectTable)
            {
                var interfaces = pair.Value.GetInterfaces();
                if (!interfaces.Contains( typeof(IEffectComponent) ) )
                {
                    throw new ArgumentException($"Unsupported effect type '{pair.Value}'");
                }
            }
        }

        public void Load()
        {
            AssetManager.LoadHitbox(Model.Path);
        }

        public void Apply(Entity Mob, bool NormalizeValues = true)
        {
            Mob.Model = new QuadrupedModel(Mob, Model);
            Mob.MaxHealth = MaxHealth;
            Mob.AttackDamage = AttackDamage;
            Mob.AttackCooldown = AttackCooldown;
            Mob.Speed = Speed;
            Mob.Level = Level;
            Mob.Name = Name;
            var dmg = new DamageComponent(Mob)
            {
                XpToGive = NormalizeValues ? NormalizeXp(XP) : XP
            };
            Mob.AddComponent(dmg);

            if (Ridable)
                Mob.AddComponent(new RideComponent(Mob));

            foreach (var template in Effects)
            {
                var effect = (IEffectComponent) Activator.CreateInstance(EffectTable[template.Name], Mob);
                effect.Chance = (int) template.Chance;
                effect.Duration = template.Duration;
                effect.Damage = template.Damage;
                Mob.AddComponent(effect as EntityComponent);
            }

            var gold = ItemPool.Grab(ItemType.Gold);
            gold.SetAttribute(CommonAttributes.Amount, Utils.Rng.Next(1 + (int) (XP / 2), 4 + (int) (XP / 2)) );
            var drop = new DropComponent(Mob)
            {
                ItemDrop = gold,
                RandomDrop = false,
                DropChance = 50
            };
            Mob.AddComponent(drop);
            

            foreach (var template in Drops)
            {
                if(!ItemPool.Exists(template.Type)) 
                    throw new ArgumentOutOfRangeException($"Item '{template.Type}' does not exist.");
                var type = template.Type;

                drop = new DropComponent(Mob)
                {
                    DropChance = template.Chance,
                    RandomDrop = false,
                    ItemDrop = ItemPool.Grab(type)
                };
                Mob.AddComponent(drop);
            }

            this.AddItemDropPerLevel(Mob);
            Mob.AddComponent(AIFactory.Instance.Build(Mob, AIType));
        }

        public void Polish(Entity Mob)
        {
            if(Model.Handler == null) return;
            var handler = ModelHandlerFactory.Instance.Build(Model.Handler);
            handler.Process(Mob, Mob.Model as AnimatedUpdatableModel);
        }

        private void AddItemDropPerLevel(IEntity Mob)
        {
            if (Mob.Level < 8)
                Mob.AddComponent(new DropComponent(Mob)
                {
                    ItemDrop = ItemPool.Grab(ItemTier.Common),
                    RandomDrop = false,
                    DropChance = .125f,
                });
            else if(Mob.Level < 16)
                Mob.AddComponent(new DropComponent(Mob)
                {
                    ItemDrop = ItemPool.Grab(ItemTier.Uncommon),
                    RandomDrop = false,
                    DropChance = .1f,
                });
            else if(Mob.Level < 32)
                Mob.AddComponent(new DropComponent(Mob)
                {
                    ItemDrop = ItemPool.Grab(ItemTier.Rare),
                    RandomDrop = false,
                    DropChance = .075f,
                });
            else if(Mob.Level < 48)
                Mob.AddComponent(new DropComponent(Mob)
                {
                    ItemDrop = ItemPool.Grab(ItemTier.Unique),
                    RandomDrop = false,
                    DropChance = .05f,
                });
            else if(Mob.Level < 64)
                Mob.AddComponent(new DropComponent(Mob)
                {
                    ItemDrop = ItemPool.Grab(ItemTier.Legendary),
                    RandomDrop = false,
                    DropChance = .025f,
                });
            else
                Mob.AddComponent(new DropComponent(Mob)
                {
                    ItemDrop = ItemPool.Grab(ItemTier.Divine),
                    RandomDrop = false,
                    DropChance = .005f,
                });
        }
        
        private float NormalizeXp(float Raw)
        {
            return Mathf.Clamp(Raw, Level * MinXpFactor, Level * MaxXpFactor);
        }
    }
}