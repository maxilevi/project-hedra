using System;
using System.Drawing;

namespace Hedra.Engine.Player
{
    public interface IMessageDispatcher
    {
        void ShowTitleMessage(string Message, float Seconds);
        void ShowTitleMessage(string Message, float Seconds, Color TextColor);
        void ShowMessage(string Message, float Seconds);
        void ShowMessage(string Message, float Seconds, Color TextColor);
        void ShowMessageWhile(string Message, Func<bool> Condition);
        void ShowMessageWhile(string Message, Color TextColor, Func<bool> Condition);
        void ShowNotification(string Message, Color FontColor, float Seconds);
        void ShowNotification(string Message, Color FontColor, float Seconds, bool PlaySound);
    }
}
