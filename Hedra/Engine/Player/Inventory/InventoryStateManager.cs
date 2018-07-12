using System;
using System.Collections;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    internal delegate void OnStateChangeEventHandler(bool State);

    internal class InventoryStateManager : StateManager
    {
        private bool _isExiting;
        public event OnStateChangeEventHandler OnStateChange;

        public InventoryStateManager(LocalPlayer Player)
        {
            this.RegisterStateItem(() => Player.View.TargetPitch, O => Player.View.TargetPitch = (float) O);
            this.RegisterStateItem(() => Player.View.CameraHeight, O => Player.View.CameraHeight = (Vector3)O);
            this.RegisterStateItem(() => Player.View.TargetDistance, O => Player.View.TargetDistance = (float)O);
            this.RegisterStateItem(() => Player.View.TargetYaw, O => Player.View.TargetYaw = (float)O);
            this.RegisterStateItem(() => Player.View.LockMouse, O => Player.View.LockMouse = (bool)O);
            this.RegisterStateItem(() => UpdateManager.CursorShown, O => UpdateManager.CursorShown = (bool)O);
            this.RegisterStateItem(() => Player.Movement.CaptureMovement, O => Player.Movement.CaptureMovement = (bool)O);
            this.RegisterStateItem(() => Player.View.CaptureMovement, O => Player.View.CaptureMovement = (bool) O);
            this.RegisterStateItem(() => Player.View.PositionDelegate, O => Player.View.PositionDelegate = (Func<Vector3>)O, true);
        }

        public override void ReleaseState()
        {
            if (!_state) throw new InvalidOperationException("Cannot release an empty state.");
            if (_isExiting) return;
            TaskManager.Concurrent(this.LerpState);
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
            {
                if (cacheItem.Key.ReleaseFirst)
                    cacheItem.Key.Setter.Invoke(cacheItem.Value);
            }
            while (_state)
            {
                bool finishedLerp = false;
                foreach (var cacheItem in _cache)
                {
                    if (cacheItem.Value is float)
                    {
                        var prevValue = (float)cacheItem.Key.Getter.Invoke();
                        cacheItem.Key.Setter.Invoke(
                            Mathf.Lerp((float)cacheItem.Key.Getter.Invoke(), (float)cacheItem.Value,
                                (float)Time.DeltaTime * 16f)
                        );
                        finishedLerp = Math.Abs((float)cacheItem.Key.Getter.Invoke() - prevValue) < 0.01f;
                    }
                    else if (cacheItem.Value is Vector3)
                    {
                        var prevValue = (Vector3)cacheItem.Key.Getter.Invoke();
                        cacheItem.Key.Setter.Invoke(
                            Mathf.Lerp((Vector3)cacheItem.Key.Getter.Invoke(), (Vector3)cacheItem.Value,
                                (float)Time.DeltaTime * 16f)
                        );
                        finishedLerp = ((Vector3)cacheItem.Key.Getter.Invoke() - prevValue).Length < 0.01f;
                    }
                }
                if (finishedLerp) break;
                yield return null;
            }
            foreach (var cacheItem in _cache)
            {
                cacheItem.Key.Setter.Invoke(cacheItem.Value);
            }
            _cache.Clear();
            _state = false;
            OnStateChange?.Invoke(_state);
            _isExiting = false;
        }
    }
}
