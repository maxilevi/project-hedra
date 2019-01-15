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
using System.Linq;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Animation.ColladaParser;

namespace Hedra.Engine.Rendering.Animation
{
    /// <summary>
    /// Description of AnimationModelLoader.
    /// </summary>
    public static class AnimationModelLoader
    {
        private static readonly Dictionary<string, AnimatedModelData> ModelCache = new Dictionary<string, AnimatedModelData>();


        /**
         * Creates an AnimatedEntity from the data in an entity file. It loads up
         * the collada model data, stores the extracted data in a VAO, sets up the
         * joint heirarchy, and loads up the entity's texture.
         * 
         * @param entityFile
         *            - the file containing the data for the entity.
         * @return The animated entity (no animation applied though)
         */
        public static AnimatedModel LoadEntity(AnimatedModelData EntityData)
        {         
            JointsData SkeletonData = EntityData.Joints;
            Joint HeadJoint = AnimationModelLoader.CreateJoints(SkeletonData.HeadJoint);
            return new AnimatedModel(EntityData.Mesh, HeadJoint, SkeletonData.JointCount);
        }

        public static AnimatedModel LoadEntity(string ModelFile)
        {
            var animatedModel = AnimationModelLoader.LoadEntity(AnimationModelLoader.GetEntityData(ModelFile));
            animatedModel.CullingBox = AssetManager.LoadHitbox(ModelFile);
            return animatedModel;
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

        private static AnimatedModelData GetEntityData(string ModelFile)
        {
            AnimatedModelData entityData;
            lock (ModelCache)
            {
                if (ModelCache.ContainsKey(ModelFile))
                {
                    entityData = ModelCache[ModelFile];
                }
                else
                {
                    string fileContents = Encoding.ASCII.GetString(AssetManager.ReadPath(ModelFile));
                    entityData = ColladaLoader.LoadColladaModel(fileContents);
                    ModelCache.Add(ModelFile, entityData);
                }
            }
            return entityData;
        }

        /// <summary>
        /// Returns a new AnimatedModel with the replaced colors.
        /// </summary>
        /// <param name="Model">Original AnimatedModel</param>
        /// <param name="Path">File path of the original model</param>
        /// <param name="ColorMap">A Dictionary composed of ColorToReplace->ColorToReplaceWith </param>
        /// <returns>A new AnimatedModel with the colors replaced.</returns>
        public static void Paint(AnimatedModel Model, string Path, Dictionary<Vector3, Vector3> ColorMap)
        {
            Vector3[] colorData = AnimationModelLoader.GetEntityData(Path).Mesh.Colors.ToArray();
            for (var i = 0; i < colorData.Length; i++)
            {
                if (ColorMap.ContainsKey(colorData[i]))
                {
                    colorData[i] = ColorMap[colorData[i]];
                }
            }
            Model.ReplaceColors(colorData);
        }

        /// <summary>
        /// Empties the model cache
        /// </summary>
        public static void EmptyCache()
        {
            lock (ModelCache)
                ModelCache.Clear();
        }
    }
}
