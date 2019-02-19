using Hedra.Engine.Player;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem
{
    public abstract class SpecialAttackPassiveSkill<T> : PassiveSkill where T : Weapon
    {
        private T _lastWeapon;
        
        private void BeforeAttacking(IEntity Parent, float Damage)
        {
            if(Level > 0 && Player.HasWeapon && Player.LeftWeapon is T weapon)
                BeforeUse(weapon);
        }
        
        private void AfterAttacking(IEntity Parent, float Damage)
        {
            if(Level > 0 && Player.HasWeapon && Player.LeftWeapon is T weapon)
                AfterUse(weapon);
        }

        protected override void Remove()
        {
            Player.BeforeAttacking -= BeforeAttacking;
            Player.AfterAttacking -= AfterAttacking;
            Player.Inventory.InventoryUpdated -= InventoryUpdated;
            InventoryUpdated();
        }

        protected override void Add()
        {
            Player.BeforeAttacking += BeforeAttacking;
            Player.AfterAttacking += AfterAttacking;
            Player.Inventory.InventoryUpdated += InventoryUpdated;
            InventoryUpdated();
        }
        
        protected virtual void Add(T Weapon)
        {
        }
        
        protected virtual void Remove(T Weapon)
        {
        }

        private void InventoryUpdated()
        {
            if(_lastWeapon == Player.Inventory.MainWeapon.Weapon) return;
            if (_lastWeapon == null && Player.Inventory.MainWeapon.Weapon is T generic)
            {
                Add(generic);
                _lastWeapon = generic;
            }
            else if (_lastWeapon != null && Player.Inventory.MainWeapon.Weapon == null)
            {
                Remove(_lastWeapon);
                _lastWeapon = null;
            }
        }
        
        protected abstract void BeforeUse(T Weapon);
        
        protected virtual void AfterUse(T Weapon){}
    }
}