using System.Reflection;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public abstract class Behaviour
    {
        protected IEntity Parent { get; }

        protected Behaviour(IEntity Parent)
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
