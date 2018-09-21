using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatModelHandler : UpdatableObjectMeshModel
    {
        private readonly BoatStateHandler _stateHandler;
        private readonly IPlayer _player;
        private Quaternion _targetTerrainOrientation;
        private Quaternion _terrainOrientation;

        public BoatModelHandler(IPlayer Player, BoatStateHandler StateHandler) : base(null)
        {
            _stateHandler = StateHandler;
            _player = Player;
            Build();
        }

        private void Build()
        {
            Model = ObjectMesh.FromVertexData(
                AssetManager.PLYLoader("Assets/Items/Misc/Boat.ply", Vector3.One * 5f)
            );
        }

        public void Update()
        {
            _targetTerrainOrientation = 
                new Matrix3(Mathf.RotationAlign(Vector3.UnitY, Physics.NormalAtPosition(this.Position))).ExtractRotation();
            _terrainOrientation = Quaternion.Slerp(_terrainOrientation, _targetTerrainOrientation, Time.IndependantDeltaTime * 8f);
            //Model.TransformationMatrix = Matrix4.CreateFromQuaternion(_terrainOrientation);
            Model.TransformationMatrix = Matrix4.Identity;
            Model.Rotation = _player.Model.Rotation;
            Model.Position = _player.Model.ModelPosition;
            Model.Enabled = _stateHandler.Enabled;
            if (_stateHandler.Enabled)
            {
                //this.MovingParticles();
            }
        }


        private void MovingParticles()
        {
            var underChunk = World.GetChunkAt(_player.Position);
            World.Particles.VariateUniformly = true;
            World.Particles.Color = new Vector4((underChunk?.Biome.Colors.WaterColor ?? Colors.DeepSkyBlue).Xyz, .5f);
            World.Particles.Scale = Vector3.One * .5f;
            World.Particles.ScaleErrorMargin = new Vector3(.15f, .15f, .15f);
            World.Particles.Direction = -_player.Orientation * .25f;
            World.Particles.ParticleLifetime = .5f;
            World.Particles.GravityEffect = .1f;
            World.Particles.PositionErrorMargin = new Vector3(1.5f, 1.5f, 1.5f);

            for (var i = 0; i < 5; i++)
            {
                World.Particles.Position = _player.Position - Vector3.UnitY * 1f - _player.Orientation * 7.5f;
                World.Particles.Emit();
            }
        }
    }
}