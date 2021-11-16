/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 02:14 a.m.
 *
 */

using Hedra.Engine.Windowing;

namespace Hedra.Engine.Events
{
    public abstract class EventListener : IEventListener
    {
        protected EventListener()
        {
            EventDispatcher.Add(this);
        }

        public virtual void OnMouseButtonUp(object Sender, MouseButtonEventArgs e)
        {
        }

        public virtual void OnMouseButtonDown(object Sender, MouseButtonEventArgs e)
        {
        }

        public virtual void OnMouseWheel(object Sender, MouseWheelEventArgs e)
        {
        }

        public virtual void OnMouseMove(object Sender, MouseMoveEventArgs e)
        {
        }

        public virtual void OnKeyDown(object Sender, KeyEventArgs EventArgs)
        {
        }

        public virtual void OnKeyUp(object Sender, KeyEventArgs e)
        {
        }

        public virtual void OnCharWritten(string Char)
        {
        }

        public virtual void Dispose()
        {
            EventDispatcher.Remove(this);
        }
    }
}