using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;

namespace Hedra.Engine.ModuleSystem
{
    public static class HumanoidFactory
    {
        private static Dictionary<string, HumanoidBehaviourTemplate> _behaviours;

        static HumanoidFactory()
        {
            _behaviours = new Dictionary<string, HumanoidBehaviourTemplate>
            {
                {"Hostile", new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Hostile)},
                {"Neutral", new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Neutral)},
                {"Friendly", new HumanoidBehaviourTemplate(HumanoidBehaviourTemplate.Friendly)}
            };
        }


        public static Humanoid BuildHumanoid(string HumanoidType, HumanoidBehaviourTemplate Behaviour)
        {
            var template = HumanoidLoader.HumanoidTemplater[HumanoidType];
            var behaviour = Behaviour ?? _behaviours[template.Behaviour];

            int levelN = Utils.Rng.Next(0, 10);
            var difficultyType = 1f;
            if (levelN <= 4) difficultyType = 1f;
            else if (levelN > 4 && levelN <= 7) difficultyType = 1.5f;
            else if (levelN > 7 && levelN <= 9) difficultyType = 2f;

            var human = new Humanoid
            {
                Level = (int) ((LocalPlayer.Instance.Level + (Utils.Rng.Next(0, 2) - 1) + 1) * difficultyType),
                ClassType = (Class) Enum.Parse(typeof(Class), template.Class)
            };
            if (human.ClassType == Class.None)
            {
                human.MaxHealth = template.MaxHealth * human.Level * .5f + template.MaxHealth;
            }
            else
            {
                human.AddonHealth = human.MaxHealth * .5f;
            }
            human.MobType = MobType.Human;
            human.Speed = template.Speed;
            human.Model = new HumanModel(human, template.Model);
            human.Physics.CanCollide = true;
            human.Physics.HasCollision = true;
            human.Health = human.MaxHealth;

            var components = HumanoidLoader.ComponentsTemplater[HumanoidType].Components;
            for (int i = 0; i < components.Length; i++)
            {
                Type type = Type.GetType(components[i].Type);
                var paramsList = new List<object>();
                paramsList.Add(human);
                paramsList.AddRange(components[i].Parameters);

                var newComponent = (EntityComponent) Activator.CreateInstance(type, paramsList.ToArray());

                human.AddComponent(newComponent);
            }

            if (template.Weapons != null && template.Weapons.Length > 0)
            {
                var weapon = template.Weapons[Utils.Rng.Next(0, template.Weapons.Length)];
                human.Ring = ItemPool.Grab( new ItemPoolSettings(ItemTier.Common, "Ring"));

                human.MainWeapon = ItemPool.Grab( new ItemPoolSettings(weapon.Tier, weapon.Type) );
                human.Model.SetWeapon(human.MainWeapon.Weapon);

                var drop = new DropComponent(human)
                {
                    DropChance = 20,
                    RandomDrop = false,
                    ItemDrop = Utils.Rng.Next(0, 2) == 1 ? human.Ring : human.MainWeapon
                };
                human.AddComponent(drop);
            }

            var barComponent = new HealthBarComponent(human, behaviour.Name ?? template.DisplayName ?? template.Name);
            barComponent.FontColor = behaviour.Color.ToColor();
            human.AddComponent(barComponent);

            human.SearchComponent<HealthBarComponent>().DistanceFromBase = 
                human.DefaultBox.Max.Y - human.DefaultBox.Min.Y + .5f;

            human.SearchComponent<DamageComponent>().Immune = template.Immune;
            human.SearchComponent<DamageComponent>().XpToGive = Math.Max(2f, (float)Math.Round(template.XP * human.Level * .65f));
            World.AddEntity(human);
            return human;
        }
    }
}
