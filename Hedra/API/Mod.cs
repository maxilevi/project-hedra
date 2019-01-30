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
        private readonly EffectRegistry _effectRegistry;
        private readonly SkillRegistry _skillRegistry;
        
        public abstract string Name { get; }
        
        protected Mod()
        {
            _weaponRegistry = new WeaponRegistry();
            _animationEventRegistry = new AnimationEventRegistry();
            _aiRegistry = new AIRegistry();
            _classRegistry = new ClassRestrictionRegistry();
            _modelHandlerRegistry = new ModelHandlerRegistry();
            _itemHandlerRegistry = new ItemHandlerRegistry();
            _effectRegistry = new EffectRegistry();
            _skillRegistry = new SkillRegistry();
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

        protected void AddSkill(string Name, Type ClassType)
        {
            _skillRegistry.Add(Name, ClassType);
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

        protected void AddEffectType(string Name, Type ClassType)
        {
            _effectRegistry.Add(Name, ClassType);
        }

        public void UnregisterContent()
        {
            _weaponRegistry.Unregister();
            _animationEventRegistry.Unregister();
            _aiRegistry.Unregister();
            _classRegistry.Unregister();
            _modelHandlerRegistry.Unregister();
            _itemHandlerRegistry.Unregister();
            _effectRegistry.Unregister();
            _skillRegistry.Unregister();
        }
    }
}