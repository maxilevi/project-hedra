using System;

namespace Hedra.API
{
    public abstract class Mod
    {
        private readonly WeaponRegistry _weaponRegistry;
        private readonly AnimationEventRegistry _animationEventRegistry;
        private readonly AIRegistry _aiRegistry;
        private readonly ClassRestrictionRegistry _classRegistry;
        private readonly ModelHandlerRegistry _modelHandlerRegistry;
        private readonly ItemHandlerRegistry _itemHandlerRegistry;
        
        public abstract string Name { get; }
        
        protected Mod()
        {
            _weaponRegistry = new WeaponRegistry();
            _animationEventRegistry = new AnimationEventRegistry();
            _aiRegistry = new AIRegistry();
            _classRegistry = new ClassRestrictionRegistry();
            _modelHandlerRegistry = new ModelHandlerRegistry();
            _itemHandlerRegistry = new ItemHandlerRegistry();
        }
        
        protected abstract void RegisterContent();

        public void Load()
        {
            UnregisterContent();
            RegisterContent();
        }

        protected void AddClassRestriction(Class Class, string EquipmentType)
        {
            _classRegistry.Add(Class, EquipmentType);
        }
        
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
        
        protected void AddModelHandler(string Name, Type ClassType)
        {
            _modelHandlerRegistry.Add(Name, ClassType);
        }
        
        protected void AddItemHandler(string Name, Type ClassType)
        {
            _itemHandlerRegistry.Add(Name, ClassType);
        }

        public void UnregisterContent()
        {
            _weaponRegistry.Unregister();
            _animationEventRegistry.Unregister();
            _aiRegistry.Unregister();
            _classRegistry.Unregister();
            _modelHandlerRegistry.Unregister();
            _itemHandlerRegistry.Unregister();
        }
    }
}