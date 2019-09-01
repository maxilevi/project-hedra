using System.Collections;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class Portal : BaseStructure, IUpdatable
    {
        private const int PortalRadius = 12;
        private static readonly VertexData PortalMesh;
        private readonly ObjectMesh _portalObject;
        private static readonly float _portalHeight;
        private readonly Vector3 _scale;
        private readonly WorldLight _ambientLight;
        private readonly WorldLight _portalLight;
        private readonly int _realm;
        private readonly Vector3 _defaultSpawn;
        private readonly bool _useLastPositionForSpawnPoint;
        private bool _isTeleporting;
        private readonly Timer _teleportedRecentlyTimer;
        protected bool TeleportedRecently { get; private set; }
        
        static Portal()
        {
            PortalMesh = AssetManager.PLYLoader("Assets/Env/Objects/Portal.ply", Vector3.One * .35f);
            _portalHeight = PortalMesh.SupportPoint(Vector3.UnitY).Y - PortalMesh.SupportPoint(-Vector3.UnitY).Y;
        }

        protected Portal(Vector3 Position, Vector3 Scale, int Realm) : this(Position, Scale, Realm, default(Vector3), true)
        { 
        }
        
        protected Portal(Vector3 Position, Vector3 Scale, int Realm, Vector3 DefaultSpawn) : this(Position, Scale, Realm, DefaultSpawn, false)
        {  
        }
        
        private Portal(Vector3 Position, Vector3 Scale, int Realm, Vector3 DefaultSpawn, bool UseLastPositionForSpawnPoint) : base(Position)
        {
            _defaultSpawn = DefaultSpawn;
            _useLastPositionForSpawnPoint = UseLastPositionForSpawnPoint;
            _realm = Realm;
            _portalObject = ObjectMesh.FromVertexData(PortalMesh.Clone().Scale(_scale = Scale));
            _portalObject.Alpha = .4f;
            _portalObject.Enabled = true;
            _portalObject.Outline = true;
            _portalObject.OutlineColor = Colors.LightBlue * 2f;
            _portalObject.ApplySSAO = false;
            _teleportedRecentlyTimer = new Timer(2f);
            DrawManager.Remove(_portalObject);
            DrawManager.AddTransparent(_portalObject);
            UpdateManager.Add(this);
            _ambientLight = new WorldLight(Position)
            {
                LightColor = Colors.LightBlue.Xyz * 1f,
                Radius = 80,
                IsNightLight = false
            };
            _portalLight = new WorldLight(Position)
            {
                LightColor = Colors.LightBlue.Xyz * 10f,
                Radius = 16,
                IsNightLight = false
            };
        }

        public void Update()
        {
            _portalObject.Position = _ambientLight.Position = _portalLight.Position = new Vector3(Position.X, Physics.HeightAtPosition(Position) + _portalHeight * .5f * _scale.Y, Position.Z);
            _portalObject.Rotation += Vector3.One * Time.DeltaTime * 2000f;
            _ambientLight.Update();
            _portalLight.Update();
            CheckTeleport();
            if (_teleportedRecentlyTimer.Tick())
                TeleportedRecently = false;
        }

        private void CheckTeleport()
        {
            if (!_isTeleporting && (GameManager.Player.Position - Position).LengthFast < PortalRadius)
            {
                _isTeleporting = true;
                RoutineManager.StartRoutine(TeleportEffect);
            }
        }
        
        private void Teleport()
        {
            SoundPlayer.PlaySound(SoundType.TeleportSound, Position);
            GameManager.Player.Realms.GoTo(_realm);
            var tpPosition = _useLastPositionForSpawnPoint ? GameManager.Player.Position : _defaultSpawn;
            GameManager.Player.Position =
                World.FindPlaceablePosition(
                    GameManager.Player,
                    tpPosition + new Vector3(Chunk.Width, 0, Chunk.Width) * (Utils.Rng.NextFloat() * 2 - 1)
                );
            TeleportedRecently = true;
            _teleportedRecentlyTimer.Reset();
        }

        private IEnumerator TeleportEffect()
        {
            var timer = new Timer(1);
            while ((GameManager.Player.Position - Position).LengthFast < PortalRadius && !timer.Tick())
            {
                GameManager.SpawningEffect = true;
                yield return null;
            }

            Teleport();
            _isTeleporting = false;
        }

        public override void Dispose()
        {
            base.Dispose();
            UpdateManager.Remove(this);
            DrawManager.RemoveTransparent(_portalObject);
            _portalObject.Dispose();
            _ambientLight.Dispose();
            _portalLight.Dispose();
        }
    }
}