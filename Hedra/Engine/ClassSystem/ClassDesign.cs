using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.API;
using Hedra.Engine.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using OpenTK;

namespace Hedra.Engine.ClassSystem
{
    public abstract class ClassDesign
    {
        public static Type[] AvailableClasses { get; }
        public static string[] ClassNames { get; }
        private static readonly Dictionary<string, Type> ClassMap;

        static ClassDesign()
        {
            var classes = Assembly.GetExecutingAssembly().GetLoadableTypes()
                .Where(T => T.IsSubclassOf(typeof(ClassDesign))).ToArray();
            ClassMap = classes.ToDictionary( T => FromType(T).ToString(), T => T);
            AvailableClasses = classes.Where(T => !Attribute.IsDefined(T, typeof(HiddenClassAttribute))).ToArray();
            ClassNames = ClassMap.Keys.ToArray();
        }

        public string Name => this.ToString();
        public abstract string Logo { get; }
        public abstract HumanType Human { get; }
        public abstract float BaseSpeed { get; }
        public abstract AbilityTreeBlueprint AbilityTreeDesign { get; }
        public abstract Item StartingItem { get; }
        public abstract float AttackResistance { get; }
        public abstract float MaxStamina { get; }
        public abstract float BaseDamage { get; }
        public abstract Matrix4 HelmetPlacement { get; }
        public abstract Matrix4 ChestplatePlacement { get; }
        public abstract Matrix4 PantsMatrixPlacement { get; }
        public abstract Matrix4 LeftBootPlacement { get; }
        public abstract Matrix4 RightBootPlacement { get; }
        public abstract Class Type { get; }

        public abstract float MaxHealthFormula(float RandomFactor);
        public abstract float MaxManaFormula(float RandomFactor);
       
        public float XPFormula(int TargetLevel)
        {
            return TargetLevel * 10f + 38;
        }
        
        public override string ToString()
        {
            return ClassDesign.ToString(this);
        }

        public static ClassDesign FromString(string Class)
        {
            if(!ClassMap.ContainsKey(Class))
                throw new ArgumentOutOfRangeException($"Provided argument '{Class}' is an invalid class type.");
            return ClassDesign.FromType(ClassMap[Class]);
        }

        public static ClassDesign FromType(Type Type)
        {
            return (ClassDesign)Activator.CreateInstance(Type);
        }

        public static string ToString(ClassDesign ClassDesign)
        {
            return ClassDesign.Type.ToString();
        }

        public static ClassDesign None { get; } = new NoneDesign();
    }
}
