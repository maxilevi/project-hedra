using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public abstract class GenericBasicAIComponent<T> : Component<T>, IBehaviouralAI, ITraverseAIComponent,
        IBasicAIComponent where T : IEntity
    {
        private Behaviour[] _behaviours;

        protected GenericBasicAIComponent(T Parent) : base(Parent)
        {
        }

        public bool Enabled { get; set; }
        public abstract AIType Type { get; }

        public void AlterBehaviour<S>(S NewBehaviour) where S : Behaviour
        {
            var type = GetType();
            var fields = GetFields(type);
            foreach (var field in fields)
                if (field.FieldType.IsSubclassOf(typeof(S)) || typeof(S) == field.FieldType)
                {
                    var previous = (Behaviour)field.GetValue(this);
                    previous.Dispose();
                    field.SetValue(this, NewBehaviour);
                }
                else if (field.FieldType.IsSubclassOf(typeof(Behaviour)) || typeof(Behaviour) == field.FieldType)
                {
                    (field.GetValue(this) as Behaviour).AlterBehaviour(NewBehaviour);
                }
                else if (field.FieldType.IsSubclassOf(typeof(GenericBasicAIComponent<T>)) ||
                         typeof(GenericBasicAIComponent<T>) == field.FieldType)
                {
                    (field.GetValue(this) as GenericBasicAIComponent<T>).AlterBehaviour(NewBehaviour);
                }
        }

        public T SearchBehaviour<T>() where T : Behaviour
        {
            if (_behaviours == null)
                _behaviours = BuildMappings(GetType(), this);
            for (var i = 0; i < _behaviours.Length; i++)
                if (_behaviours[i] is T variable)
                    return variable;
            return default;
        }

        public Vector2 GridSize
        {
            get => SearchBehaviour<TraverseBehaviour>().GridSize;
            set => SearchBehaviour<TraverseBehaviour>().ResizeGrid(value);
        }

        public Vector3 TargetPoint { get; set; }

        protected void DrawDebugCollision()
        {
            DrawDebugCollision(Parent);
        }

        public static void DrawDebugCollision(IEntity Parent)
        {
            var grid = TraverseStorage.Instance[Parent];
            grid.Draw();
        }

        private Behaviour[] BuildMappings(Type SearchType, object Instance)
        {
            var behaviours = new List<Behaviour>();
            var fields = GetFields(SearchType);
            for (var i = 0; i < fields.Length; i++)
                if (fields[i].FieldType.IsSubclassOf(typeof(Behaviour)))
                {
                    var instance = fields[i].GetValue(Instance);
                    behaviours.Add(instance as Behaviour);
                    behaviours.AddRange(BuildMappings(fields[i].FieldType, instance));
                }

            return behaviours.ToArray();
        }

        private static FieldInfo[] GetFields(Type Derived)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = Derived.GetFields(flags).ToArray();
            if (Derived.BaseType != typeof(BasicAIComponent) && Derived.BaseType != null)
                return GetFields(Derived.BaseType).Concat(fields).ToArray();

            return fields;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}