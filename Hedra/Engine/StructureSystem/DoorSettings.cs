using System.Numerics;

namespace Hedra.Engine.StructureSystem
{
    public class DoorSettings
    {
        private readonly Vector3 _position;

        public Vector3 Position => _position * Scale;
        public Vector3 Scale { get; }

        public bool InvertedPivot { get; }

        public bool InvertedRotation { get; }

        public DoorSettings(Vector3 Position, Vector3 Scale, bool InvertedRotation, bool InvertedPivot)
        {
            _position = Position;
            this.Scale = Scale;
            this.InvertedPivot = InvertedPivot;
            this.InvertedRotation = InvertedRotation;
        }
    }
}