using System;
using System.Collections.Generic;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public abstract class AnimatedUpdatableModel : UpdatableModel<AnimatedModel>
    {
        protected abstract string ModelPath { get; set; }
        
        protected AnimatedUpdatableModel(IEntity Parent) : base(Parent)
        {
        }
        
        public void Paint(Vector4[] Colors)
        {
            if(Colors.Length > AssetManager.ColorCodes.Length)
                throw new ArgumentOutOfRangeException($"Provided amount of colors cannot be higher than the color codes.");

            var colorMap = new Dictionary<Vector3, Vector3>();
            for (var i = 0; i < Colors.Length; i++)
            {
                colorMap.Add(AssetManager.ColorCodes[i].Xyz, Colors[i].Xyz);
            }
            AnimationModelLoader.Paint(Model, ModelPath, colorMap);
        }
    }
}