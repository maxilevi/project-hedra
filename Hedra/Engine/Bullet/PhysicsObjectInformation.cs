using System.Diagnostics;
using BulletSharp;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.Bullet
{
    public class PhysicsObjectInformation
    {
        private bool _addedOffset;
        private Vector2 _offset;
        public string Name { get; set; }
        public CollisionFilterGroups Group { get; set; }
        public CollisionFilterGroups Mask { get; set; }
        public bool IsLand => !IsEntity;
        public bool IsEntity => Entity != null;
        public IEntity Entity { get; set; }
        public bool IsInSimulation { get; set; }
        public bool IsSensor { get; set; }
        public bool IsDynamic { get; set; }
        public bool IsStatic { get; set; }
        public bool IsPlayer  => Entity is IPlayer;
        public Vector2 StaticOffset
        {
            get => _offset;
            set
            {
                _offset = value;
                _addedOffset = true;
            }
        }
        public bool ValidStaticObject => _addedOffset;
    }
}