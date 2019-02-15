using System;
using Hedra.Engine;
using Hedra.Engine.Management;
using Hedra.Engine.Pathfinding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Behaviours
{
    public delegate void OnGridUpdated(Grid UpdatedGrid);
    
    public class GridStorage
    {
        public event OnGridUpdated GridUpdated;
        public Grid Storage { get; set; }
        private readonly Timer _rebuildPathTimer;
        private float _currentTimeBetweenTargets;
        private float _lastTimeBetweenTargets;
        private bool _useRebuildTimer;
        public int ReferenceCounter { get; set; } = 1;

        public GridStorage(Grid Storage)
        {
            this.Storage = Storage;
            _rebuildPathTimer = new Timer(.25f)
            {
                AutoReset = false
            };
        }
        
        public void RebuildIfNecessary(IEntity Parent, bool NewTarget)
        {
            var needsRebuild = _useRebuildTimer ? _rebuildPathTimer.Ready : NewTarget;
            if (needsRebuild)
                UpdateGrid(Parent);
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