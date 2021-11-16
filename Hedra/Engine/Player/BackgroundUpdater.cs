using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Sound;
using Hedra.Game;
using ThreadState = System.Threading.ThreadState;

namespace Hedra.Engine.Player
{
    public class BackgroundUpdater
    {
        private static Thread _updateThread;
        private static Stopwatch _watch;
        private static List<IUpdatable> _updateList;
        private static TickSystem _tickSystem;
        private static AutoResetEvent _resetEvent;
        private static ConcurrentQueue<ITickable> _toAddTickables, _toRemoveTickables;
        private static ConcurrentQueue<IUpdatable> _toAddUpdatables, _toRemoveUpdatables;

        public static void Load()
        {
            _tickSystem = new TickSystem();
            _updateThread = new Thread(Update);
            _watch = new Stopwatch();
            _updateList = new List<IUpdatable>();
            _resetEvent = new AutoResetEvent(false);
            _toAddTickables = new ConcurrentQueue<ITickable>();
            _toRemoveTickables = new ConcurrentQueue<ITickable>();
            _toAddUpdatables = new ConcurrentQueue<IUpdatable>();
            _toRemoveUpdatables = new ConcurrentQueue<IUpdatable>();
        }

        public static void Dispatch()
        {
            if (_updateThread.ThreadState == ThreadState.Unstarted)
                _updateThread.Start();
            _resetEvent.Set();
        }

        private static void Update()
        {
            Time.RegisterThread();
            _watch.Start();
            var lastTick = _watch.Elapsed.TotalSeconds;
            var elapsed = .0;
            var frames = 0;
            while (Program.GameWindow.Exists)
            {
                _resetEvent.WaitOne();
                var totalSeconds = _watch.Elapsed.TotalSeconds;
                var delta = Math.Min(1.0, totalSeconds - lastTick);
                lastTick = totalSeconds;
                var frameTime = delta;
                while (frameTime > 0f)
                {
                    var physicsDelta = Math.Min(frameTime, Physics.Timestep);
                    Time.Set(physicsDelta, false);
                    UpdateEntities();
                    UpdateCommands();
                    frameTime -= physicsDelta;
                }

                Time.Set(delta);
                Time.IncrementFrame(delta);
            }
        }

        private static void UpdateEntities()
        {
            var entities = World.Entities.ToArray();
            for (var i = entities.Length - 1; i > -1; i--)
            {
                if (entities[i] == GameManager.Player) continue;
                if (GameManager.Player.Companion.Entity == entities[i]) continue;
                var canUpdate = entities[i].InUpdateRange || entities[i].UpdateWhenOutOfRange;
                if (canUpdate && !GameSettings.Paused && !GameManager.IsLoading)
                    entities[i].Update();
                else if (canUpdate && GameSettings.Paused) (entities[i].Model as IAudible)?.StopSound();
                entities[i].UpdateCriticalComponents();
            }
        }

        private static void UpdateCommands()
        {
            AddTickables();
            AddUpdatables();
            RemoveTickables();
            RemoveUpdatables();
            for (var i = 0; i < _updateList.Count; ++i) _updateList[i].Update();
            _tickSystem.Tick();
        }

        private static void AddTickables()
        {
            while (!_toAddTickables.IsEmpty)
            {
                var result = _toAddTickables.TryDequeue(out var tickable);
                if (result) _tickSystem.Add(tickable);
            }
        }

        private static void AddUpdatables()
        {
            while (!_toAddUpdatables.IsEmpty)
            {
                var result = _toAddUpdatables.TryDequeue(out var updatable);
                if (result) _updateList.Add(updatable);
            }
        }

        private static void RemoveTickables()
        {
            while (!_toRemoveTickables.IsEmpty)
            {
                var result = _toRemoveTickables.TryDequeue(out var tickable);
                if (result) _tickSystem.Remove(tickable);
            }
        }

        private static void RemoveUpdatables()
        {
            while (!_toRemoveUpdatables.IsEmpty)
            {
                var result = _toRemoveUpdatables.TryDequeue(out var updatable);
                if (result) _updateList.Remove(updatable);
            }
        }

        public static void Add(IUpdatable Update)
        {
            _toAddUpdatables.Enqueue(Update);
        }

        public static void Remove(IUpdatable Update)
        {
            _toRemoveUpdatables.Enqueue(Update);
        }

        public static void Add(ITickable Tickable)
        {
            _toAddTickables.Enqueue(Tickable);
        }

        public static void Remove(ITickable Tickable)
        {
            _toRemoveTickables.Enqueue(Tickable);
        }
    }
}