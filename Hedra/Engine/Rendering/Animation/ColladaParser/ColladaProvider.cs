using System;
using System.Xml;
using Hedra.Engine.Game;
using Hedra.Engine.IO;
using Hedra.Game;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    public class ColladaProvider : IColladaProvider
    {
        private static XmlNode ParsePath(string ColladaFile, bool AssertHasArmature)
        {
            var document = new XmlDocument();
            document.LoadXml(ColladaFile);
            if(AssertHasArmature) AssertCorrectName(document);
            return document.ChildNodes[1];
        }
        
        public AnimatedModelData LoadColladaModel(string ColladaFile)
        {
            var node = ParsePath(ColladaFile, true);
            var jointsLoader = new JointsLoader(node["library_visual_scenes"]);
            var jointsData = jointsLoader.ExtractBoneData();
            if (SkinLoader.HasSkinningData(node))
            {
                var skinningData = LoadSkinning(node);
                return new AnimatedModelData(LoadGeometry(node, skinningData), jointsData);
            }
            return new AnimatedModelData(ModelData.Empty, jointsData);
        }

        private static SkinningData LoadSkinning(XmlNode Node)
        {
            var skinLoader = new SkinLoader(Node, GeneralSettings.MaxWeights);
            return skinLoader.ExtractSkinData();
        }
        
        private static ModelData LoadGeometry(XmlNode Node, SkinningData SkinningData)
        {
            var geometryLoader = new GeometryLoader(Node["library_geometries"], SkinningData);
            return geometryLoader.ExtractModelData();
        }

        public ModelData LoadModel(string ColladaFile)
        {
            var node = ParsePath(ColladaFile, false);
            var skinningData = LoadSkinning(node);
            return LoadGeometry(node, skinningData);
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

        private static void AssertCorrectName(XmlDocument Document)
        {
            if (Document.ChildNodes[1]["library_visual_scenes"]["visual_scene"]
                    .ChildWithAttribute("node", "id", "Armature") == null)
            {
                var currentName = Document.ChildNodes[1]["library_visual_scenes"]["visual_scene"]
                    .ChildWithPattern("node", "id", "^[a-zA-Z0-9_]*Armature[a-zA-Z0-9_]*$");
                throw new ArgumentException($"Collada file armature should be named 'Armature' not {currentName.Value}");
            }
        }
    }
}