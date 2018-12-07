using System;

namespace Hedra.API
{
    public abstract class Mod
    {
        private readonly WeaponRegistry _weaponRegistry;
        private readonly AnimationEventRegistry _animationEventRegistry;
        private readonly AIRegistry _aiRegistry;
        
        public abstract string Name { get; }
        
        protected Mod()
        {
            _weaponRegistry = new WeaponRegistry();
            _animationEventRegistry = new AnimationEventRegistry();
            _aiRegistry = new AIRegistry();
        }
        
        public abstract void RegisterContent();
        
        protected void AddAIType(string Name, Type ClassType)
        {
            _aiRegistry.Add(Name, ClassType);
        }
        
        protected void AddWeaponType(string Name, Type ClassType)
        {
            _weaponRegistry.Add(Name, ClassType);
        }

        protected void AddAnimationEvent(string Name, Type ClassType)
        {
            _animationEventRegistry.Add(Name, ClassType);
        }

        public void UnregisterContent()
        {
            _weaponRegistry.Unregister();
            _animationEventRegistry.Unregister();
            _aiRegistry.Unregister();
        }
    }
}