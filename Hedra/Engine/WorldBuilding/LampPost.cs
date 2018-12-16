/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/06/2017
 * Time: 10:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
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
    public class LampPost : BaseStructure, IUpdatable
    {
        private PointLight _light;
        public Vector3 LightColor { get; set; } = Vector3.One;
        public float Radius { get; set; } = 24;
        
        
        public LampPost(Vector3 Position) : base(Position)
        {
            UpdateManager.Add(this);
        }
        
        public void Update()
        {
            var inRadius = (GameManager.Player.Position - this.Position).Xz.LengthSquared < ShaderManager.LightDistance * ShaderManager.LightDistance;
            if(_light == null && inRadius)
            {
                _light = ShaderManager.GetAvailableLight();
                if(_light != null)
                {
                    _light.Position = this.Position;
                    _light.Color = LightColor;
                    _light.Radius = Radius;
                    ShaderManager.UpdateLight(_light);
                }
                
            }
            else if(_light != null && !inRadius)
            {
                _light.Locked = false;
                _light.Position = Vector3.Zero;
                ShaderManager.UpdateLight(_light);
                _light = null;
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            if(_light != null)
            {
                _light.Locked = false;
                _light.Position = Vector3.Zero;
                ShaderManager.UpdateLight(_light);
            }
            UpdateManager.Remove(this);
        }
    }
}
