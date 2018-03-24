using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryStateManager : StateManager
    {
        public InventoryStateManager(LocalPlayer Player)
        {
            this.RegisterStateItem(() => Player.View.Pitch, O => Player.View.Pitch = (float) O);
            this.RegisterStateItem(() => Player.View.CameraHeight, O => Player.View.CameraHeight = (Vector3)O);
            this.RegisterStateItem(() => Player.View.TargetDistance, O => Player.View.TargetDistance = (float)O);
            this.RegisterStateItem(() => Player.View.Yaw, O => Player.View.Yaw = (float)O);
            this.RegisterStateItem(() => Player.View.LockMouse, O => Player.View.LockMouse = (bool)O);
            this.RegisterStateItem(() => UpdateManager.CursorShown, O => UpdateManager.CursorShown = (bool)O);
            this.RegisterStateItem(() => Player.Movement.Check, O => Player.Movement.Check = (bool)O);
            this.RegisterStateItem(() => Player.View.Check, O => Player.View.Check = (bool) O);
            this.RegisterStateItem(() => Player.View.PositionDelegate, O => Player.View.PositionDelegate = (Func<Vector3>)O, true);
        }

        public new void ReleaseState()
        {
            if (!_state) throw new InvalidOperationException("Cannot release an empty state.");
            TaskManager.Concurrent(this.LerpState);
        }

        private IEnumerator LerpState()
        {
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
                                (float)Time.deltaTime * 16f)
                        );
                        finishedLerp = Math.Abs((float)cacheItem.Key.Getter.Invoke() - prevValue) < 0.01f;
                    }
                    else if (cacheItem.Value is Vector3)
                    {
                        var prevValue = (Vector3)cacheItem.Key.Getter.Invoke();
                        cacheItem.Key.Setter.Invoke(
                            Mathf.Lerp((Vector3)cacheItem.Key.Getter.Invoke(), (Vector3)cacheItem.Value,
                                (float)Time.deltaTime * 16f)
                        );
                        finishedLerp = ((Vector3)cacheItem.Key.Getter.Invoke() - prevValue).Length < 0.01f;
                    }
                }
                if (finishedLerp)
                {
                    foreach (var cacheItem in _cache)
                    {
                        if(!cacheItem.Key.ReleaseFirst)
                            cacheItem.Key.Setter.Invoke(cacheItem.Value);
                    }
                    _cache.Clear();
                    _state = false;
                }
                yield return null;
            }       
        }
    }
}
