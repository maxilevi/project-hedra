﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Item;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering.UI;
using OpenTK;

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

            var human = new Humanoid();
            human.ClassType = (Class) Enum.Parse(typeof(Class), template.Class);
            human.Level = LocalPlayer.Instance.Level + (Utils.Rng.Next(0, 2) - 1) + 1;
            human.AddonHealth = human.MaxHealth;
            human.Level = 1;
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
                var weaponType = (ItemType) Enum.Parse(typeof(ItemType), weapon.Type);
                bool randomInfo = weapon.Material.Equals("Random", StringComparison.InvariantCultureIgnoreCase);
                var mat = Material.Copper;
                if (!randomInfo)
                    mat = (Material) Enum.Parse(typeof(Material), weapon.Material);
                ItemInfo info = randomInfo ? ItemInfo.Random(weaponType) : new ItemInfo(mat, weapon.Damage);

                human.Ring = new InventoryItem(ItemType.Ring, ItemInfo.Random(ItemType.Ring));

                human.MainWeapon = new InventoryItem(weaponType, info);
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
                (Math.Abs(human.DefaultBox.Min.Y) + Math.Abs(human.DefaultBox.Max.Y)) * .5f;

            human.SearchComponent<DamageComponent>().Immune = template.Immune;

            World.AddEntity(human);
            return human;
        }
    }
}
