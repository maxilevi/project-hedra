using System;
using System.Linq;
using System.Reflection;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.ClassSystem
{
    internal abstract class ClassDesign
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
        public abstract uint Logo { get; }
        public abstract HumanType Human { get; }
        public abstract float BaseSpeed { get; }
        public abstract AbilityTreeBlueprint AbilityTreeDesign { get; }
        public abstract Item StartingItem { get; }
        public abstract float AttackResistance { get; }

        public abstract float MaxHealthFormula(float RandomFactor);
        public abstract float MaxManaFormula(float RandomFactor);

        public override string ToString()
        {
            return ClassDesign.ToString(this);
        }

        public static ClassDesign FromString(string Class)
        {
            var fullName = $"{typeof(ClassDesign).Namespace}.{(Class.EndsWith("Design") ? Class : Class + "Design")}";
            var type = Type.GetType(fullName);
            if (type == null) throw new ArgumentNullException("Provided Argument CLASS cannot be null.");
            return ClassDesign.FromType(type);
        }

        internal static ClassDesign FromType(Type Type)
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

        internal static ClassDesign None { get; } = new NoneDesign();
    }
}
