using System;
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
        private static bool _isWaiting;
        private static Stopwatch _watch;
        private static object _updateLock;
        private static List<IUpdatable> _updateList;
        private static TickSystem _tickSystem;
        
        public static void Load()
        {
            _tickSystem = new TickSystem();
            _updateThread = new Thread(Update);
            _watch = new Stopwatch();
            _updateList = new List<IUpdatable>();
            _updateLock = new object();
        }
        
        public static void Dispatch()
        {
            if(_updateThread.ThreadState == ThreadState.Unstarted)
                _updateThread.Start();
            _isWaiting = false;
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
                if(_isWaiting) Thread.Sleep(1);
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
                _isWaiting = true;
            }
        }

        private static void UpdateEntities()
        {
            var entities = World.Entities.ToArray();
            for (var i = entities.Length - 1; i > -1; i--)
            {
                if (entities[i] != GameManager.Player && 
                    (
                        entities[i].InUpdateRange &&
                        !GameSettings.Paused &&
                        !GameManager.IsLoading ||
                        entities[i].IsBoss)
                    )
                {
                    if(GameManager.Player.Companion.Entity == entities[i]) continue;
                    
                    entities[i].Update();
                }
                else if (entities[i] != GameManager.Player && entities[i].InUpdateRange && GameSettings.Paused)
                {
                    (entities[i].Model as IAudible)?.StopSound();
                }
            }
        }

        private static void UpdateCommands()
        {
            lock (_updateLock)
            {
                for (var i = 0; i < _updateList.Count; ++i)
                {
                    _updateList[i].Update();
                }
            }
            _tickSystem.Tick();
        }

        public static void Add(IUpdatable Update)
        {
            lock (_updateLock)
            {
                _updateList.Add(Update);
            }
        }

        public static void Remove(IUpdatable Update)
        {
            lock (_updateLock)
            {
                _updateList.Remove(Update);
            }
        }
        
        public static void Add(ITickable Tickable)
        {
            lock (_updateLock)
            {
                _tickSystem.Add(Tickable);
            }
        }

        public static void Remove(ITickable Tickable)
        {
            lock (_updateLock)
            {
                _tickSystem.Remove(Tickable);
            }
        }
    }
}