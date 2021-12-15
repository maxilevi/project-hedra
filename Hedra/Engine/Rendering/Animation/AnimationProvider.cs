/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Animation.ColladaParser;

namespace Hedra.Engine.Rendering.Animation
{
    /// <summary>
    ///     Description of AnimationLoader.
    /// </summary>
    public class AnimationProvider : IAnimationProvider
    {
        private readonly Dictionary<string, AnimationData> _animationCache;

        public AnimationProvider()
        {
            _animationCache = new Dictionary<string, AnimationData>();
        }

        public void EmptyCache()
        {
            lock (_animationCache)
            {
                _animationCache.Clear();
            }
        }

        public Animation LoadAnimation(string ColladaFile)
        {
            AnimationData animationData;

            lock (_animationCache)
            {
                if (_animationCache.ContainsKey(ColladaFile))
                {
                    animationData = _animationCache[ColladaFile];
                }
                else
                {
                    var fileContents = Encoding.ASCII.GetString(AssetManager.ReadPath(ColladaFile));
                    animationData = ColladaLoader.LoadColladaAnimation(fileContents);
                    _animationCache.Add(ColladaFile, animationData);
                }
            }

            var frames = new KeyFrame[animationData.KeyFrames.Length];
            for (var i = 0; i < frames.Length; i++) frames[i] = CreateKeyFrame(animationData.KeyFrames[i]);
            return new Animation(animationData.LengthSeconds, frames);
        }

        private static KeyFrame CreateKeyFrame(KeyFrameData Data)
        {
            var dic = new Dictionary<JointName, JointTransform>();

            for (var i = 0; i < Data.JointTransforms.Count; i++)
            {
                var jointTransform = CreateTransform(Data.JointTransforms[i]);
                dic.Add(Data.JointTransforms[i].JointNameId, jointTransform);
            }

            return new KeyFrame(Data.Time, dic);
        }

        private static JointTransform CreateTransform(JointTransformData Data)
        {
            var mat4 = Data.JointLocalTransform;
            var translation = new Vector3(mat4.M41, mat4.M42, mat4.M43);

            var rotation = Extensions.FromMatrixExt(mat4);
            return new JointTransform(translation, rotation);
        }
    }
}