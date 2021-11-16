using System;
using System.Linq;
using System.Numerics;
using Hedra.Components;
using Hedra.Components.Effects;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Scenes;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class TraverseBehaviour : Behaviour
    {
        private readonly Timer _stuckTimer;
        private Action _callback;
        private bool _canReach;
        private int _currentIndex;
        private Vector2[] _currentPath;
        private float _lastBonus = 1;
        private Vector3 _lastCanNotReachPosition;
        private SpeedBonusComponent _lastComponent;
        private Vector3 _origin;
        private float _speedBonus = 1;

        public TraverseBehaviour(IEntity Parent, bool UseCollision = false) : base(Parent)
        {
            Walk = new WalkBehaviour(Parent);
            if (!UseCollision)
            {
                Parent.Physics.CollidesWithStructures = false;
                Parent.Physics.UpdateColliderList = true;
            }

            _stuckTimer = new Timer(1);
            CreateGraph();
        }

        protected WalkBehaviour Walk { get; }

        private WaypointGrid CurrentGrid => TraverseStorage.Instance[Parent];

        public Vector2 GridSize => new Vector2(CurrentGrid.DimX, CurrentGrid.DimY);

        public float ErrorMargin
        {
            get => Walk.ErrorMargin;
            set => Walk.ErrorMargin = value;
        }

        public Vector3 Target { get; private set; }

        public bool HasTarget { get; private set; }

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
            TraverseStorage.Instance.RebuildIfNecessary(Parent, Parent.IsStuck && _stuckTimer.Tick());
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

        public override void Update()
        {
            if (!HasTarget) return;
            if (!Walk.HasTarget || Parent.IsStuck) RebuildAndResetPathIfNecessary();
            Walk.Update();
            if (_canReach)
            {
                UpdateGraph();
            }
            else
            {
                if ((_lastCanNotReachPosition - Target).Xz().LengthSquared() > 2 * 2) _canReach = true;
            }
        }

        private void RebuildAndResetPathIfNecessary()
        {
            RebuildGraph();
            RebuildPathIfNecessary();
            if (_currentIndex < _currentPath.Length)
                Walk.SetTarget(CalculateTargetPoint(_currentPath[_currentIndex]),
                    () =>
                    {
                        _currentIndex++;
                        if (_currentIndex > _currentPath.Length - 1) Cancel();
                    }
                );
        }

        private void OnGridUpdated(WaypointGrid UpdatedGrid)
        {
            UpdatePath();
        }

        private void RebuildPathIfNecessary()
        {
            if (_currentPath == null)
                UpdatePath();
        }

        protected virtual Vector2[] DoUpdatePath(Vector3 Origin, out bool CanReach)
        {
            var sourceVertex = CurrentGrid.GetNearestVertex(Origin);
            var targetVertex = CurrentGrid.GetNearestVertex(Target);
            return CurrentGrid.GetShortestPath(sourceVertex, targetVertex, out CanReach).Select(W => W.Position.Xz())
                .ToArray();
        }

        private Vector3 CalculateTargetPoint(Vector2 PathPoint)
        {
            return PathPoint.ToVector3() + Parent.Position.Y * Vector3.UnitY;
        }

        protected void UpdatePath()
        {
            _origin = Parent.Position;
            _currentIndex = 0;
            _currentPath = DoUpdatePath(_origin, out var canReach);
            var dmgComponent = Parent.SearchComponent<DamageComponent>();
            if (dmgComponent != null) dmgComponent.AICanReach = canReach;
            if (!canReach) _lastCanNotReachPosition = Target;
            _canReach = canReach;
        }

        public void SetTarget(Vector3 Position, Action Callback = null)
        {
            if ((Parent.Position - Position).LengthSquared() < ErrorMargin * ErrorMargin) return;
            Target = Position;
            _callback = Callback;
            HasTarget = true;
            ForceRebuildGraph();
            RebuildPathIfNecessary();
            var traverse = Parent.SearchComponent<ITraverseAIComponent>();
            /* Sometimes we remove the component and then update */
            if (traverse != null)
                traverse.TargetPoint = Target;
        }

        public void Cancel()
        {
            HasTarget = false;
            _callback?.Invoke();
        }

        public void CancelWalk()
        {
            Walk.Cancel();
        }

        public void ResizeGrid(Vector2 Size)
        {
            TraverseStorage.Instance.ResizeGrid(Parent, Size);
        }

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