/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 30/04/2017
 * Time: 10:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Globalization;
using System.Numerics;
using Hedra.Components.Effects;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.WorldObjects;

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    ///     Description of ArcherPoisonArrow.
    /// </summary>
    public class IceArrow : SpecialRangedAttackSkill
    {
        private const float BaseDamage = 26f;
        private const float BaseCooldown = 18f;
        private const float CooldownCap = 12f;
        private const float BaseManaCost = 40f;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/IceArrow.png");
        public override string Description => Translations.Get("ice_arrow_desc");
        public override string DisplayName => Translations.Get("ice_arrow");
        private float Damage => Level * 4.5f + BaseDamage;
        public override float MaxCooldown => Math.Max(BaseCooldown - 0.80f * Level, CooldownCap);
        public override float ManaCost => BaseManaCost;
        protected override int MaxLevel => 99;
        private float FreezeDuration => 2.0f;

        public override string[] Attributes => new[]
        {
            Translations.Get("ice_arrow_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("ice_arrow_duration_change", FreezeDuration.ToString("0.0", CultureInfo.InvariantCulture))
        };

        protected override void OnHit(Projectile Proj, IEntity Victim)
        {
            base.OnHit(Proj, Victim);
            Victim.AddComponent(
                new FreezingComponent(Victim, User, FreezeDuration + Utils.Rng.NextFloat() * 2f, Damage));
        }

        protected override void OnMove(Projectile Proj)
        {
            base.OnMove(Proj);
            Proj.Mesh.Tint = Colors.Blue * new Vector4(1, 1, 3, 1) * .7f;
        }
    }
}