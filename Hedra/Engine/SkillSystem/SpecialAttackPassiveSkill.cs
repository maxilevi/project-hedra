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
        
        private void BeforeAttack(AttackOptions Options)
        {
            if(Level > 0 && User.HasWeapon && User.LeftWeapon is T weapon)
                BeforeUse(weapon, Options);
        }
        
        private void AfterAttack(AttackOptions Options)
        {
            if(Level > 0 && User.HasWeapon && User.LeftWeapon is T weapon)
                AfterUse(weapon, Options);
        }

        protected override void Remove()
        {
            User.BeforeAttack -= BeforeAttack;
            User.AfterAttack -= AfterAttack;
            User.Inventory.InventoryUpdated -= InventoryUpdated;
            InventoryUpdated();
        }

        protected override void Add()
        {
            User.BeforeAttack += BeforeAttack;
            User.AfterAttack += AfterAttack;
            User.Inventory.InventoryUpdated += InventoryUpdated;
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
            if(_lastWeapon == User.Inventory.MainWeapon?.Weapon) return;
            if (_lastWeapon == null && User.Inventory.MainWeapon.Weapon is T generic)
            {
                Add(generic);
                _lastWeapon = generic;
            }
            else if (_lastWeapon != null && User.Inventory.MainWeapon?.Weapon == null)
            {
                Remove(_lastWeapon);
                _lastWeapon = null;
            }
        }
        
        protected abstract void BeforeUse(T Weapon, AttackOptions Options);

        protected abstract void AfterUse(T Weapon, AttackOptions Options);
    }
}