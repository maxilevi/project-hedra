using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Item;

namespace Hedra.Engine.ModuleSystem
{
    public class CustomFactory : IEnemyFactory
    {

        public static Dictionary<string, Type> EffectTable;

        public string Name { get; set; }
        public float MaxHealth { get; set; }
        public float AttackDamage { get; set; }
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
            Mob.Speed = Speed;

            var ai = (AIType) Enum.Parse(typeof(AIType), this.AIType);
            Mob.AddComponent(new AIComponent(Mob, ai));
            Mob.SearchComponent<HealthBarComponent>().DistanceFromBase =
                (Mob.DefaultBox.Max.Y - Mob.DefaultBox.Min.Y) * 1.25f;

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

            if (Drops.Length == 0)
            {
                var drop = new DropComponent(Mob)
                {
                    DropChance = 12.5f,
                    RandomDrop = true,
                };
                Mob.AddComponent(drop);
            }

            foreach (DropTemplate template in Drops)
            {
                var type = (ItemType) Enum.Parse(typeof(ItemType), template.Type);
                var material = (Material) Enum.Parse(typeof(Material), template.Material);

                var drop = new DropComponent(Mob)
                {
                    DropChance = template.Chance,
                    RandomDrop = false,
                    ItemDrop = new InventoryItem(type, new ItemInfo(material, template.Amount))
                };
                Mob.AddComponent(drop);
            }
        }
    }
}
