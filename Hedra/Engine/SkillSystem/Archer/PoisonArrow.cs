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
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using Hedra.WorldObjects;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.SkillSystem.Archer
{
    /// <summary>
    /// Description of ArcherPoisonArrow.
    /// </summary>
    public class PoisonArrow : SpecialRangedAttackSkill
    {
        private const float BaseDamage = 11f;
        private const float BaseCooldown = 16f;
        private const float CooldownCap = 10f;
        private const float BaseManaCost = 70f;
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/PoisonArrow.png");
        public override string Description => Translations.Get("poison_arrow_desc");
        public override string DisplayName => Translations.Get("poison_arrow");
        private float Damage => BaseDamage * (base.Level * 0.35f) + BaseDamage;
        public override float MaxCooldown => Math.Max(BaseCooldown - 0.80f * base.Level, CooldownCap);
        public override float ManaCost => BaseManaCost;
        protected override int MaxLevel => 99;

        protected override void OnHit(Projectile Proj, IEntity Victim)
        {
            Victim.AddComponent(new PoisonComponent(Victim, User, 3 + Utils.Rng.NextFloat() * 2f, Damage));
        }

        protected override void OnMove(Projectile Proj)
        {
            Proj.Mesh.Tint = Colors.PoisonGreen * new Vector4(1, 3, 1, 1);
        }

        public override string[] Attributes => new[]
        {
            Translations.Get("poison_arrow_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}
