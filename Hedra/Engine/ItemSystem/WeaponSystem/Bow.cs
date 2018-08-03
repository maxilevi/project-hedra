/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 10/01/2017
 * Time: 05:32 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
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
		public override Vector3 SheathedPosition => new Vector3(1.5f,-0.0f,0.0f);
	    public override Vector3 SheathedRotation => new Vector3(-5,90,-125 );
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
			var player = Owner as LocalPlayer;
			var direction = player?.View.CrossDirection ?? Owner.Orientation;
			this.ShootArrow(Owner, direction);
		}
		
		protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
		{
			if(Type != AttackEventType.Mid) return;
			this.ShootTripleArrow(Owner);
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
		    var startingLocation = Owner.Model.LeftWeaponPosition + Owner.Model.Human.Orientation * 2 +
		                           (Human is LocalPlayer ? Human.IsRiding ? Vector3.UnitY * 1f : Vector3.Zero : Vector3.Zero);

		    var arrowProj = new Projectile(Human, startingLocation, _arrowDataVertexData)
		    {
		        Lifetime = 5f,
		        Propulsion = Direction * 2f - Vector3.UnitY * ArrowDownForce
            };
		    arrowProj.HitEventHandler += delegate(Projectile Sender, IEntity Hit) {
			    Hit.Damage(Human.DamageEquation * 0.75f, Human, out float exp, true);
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
		
		public void ShootTripleArrow(IHumanoid Human){
		    var player = Human as LocalPlayer;
            Vector3 direction = player?.View.CrossDirection ?? Human.Orientation;

			this.ShootArrow(Human, (direction + Vector3.UnitX*.15f).NormalizedFast());
			this.ShootArrow(Human, direction);
			this.ShootArrow(Human, (direction - Vector3.UnitX*.15f).NormalizedFast());
			SoundManager.PlaySound(SoundType.BowSound, Human.Position, false, 1f + Utils.Rng.NextFloat() * .2f - .1f, 2.5f);
		}
	}
}
