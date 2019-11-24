using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Hedra.AISystem.Humanoid;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.ClassSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.WeaponSystem;
using Microsoft.Scripting;

namespace Hedra.Engine.ModuleSystem
{
    public static class HumanoidFactory
    {
        private static Dictionary<string, HumanoidConfiguration> _behaviours;
        private static Dictionary<string, Type> _ais;
        
        static HumanoidFactory()
        {
            _behaviours = new Dictionary<string, HumanoidConfiguration>
            {
                {"Hostile", new HumanoidConfiguration(HealthBarType.Hostile)},
                {"Neutral", new HumanoidConfiguration(HealthBarType.Neutral)},
                {"Friendly", new HumanoidConfiguration(HealthBarType.Friendly)}
            };
            _ais = new Dictionary<string, Type>
            {
                {"Melee", typeof(MeleeAIComponent)},
                {"Archer", typeof(RangedAIComponent)},
                {"Mage", typeof(MageAIComponent)}
            };
        }

        public static Humanoid BuildHumanoid(string HumanoidType, int Level, HumanoidConfiguration Configuration)
        {
            return BuildHumanoid(HumanoidType, HumanoidLoader.HumanoidTemplater[HumanoidType], Level, Configuration);
        }
        
        public static Humanoid BuildHumanoid(string HumanoidType, HumanoidTemplate Template, int Level, HumanoidConfiguration Configuration)
        {
            var behaviour = Configuration ?? _behaviours[Template.Behaviour];

            var difficulty = GetDifficulty(Utils.Rng);

            var human = new Humanoid
            {
                Level = Level,
                Class = ClassDesign.FromString(Template.Class),
                Type = HumanoidType
            };
            human.Model = new HumanoidModel(human, Template.RandomModel);
            human.Physics.CollidesWithStructures = true;
            human.Physics.CollidesWithEntities = true;
            human.Health = human.MaxHealth;

            var components = HumanoidLoader.HumanoidTemplater[HumanoidType].Components;
            for (var i = 0; i < components.Length; i++)
            {
                human.AddComponent(CreateComponentFromTemplate(human, components[i]));
            }

            if (Template.Weapons != null && Template.Weapons.Length > 0)
            {
                var weapon = Template.Weapons[Utils.Rng.Next(0, Template.Weapons.Length)];
                human.Ring = ItemPool.Grab( new ItemPoolSettings(ItemTier.Common, "Ring"));
                human.MainWeapon = weapon.Name != null ?  ItemPool.Grab(weapon.Name) : ItemPool.Grab( new ItemPoolSettings(weapon.Tier, weapon.Type));

                var drop = new DropComponent(human)
                {
                    DropChance = 20,
                    RandomDrop = false,
                    ItemDrop = Utils.Rng.Next(0, 2) == 1 ? human.Ring : human.MainWeapon
                };
                human.AddComponent(drop);
            }
    
            human.AddComponent(new HealthBarComponent(human, Template.DisplayName ?? Template.Name, behaviour.Type));
            human.SearchComponent<DamageComponent>().Immune = Template.Immune;
            human.SearchComponent<DamageComponent>().XpToGive = 6f;
            human.Removable = false;
            World.AddEntity(human);
            return human;
        }

        public static void AddAI(IHumanoid Humanoid, bool Friendly)
        {
            var aiType = (Humanoid.LeftWeapon.IsMelee ? "Melee" : Humanoid.LeftWeapon is Staff ? "Mage" : "Archer");
            var instance = (Component<IHumanoid>) Activator.CreateInstance(_ais[aiType], Humanoid, Friendly);
            Humanoid.AddComponent(instance);
        }

        private static IComponent<IEntity> CreateComponentFromTemplate(IHumanoid Human, HumanoidComponentsItemTemplate Template)
        {
            var paramsList = new List<object>(new[] {Human});
            paramsList.AddRange(Template.Parameters);
            var type = Type.GetType(Template.Type); 
            var component = (IComponent<IEntity>) Activator.CreateInstance(type, paramsList.ToArray()); 
            return component;
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
