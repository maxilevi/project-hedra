using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
    public sealed class Staff : RangedWeapon
    {
        protected override string AttackStanceName => "Assets/Chr/MageStaff-Stance.dae";
        protected override float PrimarySpeed => 0.9f;
        protected override bool ShouldPlaySound => false;
        protected override string[] PrimaryAnimationsNames => new []
        {
            "Assets/Chr/MageStaff-PrimaryAttack.dae"
        };
        protected override float SecondarySpeed => 1.0f;
        protected override string[] SecondaryAnimationsNames => new []
        {
            "Assets/Chr/MageStaff-SecondaryAttack.dae"
        };

        protected override Vector3 SheathedPosition => new Vector3(1.5f,-1.0f,-0.75f);
        protected override Vector3 SheathedRotation => new Vector3(-5,90,-125 );


        public Staff(VertexData Contents) : base(Contents)
        {  
        }
        
        protected override void OnPrimaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.Mid) return;
            ShootEffect();
            var player = Owner as IPlayer; 
            Fireball.Create(
                Owner,
                Owner.Position + Vector3.UnitY * 4f,
                player?.View.LookingDirection ?? Owner.Orientation,
                Owner.DamageEquation * Options.DamageModifier,
                player?.Pet.Pet
            );
        }
        
        protected override void OnSecondaryAttackEvent(AttackEventType Type, AttackOptions Options)
        {
            if(Type != AttackEventType.End) return;
            Firewave.Create(Owner, Owner.DamageEquation * 8 * Options.Charge, Options.Charge);
        }
        
        public override void Update(IHumanoid Human)
        {
            base.Update(Human);
            base.SetToDefault(MainMesh);

            if(Sheathed)
            {
                this.MainMesh.TransformationMatrix = Owner.Model.ChestMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.ChestPosition - Vector3.UnitY * .25f);
                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.LocalRotation = this.SheathedRotation;
                this.MainMesh.BeforeLocalRotation = this.SheathedPosition * this.Scale;
            }

            if (base.InAttackStance || Owner.IsAttacking || Owner.WasAttacking)
            {
                var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);
                    
                this.MainMesh.TransformationMatrix = mat4;
                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.TargetRotation = new Vector3(90,25,180);
                this.MainMesh.BeforeLocalRotation = Vector3.Zero;     
            } 

            if(SecondaryAttack)
            {
                var mat4 = Owner.Model.LeftWeaponMatrix.ClearTranslation() * Matrix4.CreateTranslation(-Owner.Model.Position + Owner.Model.LeftWeaponPosition);

                this.MainMesh.TransformationMatrix = mat4;
                this.MainMesh.Position = Owner.Model.Position;
                this.MainMesh.TargetRotation = new Vector3(90, 0, 180);
                this.MainMesh.BeforeLocalRotation = Vector3.Zero;
            }
        }
        
        private void ShootEffect()
        {
            return;
            World.Particles.Color = Particle3D.FireColor;
            World.Particles.VariateUniformly = false;
            World.Particles.Position = Owner.Position;
            World.Particles.Scale = Vector3.One;
            World.Particles.ScaleErrorMargin = new Vector3(.25f, .25f, .25f);
            World.Particles.Direction = Vector3.UnitY;
            World.Particles.ParticleLifetime = 0.5f;
            World.Particles.GravityEffect = 0f;
            World.Particles.PositionErrorMargin = Owner.Model.Dimensions.Size.Xz.ToVector3();
            for(var i = 0; i < 10; i++) World.Particles.Emit();
        }
    }
}
