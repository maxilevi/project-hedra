/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 20/01/2017
 * Time: 09:16 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

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
    /// Description of MountAIComponent.
    /// </summary>
    public class MountAIComponent : BasicAIComponent
    {
        private Entity _target;
        private readonly Entity _owner;
        private RideComponent _rideComp;
        
        public MountAIComponent(IEntity Parent, Entity Owner) : base(Parent)
        {
            this._owner = Owner;
            this._target = Owner;
        }
        
        public override void Update()
        {
            if(!Enabled) return;
            
            if( _target != null && _target != _owner && (_target.Position - Parent.Position).LengthSquared > 72*72 ){
                _target = _owner;
            }
            
            if( (Parent.Position - _owner.Position).LengthSquared > 128*128)
            {
                Parent.Position = _owner.BlockPosition + Vector3.UnitX * 12f;
            }
            
            float distance = (_target == _owner) ? 8 : 3;
            var distSqr = distance * distance;

            if (_target != null && ((_target.Position - Parent.Position).LengthSquared > distSqr * Chunk.BlockSize * Chunk.BlockSize && !Parent.IsMoving
                || (_target.Position - Parent.Position).LengthSquared > distSqr * .75f * Chunk.BlockSize * Chunk.BlockSize && Parent.IsMoving))
            {
                Parent.Orientation = (_target.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
                Parent.Physics.Move();
            }
        }
        
        public override AIType Type => AIType.Friendly;
    }
}
