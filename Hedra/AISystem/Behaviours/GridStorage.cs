using System;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Engine.Scenes;

namespace Hedra.AISystem.Behaviours
{
    public delegate void OnGridUpdated(WaypointGrid UpdatedGrid);
    
    public class GridStorage
    {
        public event OnGridUpdated GridUpdated;
        public WaypointGrid Storage { get; set; }
        private readonly Timer _rebuildPathTimer;
        private float _currentTimeBetweenTargets;
        private float _lastTimeBetweenTargets;
        private bool _useRebuildTimer;
        public int ReferenceCounter { get; set; } = 1;

        public GridStorage(WaypointGrid Storage)
        {
            this.Storage = Storage;
            _rebuildPathTimer = new Timer(.25f)
            {
                AutoReset = false
            };
        }
        
        public bool RebuildIfNecessary(IEntity Parent, bool NewTarget)
        {
            var needsRebuild = _useRebuildTimer ? _rebuildPathTimer.Ready : NewTarget;
            if (needsRebuild)
                UpdateGrid(Parent);
            return needsRebuild;
        }

        public void ResetTime()
        {
            _lastTimeBetweenTargets = _currentTimeBetweenTargets;
            _currentTimeBetweenTargets = 0;
        }

        public void Update()
        {
            UpdateStrategy();
            _rebuildPathTimer.Tick();
        }
        
        private void UpdateStrategy()
        {
            _currentTimeBetweenTargets += Time.DeltaTime;
            _useRebuildTimer = Math.Max(_currentTimeBetweenTargets, _lastTimeBetweenTargets) < _rebuildPathTimer.AlertTime;
        }

        private void UpdateGrid(IEntity Parent)
        {
            Finder.UpdateGrid(Parent, Storage);
            _rebuildPathTimer.Reset();
            GridUpdated?.Invoke(Storage);
        }
    }
}