namespace Hedra.EntitySystem
{
    public interface IComponent<out T> where T : IEntity
    {
        bool Drawable { get; }

        void Update();

        void Draw();

        void Dispose();
    }
}