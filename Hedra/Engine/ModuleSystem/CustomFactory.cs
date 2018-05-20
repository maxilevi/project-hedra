using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.AISystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.ModuleSystem
{
    public class CustomFactory : IEnemyFactory
    {

        public static Dictionary<string, Type> EffectTable;
        public static Dictionary<string, Type> AITable;

        public string Name { get; set; }
        public float MaxHealth { get; set; }
        public float AttackDamage { get; set; }
        public float AttackCooldown { get; set; } = 1.5f;
        public float Speed { get; set; }
        public float XP { get; set; }
        public string AIType { get; set; }
        public bool Ridable { get; set; }
        public EffectTemplate[] Effects;
        public DropTemplate[] Drops;
        public ModelTemplate Model;

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

            AITable = new Dictionary<string, Type>
            {
                {"Friendly", typeof(FriendlyAIComponent)},
                {"Neutral", typeof(NeutralAIComponent)},
                {"Hostile", typeof(HostileAIComponent)},
            };

            foreach (KeyValuePair<string, Type> pair in EffectTable)
            {
                Type[] interfaces = pair.Value.GetInterfaces();
                if (!interfaces.Contains( typeof(IEffectComponent) ) )
                {
                    throw new ArgumentException("Unsupported effect type '" + pair.Value + "'");
                }
            }
        }

        public void Apply(Entity Mob)
        {
            Mob.Model = new QuadrupedModel(Mob, Model);
            Mob.MaxHealth = MaxHealth;
            Mob.AttackDamage = this.DamageFormula(AttackDamage);
            Mob.AttackCooldown = AttackCooldown;
            Mob.Speed = Speed;

            Mob.SearchComponent<HealthBarComponent>().DistanceFromBase = Mob.BaseBox.Max.Y - Mob.BaseBox.Min.Y + 0.1f;

            var dmg = new DamageComponent(Mob)
            {
                XpToGive = XP
            };
            Mob.AddComponent(dmg);

            if (Ridable)
                Mob.AddComponent(new RideComponent(Mob));

            foreach (EffectTemplate template in Effects)
            {
                var effect = (EntityComponent) Activator.CreateInstance(EffectTable[template.Name], Mob);
                Mob.AddComponent(effect);
            }

            var gold = ItemPool.Grab(ItemType.Gold);
            gold.SetAttribute(CommonAttributes.Amount, Utils.Rng.Next(1 + (int) (XP / 2), 4 + (int) (XP / 2)) );
            var drop = new DropComponent(Mob)
            {
                ItemDrop = gold,
                RandomDrop = false,
                DropChance = 50f
            };
            Mob.AddComponent(drop);
            

            foreach (DropTemplate template in Drops)
            {
                var type = (ItemType) Enum.Parse(typeof(ItemType), template.Type);

                drop = new DropComponent(Mob)
                {
                    DropChance = template.Chance,
                    RandomDrop = false,
                    ItemDrop = ItemPool.Grab(type)
                };
                Mob.AddComponent(drop);
            }
            Mob.AddComponent((EntityComponent)Activator.CreateInstance(AITable[this.AIType], Mob));
        }
    }
}
