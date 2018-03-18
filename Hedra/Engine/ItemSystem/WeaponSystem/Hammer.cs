/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 08/05/2016
 * Time: 06:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.ItemSystem.WeaponSystem
{
	/// <summary>
	/// Description of TwoHandedSword.
	/// </summary>
	public class Hammer : Weapon
	{
	    public override bool IsMelee { get; protected set; } = true;

        public Hammer(VertexData Contents) : base(Contents)
		{	
			VertexData BaseMesh = Contents.Clone();
			BaseMesh.Scale(Vector3.One * 1.75f);
			base.Mesh = EntityMesh.FromVertexData(BaseMesh);
			AttackStanceAnimation = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSmash-Stance.dae");
			
            PrimaryAnimations = new Animation[2];
			PrimaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Right.dae");
		    PrimaryAnimations[1] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Left.dae");

		    for (int i = 0; i < PrimaryAnimations.Length; i++)
		    {
		        PrimaryAnimations[i].Loop = false;
		        PrimaryAnimations[i].Speed = 1.0f;
		        PrimaryAnimations[i].OnAnimationMid += delegate
		        {
		            Model.Human.Attack(Model.Human.DamageEquation);
		        };

		        PrimaryAnimations[i].OnAnimationEnd += delegate
		        {
		            _trail.Emit = false;
		        };
		    }

		    SecondaryAnimations = new Animation[1];
            SecondaryAnimations[0] = AnimationLoader.LoadAnimation("Assets/Chr/WarriorSlash-Front.dae");
		    for (int i = 0; i < SecondaryAnimations.Length; i++)
		    {
		        SecondaryAnimations[i].Loop = false;
		        SecondaryAnimations[0].OnAnimationEnd += delegate
		        {
		            Model.Human.Attack(Model.Human.DamageEquation * 1.25f, delegate(Entity mob)
		            {
		                if (Utils.Rng.Next(1, 3) == 1)
		                    mob.KnockForSeconds(1.5f + Utils.Rng.NextFloat() * 2f);
		            });

		        };

		        SecondaryAnimations[i].OnAnimationEnd += delegate
		        {
		            _trail.Emit = false;
		        };
		    }
		}
		
		
		public override void Attack1(HumanModel Model){
		    if (!base.MeetsRequirements()) return;

			base.Attack1(Model);

		    TaskManager.Delay(250, () => _trail.Emit = true);
        }
		
		public override void Attack2(HumanModel Model){
		    if (!base.MeetsRequirements()) return;

            base.Attack2(Model);

		    TaskManager.Delay(200, () => _trail.Emit = true);

        }
	}
}