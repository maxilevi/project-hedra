using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
    public class EntityComponentManager
    {
        private readonly Entity _parent;
        private readonly HashSet<EntityComponent> _components;

        public EntityComponentManager(Entity Parent)
        {
            _parent = Parent;
            _components = new HashSet<EntityComponent>();
        }

        public void AddComponent(EntityComponent Component)
        {
            if(_components.Contains(Component)) throw new ArgumentException("Provided component already exists.");
            _components.Add(Component);
            _parent.AddComponent(Component);
        }

        public void RemoveComponent(EntityComponent Component)
        {
            if(!_components.Contains(Component)) throw new KeyNotFoundException("Provided component does not exist.");
            _components.Remove(Component);
            _parent.RemoveComponent(Component);
        }

        public void AddComponentWhile(EntityComponent Component, Func<bool> Condition)
        {
            var name = _parent.Name.Clone();
            Func<bool> realCondition = () => Condition() && _parent.Name == name;
            CoroutineManager.StartCoroutine(this.WhileCoroutine, Component, realCondition);
        }

        private IEnumerator WhileCoroutine(object[] Params)
        {
            var component = (EntityComponent) Params[0];
            var condition = (Func<bool>) Params[1];

            this.AddComponent(component);
            while (condition())
            {
                yield return null;
            }
            this.RemoveComponent(component);
        }
    }
}
