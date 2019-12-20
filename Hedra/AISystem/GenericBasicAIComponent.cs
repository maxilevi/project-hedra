using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Engine.Rendering;

namespace Hedra.AISystem
{
    public abstract class GenericBasicAIComponent<T> : Component<T>, IBehaviouralAI, ITraverseAIComponent, IBasicAIComponent where T : IEntity
    {
        public bool Enabled { get; set; }
        public abstract AIType Type { get; }
        private Behaviour[] _behaviours;

        protected GenericBasicAIComponent(T Parent) : base(Parent)
        {

        }

        protected void DrawDebugCollision()
        {
            DrawDebugCollision(Parent);
        }

        public static void DrawDebugCollision(IEntity Parent)
        {
            var grid = TraverseStorage.Instance[Parent];
            grid.Draw();
        }

        public void AlterBehaviour<S>(S NewBehaviour) where S : Behaviour
        {
            var type = this.GetType();
            var fields = GetFields(type);
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsSubclassOf(typeof(S)) || typeof(S) == field.FieldType)
                {
                    field.SetValue(this, NewBehaviour);
                }
                else if (field.FieldType.IsSubclassOf(typeof(Behaviour)) || typeof(Behaviour) == field.FieldType)
                {
                    (field.GetValue(this) as Behaviour).AlterBehaviour<S>(NewBehaviour);
                }
                else if (field.FieldType.IsSubclassOf(typeof(GenericBasicAIComponent<T>)) || typeof(GenericBasicAIComponent<T>) == field.FieldType)
                {
                    (field.GetValue(this) as GenericBasicAIComponent<T>).AlterBehaviour<S>(NewBehaviour);
                }
            }
        }

        public Vector2 GridSize
        {
            get => SearchBehaviour<TraverseBehaviour>().GridSize;
            set => SearchBehaviour<TraverseBehaviour>().ResizeGrid(value);
        }

        public Vector3 TargetPoint { get; set; }

        public T SearchBehaviour<T>() where T : Behaviour
        {
            if (_behaviours == null)
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
            for (var i = 0; i < fields.Length; i++)
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