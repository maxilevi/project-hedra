/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/12/2016
 * Time: 11:27 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Scenes;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class HeadIconComponent : EntityComponent
    {
        private ObjectMesh _iconMesh;
        private readonly Humanoid _humanoidParent;
        private bool ParentIsHumanoid => _humanoidParent != null;
        public bool RotateIcon { get; set; } = true;

        public HeadIconComponent(Entity Parent) : base(Parent)
        {
            _humanoidParent = Parent as Humanoid;
        }

        public void ShowIcon(CacheItem? IconType)
        {
            if (IconType != null)
            {
                _iconMesh?.Dispose();
                var model = CacheManager.GetModel(IconType.Value).Clone();
                model.Scale(Parent.Model.Scale);
                _iconMesh = ObjectMesh.FromVertexData(model);
            }
            else
            {
                _iconMesh?.Dispose();
                _iconMesh = null;
            }
        }

        public override void Update()
        {
            if (_iconMesh != null)
            {
                if (RotateIcon)
                {
                    _iconMesh.TargetRotation += Vector3.UnitY * (float) Time.deltaTime * 35f;
                }
                Vector3 headOffset;
                if (ParentIsHumanoid)
                {
                    headOffset = _humanoidParent.Model.HeadPosition + Vector3.UnitY * 2f;
                }
                else
                {
                    headOffset = Parent.Position + Vector3.UnitY * (2f + Parent.Model.Height);
                }
                _iconMesh.Position = headOffset;
                _iconMesh.Enabled =
                    (LocalPlayer.Instance.Position - Parent.Position).LengthSquared < 128 * 128;
            }
        }

        public override void Dispose()
        {
            _iconMesh?.Dispose();
        }
    }
}