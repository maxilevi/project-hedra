using System.Collections;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class Portal : BaseStructure, IUpdatable
    {
        private const int PortalRadius = 16;
        private static readonly VertexData PortalMesh;
        private readonly ObjectMesh _portalObject;
        private static readonly float _portalHeight;
        private readonly Vector3 _scale;
        private readonly WorldLight _ambientLight;
        private readonly WorldLight _portalLight;
        private bool _isTeleporting;
        private int _realm;
        
        static Portal()
        {
            PortalMesh = AssetManager.PLYLoader("Assets/Env/Objects/Portal.ply", Vector3.One * .35f);
            _portalHeight = PortalMesh.SupportPoint(Vector3.UnitY).Y - PortalMesh.SupportPoint(-Vector3.UnitY).Y;
        }
        
        protected Portal(Vector3 Position, Vector3 Scale, int Realm) : base(Position)
        {
            _realm = Realm;
            _portalObject = ObjectMesh.FromVertexData(PortalMesh.Clone().Scale(_scale = Scale));
            _portalObject.Alpha = .4f;
            _portalObject.Enabled = true;
            _portalObject.Outline = true;
            _portalObject.OutlineColor = Colors.LightBlue * 2f;
            _portalObject.ApplySSAO = false;
            DrawManager.Remove(_portalObject);
            DrawManager.AddTransparent(_portalObject);
            UpdateManager.Add(this);
            _ambientLight = new WorldLight(Position)
            {
                LightColor = Colors.LightBlue.Xyz * 1f,
                Radius = 80,
                DisableAtNight = false
            };
            _portalLight = new WorldLight(Position)
            {
                LightColor = Colors.LightBlue.Xyz * 10f,
                Radius = 16,
                DisableAtNight = false
            };
        }

        public void Update()
        {
            _portalObject.Position = _ambientLight.Position = _portalLight.Position = new Vector3(Position.X, Physics.HeightAtPosition(Position) + _portalHeight * .5f * _scale.Y, Position.Z);
            _portalObject.Rotation += Vector3.One * Time.DeltaTime * 2000f;
            _ambientLight.Update();
            _portalLight.Update();
            CheckTeleport();
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
            GameManager.Player.Realms.GoTo(_realm);
            SoundPlayer.PlaySound(SoundType.TeleportSound, Position);
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