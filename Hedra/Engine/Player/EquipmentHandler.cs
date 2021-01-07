using System;
using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.ArmorSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.EntitySystem;
using Hedra.Items;
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
        public bool ShouldUpdateDefaultModels { get; set; } = true;

        private readonly IHumanoid _owner;
        private readonly CustomizationData _lastCustomization;
        private ModelData _defaultHead;
        private ModelData _defaultChest;
        private ModelData _defaultPants;
        private ModelData _defaultBoots;
        private Class _lastClass;
        private Item _mainWeapon;
        private Item _ring;

        public EquipmentHandler(IHumanoid Owner)
        {
            _owner = Owner;
            _lastCustomization = new CustomizationData();
            UpdateDefaultModels();
        }

        public void Update()
        {
            LeftWeapon?.Update(_owner);
            Chest?.Update(_owner);
            Helmet?.Update(_owner);
            Pants?.Update(_owner);
            Boots?.Update(_owner);
            if (ShouldUpdateDefaultModels)
            {
                AddDefaultModels();
                UpdateDefaultModels();
            }
        }

        private void AddDefaultModels()
        {
            var model = _owner.Model;
            var changed = false;
            changed |= AddDefaultModel(Helmet, model, _defaultHead, true);
            changed |= AddDefaultModel(Chest, model, _defaultChest);
            changed |= AddDefaultModel(Pants, model, _defaultPants);
            changed |= AddDefaultModel(Boots, model, _defaultBoots);
            if (changed)
                model.Rebuild();
        }

        private bool AddDefaultModel(ArmorPiece Piece, HumanoidModel Model, ModelData Default, bool AlwaysOn = false)
        {
            if (!Model.UsesBodyParts) return false;
            if (AlwaysOn)
            {
                if (!Model.HasModel(Default))
                {
                    Model.AddBodyPartModel(Default, true, false);
                    return true;
                }
            }
            else
            {
                if (Piece == null && !Model.HasModel(Default))
                {
                    Model.AddBodyPartModel(Default, true, false);
                    return true;
                }
                else if (Piece != null && Model.HasModel(Default))
                {
                    Model.RemoveBodyPartModel(Default, false);
                    return true;
                }
            }

            return false;
        }

        private void UpdateDefaultModels()
        {
            if (_defaultHead != null && _owner.Class.Type == _lastClass && !IsCustomizationDifferent()) return;
            if (_defaultHead != null)
            {
                var model = _owner.Model;
                model.RemoveModel(_defaultHead);
                model.RemoveModel(_defaultChest);
                model.RemoveModel(_defaultPants);
                model.RemoveModel(_defaultBoots);
            }

            _defaultHead = HumanoidModel.LoadHead(_owner);
            _defaultChest = HumanoidModel.LoadChest(_owner);
            _defaultPants = HumanoidModel.LoadLegs(_owner);
            _defaultBoots = HumanoidModel.LoadFeet(_owner);
            _lastClass = _owner.Class.Type;
            UpdateCustomization();
        }

        private bool IsCustomizationDifferent()
        {
            return _owner.Customization.FirstHairColor != _lastCustomization.FirstHairColor
                || _owner.Customization.SecondHairColor != _lastCustomization.SecondHairColor
                || _owner.Customization.SkinColor != _lastCustomization.SkinColor
                || _owner.Customization.Gender != _lastCustomization.Gender;
        }

        private void UpdateCustomization()
        {
            _lastCustomization.FirstHairColor = _owner.Customization.FirstHairColor;
            _lastCustomization.SecondHairColor = _owner.Customization.SecondHairColor;
            _lastCustomization.SkinColor = _owner.Customization.SkinColor;
            _lastCustomization.Gender = _owner.Customization.Gender;
        }

        public void Reset()
        {
            UpdateDefaultModels();
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
            if (New == Old) return;

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
                if (_mainWeapon == value) return;
                _mainWeapon = value;
                SetWeapon(_mainWeapon?.Weapon ?? Weapon.Empty);
            }
        }

        public Item Ring
        {
            get => _ring;
            set
            {
                if (this.Ring == value) return;
                _ring = value;

                if (this.Ring != null)
                {
                    var effectType =
                        (EffectType) Enum.Parse(typeof(EffectType), _ring.GetAttribute<string>("EffectType"));
                    if (effectType != EffectType.None) _owner.ApplyEffectWhile(effectType, () => this.Ring == value);

                    _owner.AddBonusSpeedWhile(this.Ring.GetAttribute<float>("MovementSpeed"), () => this.Ring == value);
                    _owner.AddBonusAttackSpeedWhile(_owner.AttackSpeed * this.Ring.GetAttribute<float>("AttackSpeed"),
                        () => this.Ring == value);
                    _owner.AddBonusHealthWhile(_owner.MaxHealth * this.Ring.GetAttribute<float>("Health"),
                        () => this.Ring == value);
                }
            }
        }

        public Item[] MainEquipment
        {
            get => new[]
            {
                _owner.Inventory.MainWeapon,
                _owner.Inventory.Helmet,
                _owner.Inventory.Chest,
                _owner.Inventory.Pants,
                _owner.Inventory.Boots
            };
            set
            {
                _owner.Inventory.SetItem(PlayerInventory.WeaponHolder, value[0]);
                _owner.Inventory.SetItem(PlayerInventory.HelmetHolder, value[1]);
                _owner.Inventory.SetItem(PlayerInventory.ChestplateHolder, value[2]);
                _owner.Inventory.SetItem(PlayerInventory.PantsHolder, value[3]);
                _owner.Inventory.SetItem(PlayerInventory.BootsHolder, value[4]);
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