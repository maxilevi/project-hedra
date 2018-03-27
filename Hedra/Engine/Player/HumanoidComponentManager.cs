using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.Player
{
    public class HumanoidComponentManager
    {
        private readonly Humanoid _humanoid;
        private readonly HashSet<EntityComponent> _components;

        public HumanoidComponentManager(Humanoid Humanoid)
        {
            _humanoid = Humanoid;
            _components = new HashSet<EntityComponent>();
        }

        public void AddComponent(EntityComponent Component)
        {
            if(_components.Contains(Component)) throw new ArgumentException("Provided component already exists.");
            _components.Add(Component);
            _humanoid.AddComponent(Component);
        }

        public void RemoveComponent(EntityComponent Component)
        {
            if(!_components.Contains(Component)) throw new KeyNotFoundException("Provided component does not exist.");
            _components.Remove(Component);
            _humanoid.RemoveComponent(Component);
        }

        public void AddComponentWhile(EntityComponent Component, Func<bool> Condition)
        {
            CoroutineManager.StartCoroutine(this.WhileCoroutine, Component, Condition);
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
