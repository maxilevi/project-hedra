/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/12/2016
 * Time: 11:27 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Components
{
    public class HeadIconComponent : EntityComponent
    {
        private readonly Humanoid _humanoidParent;
        private ObjectMesh _iconMesh;

        public HeadIconComponent(IEntity Parent) : base(Parent)
        {
            _humanoidParent = Parent as Humanoid;
        }

        private bool ParentIsHumanoid => _humanoidParent != null;
        public bool RotateIcon { get; set; } = true;

        public void ShowIcon(CacheItem? IconType)
        {
            if (IconType != null)
            {
                _iconMesh?.Dispose();
                var model = CacheManager.GetModel(IconType.Value).Clone();
                model.Scale(Parent.Model.BaseBroadphaseBox.Size.Average() * Vector3.One * .15f);
                _iconMesh = ObjectMesh.FromVertexData(model);
            }
            else
            {
                _iconMesh?.Dispose();
                _iconMesh = null;
            }
        }

        public void ShowIconFor(CacheItem? IconType, float Seconds)
        {
            ShowIcon(IconType);
            TaskScheduler.After(Seconds, () => ShowIcon(null));
        }

        public override void Update()
        {
            if (_iconMesh != null)
            {
                if (RotateIcon) _iconMesh.LocalRotation += Vector3.UnitY * Time.DeltaTime * 35f;
                Vector3 headOffset;
                if (ParentIsHumanoid)
                    headOffset = _humanoidParent.Model.HeadPosition + Vector3.UnitY * 2f;
                else
                    headOffset = Parent.Position + Vector3.UnitY * (2f + Parent.Model.Height);
                _iconMesh.Position = headOffset;
                _iconMesh.Enabled =
                    (LocalPlayer.Instance.Position - Parent.Position).LengthSquared() < 128 * 128;
            }
        }

        public override void Dispose()
        {
            _iconMesh?.Dispose();
        }
    }
}