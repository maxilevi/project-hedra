/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/06/2017
 * Time: 10:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using OpenTK;
using Hedra.Engine.Player;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Rendering;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Manages a static light
    /// </summary>
    public class WorldLight : BaseStructure, IUpdatable
    {
        public PointLight LightObject { get; private set; }
        public Vector3 LightColor { get; set; } = Vector3.One;
        public float Radius { get; set; } = PointLight.DefaultRadius;
        public bool Enabled { get; set; } = true;
        public bool DisableAtNight { get; set; } = true;
        
        
        public WorldLight(Vector3 Position) : base(Position)
        {
            UpdateManager.Add(this);
        }
        
        public void Update()
        {
            var inRadius = (GameManager.Player.Position - Position).Xz.LengthSquared < ShaderManager.LightDistance * ShaderManager.LightDistance;
            var isOn = inRadius && (SkyManager.IsNight || !DisableAtNight) && Enabled;

            if(LightObject == null && isOn)
            {
                LightObject = ShaderManager.GetAvailableLight();
                if (LightObject == null) return;
                LightObject.Position = this.Position;
                LightObject.Color = LightColor;
                LightObject.Radius = Radius;
                ShaderManager.UpdateLight(LightObject);

            }
            else if(LightObject != null && !isOn)
            {
                LightObject.Locked = false;
                LightObject.Position = Vector3.Zero;
                ShaderManager.UpdateLight(LightObject);
                LightObject = null;
            }
            if (LightObject != null)
            {
                if ((LightObject.Position - this.Position).LengthFast > 0.05f)
                {
                    LightObject.Position = this.Position;
                    ShaderManager.UpdateLight(LightObject);
                }
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            if(LightObject != null)
            {
                LightObject.Locked = false;
                LightObject.Position = Vector3.Zero;
                ShaderManager.UpdateLight(LightObject);
            }
            UpdateManager.Remove(this);
        }
    }
}
