
using System;
using System.Drawing;

namespace Hedra.Engine.Player
{
    public class DummyMessageDispatcher : IMessageDispatcher
    {
        public void ShowTitleMessage(string Message, float Seconds)
        {

        }

        public void ShowTitleMessage(string Message, float Seconds, Color TextColor)
        {

        }

        public void ShowMessage(string Message, float Seconds)
        {

        }

        public void ShowMessage(string Message, float Seconds, Color TextColor)
        {

        }

        public void ShowMessageWhile(string Message, Func<bool> Condition)
        {

        }

        public void ShowMessageWhile(string Message, Color TextColor, Func<bool> Condition)
        {

        }

        public void ShowNotification(string Message, Color FontColor, float Seconds)
        {

        }

        public void ShowNotification(string Message, Color FontColor, float Seconds, bool PlaySound)
        {

        }

        public void ShowPlaque(string Message, float Seconds, bool PlaySound = true)
        {
        }

        public bool HasTitleMessages => false;
    }
}
