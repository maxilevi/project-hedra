using System;

namespace Hedra.Engine.Rendering.Core
{
    public class RenderException : Exception
    {
        public RenderException(string Message) : base(Message)
        {
        }
    }
}