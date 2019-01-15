using System;

namespace Hedra.Engine.Native
{
    public class DummyMessageManager : IMessageManager
    {
        public void Show(string Message, string Title)
        {
            Console.WriteLine($"{Title}: {Message}");
        }
    }
}