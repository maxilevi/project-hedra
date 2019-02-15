using System;

namespace Hedra.EntitySystem
{
    public interface IComponent<out T> where T : IEntity, IDisposable
    {
        bool Drawable { get; }

        void Update();

        void Draw();

        void Dispose();
    }
}