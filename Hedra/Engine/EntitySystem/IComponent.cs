namespace Hedra.Engine.EntitySystem
{
    public interface IComponent<out T> where T : IEntity
    {
        bool Renderable { get; }

        void Update();

        void Draw();

        void Dispose();
    }
}