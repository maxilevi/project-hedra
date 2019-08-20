using System;

namespace Hedra.EntitySystem
{
    public interface IComponent<out T> where T : IEntity, IDisposable
    {
        void Update();

        void Draw();

        void Dispose();
    }
}