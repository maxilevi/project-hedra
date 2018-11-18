using System;
using System.Linq;
using System.Reflection;
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

        static ClassDesign()
        {
            AvailableClasses = Reflection.GetLoadableTypes(Assembly.GetExecutingAssembly(), typeof(ClassDesign).Namespace)
                .Where(T => T.IsSubclassOf(typeof(ClassDesign)) ).Where(T => !Attribute.IsDefined(T, typeof(HiddenClassAttribute)) ).ToArray();
            ClassNames = AvailableClasses.Select(ClassDesign.ToString).ToArray();
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
            var fullName = $"{typeof(ClassDesign).Namespace}.{(Class.EndsWith("Design") ? Class : Class + "Design")}";
            var type = Type.GetType(fullName);
            if (type == null) throw new ArgumentNullException($"Provided argument class '{Class}' cannot be null.");
            return ClassDesign.FromType(type);
        }

        public static ClassDesign FromType(Type Type)
        {
            return (ClassDesign)Activator.CreateInstance(Type);
        }

        public static string ToString(ClassDesign ClassDesign)
        {
            return ClassDesign.ToString(ClassDesign.GetType());
        }

        public static string ToString(Type ClassDesign)
        {
            return ClassDesign.Name.Replace("Design", string.Empty);
        }

        public static ClassDesign None { get; } = new NoneDesign();
    }
}
