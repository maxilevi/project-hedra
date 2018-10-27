using System;
using System.Drawing;
using Hedra.Engine.Player;

namespace HedraTests
{
    
    public class SimpleMessageDispatcherMock : IMessageDispatcher
    {
        public string LastMessage { get; private set; }
        
        public void ShowTitleMessage(string Message, float Seconds)
        {
            LastMessage = Message;
        }

        public void ShowMessage(string Message, float Seconds)
        {
            LastMessage = Message;
        }

        public void ShowMessageWhile(string Message, Func<bool> Condition)
        {
            if(!Condition()) return;
            LastMessage = Message;
        }

        public void ShowNotification(string Message, Color FontColor, float Seconds, bool PlaySound)
        {
            LastMessage = Message;
        }

        public void ShowNotification(string Message, Color FontColor, float Seconds)
        {
            LastMessage = Message;
        }

        public void ShowMessageWhile(string Message, Color TextColor, Func<bool> Condition)
        {
            if(!Condition()) return;
            LastMessage = Message;
        }

        public void ShowMessage(string Message, float Seconds, Color TextColor)
        {
            LastMessage = Message;
        }

        public void ShowTitleMessage(string Message, float Seconds, Color TextColor)
        {
            LastMessage = Message;
        }

        public void Reset()
        {
            LastMessage = null;
        }
    }
}