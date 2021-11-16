using System;
using System.Linq;
using System.Reflection;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public abstract class Behaviour : IDisposable
    {
        protected Behaviour(IEntity Parent)
        {
            this.Parent = Parent;
        }

        protected IEntity Parent { get; }

        public virtual void Dispose()
        {
        }

        public void AlterBehaviour<T>(T NewBehaviour) where T : Behaviour
        {
            var types = GetFields(GetType());
            foreach (var field in types)
                if (field.FieldType.IsSubclassOf(typeof(T)) || typeof(T) == field.FieldType)
                    field.SetValue(this, NewBehaviour);
                else if (field.FieldType.IsSubclassOf(typeof(Behaviour)) || typeof(Behaviour) == field.FieldType)
                    (field.GetValue(this) as Behaviour).AlterBehaviour(NewBehaviour);
        }

        private static FieldInfo[] GetFields(Type Derived)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = Derived.GetFields(flags).ToArray();
            if (Derived.BaseType != typeof(Behaviour) && Derived.BaseType != null)
                return GetFields(Derived.BaseType).Concat(fields).ToArray();
            return fields;
        }

        public virtual void Update()
        {
        }
    }
}