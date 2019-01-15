/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/06/2016
 * Time: 10:32 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Hedra.Core;
using Hedra.Engine.Events;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of OnDrag.
    /// </summary>
    public class DisplayOnDrag : EventListener
    {
        private bool _inFocus;
        public Vector2 Scale, Position;
        public float Value = -1f;
        public float Increment = 20;

        public override void OnMouseMove(object Sender, MouseMoveEventArgs E){
            
            if(_inFocus){
                Value += E.XDelta * Increment * Time.IndependantDeltaTime;
            }
            
            Vector2 coords = Mathf.ToNormalizedDeviceCoordinates(E.Mouse.X, E.Mouse.Y);
            
            if(Position.Y + Scale.Y > -coords.Y && Position.Y - Scale.Y < -coords.Y 
                && Position.X + Scale.X > coords.X && Position.X - Scale.X < coords.X ){
                if(E.Mouse.IsButtonDown(MouseButton.Left))
                    _inFocus = true;
                
            }
            
            if(E.Mouse.IsButtonUp(MouseButton.Left))
                _inFocus = false;
        }
    }
}
