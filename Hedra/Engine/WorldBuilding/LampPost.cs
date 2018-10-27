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

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Manages a static light
    /// </summary>
    public class LampPost : BaseStructure, IUpdatable
    {
        public PointLight Light;
        public Vector3 LightColor = new Vector3(1,1,1f);
        public float Radius = 24;
        
        
        public LampPost(Vector3 Position){
            this.Position = Position;
            UpdateManager.Add(this);
        }
        
        public void Update(){
            var Player = GameManager.Player;
            bool InRadius = (Player.Position - this.Position).Xz.LengthSquared < ShaderManager.LightDistance * ShaderManager.LightDistance;
            if(Light == null && InRadius){
                Light = ShaderManager.GetAvailableLight();
                if(Light != null){
                    Light.Position = this.Position;
                    Light.Color = LightColor;
                    Light.Radius = Radius;
                    ShaderManager.UpdateLight(Light);
                }
                
            }else if(Light != null && !InRadius){
                //Light exists, make it dissapear
                
                Light.Locked = false;
                Light.Position = Vector3.Zero;
                ShaderManager.UpdateLight(Light);
                Light = null;
            }
        }
        
        public override void Dispose(){
            Disposed = true;
            if(Light != null){
                Light.Locked = false;
                Light.Position = Vector3.Zero;
                ShaderManager.UpdateLight(Light);
            }
            UpdateManager.Remove(this);
        }
    }
}
