using System;
using System.Collections.Generic;
using Hedra.Engine.Management;

namespace Hedra.Engine.PhysicsSystem
{
    public static class PhysicsScheduler
    {
        private static uint _lastId;
        private static readonly Dictionary<uint, PhysicsListener> Listeners = new Dictionary<uint, PhysicsListener>();
        private static readonly SchedulerWorker Worker = new SchedulerWorker(Listeners);

        public static void RemoveListener(uint Identifier)
        {
            PhysicsListener listener;
            lock (Listeners)
            {
                listener = Listeners[Identifier];
                Listeners.Remove(Identifier);
            }
            listener.Dispose();
        }

        public static bool ExistsListener(uint Identifier)
        {
            lock (Listeners)
            {
                return Listeners.ContainsKey(Identifier);
            }
        }

        public static uint AddListener(Func<CollisionShape[]> Shapes0, Func<CollisionShape[]> Shapes1, Action Callback)
        {
            var listener = new PhysicsListener(_lastId++, Shapes0, Shapes1, Callback);
            lock(Listeners) Listeners.Add(listener.Id, listener);
            return listener.Id;
        }

        public static void TemporalListener(float Seconds, Func<CollisionShape[]> Shapes0, Func<CollisionShape[]> Shapes1, Action Callback, Action FailureCallback)
        {
            var identifier = AddListener(Shapes0, Shapes1, Callback);
            TaskManager.After((int) (Seconds * 1000f), delegate
            {
                if (ExistsListener(identifier))
                {
                    RemoveListener(identifier);
                    FailureCallback();
                }
            });
        }

        public static void Update()
        {
            Worker.Wakeup();
        }
    }
}
