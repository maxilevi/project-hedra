using System;
using System.Linq;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Mage.Druid
{
    public abstract class MorphSkill : ActivateDurationSkill
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
                var oldWeapon = Player.Inventory.MainWeapon;
                Player.Inventory.AddRestriction(PlayerInventory.WeaponHolder, EquipmentRestriction);
                Player.Inventory.SetItem(PlayerInventory.WeaponHolder, null);
                if (oldWeapon != null) Player.Inventory.AddItem(oldWeapon);
                _oldRestrictions = Player.Inventory.GetRestrictions(PlayerInventory.WeaponHolder);
                Player.SetWeapon(CustomWeapon);
            }

            MorphEffect();
            Casting = !CanUseOtherSkills;
        }

        private void MorphEffect()
        {
            SkillUtils.SpawnParticles(Player.Position, Vector4.One);
        }
        
        private void UnMorph()
        {
            _isMorphed = false;
            SwitchModel(_oldModel);
            _newModel.Dispose();
            _newModel = null;
            if (RestrictWeapons)
            {
                Player.Inventory.SetRestrictions(PlayerInventory.WeaponHolder, _oldRestrictions);
                Player.SetWeapon(null);
                var weapon = Player.Inventory.Search(I =>
                    _oldRestrictions.Contains(I.EquipmentType) || _oldRestrictions.Length == 0);
                if (weapon != null)
                {
                    Player.Inventory.SetItem(Player.Inventory.IndexOf(weapon), null);
                    Player.Inventory.SetItem(PlayerInventory.WeaponHolder, weapon);
                }
            }
            MorphEffect();
            Casting = false;
        }

        private AnimatedModel SwitchModel(AnimatedModel New)
        {
            return Player.Model.SwitchModel(New);
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