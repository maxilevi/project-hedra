using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.AISystem
{
    public abstract class BasicAIComponent : EntityComponent
    {
        public bool Enabled { get; set; }
        private Behaviour[] _behaviours;

        protected BasicAIComponent(IEntity Parent) : base(Parent)
        {

        }

        public void AlterBehaviour<T>(T NewBehaviour) where T : Behaviour
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in this.GetType().GetFields(flags))
            {
                if (field.FieldType.IsSubclassOf(typeof(T)) || typeof(T) == field.FieldType)
                {
                    field.SetValue(this, NewBehaviour);
                }
                else if(field.FieldType.IsSubclassOf(typeof(Behaviour)) || typeof(Behaviour) == field.FieldType)
                {
                    (field.GetValue(this) as Behaviour).AlterBehaviour<T>(NewBehaviour);
                }
            }
        }

        public T SearchBehaviour<T>() where T : Behaviour
        {
            this.BuildMappings();
            for (var i = 0; i < _behaviours.Length; i++)
                if (_behaviours[i] is T variable)
                    return variable;
            return default(T);
        }

        private void BuildMappings()
        {
            if(_behaviours != null) return;

            var behaviours = new List<Behaviour>();
            var type = this.GetType();
            var fields = GetFields(type);
            for(var i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType.IsSubclassOf(typeof(Behaviour)))
                {
                    behaviours.Add(fields[i].GetValue(this) as Behaviour);
                }
            }
            _behaviours = behaviours.ToArray();
        }

        private static FieldInfo[] GetFields(Type Derived)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = Derived.GetFields(flags).ToArray();
            if (Derived.BaseType != typeof(BasicAIComponent))
            {
                return GetFields(Derived.BaseType).Concat(fields).ToArray();
            }
            return fields;
        }
    }
}
