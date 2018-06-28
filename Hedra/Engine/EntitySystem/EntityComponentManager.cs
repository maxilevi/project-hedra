using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
    internal class EntityComponentManager
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

        public void AddComponentForSeconds(EntityComponent Component, float Seconds)
        {
            CoroutineManager.StartCoroutine(this.ForCoroutine, Component, Seconds);
        }

        private IEnumerator ForCoroutine(object[] Params)
        {
            var component = (EntityComponent)Params[0];
            var seconds = (float)Params[1];

            this.AddComponent(component);
            var k = 0f;
            while (k < seconds)
            {
                k += Time.ScaledFrameTimeSeconds;
                yield return null;
            }
            this.RemoveComponent(component);
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
