using System;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Pathfinding;
using Hedra.EntitySystem;
using System.Drawing;
using Hedra.Components.Effects;
using Hedra.Engine.Game;
using OpenTK;

namespace Hedra.AISystem.Behaviours
{
    public class TraverseBehaviour : Behaviour
    {
        protected WalkBehaviour Walk { get; }
        private readonly Timer _rebuildPathTimer;
        private Vector2[] _currentPath;
        private int _currentIndex;
        private readonly Grid _currentGrid;
        private Vector3 _origin;
        private bool _reached;
        private Action _callback;
        private float _lastTimeBetweenTargets;
        private float _currentTimeBetweenTargets;
        private bool _useRebuildTimer;
        private float _speedBonus;
        private float _lastBonus;
        private SpeedBonusComponent _lastComponent;

        public TraverseBehaviour(IEntity Parent, bool UseCollision = false) : base(Parent)
        {
            Walk = new WalkBehaviour(Parent);
            if (!UseCollision)
            {
                Parent.Physics.CollidesWithStructures = false;
                Parent.Physics.UpdateColliderList = true;
            }
            _reached = true;
            _currentGrid = new Grid(32, 32);
            _rebuildPathTimer = new Timer(.2f)
            {
                AutoReset = false
            };
        }

        public override void Update()
        {
            if (!_reached)
            {
                if (!Walk.HasTarget || Parent.IsStuck)
                {
                    RebuildIfNecessary();
                    if (_currentIndex < _currentPath.Length)
                        Walk.SetTarget(
                            (_currentPath[_currentIndex] - new Vector2((int)(_currentGrid.DimX / 2f), (int)(_currentGrid.DimY / 2f))).ToVector3() * Chunk.BlockSize + _origin,
                            () =>
                            {
                                _currentIndex++;
                                if (_currentIndex > _currentPath.Length - 1) Cancel();
                            }
                        );
                }
            }
            else
            {
                Walk.Cancel();
            }
            _rebuildPathTimer.Tick();
            Walk.Update();
            UpdateStrategy();
            UpdateSpeedBonus();
        }

        private void UpdateSpeedBonus()
        {
            if (Math.Abs(_lastBonus - _speedBonus) > 0.005f)
            {
                _lastComponent?.Dispose();
                if(_lastComponent != null) Parent.RemoveComponent(_lastComponent);
                Parent.AddComponent(_lastComponent = new SpeedBonusComponent(Parent, -Parent.Speed + (_lastBonus = _speedBonus) * Parent.Speed));
            }
        }

        private void UpdateStrategy()
        {
            _currentTimeBetweenTargets += Time.DeltaTime;
            _useRebuildTimer = Math.Max(_currentTimeBetweenTargets, _lastTimeBetweenTargets) < _rebuildPathTimer.AlertTime;
        }
        
        private void RebuildIfNecessary(bool NewTarget = false)
        {
            var needsRebuild = _useRebuildTimer ? _rebuildPathTimer.Ready : NewTarget;
            if (needsRebuild || _currentPath == null)
                UpdatePath();
        }

        private void UpdatePath()
        {
            Finder.UpdateGrid(Parent, _currentGrid);
            
            _origin = Parent.Position;
            var target = (Target.Xz - _origin.Xz) * new Vector2(1f / Chunk.BlockSize, 1f / Chunk.BlockSize);
            var center = new Vector2((int)(_currentGrid.DimX / 2f), (int)(_currentGrid.DimY / 2f));
            var end = center + new Vector2((int)target.X, (int)target.Y);
            var clampedEnd = new Vector2(Math.Max(Math.Min(end.X, _currentGrid.DimX - 1), 0), Math.Max(Math.Min(end.Y, _currentGrid.DimY - 1), 0)); 
            var unblockedCenter = Finder.NearestUnblockedCell(_currentGrid, center);
            
            if (float.IsInfinity(_currentGrid.GetCellCost(clampedEnd))) 
                clampedEnd = Finder.NearestUnblockedCell(_currentGrid, clampedEnd, unblockedCenter);
            
            if (unblockedCenter == center)
                _currentPath = Finder.GetPath(_currentGrid, center, clampedEnd);
            else
                _currentPath = new []{ unblockedCenter };
            _currentIndex = 0;
            _rebuildPathTimer.Reset();

            _speedBonus = _currentGrid.BlockedCellCount < _currentGrid.TotalCellCount * .25f 
                ? 1.15f
                : 1f;

            if (Parent.Type != "Ent") return;
            
            //Debug
            /*
            var bmp0 = new Bitmap(_currentGrid.DimX, _currentGrid.DimY);
            var bmp1 = new Bitmap(_currentGrid.DimX, _currentGrid.DimY);
            for (var x = 0; x < bmp1.Width; ++x)
            {
                for (var y = 0; y < bmp1.Width; ++y)
                {
                    bmp0.SetPixel(x, y, float.IsInfinity(_currentGrid.GetCellCost(new Vector2(x,y))) ? Color.Red : Color.LawnGreen);
                    bmp1.SetPixel(x, y, float.IsInfinity(_currentGrid.GetCellCost(new Vector2(x,y))) ? Color.Red : Color.LawnGreen);
                } 
            }

            for (var i = 0; i < _currentPath.Length; ++i)
            {
                bmp1.SetPixel((int)_currentPath[i].X, (int)_currentPath[i].Y, Color.CornflowerBlue);
            }
            bmp1.SetPixel((int)clampedEnd.X, (int)clampedEnd.Y, Color.Black);
            bmp1.SetPixel((int)unblockedCenter.X, (int)unblockedCenter.Y, Color.White);
            bmp1.SetPixel((int)center.X, (int)center.Y, Color.Violet);
            bmp0.Save(GameLoader.AppPath + "/test0.png");
            bmp1.Save(GameLoader.AppPath + "/test1.png");*/
        }

        public void SetTarget(Vector3 Position, Action Callback = null)
        {
            Target = Position;
            _callback = Callback;
            _reached = false;
            _lastTimeBetweenTargets = _currentTimeBetweenTargets;
            _currentTimeBetweenTargets = 0;
            RebuildIfNecessary(true);
        }

        public void Cancel()
        {
            _reached = true;
            _callback?.Invoke();
        }

        public Vector3 Target { get; private set; }

        public bool HasTarget => !_reached;
    }
}
