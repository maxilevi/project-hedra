using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.AISystem
{
    public abstract class BasicAIComponent : EntityComponent, IBehaviourComponent, ITraverseAIComponent
    {
        public bool Enabled { get; set; }
        public abstract AIType Type { get; }
        private Behaviour[] _behaviours;

        protected BasicAIComponent(IEntity Parent) : base(Parent)
        {

        }

        public void AlterBehaviour<T>(T NewBehaviour) where T : Behaviour
        {
            var type = this.GetType();
            var fields = GetFields(type);
            foreach (FieldInfo field in fields)
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

        public Vector2 GridSize
        {
            get => SearchBehaviour<TraverseBehaviour>().GridSize;
            set => SearchBehaviour<TraverseBehaviour>().ResizeGrid(value);
        }

        public T SearchBehaviour<T>() where T : Behaviour
        {
            if(_behaviours == null)
                _behaviours = BuildMappings(GetType(), this);
            for (var i = 0; i < _behaviours.Length; i++)
                if (_behaviours[i] is T variable)
                    return variable;
            return default(T);
        }

        private Behaviour[] BuildMappings(Type SearchType, object Instance)
        {
            var behaviours = new List<Behaviour>();
            var fields = GetFields(SearchType);
            for(var i = 0; i < fields.Length; i++)
            {
                if (fields[i].FieldType.IsSubclassOf(typeof(Behaviour)))
                {
                    var instance = fields[i].GetValue(Instance);
                    behaviours.Add(instance as Behaviour);
                    behaviours.AddRange(BuildMappings(fields[i].FieldType, instance));
                }
            }
            return behaviours.ToArray();
        }

        private static FieldInfo[] GetFields(Type Derived)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = Derived.GetFields(flags).ToArray();
            if (Derived.BaseType != typeof(BasicAIComponent) && Derived.BaseType != null)
            {
                return GetFields(Derived.BaseType).Concat(fields).ToArray();
            }
            return fields;
        }
    }
}
