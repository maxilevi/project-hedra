/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;

namespace Hedra.Engine.Rendering.Animation
{
	/// <summary>
	/// Description of JointTransform.
	/// </summary>
	public class JointTransform
	{
		public Vector3 Position {get; private set;}
		public Quaternion Rotation {get; private set;}
	
		public JointTransform(Vector3 Position, Quaternion Rotation) {
			this.Position = Position;
			this.Rotation = Rotation;
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
		public Matrix4 LocalTransform{
			get{
				Matrix4 Matrix = Matrix4.CreateTranslation(Position);
				Matrix = Rotation.ToMatrix() * Matrix;
				return Matrix;
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
		public static JointTransform Interpolate(JointTransform FrameA, JointTransform FrameB, float Progression) {
			Vector3 pos = JointTransform.Interpolate(FrameA.Position, FrameB.Position, Progression);
			Quaternion rot = Extensions.SlerpExt(FrameA.Rotation, FrameB.Rotation, Progression);
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
		private static Vector3 Interpolate(Vector3 Start, Vector3 End, float Progression) {
			float x = Start.X + (End.X - Start.X) * Progression;
			float y = Start.Y + (End.Y - Start.Y) * Progression;
			float z = Start.Z + (End.Z - Start.Z) * Progression;
			return new Vector3(x, y, z);
		}
	}
}
