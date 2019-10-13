using Hedra.Engine.Events;
using OpenToolkit.Windowing.Common.Input;

namespace Hedra.Engine.Input
{
    public static class ArrowSelector
    {
        public static void ProcessKeyDown(KeyEventArgs EventArgs, ArrowSelectorState State)
        {
            switch (EventArgs.Key)
            {
                case Key.Up:
                    if(!State.UpPressed)
                        State.OnUp?.Invoke();
                    State.UpPressed = true;
                    break;
                case Key.Down:
                    if(!State.DownPressed)
                        State.OnDown?.Invoke();
                    State.DownPressed = true;
                    break;
                case Key.Right:
                    if(!State.RightPressed)
                        State.OnRight?.Invoke();
                    State.RightPressed = true;
                    break;
                case Key.Left:
                    if(!State.LeftPressed)
                        State.OnLeft?.Invoke();
                    State.LeftPressed = true;
                    break;
                case Key.Enter:
                    if (!State.EnterPressed)
                        State.OnEnter?.Invoke();
                    State.EnterPressed = true;
                    break;
                default:
                    return;
            }
        }

        public static void ProcessKeyUp(KeyEventArgs EventArgs, ArrowSelectorState State)
        {
            switch (EventArgs.Key)
            {
                case Key.Enter:
                    State.EnterPressed = false;
                    break;
                case Key.Right:
                    State.RightPressed = false;
                    break;
                case Key.Left:
                    State.LeftPressed = false;
                    break;
                case Key.Down:
                    State.DownPressed = false;
                    break;
                case Key.Up:
                    State.UpPressed = false;
                    break;
                default:
                    return;
            }
        }
    }
}