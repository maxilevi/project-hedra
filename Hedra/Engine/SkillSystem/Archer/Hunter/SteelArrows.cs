using System;
using Hedra.Engine.ItemSystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using Hedra.WorldObjects;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class SteelArrows : SpecialAttackPassiveSkill<Bow>
    {
        private Item _previousAmmo;
        protected override int MaxLevel => 15;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SteelArrows.png");
        private Item SteelArrow { get; } = ItemPool.Grab(ItemType.SteelArrow);

        public override string Description => Translations.Get("steel_arrows_desc");
        public override string DisplayName => Translations.Get("steel_arrows");
        private float Chance => Math.Max(.1f, Level / (float)MaxLevel);

        public override string[] Attributes => new[]
        {
            Translations.Get("steel_arrows_percentage_change", (int)(100 * Chance))
        };

        protected override void BeforeUse(Bow Weapon, AttackOptions Options)
        {
            Weapon.BowModifiers += Modifier;
        }

        protected override void AfterUse(Bow Weapon, AttackOptions Options)
        {
            Weapon.BowModifiers -= Modifier;
        }

        private void Modifier(Projectile ArrowProj)
        {
            if (Utils.Rng.NextFloat() < Chance)
            {
                ArrowProj.DisposeOnHit = false;
                ArrowProj.Mesh.Outline = true;
                ArrowProj.Mesh.OutlineColor = Colors.Red;
            }
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
    }
}