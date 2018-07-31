/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using System.Collections.Generic;
using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Animation
{
	/// <summary>
	/// Description of AnimationLoader.
	/// </summary>
	public static class AnimationLoader
	{
		
		private static readonly Dictionary<string, AnimationData> AnimationCache = new Dictionary<string, AnimationData>();

	    public static void EmptyCache()
	    {
	        lock (AnimationCache)
	            AnimationCache.Clear();
	    }

        /**
		 * Loads up a collada animation file, and returns and animation created from
		 * the extracted animation data from the file.
		 * 
		 * @param colladaFile
		 *            - the collada file containing data about the desired
		 *            animation.
		 * @return The animation made from the data in the file.
		 */
        public static Animation LoadAnimation(string ColladaFile) {
			
			AnimationData AnimationData;

            lock (AnimationCache)
            {
                if (AnimationCache.ContainsKey(ColladaFile))
                {

                    AnimationData = AnimationCache[ColladaFile];
                }
                else
                {
                    string FileContents = Encoding.ASCII.GetString(AssetManager.ReadPath(ColladaFile));
                    AnimationData = ColladaLoader.LoadColladaAnimation(FileContents);
                    AnimationCache.Add(ColladaFile, AnimationData);
                }
            }
			KeyFrame[] Frames = new KeyFrame[AnimationData.KeyFrames.Length];
			
			for (int i = 0; i < Frames.Length; i++) {
				Frames[i] = AnimationLoader.CreateKeyFrame(AnimationData.KeyFrames[i]);
			}
			return new Animation(AnimationData.LengthSeconds, Frames);
		}
	
		/**
		 * Creates a keyframe from the data extracted from the collada file.
		 * 
		 * @param data
		 *            - the data about the keyframe that was extracted from the
		 *            collada file.
		 * @return The keyframe.
		 */
		private static KeyFrame CreateKeyFrame(KeyFrameData Data) {
			Dictionary<string, JointTransform> Dic = new Dictionary<string, JointTransform>();
			
			for(int i = 0; i < Data.JointTransforms.Count; i++){
				JointTransform JointTransform = AnimationLoader.CreateTransform(Data.JointTransforms[i]);
				Dic.Add(Data.JointTransforms[i].JointNameId, JointTransform);
			}
			return new KeyFrame(Data.Time, Dic);
		}
	
		/**
		 * Creates a joint transform from the data extracted from the collada file.
		 * 
		 * @param data
		 *            - the data from the collada file.
		 * @return The joint transform.
		 */
		private static JointTransform CreateTransform(JointTransformData Data) {
			Matrix4 Mat4 = Data.JointLocalTransform;
			Vector3 Translation = new Vector3(Mat4.M41, Mat4.M42, Mat4.M43);
			
			Quaternion Rotation = Extensions.FromMatrixExt(Mat4);
			return new JointTransform(Translation, Rotation);
		}
	}
}
