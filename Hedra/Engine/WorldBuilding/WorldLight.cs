/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/06/2017
 * Time: 10:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    ///     Manages a static light
    /// </summary>
    public class WorldLight : BaseStructure, IUpdatable
    {
        public static readonly Vector3 DefaultColor = new Vector3(1, .6f, .5f);
        private readonly bool _useReserved;

        public WorldLight(Vector3 Position, bool UseReservedLight = false) : base(Position)
        {
            _useReserved = UseReservedLight;
            BackgroundUpdater.Add(this);
        }

        public PointLight LightObject { get; private set; }
        public Vector3 LightColor { get; set; } = Vector3.One;
        public float Radius { get; set; } = PointLight.DefaultRadius;
        public bool Enabled { get; set; } = true;
        public bool IsNightLight { get; set; } = true;

        public void Update()
        {
            var inRadius = (GameManager.Player.Position - Position).Xz().LengthSquared() <
                           ShaderManager.LightDistance * ShaderManager.LightDistance;
            var isOn = inRadius && (SkyManager.IsNight || !IsNightLight) && Enabled;

            if (LightObject == null && isOn)
            {
                LightObject = ShaderManager.GetAvailableLight(_useReserved);
                if (LightObject == null) return;
                LightObject.Position = Position;
                LightObject.Color = LightColor;
                LightObject.Radius = Radius;
                ShaderManager.UpdateLight(LightObject);
            }
            else if (LightObject != null && !isOn)
            {
                LightObject.Locked = false;
                LightObject.Position = Vector3.Zero;
                ShaderManager.UpdateLight(LightObject);
                LightObject = null;
            }

            if (LightObject != null)
                if ((LightObject.Position - Position).LengthFast() > 0.05f)
                {
                    LightObject.Position = Position;
                    ShaderManager.UpdateLight(LightObject);
                }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (LightObject != null)
            {
                LightObject.Locked = false;
                LightObject.Position = Vector3.Zero;
                ShaderManager.UpdateLight(LightObject);
            }

            BackgroundUpdater.Remove(this);
        }
    }
}