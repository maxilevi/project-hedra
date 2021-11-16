using System;
using SixLabors.ImageSharp;
using SixLabors.Fonts;

namespace Hedra.Engine.Player
{
    public interface IMessageDispatcher
    {
        void ShowTitleMessage(string Message, float Seconds);
        void ShowMessage(string Message, float Seconds);
        void ShowMessage(string Message, float Seconds, Color TextColor);
        void ShowMessageWhile(string Message, Func<bool> Condition);
        void ShowMessageWhile(string Message, Color TextColor, Func<bool> Condition);
        void ShowNotification(string Message, Color FontColor, float Seconds);
        void ShowNotification(string Message, Color FontColor, float Seconds, bool PlaySound);
        void ShowPlaque(string Message, float Seconds, bool PlaySound = true);
        bool HasTitleMessages { get; }
    }
}