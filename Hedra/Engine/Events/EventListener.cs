/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 02:14 a.m.
 *
 */
using System;
using OpenTK.Input;
using OpenTK;
namespace Hedra.Engine.Events
{
    public class EventListener : IEventListener
    {
        public EventListener()
        {
            EventDispatcher.Add(this);
        }
        
        public virtual void OnMouseButtonUp(object Sender, MouseButtonEventArgs e){}
        
        public virtual void OnMouseButtonDown(object Sender, MouseButtonEventArgs e){}
        
        public virtual void OnMouseWheel(object Sender, MouseWheelEventArgs e){}
        
        public virtual void OnMouseMove(object Sender, MouseMoveEventArgs e){}
        
        public virtual void OnKeyDown(object Sender, KeyboardKeyEventArgs EventArgs){}

        public virtual void OnKeyUp(object Sender, KeyboardKeyEventArgs e){}
        
        public virtual void OnKeyPress(object Sender, KeyPressEventArgs e){}
    }
}
