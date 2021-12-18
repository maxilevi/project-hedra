using System;
using System.Linq;
using System.Numerics;
using System.Xml;
using Hedra.Game;
using Hedra.Numerics;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    public class ColladaProvider : IColladaProvider
    {
        public AnimatedModelData LoadColladaModel(string ColladaFile, LoadOptions Options)
        {
            var node = ParsePath(ColladaFile, true);
            var skinningData = LoadSkinning(node);
            var jointsLoader = new JointsLoader(node["library_visual_scenes"],
                !Options.LoadAllJoints ? skinningData.JointOrder : null);
            var jointsData = jointsLoader.ExtractBoneData();

            var modelData = LoadGeometry(node, skinningData, Options.FlipNormals);
            /* We dont apply the scale here because its applied in the shader */
            modelData.Transform(Matrix4x4.CreateFromQuaternion(QuaternionMath.FromEuler(jointsLoader.Rotation * Mathf.Radian)));
            modelData.Transform(Matrix4x4.CreateTranslation(jointsLoader.Translation));
            return new AnimatedModelData(modelData, jointsData, jointsLoader.Scale, jointsLoader.Rotation, jointsLoader.Translation);
        }

        public ModelData LoadModel(string ColladaFile)
        {
            var node = ParsePath(ColladaFile, false);
            var skinningData = LoadSkinning(node);
            return LoadGeometry(node, skinningData, false);
        }

        public AnimationData LoadColladaAnimation(string ColladaFile)
        {
            var document = new XmlDocument();
            document.LoadXml(ColladaFile);
            AssertCorrectName(document);
            var node = document.ChildNodes[1];
            XmlNode animNode = node["library_animations"];
            XmlNode jointsNode = node["library_visual_scenes"];
            var loader = new AnimationLoader(animNode, jointsNode);
            var animData = loader.ExtractAnimation();
            return animData;
        }

        private static XmlNode ParsePath(string ColladaFile, bool AssertHasArmature)
        {
            var document = new XmlDocument();
            document.LoadXml(ColladaFile);
            if (AssertHasArmature) AssertCorrectName(document);
            return document.ChildNodes[1];
        }

        private static SkinningData LoadSkinning(XmlNode Node)
        {
            var skinLoader = new SkinLoader(Node, GeneralSettings.MaxWeights);
            return skinLoader.ExtractSkinData();
        }

        private static ModelData LoadGeometry(XmlNode Node, SkinningData SkinningData, bool FlipNormals)
        {
            var geometryLoader = new GeometryLoader(Node["library_geometries"], SkinningData);
            return geometryLoader.ExtractModelData(FlipNormals);
        }

        private static void AssertCorrectName(XmlDocument Document)
        {
            if (Document.ChildNodes[1]["library_visual_scenes"]["visual_scene"]
                .ChildWithAttribute("node", "id", "Armature") == null)
            {
                var currentName = Document.ChildNodes[1]["library_visual_scenes"]["visual_scene"]
                    .ChildWithPattern("node", "id", "^[a-zA-Z0-9_]*Armature[a-zA-Z0-9_]*$");
                throw new ArgumentException(
                    $"Collada file armature should be named 'Armature' not {currentName.Value}");
            }
        }
    }
}