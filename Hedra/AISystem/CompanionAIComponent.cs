/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 20/01/2017
 * Time: 09:16 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.AISystem.Behaviours;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem
{
    /// <summary>
    /// Description of CompanionAIComponent.
    /// </summary>
    public class CompanionAIComponent : BasicAIComponent
    {
        public override AIType Type => AIType.Friendly;
        private readonly IEntity _owner;
        private bool _isStayingStill;
        private AttackBehaviour Attack { get; }
        private FollowBehaviour Follow { get; }
        private TraverseBehaviour Traverse { get; }
        
        public CompanionAIComponent(IEntity Parent, IEntity Owner) : base(Parent)
        {
            Attack = new AttackBehaviour(Parent);
            Traverse = new TraverseBehaviour(Parent);
            Follow = new FollowBehaviour(Parent)
            {
                ErrorMargin = 4 * Chunk.BlockSize,
                Target = Owner
            };
            _owner = Owner;
            _owner.SearchComponent<DamageComponent>().OnDamageEvent += OnOwnerDamaged;
            _owner.AfterDamaging += OnOwnerDamaging;
        }
        
        public override void Update()
        {
            if (!Enabled) return;
            if((Parent.Position - _owner.Position).LengthSquared > 128*128)
            {
                Parent.Position = _owner.BlockPosition + Vector3.UnitX * 12f;
                Attack.ResetTarget();
            }

            if (!_isStayingStill)
            {
                if (Attack.Enabled)
                {
                    Attack.Update();
                }
                else
                {
                    Follow.Target = _owner;
                    Follow.Update();
                }
            }
            else
            {
                Traverse.Update();
            }
        }

        public void StartStayStillAt(Vector3 Position)
        {
            _isStayingStill = true;
            Traverse.SetTarget(Position);
        }

        public void StopStayingStillAt()
        {
            _isStayingStill = false;
        }

        private void OnOwnerDamaged(DamageEventArgs Args)
        {
            Attack.SetTarget(Args.Damager);
        }

        private void OnOwnerDamaging(IEntity Victim, float Amount)
        {
            Attack.SetTarget(Victim);
        }

        public override void Dispose()
        {
            base.Dispose();
            _owner.SearchComponent<DamageComponent>().OnDamageEvent -= OnOwnerDamaged;
            _owner.AfterDamaging -= OnOwnerDamaging;
        }
    }
}
