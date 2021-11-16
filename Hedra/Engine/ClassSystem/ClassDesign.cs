using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Hedra.API;
using Hedra.Engine.ClassSystem.Templates;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Items;
using Hedra.Rendering;

namespace Hedra.Engine.ClassSystem
{
    public abstract class ClassDesign
    {
        private static readonly Dictionary<string, Type> ClassMap;

        static ClassDesign()
        {
            var classes = Assembly.GetExecutingAssembly().GetLoadableTypes()
                .Where(T => T.IsSubclassOf(typeof(ClassDesign))).ToArray();
            ClassMap = classes.ToDictionary(T => FromType(T).ToString(), T => T);
            AvailableClasses = classes.Where(T => !Attribute.IsDefined(T, typeof(HiddenClassAttribute))).ToArray();
            ClassNames = ClassMap.Keys.ToArray();
            AvailableClassNames = AvailableClasses.Select(T => FromType(T).ToString()).ToArray();
        }

        public static Type[] AvailableClasses { get; }
        public static string[] ClassNames { get; }
        public static string[] AvailableClassNames { get; }

        public string Name => ToString();
        public virtual HumanoidModelTemplate HeadModelTemplate => ClassLoader.Instance[Type].HeadModel;
        public virtual HumanoidModelTemplate ChestModelTemplate => ClassLoader.Instance[Type].ChestModel;
        public virtual HumanoidModelTemplate LegsModelTemplate => ClassLoader.Instance[Type].LegsModel;
        public virtual HumanoidModelTemplate FeetModelTemplate => ClassLoader.Instance[Type].FeetModel;
        public virtual HumanoidModelTemplate FemaleHeadModelTemplate => ClassLoader.Instance[Type].FemaleHeadModel;
        public virtual HumanoidModelTemplate FemaleChestModelTemplate => ClassLoader.Instance[Type].FemaleChestModel;
        public virtual HumanoidModelTemplate FemaleLegsModelTemplate => ClassLoader.Instance[Type].FemaleLegsModel;
        public virtual HumanoidModelTemplate FemaleFeetModelTemplate => ClassLoader.Instance[Type].FemaleFeetModel;
        public virtual HumanoidModelTemplate ModelTemplate => ClassLoader.Instance[Type].Model;
        public virtual Vector4 DefaultSkinColor => Colors.FromHtml(ClassLoader.Instance[Type].DefaultSkinColor);

        public virtual Vector4 DefaultFirstHairColor =>
            Colors.FromHtml(ClassLoader.Instance[Type].DefaultFirstHairColor);

        public bool HasSecondHairColor => ClassLoader.Instance[Type].DefaultSecondHairColor != null;

        public virtual Vector4 DefaultSecondHairColor => HasSecondHairColor
            ? Colors.FromHtml(ClassLoader.Instance[Type].DefaultSecondHairColor)
            : Vector4.One;

        public virtual Vector4 FemaleDefaultSkinColor =>
            Colors.FromHtml(ClassLoader.Instance[Type].FemaleDefaultSkinColor);

        public virtual Vector4 FemaleDefaultFirstHairColor =>
            Colors.FromHtml(ClassLoader.Instance[Type].FemaleDefaultFirstHairColor);

        public bool HasSecondFemaleHairColor => ClassLoader.Instance[Type].FemaleDefaultSecondHairColor != null;

        public virtual Vector4 FemaleDefaultSecondHairColor => HasSecondFemaleHairColor
            ? Colors.FromHtml(ClassLoader.Instance[Type].FemaleDefaultSecondHairColor)
            : Vector4.One;

        public virtual string Logo => ClassLoader.Instance[Type].Logo;
        public bool IsRanged => ClassLoader.Instance[Type].IsRanged;
        public virtual float BaseSpeed => ClassLoader.Instance[Type].BaseSpeed;

        public virtual AbilityTreeBlueprint MainTree =>
            AbilityTreeLoader.Instance[ClassLoader.Instance[Type].MainAbilityTree];

        public virtual AbilityTreeBlueprint FirstSpecializationTree =>
            AbilityTreeLoader.Instance[ClassLoader.Instance[Type].FirstSpecializationTree];

        public virtual AbilityTreeBlueprint SecondSpecializationTree =>
            AbilityTreeLoader.Instance[ClassLoader.Instance[Type].SecondSpecializationTree];

        public virtual KeyValuePair<int, Item>[] StartingItems =>
            ClassLoader.Instance[Type].StartingItems.Select(ParseStartingItems).ToArray();

        public virtual Item[] StartingRecipes =>
            ClassLoader.Instance[Type].StartingRecipes.Select(S => ItemPool.Grab(S)).ToArray();

        public virtual float AttackResistance => ClassLoader.Instance[Type].AttackResistance;
        public virtual float MaxStamina => ClassLoader.Instance[Type].MaxStamina;
        public virtual float BaseDamage => ClassLoader.Instance[Type].BaseDamage;
        public virtual float BaseHealth => ClassLoader.Instance[Type].BaseHealth;
        public virtual float AttackingSpeedModifier => ClassLoader.Instance[Type].AttackingSpeedModifier;
        public abstract Class Type { get; }

        public static ClassDesign None { get; } = new NoneDesign();

        private static KeyValuePair<int, Item> ParseStartingItems(StartingItemTemplate Template)
        {
            var item = ItemPool.Grab(Template.Name);
            var amount = Template.Amount;
            if (amount > 1)
                item.SetAttribute(CommonAttributes.Amount, amount);
            return new KeyValuePair<int, Item>(Template.Index, item);
        }

        public virtual float MaxHealthFormula(float RandomFactor)
        {
            return ClassLoader.Instance[Type].BaseHealthPerLevel + RandomFactor;
        }

        public virtual float MaxManaFormula(float RandomFactor)
        {
            return ClassLoader.Instance[Type].BaseManaPerLevel + RandomFactor;
        }

        public static float XPFormula(int TargetLevel)
        {
            return (float)Math.Pow(TargetLevel, 1.05f) * 10f + 38;
        }

        public override string ToString()
        {
            return ToString(this);
        }

        public static ClassDesign FromString(string Class)
        {
            if (!ClassMap.ContainsKey(Class))
                throw new ArgumentOutOfRangeException($"Provided argument '{Class}' is an invalid class type.");
            return FromType(ClassMap[Class]);
        }

        public static ClassDesign FromString(Class ClassType)
        {
            return FromString(ClassType.ToString());
        }

        public static ClassDesign FromType(Type Type)
        {
            return (ClassDesign)Activator.CreateInstance(Type);
        }

        public static string ToString(ClassDesign ClassDesign)
        {
            return ClassDesign.Type.ToString();
        }
    }
}