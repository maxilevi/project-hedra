using System.Reflection;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem.Behaviours
{
    internal abstract class Behaviour
    {
        protected Entity Parent { get; }

        protected Behaviour(Entity Parent)
        {
            this.Parent = Parent;
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

        public virtual void Update() { }
    }
}
