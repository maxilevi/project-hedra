using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Particles;
using OpenTK;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatModelHandler : UpdatableObjectMeshModel
    {
        private readonly BoatStateHandler _stateHandler;
        private readonly IPlayer _player;
        private Quaternion _targetTerrainOrientation;
        private Quaternion _terrainOrientation;
        private bool _wasInWater;

        public BoatModelHandler(IPlayer Player, BoatStateHandler StateHandler) : base(null)
        {
            _stateHandler = StateHandler;
            _player = Player;
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
            if (Model.Enabled && _player.CanInteract)
            {
                var waterNormal = Physics.WaterNormalAtPosition(this.Position);
                _targetTerrainOrientation =
                    new Matrix3(Mathf.RotationAlign(Vector3.UnitY, waterNormal)).ExtractRotation();
                _terrainOrientation = Quaternion.Slerp(_terrainOrientation, _targetTerrainOrientation,
                    Time.IndependantDeltaTime * 1f);
                _player.Model.TransformationMatrix *= Matrix4.CreateFromQuaternion(_terrainOrientation);
            }
            Model.TransformationMatrix = _player.Model.TransformationMatrix;
            Model.Rotation = _player.Model.Rotation;
            Model.Position = _player.Model.ModelPosition;
            Model.Enabled = _stateHandler.Enabled;
            if (_stateHandler.Velocity.LengthFast > 5 && _stateHandler.Enabled && _stateHandler.InWater)
            {
                this.MovingParticles(Model.TransformPoint(-Vector3.UnitZ * 2.5f));
            }
        }


        private void MovingParticles(Vector3 BoatTip)
        {
            var underChunk = World.GetChunkAt(_player.Position);
            World.Particles.VariateUniformly = true;
            World.Particles.Color = new Vector4((underChunk?.Biome.Colors.WaterColor ?? Colors.DeepSkyBlue).Xyz * .75f, .5f);
            World.Particles.Scale = Vector3.One * .2f * (Math.Min(_stateHandler.Velocity.LengthFast, 15) / 15);
            World.Particles.ScaleErrorMargin = new Vector3(.15f, .15f, .15f);
            World.Particles.Direction = -_player.Orientation * .1f;
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