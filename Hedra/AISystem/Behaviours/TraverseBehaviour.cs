using System;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Pathfinding;
using Hedra.EntitySystem;
using System.Drawing;
using Hedra.Components.Effects;
using Hedra.Engine.Game;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Scenes;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class TraverseBehaviour : Behaviour
    {
        protected WalkBehaviour Walk { get; }
        private Vector2[] _currentPath;
        private int _currentIndex;
        private Vector3 _origin;
        private bool _reached;
        private Action _callback;
        private float _speedBonus = 1;
        private float _lastBonus = 1;
        private SpeedBonusComponent _lastComponent;
        private bool _canReach;
        private Vector3 _lastCanNotReachPosition;

        public TraverseBehaviour(IEntity Parent, bool UseCollision = false) : base(Parent)
        {
            Walk = new WalkBehaviour(Parent);
            if (!UseCollision)
            {
                Parent.Physics.CollidesWithStructures = false;
                Parent.Physics.UpdateColliderList = true;
            }
            _reached = true;
            CreateGraph();
        }

        protected virtual void CreateGraph()
        {
            TraverseStorage.Instance.CreateIfNecessary(Parent, OnGridUpdated);
        }
        
        protected virtual void UpdateGraph()
        {
            TraverseStorage.Instance.Update(Parent);
        }
        
        protected virtual void RebuildGraph()
        {
            /* Force the rebuild because its stuck */
            TraverseStorage.Instance.RebuildIfNecessary(Parent, Parent.IsStuck);
        }

        protected virtual void DisposeGraph()
        {
            TraverseStorage.Instance.RemoveIfNecessary(Parent, OnGridUpdated);
        }

        protected virtual void ForceRebuildGraph()
        {
            TraverseStorage.Instance.ResetTime(Parent);
            TraverseStorage.Instance.RebuildIfNecessary(Parent, true);
        }
        protected virtual Vector3 CalculateTargetPoint(Vector2 PathPoint)
        {
            var centeredOffset =
                (PathPoint -
                 new Vector2((int) (CurrentGrid.DimX / 2f), (int) (CurrentGrid.DimY / 2f))).ToVector3();
            return centeredOffset * Chunk.BlockSize + _origin;
        }
        
        public override void Update()
        {
            if (!_reached && _canReach)
            {
                if (!Walk.HasTarget || Parent.IsStuck)
                {
                    RebuildAndResetPathIfNecessary();
                }
            }
            else
            {
                Walk.Cancel();
            }
            Walk.Update();
            if (_canReach)
            {
                UpdateGraph();
            }
            else
            {
                if ((_lastCanNotReachPosition - Target).Xz().LengthSquared() > 2 * 2)
                {
                    _canReach = true;
                }
                //if(Walk.HasTarget)
                //    Parent.IsStuck = true;
            }    
            /* UpdateSpeedBonus(); */
        }

        private void RebuildAndResetPathIfNecessary()
        {
            RebuildGraph();
            RebuildPathIfNecessary();
            if (_currentIndex < _currentPath.Length)
            {
                Walk.SetTarget(CalculateTargetPoint(_currentPath[_currentIndex]),
                    () =>
                    {
                        _currentIndex++;
                        if (_currentIndex > _currentPath.Length - 1) Cancel();
                    }
                );
            }
        }

        private void OnGridUpdated(Grid UpdatedGrid)
        {
            UpdatePath();
        }

        private void RebuildPathIfNecessary()
        {
            if(_currentPath == null)
                UpdatePath();
        }
        
        private void UpdateSpeedBonus()
        {
            if (Math.Abs(_lastBonus - _speedBonus) > 0.005f)
            {
                if(_lastComponent != null) Parent.RemoveComponent(_lastComponent);
                _lastComponent = null;
                if (Math.Abs((_lastBonus = _speedBonus) * Parent.Speed - Parent.Speed) < 0.005f) return;
                Parent.AddComponent(_lastComponent = new SpeedBonusComponent(Parent, -Parent.Speed + _speedBonus * Parent.Speed));
            }
        }

        protected virtual Vector2[] DoUpdatePath(Vector3 Origin, out bool CanReach)
        {
            CanReach = true;
            var target = (Target.Xz() - Origin.Xz()) * new Vector2(1f / Chunk.BlockSize, 1f / Chunk.BlockSize);
            var center = new Vector2((int)(CurrentGrid.DimX / 2f), (int)(CurrentGrid.DimY / 2f));
            var end = center + new Vector2((int)target.X, (int)target.Y);
            var clampedEnd = new Vector2(Math.Max(Math.Min(end.X, CurrentGrid.DimX - 1), 0), Math.Max(Math.Min(end.Y, CurrentGrid.DimY - 1), 0)); 
            var unblockedCenter = Finder.NearestUnblockedCell(CurrentGrid, center);
            
            if (float.IsInfinity(CurrentGrid.GetCellCost(clampedEnd))) 
                clampedEnd = Finder.NearestUnblockedCell(CurrentGrid, clampedEnd, unblockedCenter);
            
            var currentPath = (Vector2[]) null;
            currentPath = unblockedCenter == center 
                ? Finder.GetPath(CurrentGrid, center, clampedEnd) 
                : new []{ unblockedCenter };

            if(currentPath.Length == 1 && currentPath[0] == center)
            {
                CanReach = false;
            }
/*
            _speedBonus = CurrentGrid.BlockedCellCount < CurrentGrid.TotalCellCount * .25f && HasTarget && (Target - Parent.Position).Xz().LengthSquared() > 24*24
                ? 1.15f
                : 1f;
                */
            return currentPath;
        }
        
        protected void UpdatePath()
        {
            if (_reached || !_canReach) return;
            _origin = Parent.Position;
            _currentIndex = 0;
            _currentPath = DoUpdatePath(_origin, out var canReach);
            if (!canReach) _lastCanNotReachPosition = Target;
            _canReach = canReach;
        }

        public void SetTarget(Vector3 Position, Action Callback = null)
        {
            if ((Parent.Position - Position).LengthSquared() < ErrorMargin * ErrorMargin) return;
            Target = Position;
            _callback = Callback;
            _reached = false;
            ForceRebuildGraph();
            RebuildPathIfNecessary();
        }

        public void Cancel()
        {
            _reached = true;
            _callback?.Invoke();
        }

        public void CancelWalk()
        {
            Walk.Cancel();
        }

        private Grid CurrentGrid => TraverseStorage.Instance[Parent];

        public Vector2 GridSize => new Vector2(CurrentGrid.DimX, CurrentGrid.DimY);

        public void ResizeGrid(Vector2 Size)
        {
            TraverseStorage.Instance.ResizeGrid(Parent, Size);
        }

        public float ErrorMargin
        {
            get => Walk.ErrorMargin;
            set => Walk.ErrorMargin = value;
        }
        
        public bool HasInvalidPath => _currentPath == null;
        
        public Vector3 Target { get; private set; }

        public bool HasTarget => !_reached;
        
        public override void Dispose()
        {
            Walk.Dispose();
            DisposeGraph();
        }
        
    }
}

#region DEBUG
/*
if (Parent.Type != "Boar" || true) return currentPath;

//Debug

var bmp1 = new Bitmap(CurrentGrid.DimX, CurrentGrid.DimY);
for (var x = 0; x < bmp1.Width; ++x)
{
    for (var y = 0; y < bmp1.Width; ++y)
    {
        bmp1.SetPixel(x, y, float.IsInfinity(CurrentGrid.GetCellCost(new Vector2(x,y))) ? Color.Red : Color.LawnGreen);
    } 
}

for (var i = 0; i < _currentPath.Length; ++i)
{
    bmp1.SetPixel((int)_currentPath[i].X, (int)_currentPath[i].Y, Color.CornflowerBlue);
}
bmp1.SetPixel((int)clampedEnd.X, (int)clampedEnd.Y, Color.Black);
bmp1.SetPixel((int)unblockedCenter.X, (int)unblockedCenter.Y, Color.White);
bmp1.SetPixel((int)center.X, (int)center.Y, Color.Violet);
bmp1.Save(GameLoader.AppPath + $"/test{Parent.MobId}.png");
*/
#endregion
