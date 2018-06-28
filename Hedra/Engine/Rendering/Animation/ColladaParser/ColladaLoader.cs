/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
	/// <summary>
	/// Description of ColladaLoader.
	/// </summary>
	internal static class ColladaLoader
	{
		public static AnimatedModelData LoadColladaModel(string ColladaFile, int MaxWeights) {
			XmlDocument Document = new XmlDocument();
			Document.LoadXml(ColladaFile);
            ColladaLoader.AssertCorrectName(Document);
			XmlNode Node = Document.ChildNodes[1];
	
			SkinLoader SkinLoader = new SkinLoader(Node["library_controllers"], MaxWeights);
			SkinningData SkinningData = SkinLoader.ExtractSkinData();
	
			JointsLoader JointsLoader = new JointsLoader(Node["library_visual_scenes"], SkinningData.JointOrder);
            JointsData JointsData = JointsLoader.ExtractBoneData();
	
			GeometryLoader GeometryLoader = new GeometryLoader(Node["library_geometries"], SkinningData.VerticesSkinData);
			ModelData ModelData = GeometryLoader.ExtractModelData();
	
			return new AnimatedModelData(ModelData, JointsData);
		}
	
		public static AnimationData LoadColladaAnimation(string ColladaFile) {
			XmlDocument Document = new XmlDocument();
			Document.LoadXml(ColladaFile);
            ColladaLoader.AssertCorrectName(Document);
			XmlNode Node = Document.ChildNodes[1];			
			XmlNode AnimNode = Node["library_animations"];
			XmlNode jointsNode = Node["library_visual_scenes"];
			AnimationLoader loader = new AnimationLoader(AnimNode, jointsNode);
			AnimationData animData = loader.ExtractAnimation();
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
