/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 22/01/2017
 * Time: 01:33 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.Player
{
    /// <summary>
    /// Description of Lamp.
    /// </summary>
    public class HandLamp
    {
        private IHumanoid Humanoid { get; }
        private readonly WorldLight _lamp;
        private bool _enabled;

        public HandLamp(IHumanoid Humanoid)
        {
            this.Humanoid = Humanoid;
            _lamp = new WorldLight(Vector3.Zero)
            {
                LightColor = WorldLight.DefaultColor,
                IsNightLight = false,
                Enabled = false
            };
            UpdateManager.Remove(_lamp);
        }

        public void Update()
        {
            _lamp.Position = Humanoid.Position;
            _lamp.Update();
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                _lamp.Enabled = _enabled;
                Humanoid.Model.SetLamp(_enabled);
            }
        }

        public PointLight LightObject => _lamp.LightObject;
        
        public void Dispose()
        {
            _lamp.Dispose();
        }
    }
}
