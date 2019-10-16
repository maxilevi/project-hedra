using System;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using Hedra.EntitySystem;
using Hedra.Rendering;
using Hedra.Rendering.Particles;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatModelHandler : UpdatableObjectMeshModel
    {
        private readonly BoatStateHandler _stateHandler;
        private readonly IHumanoid _humanoid;
        private bool _wasInWater;

        public BoatModelHandler(IHumanoid Humanoid, BoatStateHandler StateHandler) : base(null)
        {
            _stateHandler = StateHandler;
            _humanoid = Humanoid;
            Build();
        }

        private void Build()
        {
            var mesh = ObjectMesh.FromVertexData(
                AssetManager.PLYLoader("Assets/Items/Misc/Boat.ply", Vector3.One * 5f)
            );
            mesh.ApplyNoiseTexture = true;
            Model = mesh;
        }

        public void Update()
        {
            if (Model.Enabled && _humanoid.CanInteract)
            {
                _humanoid.Model.TransformationMatrix *= _stateHandler.Transformation;
            }
            Model.TransformationMatrix = _humanoid.Model.TransformationMatrix;
            Model.LocalRotation = _humanoid.Model.LocalRotation;
            Model.Position = _humanoid.Model.ModelPosition;
            Model.Enabled = _stateHandler.Enabled;
            if (_stateHandler.Velocity.LengthFast() > 5 && _stateHandler.Enabled && _stateHandler.InWater)
            {
                this.MovingParticles(Model.TransformPoint(-Vector3.UnitZ * 2.5f));
            }
        }


        private void MovingParticles(Vector3 BoatTip)
        {
            var underChunk = World.GetChunkAt(_humanoid.Position);
            World.Particles.VariateUniformly = true;
            World.Particles.Color = new Vector4((underChunk?.Biome.Colors.WaterColor ?? Colors.DeepSkyBlue).Xyz() * .75f, .5f);
            World.Particles.Scale = Vector3.One * .2f * (Math.Min(_stateHandler.Velocity.LengthFast(), 15) / 15);
            World.Particles.ScaleErrorMargin = new Vector3(.15f, .15f, .15f);
            World.Particles.Direction = -_humanoid.Orientation * .1f;
            World.Particles.ParticleLifetime = 1f;
            World.Particles.GravityEffect = .1f;
            World.Particles.ConeAngle = 180;
            World.Particles.Shape = ParticleShape.Cone;

            for (var i = 0; i < 5; i++)
            {
                World.Particles.Position = BoatTip - Vector3.UnitY * .5f;
                World.Particles.Emit();
            }
            World.Particles.Shape = ParticleShape.Sphere;
        }
    }
}