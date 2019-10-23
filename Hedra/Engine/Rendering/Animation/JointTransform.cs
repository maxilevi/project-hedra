/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Numerics;

namespace Hedra.Engine.Rendering.Animation
{
    /// <summary>
    /// Description of JointTransform.
    /// </summary>
    public struct JointTransform : IEquatable<JointTransform>
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        
        private Matrix4x4 _lastLocalTransform;
        private Quaternion _lastRotation;
        private Vector3 _lastPosition;
    
        public JointTransform(Vector3 Position, Quaternion Rotation)
        {
            this.Position = Position;
            this.Rotation = Rotation;
            _lastLocalTransform = Matrix4x4.Identity;
            _lastRotation = Quaternion.Identity;
            _lastPosition = Vector3.Zero;
        }
    
        /**
         * In this method the bone-space transform matrix is constructed by
         * translating an identity matrix using the position variable and then
         * applying the rotation. The rotation is applied by first converting the
         * quaternion into a rotation matrix, which is then multiplied with the
         * transform matrix.
         * 
         * @return This bone-space joint transform as a matrix. The exact same
         *         transform as represented by the position and rotation in this
         *         instance, just in matrix form.
         */
        public Matrix4x4 LocalTransform
        {
            get
            {
                if (_lastPosition == Position && _lastRotation == Rotation) return _lastLocalTransform;
                _lastPosition = Position;
                _lastRotation = Rotation;
                var matrix = Matrix4x4.CreateTranslation(Position);
                matrix = Rotation.ToMatrix() * matrix;
                return _lastLocalTransform = matrix;
            }
        }
    
        /**
         * Interpolates between two transforms based on the progression value. The
         * result is a new transform which is part way between the two original
         * transforms. The translation can simply be linearly interpolated, but the
         * rotation interpolation is slightly more complex, using a method called
         * "SLERP" to spherically-linearly interpolate between 2 quaternions
         * (rotations). This gives a much much better result than trying to linearly
         * interpolate between Euler rotations.
         * 
         * @param frameA
         *            - the previous transform
         * @param frameB
         *            - the next transform
         * @param progression
         *            - a number between 0 and 1 indicating how far between the two
         *            transforms to interpolate. A progression value of 0 would
         *            return a transform equal to "frameA", a value of 1 would
         *            return a transform equal to "frameB". Everything else gives a
         *            transform somewhere in-between the two.
         * @return
         */
        public static JointTransform Interpolate(JointTransform FrameA, JointTransform FrameB, float Progression)
        {
            var pos = Interpolate(FrameA.Position, FrameB.Position, Progression);
            var rot = Extensions.SlerpExt(FrameA.Rotation, FrameB.Rotation, Progression);
            return new JointTransform(pos, rot);
        }
        
        /**
         * Linearly interpolates between two translations based on a "progression"
         * value.
         * 
         * @param start
         *            - the start translation.
         * @param end
         *            - the end translation.
         * @param progression
         *            - a value between 0 and 1 indicating how far to interpolate
         *            between the two translations.
         * @return
         */
        private static Vector3 Interpolate(Vector3 Start, Vector3 End, float Progression)
        {
            var x = Start.X + (End.X - Start.X) * Progression;
            var y = Start.Y + (End.Y - Start.Y) * Progression;
            var z = Start.Z + (End.Z - Start.Z) * Progression;
            return new Vector3(x, y, z);
        }

        public bool Equals(JointTransform Other)
        {
            return Position.Equals(Other.Position) && Rotation.Equals(Other.Rotation);
        }

        public override bool Equals(object Obj)
        {
            if (ReferenceEquals(null, Obj)) return false;
            if (Obj.GetType() != this.GetType()) return false;
            return Equals((JointTransform) Obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ Rotation.GetHashCode();
            }
        }

        public JointTransform Clone()
        {
            return new JointTransform(Position, Rotation);
        }
        
        public static JointTransform Default { get; } = new JointTransform(Vector3.Zero, Quaternion.Identity);
    }
}
