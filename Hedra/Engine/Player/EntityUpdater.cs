using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hedra.Core;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Sound;
using Hedra.Game;

namespace Hedra.Engine.Player
{
    public class EntityUpdater
    {
        private static SpinWait _spinner;
        private static Thread _updateThread;
        private static bool _isWaiting;
        private static Stopwatch _watch;
        
        public static void Load()
        {
            _updateThread = new Thread(Update);
            _updateThread.Start();
            _spinner = new SpinWait();
            _watch = new Stopwatch();
        }
        
        public static void Dispatch()
        {
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
                if(_isWaiting) _spinner.SpinOnce();
                var totalSeconds = _watch.Elapsed.TotalSeconds;
                var Delta = Math.Min(1.0, totalSeconds - lastTick);
                lastTick = totalSeconds;
                var frameTime = Delta;
                while (frameTime > 0f)
                {
                    var delta = Math.Min(frameTime, Physics.Timestep);
                    Time.Set(delta, false);
                    DoUpdate();
                    frameTime -= delta;
                }
                Time.Set(Delta);
                Time.IncrementFrame(Delta);
                _isWaiting = true;
            }
        }

        private static void DoUpdate()
        {
            var entities = World.Entities.ToArray();
            for (var i = entities.Length - 1; i > -1; i--)
            {
                if (entities[i] != GameManager.Player && entities[i].InUpdateRange && !GameSettings.Paused &&
                    !GameManager.IsLoading
                    || entities[i].IsBoss)
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
    }
}