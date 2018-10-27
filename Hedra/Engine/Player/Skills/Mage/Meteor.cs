/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/08/2016
 * Time: 08:16 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Rendering;
using System.Drawing;
using System.IO;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of Meteor.
    /// </summary>
    public class Meteor : BaseSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Meteor.png");
        private bool LeftHand;
        private float Damage = 27.5f;
        private ParticleSystem HandParticles;
        private Animation FireballAnimation;
        public override string DisplayName => "Meteor";
        
        public Meteor() : base() {
            base.MaxCooldown = 8.5f;
            base.ManaCost = 160f;
            this.HandParticles = new ParticleSystem(Vector3.Zero);
            this.HandParticles.Direction = Vector3.UnitY;
            this.HandParticles.Scale = new Vector3(.4f,.4f,.4f);
            this.HandParticles.Color = Particle3D.FireColor;
            this.HandParticles.ParticleLifetime = .15f;
            
            /*FireballAnimation = AnimationLoader.LoadAnimation("Assets/Chr/MageFireball.dae");
            FireballAnimation.OnAnimationEnd += delegate(Animation Sender) { 
                Sound.SoundManager.PlaySound(Sound.SoundType.SWOOSH_SOUND, Player.Position, false, 0.8f, 1f);
                this.CreateProjectile();
            };*/
        }
        
        public override string Description {
            get {
                return "Shoot a giant fireball in your direction.";
            }
        }
        
        private int FireballCombo = 5;
        public override void Use(){
            Player.IsCasting = true;
            Casting = true;
            LeftHand = !LeftHand;
            Player.Model.PlayAnimation(FireballAnimation);
        }
        
        public override void Update(){}
        
        public void CreateProjectile()
        {
            /*
            float RandomScale = Mathf.Clamp(Utils.Rng.NextFloat() * 2f -1f, 1, 2);
            ParticleProjectile Fire = new ParticleProjectile(Vector3.One + new Vector3(RandomScale, RandomScale, RandomScale) * 0.35f,
                                        ((LeftHand) ? Player.Model.LeftWeaponPosition - Vector3.UnitX * .5f : Player.Model.RightWeaponPosition + Vector3.UnitX * .5f) + Player.Orientation * 2 + Vector3.UnitY * 2f,
                                        2, Player.View.CrossDirection * 4f, Player);
            
            ParticleSystem Particles = new ParticleSystem(Fire.Position);
            Particles.Color = Particle3D.FireColor;
            Particles.Direction = Fire.Direction;
            Particles.ParticleLifetime = 5f;
            Particles.GravityEffect = 0f;
            Particles.PositionErrorMargin = new Vector3(2f,2f,2f);
            Particles.Scale = Vector3.One * .5f;
            Particles.ScaleErrorMargin = new Vector3(.35f,.35f,.35f);

            for(int i = 0; i < 120; i++){
                Particles.Emit();
            }
            
            Fire.HitEventHandler += delegate(ParticleProjectile Sender, Entity Hit) { 
                float Exp;
                Hit.Damage(Damage * Math.Max(1, 2.75f + base.Level * 1.85f), Player, out Exp);
                Player.XP += Exp;
                Particles.Position = Hit.Position; 
                Particles.PositionErrorMargin = new Vector3(2f,2f,2f);
                Particles.ParticleLifetime = 0.5f;
                for(int i = 0; i < 100; i++){
                    Vector3 Dir = (Mathf.RandomVector3(Utils.Rng) - Vector3.One * 0.5f);
                    Particles.Direction = Dir;
                    Particles.Emit();
                }
                Sender.Dispose();
            };*/
        }
    }
}
