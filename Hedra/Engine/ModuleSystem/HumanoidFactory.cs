using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Hedra.Engine.ClassSystem;
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


        public static Humanoid BuildHumanoid(string HumanoidType, int Level, HumanoidBehaviourTemplate Behaviour)
        {
            var template = HumanoidLoader.HumanoidTemplater[HumanoidType];
            var behaviour = Behaviour ?? _behaviours[template.Behaviour];

            var difficulty = GetDifficulty(Utils.Rng);
            var difficultyModifier = GetDifficultyModifier(difficulty);

            var human = new Humanoid
            {
                Level = Level,
                Class = ClassDesign.FromString(template.Class),
                MobType = MobType.Human
            };
            human.Model = new HumanoidModel(human, template.Model);
            human.Physics.CanCollide = true;
            human.Physics.HasCollision = true;
            human.Health = human.MaxHealth;

            var components = HumanoidLoader.ComponentsTemplater[HumanoidType].Components;
            for (var i = 0; i < components.Length; i++)
            {
                var type = Type.GetType(components[i].Type);
                var paramsList = new List<object>
                {
                    human
                };
                paramsList.AddRange(components[i].Parameters);

                var newComponent = (EntityComponent) Activator.CreateInstance(type, paramsList.ToArray());

                human.AddComponent(newComponent);
            }

            if (template.Weapons != null && template.Weapons.Length > 0)
            {
                var weapon = template.Weapons[Utils.Rng.Next(0, template.Weapons.Length)];
                human.Ring = ItemPool.Grab( new ItemPoolSettings(ItemTier.Common, "Ring"));
                human.MainWeapon = ItemPool.Grab( new ItemPoolSettings(weapon.Tier, weapon.Type) );

                var drop = new DropComponent(human)
                {
                    DropChance = 20,
                    RandomDrop = false,
                    ItemDrop = Utils.Rng.Next(0, 2) == 1 ? human.Ring : human.MainWeapon
                };
                human.AddComponent(drop);
            }

            var barComponent =
                new HealthBarComponent(human, behaviour.Name ?? template.DisplayName ?? template.Name)
                {
                    FontColor = behaviour.Color.ToColor()
                };
            human.AddComponent(barComponent);
            human.SearchComponent<DamageComponent>().Immune = template.Immune;
            human.SearchComponent<DamageComponent>().XpToGive = 6f;
            human.Removable = false;
            World.AddEntity(human);
            return human;
        }
        
        private static int GetDifficulty(Random Rng)
        {
            var levelN = Rng.Next(0, 10);
            var mobDifficulty = 1;
            if (levelN <= 4) return 1;
            if (levelN > 4 && levelN <= 7) return 2;
            if (levelN > 7 && levelN <= 9) return 3;
            throw new ArgumentOutOfRangeException($"Rng is not 0 < {levelN} < 10");
        }

        private static float GetDifficultyModifier(int DifficultyLevel)
        {
            switch (DifficultyLevel)
            {
                case 1:
                    return 1;
                case 2:
                    return 1.25f;
                case 3:
                    return 1.5f;
                default:
                    throw new ArgumentOutOfRangeException($"Mob difficulty level is not 1 <= {DifficultyLevel} <= 2");
            }
        }
    }
}
