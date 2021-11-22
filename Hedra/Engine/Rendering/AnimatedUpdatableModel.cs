using System;
using System.Collections.Generic;
using System.Numerics;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Player.QuestSystem;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Rendering.Animation.ColladaParser;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering
{
    public abstract class AnimatedUpdatableModel : UpdatableModel<AnimatedModel>
    {
        protected AnimatedUpdatableModel(IEntity Parent) : base(Parent)
        {
        }

        protected abstract string ModelPath { get; set; }
        public bool UsesBodyParts { get; set; }

        public uint DrawPreview()
        {
            return EntityRenderer.Draw(Model, Height);
        }

        public void AddBodyPartModel(ModelData Data, bool IsDefault = false, bool Rebuild = true)
        {
            if (UsesBodyParts)
                Model.AddModel(Data, IsDefault, Rebuild);
        }

        public void Rebuild()
        {
            Model.RebuildBuffers();
        }

        public void RemoveBodyPartModel(ModelData Data, bool Rebuild = true)
        {
            if (UsesBodyParts)
                Model.RemoveModel(Data, Rebuild);
        }

        public void AddModel(ModelData Data, bool IsDefault = false)
        {
            Model.AddModel(Data, IsDefault);
        }

        public void RemoveModel(ModelData Data, bool Rebuild = true)
        {
            Model.RemoveModel(Data, Rebuild);
        }

        public void ClearModel()
        {
            Model.ClearModel();
        }

        public void Paint(params Vector4[] Colors)
        {
            Paint(Model, ModelPath, Colors);
        }

        public static void Paint(AnimatedModel Model, string Path, params Vector4[] Colors)
        {
            if (Colors.Length > AssetManager.ColorCodes.Length)
                throw new ArgumentOutOfRangeException(
                    "Provided amount of colors cannot be higher than the color codes.");

            var colorMap = new Dictionary<Vector3, Vector3>();
            for (var i = 0; i < Colors.Length; i++) colorMap.Add(AssetManager.ColorCodes[i].Xyz(), Colors[i].Xyz());
            AnimationModelLoader.Paint(Model, Path, colorMap);
        }

        public AnimatedModel SwitchModel(AnimatedModel New)
        {
            var previous = Model;
            New.Position = Model.Position;
            New.Alpha = Model.Alpha;
            New.Enabled = Model.Enabled;
            New.Outline = Model.Outline;
            New.OutlineColor = Model.OutlineColor;
            New.Pause = Model.Pause;
            New.Tint = Model.Tint;
            New.BaseTint = Model.BaseTint;
            New.ApplyFog = Model.ApplyFog;
            New.LocalRotation = Model.LocalRotation;
            New.TransformationMatrix = Model.TransformationMatrix;
            New.AnimationSpeed = Model.AnimationSpeed;
            New.PrematureCulling = Model.PrematureCulling;
            Model = New;
            return previous;
        }
    }
}