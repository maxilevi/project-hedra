using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public class EntityComponentManager
    {
        private readonly HashSet<IComponent<IEntity>> _components;
        private readonly IEntity _parent;

        public EntityComponentManager(IEntity Parent)
        {
            _parent = Parent;
            _components = new HashSet<IComponent<IEntity>>();
        }

        public void AddComponent(IComponent<IEntity> Component)
        {
            if (_components.Contains(Component)) throw new ArgumentException("Provided component already exists.");
            _components.Add(Component);
            _parent.AddComponent(Component);
        }

        public void RemoveComponent(IComponent<IEntity> Component)
        {
            if (!_components.Contains(Component)) throw new KeyNotFoundException("Provided component does not exist.");
            _components.Remove(Component);
            _parent.RemoveComponent(Component);
        }

        public void AddComponentWhile(IComponent<IEntity> Component, Func<bool> Condition)
        {
            var name = _parent.Name.Clone();

            bool RealCondition()
            {
                return Condition() && _parent.Name == name;
            }

            RoutineManager.StartRoutine(WhileCoroutine, Component, (Func<bool>)RealCondition);
        }

        private bool ContainsComponent(IComponent<IEntity> Component)
        {
            return _components.Contains(Component);
        }

        public void Clear()
        {
            var componentsCopy = new List<IComponent<IEntity>>(_components);
            foreach (var component in componentsCopy) RemoveComponent(component);
        }

        public void AddComponentForSeconds(IComponent<IEntity> Component, float Seconds)
        {
            RoutineManager.StartRoutine(ForCoroutine, Component, Seconds);
        }

        private IEnumerator ForCoroutine(object[] Params)
        {
            var component = (IComponent<IEntity>)Params[0];
            var seconds = (float)Params[1];

            AddComponent(component);
            var k = 0f;
            while (k < seconds)
            {
                k += Time.DeltaTime;
                yield return null;
            }

            if (!ContainsComponent(component)) yield break;
            RemoveComponent(component);
        }

        private IEnumerator WhileCoroutine(object[] Params)
        {
            var component = (IComponent<IEntity>)Params[0];
            var condition = (Func<bool>)Params[1];

            AddComponent(component);
            while (condition()) yield return null;
            if (!ContainsComponent(component)) yield break;
            RemoveComponent(component);
        }
    }
}