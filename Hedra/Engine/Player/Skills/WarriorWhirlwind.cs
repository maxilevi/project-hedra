/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/02/2017
 * Time: 05:13 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.EntitySystem;
using OpenTK;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.ToolbarSystem;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of WeaponThrow.
	/// </summary>
	public class Whirlwind : BaseSkill
	{
	    private readonly Animation _whirlwindAnimation;
	    private readonly TrailRenderer _trail;
	    private float _frameCounter;
	    private float _passedTime;
	    private float _rotationY;

        public Whirlwind() : base() {
			base.TexId = Graphics2D.LoadFromAssets("Assets/Skills/Spin.png");
			base.ManaCost = 85;
			base.MaxCooldown = 8.5f;
            _trail = new TrailRenderer( () => LocalPlayer.Instance.Model.LeftWeapon.WeaponTip, Vector4.One);
			
			_whirlwindAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorWhirlwind.dae");
		}

		public override bool MeetsRequirements(Toolbar Bar, int CastingAbilityCount)
		{
			return base.MeetsRequirements(Bar, CastingAbilityCount) && !Player.Toolbar.DisableAttack && Player.HasWeapon;
		}
		
		public override void Use()
        {
			base.MaxCooldown = 8.5f - base.Level * .5f;
			Player.IsCasting = true;
			Casting = true;
			Player.IsAttacking = true;
			_passedTime = 0;
			Player.Model.Model.Animator.StopBlend();
			Player.Model.Model.PlayAnimation(_whirlwindAnimation);
			Player.Model.Model.BlendAnimation(_whirlwindAnimation);
		    _trail.Emit = true;
		}

	    public void Disable()
	    {
	        Player.IsCasting = false;
	        Casting = false;
	        Player.IsAttacking = false;
	        Player.Model.LeftWeapon.LockWeapon = false;
	        Player.Model.Model.Animator.StopBlend();
            _trail.Emit = false;
        }
		
		public override void Update()
		{
			this.Grayscale = !Player.HasWeapon;    
			if(Player.IsCasting && Casting)
            {
				if(Player.IsDead || Player.Knocked || _passedTime > 4){
					this.Disable();
					return;
				}

			    var underChunk = World.GetChunkAt(Player.Position);
                _rotationY += Time.DeltaTime * 1000f;
                Player.Model.Model.TransformationMatrix =
                    Matrix4.CreateRotationY(-Player.Model.Model.Rotation.Y * Mathf.Radian) *
                    Matrix4.CreateRotationY(_rotationY * Mathf.Radian) *
                    Matrix4.CreateRotationY(Player.Model.Model.Rotation.Y * Mathf.Radian);
                this.ManageParticles(underChunk);
				
				if(_frameCounter >= .25f)
                {

                    for (var i = World.Entities.Count - 1; i > 0; i--)
                    {
                        if (!Player.InAttackRange(World.Entities[i])) continue;

                        float dmg = Player.DamageEquation * .2f * 2f * (1 + base.Level * .1f);
                        World.Entities[i].Damage(dmg, Player, out float exp, true);
                        Player.XP += exp;
                    }
					_frameCounter = 0;
				}
				_passedTime += Time.DeltaTime;
				_frameCounter += Time.DeltaTime;
			}
            _trail.Update();
		}

	    private void ManageParticles(Chunk UnderChunk)
	    {
	        World.Particles.VariateUniformly = true;
	        World.Particles.Color = World.GetHighestBlockAt((int)this.Player.Position.X, (int)this.Player.Position.Z).GetColor(UnderChunk.Biome.Colors);
	        World.Particles.Position = this.Player.Position - Vector3.UnitY;
	        World.Particles.Scale = Vector3.One * .15f;
	        World.Particles.ScaleErrorMargin = new Vector3(.35f, .35f, .35f);
	        World.Particles.Direction = (-this.Player.Orientation + Vector3.UnitY * 2.75f) * .15f;
	        World.Particles.ParticleLifetime = 1;
	        World.Particles.GravityEffect = .1f;
	        World.Particles.PositionErrorMargin = new Vector3(.75f, .75f, .75f);
	        if (World.Particles.Color == Block.GetColor(BlockType.Grass, UnderChunk.Biome.Colors))
	            World.Particles.Color = UnderChunk.Biome.Colors.GrassColor;
	        World.Particles.Emit();
        }
		
		public override string Description => "A fierce spinning attack.";
	}
}