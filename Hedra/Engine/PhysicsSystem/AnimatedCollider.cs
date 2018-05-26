using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Rendering.Animation;

namespace Hedra.Engine.PhysicsSystem
{
    public class AnimatedCollider : IDisposable
    {
        public AnimatedModel Model { get; }
        private readonly BoneData[] _bonesData;
        private BoneBox[] _defaultBoneBoxes;

        public AnimatedCollider(AnimatedModel Model)
        {
            this.Model = Model;
            this._bonesData = BoneData.FromArrays(Model.JointIdsArray, Model.VerticesArray);
            this.Build();
        }

        private void Build()
        {
            var boxes = new List<BoneBox>();
            for (var i = 0; i < _bonesData.Length; i++)
            {
                boxes.Add(BoneBox.From(_bonesData[i]));
            }
            _defaultBoneBoxes = boxes.ToArray();
        }

        public BoneBox[] Colliders
        {
            get
            {
                var boneBoxes = _defaultBoneBoxes.Select(B => B.Clone()).ToArray();
                var transforms = Model.JointTransforms;
                for (var i = 0; i < boneBoxes.Length; i++)
                {
                    boneBoxes[i].Transform(transforms[boneBoxes[i].JointId]);
                }
                return boneBoxes;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
