/*
 * Author: Zaphyk
 * Date: 07/02/2016
 * Time: 01:34 a.m.
 *
 */


using Hedra.Engine.Windowing;

namespace Hedra.Engine.Events
{
    public interface IEventListener
    {
        void OnMouseButtonUp(object sender, MouseButtonEventArgs e);

        void OnMouseButtonDown(object sender, MouseButtonEventArgs e);

        void OnMouseWheel(object sender, MouseWheelEventArgs e);

        void OnMouseMove(object sender, MouseMoveEventArgs e);

        void OnKeyDown(object sender, KeyEventArgs EventArgs);

        void OnKeyUp(object sender, KeyEventArgs e);

        void OnCharWritten(string Char);
    }
}