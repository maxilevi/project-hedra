using System;

namespace Hedra.Engine.PhysicsSystem
{
    public class PhysicsListener : IDisposable
    { 
        public uint Id { get; set; }
        public Func<CollisionShape[]> Shapes0 { get; set; }
        public Func<CollisionShape[]> Shapes1 { get; set; }
        public Action Callback { get; set; }

        public PhysicsListener(uint Id, Func<CollisionShape[]> Shapes0, Func<CollisionShape[]> Shapes1, Action Callback)
        {
            this.Id = Id;
            this.Shapes0 = Shapes0;
            this.Shapes1 = Shapes1;
            this.Callback = Callback;
        }

        public void Dispose()
        {
            
        }
    }
}
