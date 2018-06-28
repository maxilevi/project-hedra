using System.Collections.Generic;
using System.Reflection;
using Hedra.Engine.AISystem.Behaviours;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    internal abstract class BasicAIComponent : EntityComponent
    {
        public bool Enabled { get; set; }
        private Behaviour[] _behaviours;

        protected BasicAIComponent(Entity Parent) : base(Parent)
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
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            foreach (FieldInfo field in this.GetType().GetFields(flags))
            {
                if (field.FieldType.IsSubclassOf(typeof(Behaviour)))
                {
                    behaviours.Add(field.GetValue(this) as Behaviour);
                }
            }
            _behaviours = behaviours.ToArray();
        }
    }
}
