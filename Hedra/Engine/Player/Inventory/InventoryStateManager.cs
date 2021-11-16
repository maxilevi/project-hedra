using System;
using System.Collections;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Input;
using Hedra.Numerics;

namespace Hedra.Engine.Player.Inventory
{
    public delegate void OnStateChangeEventHandler(bool State);

    public class InventoryStateManager : StateManager
    {
        private bool _isExiting;

        public InventoryStateManager(IPlayer Player)
        {
            RegisterStateItem(() => Player.View.TargetPitch, O => Player.View.TargetPitch = (float)O);
            RegisterStateItem(() => Player.View.CameraHeight, O => Player.View.CameraHeight = (Vector3)O);
            RegisterStateItem(() => Player.View.TargetDistance, O => Player.View.TargetDistance = (float)O);
            RegisterStateItem(() => Player.View.TargetYaw, O => Player.View.TargetYaw = (float)O);
            RegisterStateItem(() => Player.View.LockMouse, O => Player.View.LockMouse = (bool)O);
            RegisterStateItem(() => Cursor.Show, O => Cursor.Show = (bool)O);
            RegisterStateItem(() => Player.Movement.CaptureMovement, O => Player.Movement.CaptureMovement = (bool)O);
            RegisterStateItem(() => Player.View.CaptureMovement, O => Player.View.CaptureMovement = (bool)O);
            RegisterStateItem(() => Player.View.PositionDelegate, O => Player.View.PositionDelegate = (Func<Vector3>)O,
                true);
            RegisterStateItem(() => Player.IsSitting, O => Player.IsSitting = (bool)O, true);
        }

        public event OnStateChangeEventHandler OnStateChange;

        public override void ReleaseState()
        {
            if (!_state) throw new InvalidOperationException("Cannot release an empty state.");
            if (_isExiting) return;
            TaskScheduler.Concurrent(LerpState);
        }

        public override void CaptureState()
        {
            base.CaptureState();
            OnStateChange?.Invoke(_state);
        }

        private IEnumerator LerpState()
        {
            _isExiting = true;
            foreach (var cacheItem in _cache)
                if (cacheItem.Key.ReleaseFirst)
                    cacheItem.Key.Setter.Invoke(cacheItem.Value);
            while (_state)
            {
                var finishedLerp = false;
                foreach (var cacheItem in _cache)
                    if (cacheItem.Value is float)
                    {
                        var prevValue = (float)cacheItem.Key.Getter.Invoke();
                        cacheItem.Key.Setter.Invoke(
                            Mathf.Lerp((float)cacheItem.Key.Getter.Invoke(), (float)cacheItem.Value,
                                Time.DeltaTime * 16f)
                        );
                        finishedLerp = Math.Abs((float)cacheItem.Key.Getter.Invoke() - prevValue) < 0.01f;
                    }
                    else if (cacheItem.Value is Vector3)
                    {
                        var prevValue = (Vector3)cacheItem.Key.Getter.Invoke();
                        cacheItem.Key.Setter.Invoke(
                            Mathf.Lerp((Vector3)cacheItem.Key.Getter.Invoke(), (Vector3)cacheItem.Value,
                                Time.DeltaTime * 16f)
                        );
                        finishedLerp = ((Vector3)cacheItem.Key.Getter.Invoke() - prevValue).Length() < 0.01f;
                    }

                if (finishedLerp) break;
                yield return null;
            }

            foreach (var cacheItem in _cache) cacheItem.Key.Setter.Invoke(cacheItem.Value);
            _cache.Clear();
            _state = false;
            OnStateChange?.Invoke(_state);
            _isExiting = false;
        }
    }
}