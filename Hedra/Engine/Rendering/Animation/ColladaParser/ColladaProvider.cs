using System;
using System.Xml;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    public class ColladaProvider : IColladaProvider
    {
        public AnimatedModelData LoadColladaModel(string ColladaFile, int MaxWeights)
        {
            var document = new XmlDocument();
            document.LoadXml(ColladaFile);
            AssertCorrectName(document);
            var node = document.ChildNodes[1];
    
            var skinLoader = new SkinLoader(node["library_controllers"], MaxWeights);
            var skinningData = skinLoader.ExtractSkinData();
    
            var jointsLoader = new JointsLoader(node["library_visual_scenes"], skinningData.JointOrder);
            var jointsData = jointsLoader.ExtractBoneData();
    
            var geometryLoader = new GeometryLoader(node["library_geometries"], skinningData.VerticesSkinData);
            var modelData = geometryLoader.ExtractModelData();
    
            return new AnimatedModelData(modelData, jointsData);
        }
    
        public  AnimationData LoadColladaAnimation(string ColladaFile)
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