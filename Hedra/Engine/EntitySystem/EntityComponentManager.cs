using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
    public class EntityComponentManager
    {
        private readonly IEntity _parent;
        private readonly HashSet<EntityComponent> _components;

        public EntityComponentManager(IEntity Parent)
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
            bool RealCondition() => Condition() && _parent.Name == name;
            CoroutineManager.StartCoroutine(this.WhileCoroutine, Component, (Func<bool>) RealCondition);
        }

        private bool ContainsComponent(EntityComponent Component)
        {
            return _components.Contains(Component);
        }

        public void Clear()
        {
            var componentsCopy = new List<EntityComponent>(_components);
            foreach (var component in componentsCopy)
            {
                this.RemoveComponent(component);
            }
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
                k += Time.DeltaTime;
                yield return null;
            }
            if (!this.ContainsComponent(component)) yield break;
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
            if(!this.ContainsComponent(component)) yield break;
            this.RemoveComponent(component);
        }
    }
}
