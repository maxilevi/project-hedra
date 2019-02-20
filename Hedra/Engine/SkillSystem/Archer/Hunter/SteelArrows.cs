using System;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class SteelArrows : SpecialAttackPassiveSkill<Bow>
    {
        protected override int MaxLevel => 25;
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SteelArrows.png");
        private Item SteelArrow { get; } = ItemPool.Grab(ItemType.SteelArrow);
        private Item _previousAmmo;
        
        protected override void BeforeUse(Bow Weapon, AttackOptions Options)
        {
            Weapon.BowModifiers += Modifier;
        }

        protected override void AfterUse(Bow Weapon, AttackOptions Options)
        {
            Weapon.BowModifiers -= Modifier;
        }
        
        private static void Modifier(Projectile ArrowProj)
        {
            ArrowProj.DisposeOnHit = false;
            ArrowProj.Mesh.Outline = true;
            ArrowProj.Mesh.OutlineColor = Colors.Red;
        }

        protected override void Add(Bow Weapon)
        {
            _previousAmmo = Weapon.Ammo;
            Weapon.Ammo = SteelArrow;
        }
        
        protected override void Remove(Bow Weapon)
        {
            Weapon.Ammo = _previousAmmo;
        }

        public override string Description => Translations.Get("steel_arrows_desc");
        public override string DisplayName => Translations.Get("steel_arrows");
    }
}