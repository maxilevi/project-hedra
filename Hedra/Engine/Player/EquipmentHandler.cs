using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.Player
{
    public class EquipmentHandler : IDisposable
    {
        public Weapon LeftWeapon { get; private set; }
        public ChestPiece Chest { get; private set; }
        public HelmetPiece Helmet { get; private set; }
        public PantsPiece Pants { get; private set; }
        public BootsPiece Boots { get; private set; }
        
        private readonly IHumanoid _owner;
        private Item _mainWeapon;
        private Item _ring;

        public EquipmentHandler(IHumanoid Owner)
        {
            _owner = Owner;
        }

        public void Update()
        {          
            LeftWeapon?.Update(_owner);
            Chest?.Update(_owner);
            Helmet?.Update(_owner);
            Pants?.Update(_owner);
            Boots?.Update(_owner);
        }

        public void Reset()
        {
            SetHelmet(null);
            SetChest(null);
            SetPants(null);
            SetBoots(null);
            SetWeapon(null);
        }

        public void SetHelmet(HelmetPiece New)
        {
            Set(New, Helmet, H => Helmet = H);
        }
        
        public void SetChest(ChestPiece New)
        {
            Set(New, Chest, C => Chest = C);
        }
        
        public void SetPants(PantsPiece New)
        {
            Set(New, Pants, P => Pants = P);
        }
        
        public void SetBoots(BootsPiece New)
        {
            Set(New, Boots, B => Boots = B);
        }
        
        public void SetWeapon(Weapon New)
        {
            Set(New, LeftWeapon, W => LeftWeapon = W);
            (_owner as LocalPlayer)?.Toolbar.SetAttackType(LeftWeapon);
        }
        
        private void Set<T>(T New, T Old, Action<T> Setter) where T : class, IModel
        {
            if(New == Old) return;

            if (Old != null)
            {
                Old.Dispose();
                _owner.Model.UnregisterEquipment(Old);
            }
            Setter(New);
            if (New != null)
            {
                _owner.Model.RegisterEquipment(New);
            }
        }
        
        public Item MainWeapon
        {
            get => _mainWeapon;
            set
            {
                if(_mainWeapon == value) return;
                _mainWeapon = value;
                SetWeapon(_mainWeapon?.Weapon ?? Weapon.Empty);           
            }
        }
                     
        public Item Ring
        { 
            get => _ring;
            set
            {
                if (this.Ring == value)  return;             
                _ring = value;

                if (this.Ring != null)
                {
                    var effectType = (EffectType) Enum.Parse(typeof(EffectType), _ring.GetAttribute<string>("EffectType"));
                    if (effectType != EffectType.None) _owner.ApplyEffectWhile(effectType, () => this.Ring == value);

                    _owner.AddBonusSpeedWhile(this.Ring.GetAttribute<float>("MovementSpeed"), () => this.Ring == value);
                    _owner.AddBonusAttackSpeedWhile(_owner.AttackSpeed * this.Ring.GetAttribute<float>("AttackSpeed"), () => this.Ring == value);
                    _owner.AddBonusHealthWhile(_owner.MaxHealth * this.Ring.GetAttribute<float>("Health"), () => this.Ring == value);
                }
            }
        }
        
        public void Dispose()
        {
            LeftWeapon?.Dispose();
            Chest?.Dispose();
            Helmet?.Dispose();
            Pants?.Dispose();
            Boots?.Dispose();
        }
    }
}