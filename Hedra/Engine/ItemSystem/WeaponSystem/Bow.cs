/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 05:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
	public delegate void OnModifyArrowEvent(Projectile Arrow);
		
	public sealed class Bow : RangedWeapon
	{			
		protected override string AttackStanceName => "Assets/Chr/ArcherShootStance.dae";
		protected override float PrimarySpeed => 0.9f;
		protected override string[] PrimaryAnimationsNames => new []
		{
			"Assets/Chr/ArcherShoot.dae"
		};
		protected override float SecondarySpeed => 0.9f;
		protected override string[] SecondaryAnimationsNames => new []
		{
			"Assets/Chr/ArcherTripleShoot.dae"
		};
		
	    private static readonly VertexData ArrowVertexData;
	    private static readonly VertexData _arrowDataVertexData;
	    private static readonly VertexData QuiverVertexData;
		protected override Vector3 SheathedPosition => new Vector3(1.5f,-0.0f,0.0f);
	    protected override Vector3 SheathedRotation => new Vector3(-5,90,-125 );
        private readonly ObjectMesh _quiver;
        private readonly ObjectMesh _arrow;
		public OnModifyArrowEvent BowModifiers;
        public float ArrowDownForce { get; set; }
		

        static Bow()
        {
            ArrowVertexData = AssetManager.PLYLoader("Assets/Items/Arrow.ply", Vector3.One * 4f * 1.5f,
                Vector3.UnitX * .35f, Vector3.Zero);
            QuiverVertexData = AssetManager.PLYLoader("Assets/Items/Quiver.ply",
                Vector3.One * new Vector3(2.2f, 2.8f, 2.2f) * 1.5f);
            _arrowDataVertexData = AssetManager.PLYLoader("Assets/Items/Arrow.ply", Vector3.One * 5f, Vector3.Zero,
                new Vector3(-90, 0, 90) * Mathf.Radian);
        }

		public Bow(VertexData Contents) : base(Contents)
        {
			_arrow = ObjectMesh.FromVertexData(ArrowVertexData);
			_quiver = ObjectMesh.FromVertexData(QuiverVertexData);
			_quiver.TargetPosition = this.SheathedPosition + new Vector3(.3f, -0.75f, -0.2f);
			_quiver.LocalRotationPoint = new Vector3(0, 0, _quiver.TargetPosition.Z);
			_quiver.TargetRotation = new Vector3(SheathedRotation.X, SheathedRotation.Y, SheathedRotation.Z+90);
	        base.ShouldPlaySound = false;		    
        }
		
		protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
		{
			if(Type != AttackEventType.Mid) return;
			var player = Owner as IPlayer;
			var direction = player?.View.CrossDirection ?? Owner.Orientation;
			this.ShootArrow(Owner, direction, Options);
		}
		
		protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
		{
			if(Type != AttackEventType.Mid) return;
		    CoroutineManager.StartCoroutine(TripleArrowCoroutine, Options);
        }
		
		public override void Update(IHumanoid Human)
		{
			base.Update(Human);

            base.SetToDefault(MainMesh);

			if(Sheathed){
                this.MainMesh.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition - Vector3.UnitY * .25f);
                this.MainMesh.Position = Owner.Model.Position;
			    this.MainMesh.LocalRotation = this.SheathedRotation;
                this.MainMesh.BeforeLocalRotation = this.SheathedPosition * this.Scale;
            }

            if (base.InAttackStance || Owner.IsAttacking || Owner.WasAttacking)
            {
				Matrix4 Mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
					
				this.MainMesh.TransformationMatrix = Mat4;
				this.MainMesh.Position = Owner.Model.Position;
				this.MainMesh.TargetRotation = new Vector3(90,25,180);
				this.MainMesh.BeforeLocalRotation = Vector3.Zero;				
			}
			
            base.SetToDefault(this._quiver);

		    this._quiver.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition);
		    this._quiver.Position = Owner.Model.Position;
		    this._quiver.BeforeLocalRotation = (-Vector3.UnitY * 1.5f - Vector3.UnitZ * 1.8f) * this.Scale;

            base.SetToDefault(this._arrow);

            Matrix4 ArrowMat4 = Owner.Model.RightWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.RightWeaponPosition);
			
			this._arrow.TransformationMatrix = ArrowMat4;
			this._arrow.Position = Owner.Model.Position;
			this._arrow.BeforeLocalRotation = Vector3.UnitZ * 0.5f;
			this._arrow.Enabled = (base.InAttackStance || Owner.IsAttacking) && this._quiver.Enabled;	
			
		}
		
		public Projectile AddModifiers(Projectile ArrowProj)
		{
		    BowModifiers?.Invoke(ArrowProj);
		    return ArrowProj;
		}

		public Projectile ShootArrow(IHumanoid Human, Vector3 Direction, int KnockChance = -1)
		{
			return this.ShootArrow(Human, Direction, AttackOptions.Default, KnockChance);
		}

		public Projectile ShootArrow(IHumanoid Human, Vector3 Direction, AttackOptions Options,
			int KnockChance = -1)
		{
			return this.ShootArrow(Human, this.ArrowOrigin, Direction, Options, KnockChance);
		}
		
		public Projectile ShootArrow(IHumanoid Human, Vector3 Origin, Vector3 Direction, AttackOptions Options, int KnockChance = -1)
		{

		    var arrowProj = new Projectile(Human, Origin, _arrowDataVertexData)
		    {
		        Lifetime = 5f,
		        Propulsion = Direction * 2f - Vector3.UnitY * ArrowDownForce
            };
		    arrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit) {
			    Hit.Damage(Human.DamageEquation * 0.75f * Options.DamageModifier, Human, out float exp, true);
				Human.XP += exp;
			    if(KnockChance != -1 && Utils.Rng.Next(0, KnockChance) == 0)
                    Hit.KnockForSeconds(3);
			};
		    arrowProj.LandEventHandler += S => Human.ProcessHit(false);
		    arrowProj.HitEventHandler += (S,V) => Human.ProcessHit(true);
			arrowProj = this.AddModifiers(arrowProj);
			SoundManager.PlaySound(SoundType.BowSound, Human.Position, false,  1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
			return arrowProj;
		}

		private Vector3 ArrowOrigin => Owner.Model.LeftWeaponPosition + Owner.Model.Human.Orientation * 2 +
		                               (Owner is IPlayer
			                               ? Owner.IsRiding ? Vector3.UnitY * 1f : Vector3.Zero
			                               : Vector3.Zero);

	    private IEnumerator TripleArrowCoroutine(object[] Params)
	    {
	        var options = (AttackOptions) Params[0];
	        var player = Owner as IPlayer;
	        var direction = player?.View.CrossDirection ?? Owner.Orientation;
		    var origin = ArrowOrigin;
	        this.ShootArrow(Owner, ArrowOrigin, direction, options);
	        var time = 0f;
	        while (time < .20f)
	        {
	            time += Time.DeltaTime;
	            yield return null;
	        }
            this.ShootArrow(Owner, ArrowOrigin, direction, options);
	        time = 0f;
	        while (time < .20f)
	        {
	            time += Time.DeltaTime;
	            yield return null;
	        }
            this.ShootArrow(Owner, ArrowOrigin, direction, options);
        }
	}
}
