/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:24 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using OpenTK;
using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.Animation
{
	/// <summary>
	/// Description of AnimationModelLoader.
	/// </summary>
	public static class AnimationModelLoader
	{
		private static readonly Dictionary<string, AnimatedModelData> ModelCache = new Dictionary<string, AnimatedModelData>();

	    public static void EmptyCache()
	    {
	        lock (ModelCache)
                ModelCache.Clear();
	    }

        /**
		 * Creates an AnimatedEntity from the data in an entity file. It loads up
		 * the collada model data, stores the extracted data in a VAO, sets up the
		 * joint heirarchy, and loads up the entity's texture.
		 * 
		 * @param entityFile
		 *            - the file containing the data for the entity.
		 * @return The animated entity (no animation applied though)
		 */
        public static AnimatedModel LoadEntity(string ModelFile) {
            AnimatedModelData EntityData;

            lock (ModelCache) {
                if (ModelCache.ContainsKey(ModelFile)) {
                    EntityData = ModelCache[ModelFile];
                } else {
                    string FileContents = Encoding.ASCII.GetString(AssetManager.ReadPath(ModelFile));
                    EntityData = ColladaLoader.LoadColladaModel(FileContents, GeneralSettings.MaxWeights);
                    ModelCache.Add(ModelFile, EntityData);
                }
            }
			JointsData SkeletonData = EntityData.Joints;
			Joint HeadJoint = AnimationModelLoader.CreateJoints(SkeletonData.HeadJoint);
			return new AnimatedModel(EntityData.Mesh, HeadJoint, SkeletonData.JointCount);
		}
	
		/**
		 * Constructs the joint-hierarchy skeleton from the data extracted from the
		 * collada file.
		 * 
		 * @param data
		 *            - the joints data from the collada file for the head joint.
		 * @return The created joint, with all its descendants added.
		 */
		private static Joint CreateJoints(JointData Data) {
			Joint Joint = new Joint(Data.Index, Data.NameId, Data.BindLocalTransform);
			for (int i = 0; i < Data.Children.Count; i++){
				Joint.AddChild( AnimationModelLoader.CreateJoints(Data.Children[i]) );
			}
			return Joint;
		}
	}
}
