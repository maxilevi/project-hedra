using System;
using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.WeaponSystem;
using System.Numerics;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public abstract class MorphSkill : ActivateDurationSkill<IPlayer>
    {
        private const string EquipmentRestriction = "WerewolfHands";
        private static bool _isMorphed;
        private AnimatedModel _oldModel;
        private AnimatedModel _newModel;
        private string[] _oldRestrictions;

        protected sealed override void DoEnable()
        {
            Morph();
            AddEffects();
        }

        protected sealed override void DoDisable()
        {
            UnMorph();
            RemoveEffects();
        }

        private void Morph()
        {
            _isMorphed = true;
            var template = HumanoidLoader.HumanoidTemplater[Type].Model;
            _newModel = AnimationModelLoader.LoadEntity(template.Path);
            _newModel.Scale = Vector3.One * template.Scale;
            ApplyVisuals(_newModel, template.Path);
            _oldModel = SwitchModel(_newModel);
            _oldModel.Enabled = false;
            if (RestrictWeapons)
            {
                var oldWeapon = User.Inventory.MainWeapon;
                User.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentRestriction);
                User.Inventory.SetItem(PlayerInventory.WeaponHolder, null);
                if (oldWeapon != null) User.Inventory.AddItem(oldWeapon);
                _oldRestrictions = User.Inventory.GetRestrictions(PlayerInventory.WeaponHolder);
                User.SetWeapon(CustomWeapon);
            }

            MorphEffect();
            Casting = !CanUseOtherSkills;
        }

        private void MorphEffect()
        {
            SkillUtils.SpawnParticles(User.Position, Vector4.One);
        }
        
        private void UnMorph()
        {
            _isMorphed = false;
            SwitchModel(_oldModel);
            _newModel.Dispose();
            _newModel = null;
            if (RestrictWeapons)
            {
                User.Inventory.SetRestrictions(PlayerInventory.WeaponHolder, _oldRestrictions);
                User.SetWeapon(null);
                var weapon = User.Inventory.Search(I =>
                    _oldRestrictions.Contains(I.EquipmentType) || _oldRestrictions.Length == 0);
                if (weapon != null)
                {
                    User.Inventory.SetItem(User.Inventory.IndexOf(weapon), null);
                    User.Inventory.SetItem(PlayerInventory.WeaponHolder, weapon);
                }
            }
            MorphEffect();
            Casting = false;
        }

        private AnimatedModel SwitchModel(AnimatedModel New)
        {
            return User.Model.SwitchModel(New);
        }

        protected abstract void AddEffects();
        protected abstract void RemoveEffects();

        protected virtual void ApplyVisuals(AnimatedModel Model, string ModelPath)
        {
            
        }
        
        protected abstract HumanType Type { get; }
        protected abstract bool CanUseOtherSkills { get; }
        protected sealed override float Duration => 18 + 32 * (Level / (float) MaxLevel);
        protected sealed override float CooldownDuration => 28;
        protected sealed override int MaxLevel => 15;
        protected override bool ShouldDisable => _isMorphed;
        protected abstract bool RestrictWeapons { get; }
        protected virtual Weapon CustomWeapon => throw new NotImplementedException();
    }
}