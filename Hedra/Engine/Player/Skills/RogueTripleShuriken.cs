/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.ToolbarSystem;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class TripleShuriken : BaseSkill
	{
		private Animation ThrowAnimation;
		private VertexData ShurikenData;
		
		public TripleShuriken() : base() {
			base.TextureId = Graphics2D.LoadFromAssets("Assets/Skills/TripleShuriken.png");
			base.ManaCost = 35f;
			base.MaxCooldown = 8.5f;
            ShurikenData = AssetManager.PLYLoader("Assets/Items/Shuriken.ply", new Vector3(1,2,1) );
			
			ThrowAnimation = AnimationLoader.LoadAnimation("Assets/Chr/RogueShurikenThrow.dae");
			ThrowAnimation.Loop = false;
			ThrowAnimation.OnAnimationMid += delegate(Animation Sender) { 
				
				Vector3 Direction = Player.View.CrossDirection.NormalizedFast();
				Matrix4 D10 = Matrix4.CreateRotationY(10 * Mathf.Radian);
				Matrix4 DN10 = Matrix4.CreateRotationY(-10 * Mathf.Radian);
				
				Shuriken.ShootShuriken(Player, Direction, 30f * base.Level * .5f, 12);
			    Shuriken.ShootShuriken(Player, Vector3.TransformVector(Direction, D10), 30f * base.Level * .5f, 12);
			    Shuriken.ShootShuriken(Player, Vector3.TransformVector(Direction, DN10), 30f * base.Level * .5f, 12);
			};
			ThrowAnimation.OnAnimationEnd += delegate(Animation Sender) {
				Player.IsCasting = false;
				Casting = false;
				Player.IsAttacking = false;
			};
		}
		
		public override void Use(){
			base.MaxCooldown = 8.5f - Math.Min(5f, base.Level * .5f);
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			Player.LeftWeapon.InAttackStance = false;
			Player.Model.PlayAnimation(ThrowAnimation);
			Player.Movement.Orientate();
		}
		
		public override void Update(){
			if(Player.IsCasting && Casting){
				
				World.Particles.Color = new Vector4(1,1,1,1);
				World.Particles.ParticleLifetime = 1f;
				World.Particles.GravityEffect = .0f;
				World.Particles.Direction = Vector3.Zero;
				World.Particles.Scale = new Vector3(.25f,.25f,.25f);
				World.Particles.Position = Player.Model.LeftWeaponPosition;
				World.Particles.PositionErrorMargin = Vector3.One * 0.75f;
				
				for(int i = 0; i < 1; i++)
					World.Particles.Emit();
			}
		}
		
		public override string Description {
			get {
				return "Throw your a series of shurikens at your foes.";
			}
		}
	}
}