/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 17/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Globalization;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Warrior
{
    /// <summary>
    /// Description of Bash.
    /// </summary>
    public sealed class Bash : CappedSkill<IPlayer>
    {
        private readonly Animation _bashAnimation;
        
        private const float BaseManaCost = 60;
        private const float ManaCostCap = 20;
        private const float ManaCostChangeRate = -2.5f;
        
        private const float BaseCooldown = 8; 
        private const float CooldownCap = 4;
        private const float CooldownChangeRate = -.25f;
        
        private const float BaseRange = 16;
        private const float RangeCap = 24;
        private const float RangeChangeRate = .25f;
        
        private const float BaseRadius = .75f;
        private const float RadiusCap = .65f;
        private const float RadiusChangeRate = -.01f;
        
        private const float BaseDamage = 15f;
        private const float DamageCap = float.MaxValue;
        private const float DamageChangeRate = 10f;
        
        public Bash()
        {
            this._bashAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorHeadbutt.dae");
            this._bashAnimation.OnAnimationMid += Sender => OnDamage();
        }

        private void OnDamage()
        {
            World.Entities.ToList().ForEach(delegate(IEntity Entity)
            {
                if(Entity == User) return;
                    
                var dot = Vector3.Dot((Entity.Position - User.Position).NormalizedFast(), User.Orientation);
                if(dot >= Radius && (Entity.Position - User.Position).LengthSquared < Math.Pow(Range, 2))
                {
                    if (Utils.Rng.Next(0, 5) == 1)
                    {
                        Entity.KnockForSeconds(1.5f);
                        Entity.Physics.Translate(-Entity.Orientation);
                    }
                    Entity.Damage(Damage * dot * 1.25f, User, out var exp);
                    User.XP += exp;
                }
            });
            User.Movement.Orientate();
        }

        protected override void DoUse()
        {
            SoundPlayer.PlaySound(SoundType.SlashSound, User.Position);
            User.Model.BlendAnimation(_bashAnimation);
        }

        private float Damage => Math.Min(BaseDamage + DamageChangeRate * Level, DamageCap);
        private float Range => Math.Min(BaseRange + RangeChangeRate * Level, RangeCap);
        private float Radius => Math.Max(BaseRadius + RadiusChangeRate * Level, RadiusCap);
        protected override int MaxLevel => 25;
        public override float MaxCooldown => Math.Max(BaseCooldown + CooldownChangeRate * Level, CooldownCap);
        public override float ManaCost => Math.Max(BaseManaCost + ManaCostChangeRate * Level, ManaCostCap);
        public override string Description => Translations.Get("bash_desc");
        public override string DisplayName => Translations.Get("bash_skill");
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Bash.png");
        public override string[] Attributes => new []
        {
            Translations.Get("bash_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("bash_radius_change", Radius.ToString("0.0", CultureInfo.InvariantCulture))
        };
    }
}
