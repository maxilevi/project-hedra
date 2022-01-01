using System;
using Hedra.AISystem;
using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem.ModelHandlers;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Numerics;

namespace Hedra.Engine.ModuleSystem.Templates
{
    public class CustomFactory : IEnemyFactory
    {
        public const float MinXpFactor = .45f;
        public const float MaxXpFactor = .75f;
        public float AttackCooldown { get; set; } = 1.5f;
        public float Speed { get; set; }
        public string AIType { get; set; }
        public EffectTemplate[] Effects { get; set; }
        public DropTemplate[] Drops { get; set; }
        public ModelTemplate Model { get; set; }
        public int Level { get; set; }

        public string DisplayName { get; set; }
        public string Name { get; set; }
        public float MaxHealth { get; set; }
        public float AttackDamage { get; set; }
        public float XP { get; set; }

        public void Apply(SkilledAnimableEntity Mob, bool NormalizeValues = true)
        {
            Mob.Model = new QuadrupedModel(Mob, Model);
            Mob.MaxHealth = MaxHealth;
            Mob.AttackDamage = AttackDamage * Balancer.MobDamageMultiplier;
            Mob.AttackCooldown = AttackCooldown;
            Mob.Speed = Speed;
            Mob.Level = Level;
            Mob.Name = DisplayName ?? Name;
            var dmg = new DamageComponent(Mob)
            {
                XpToGive = NormalizeValues ? NormalizeXp(XP) : XP
            };
            Mob.AddComponent(dmg);

            foreach (var template in Effects) Mob.AddComponent(EffectFactory.Instance.Build(template, Mob));

            var gold = ItemPool.Grab(ItemType.Gold);
            gold.SetAttribute(CommonAttributes.Amount, Utils.Rng.Next(1 + (int)(XP / 2), 4 + (int)(XP / 2)));
            var drop = new DropComponent(Mob)
            {
                ItemDrop = gold,
                DropChance = 50
            };
            Mob.AddComponent(drop);

            foreach (var template in Drops)
            {
                if (!ItemPool.Exists(template.Type))
                    throw new ArgumentOutOfRangeException($"Item '{template.Type}' does not exist.");
                var type = template.Type;
                var item = ItemPool.Grab(type);
                item.SetAttribute(CommonAttributes.Amount, Utils.Rng.Next(template.Min, template.Max + 1));
                drop = new DropComponent(Mob)
                {
                    DropChance = template.Chance,
                    ItemDrop = item
                };
                Mob.AddComponent(drop);
            }

            AddItemDropPerLevel(Mob);
            Mob.AddComponent(AIFactory.Instance.Build(Mob, AIType));
        }

        public void Polish(Entity Mob)
        {
            if (Model.Handler == null) return;
            var handler = ModelHandlerFactory.Instance.Build(Model.Handler);
            handler.Process(Mob, Mob.Model as AnimatedUpdatableModel);
        }

        public void Load()
        {
            Model.Resolve();
            if (Model.Path != null)
                AssetManager.LoadHitbox(Model.Path);
            if (Model.Paths != null)
            {
                foreach (var path in Model.Paths)
                {
                    AssetManager.LoadHitbox(path);
                }
            }
        }

        private void AddItemDropPerLevel(IEntity Mob)
        {
            Mob.AddComponent(new DropComponent(Mob)
            {
                ItemDrop = ItemPool.Grab(ItemTier.Common),
                DropChance = 2.5f + (Mob.Level < 8 ? 5f : 0f)
            });
            Mob.AddComponent(new DropComponent(Mob)
            {
                ItemDrop = ItemPool.Grab(ItemTier.Uncommon),
                DropChance = 1.5f + (Mob.Level < 16 && Mob.Level > 8 ? 5f : 0f)
            });
            Mob.AddComponent(new DropComponent(Mob)
            {
                ItemDrop = ItemPool.Grab(ItemTier.Rare),
                DropChance = 1.0f + (Mob.Level < 32 && Mob.Level > 16 ? 5f : 0f)
            });
            Mob.AddComponent(new DropComponent(Mob)
            {
                ItemDrop = ItemPool.Grab(ItemTier.Unique),
                DropChance = 0.5f + (Mob.Level < 48 && Mob.Level > 32 ? 5f : 0f)
            });
            Mob.AddComponent(new DropComponent(Mob)
            {
                ItemDrop = ItemPool.Grab(ItemTier.Legendary),
                DropChance = .25f + (Mob.Level < 64 && Mob.Level > 48 ? 2.5f : 0f)
            });
            Mob.AddComponent(new DropComponent(Mob)
            {
                ItemDrop = ItemPool.Grab(ItemTier.Divine),
                DropChance = 0.1f + (Mob.Level > 64 ? 5f : 0f)
            });
        }

        private float NormalizeXp(float Raw)
        {
            return Mathf.Clamp(Raw, Level * MinXpFactor, Level * MaxXpFactor);
        }
    }
}