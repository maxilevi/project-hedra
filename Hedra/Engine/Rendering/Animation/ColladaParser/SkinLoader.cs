/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/03/2017
 * Time: 07:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    ///     Description of SkinLoader.
    /// </summary>
    public class SkinLoader
    {
        private readonly int MaxWeights;
        private readonly XmlNode SkinningData;

        public SkinLoader(XmlNode Node, int MaxWeights)
        {
            SkinningData = Node["library_controllers"]?["controller"]?["skin"];
            if (SkinningData == null)
                throw new ArgumentOutOfRangeException("Provided model has no skinning data.");
            this.MaxWeights = MaxWeights;
        }

        public static bool HasSkinningData(XmlNode Node)
        {
            return Node["library_controllers"]?["controller"]?["skin"] != null;
        }

        public SkinningData ExtractSkinData()
        {
            var jointsList = LoadJointsList();
            var Weights = LoadWeights();
            XmlNode WeightsDataNode = SkinningData["vertex_weights"];
            var EffectorJointCounts = GetEffectiveJointsCounts(WeightsDataNode);
            var VertexWeights = GetSkinData(WeightsDataNode, EffectorJointCounts, Weights);
            return new SkinningData(jointsList, VertexWeights);
        }

        private List<string> LoadJointsList()
        {
            XmlNode inputNode = SkinningData["vertex_weights"];
            var jointDataId = inputNode.ChildWithAttribute("input", "semantic", "JOINT").GetAttribute("source").Value
                .Substring(1);
            XmlNode jointsNode = SkinningData.ChildWithAttribute("source", "id", jointDataId)["Name_array"];
            var Names = jointsNode.InnerText.Split(' ');
            var jointsList = new List<string>();
            for (var i = 0; i < Names.Length; i++) jointsList.Add(Names[i]);
            return jointsList;
        }

        private float[] LoadWeights()
        {
            XmlNode inputNode = SkinningData["vertex_weights"];
            var weightsDataId = inputNode.ChildWithAttribute("input", "semantic", "WEIGHT").GetAttribute("source").Value
                .Substring(1);
            XmlNode weightsNode = SkinningData.ChildWithAttribute("source", "id", weightsDataId)["float_array"];
            var rawData = weightsNode.InnerText.Split(' ');
            var weights = new float[rawData.Length];
            for (var i = 0; i < weights.Length; i++)
                weights[i] = float.Parse(rawData[i], NumberStyles.Any, CultureInfo.InvariantCulture);
            return weights;
        }

        private int[] GetEffectiveJointsCounts(XmlNode weightsDataNode)
        {
            var rawData = weightsDataNode["vcount"].InnerText.Split(' ');
            var counts = new int[rawData.Length - 1];
            for (var i = 0; i < rawData.Length - 1; i++) counts[i] = int.Parse(rawData[i]);
            return counts;
        }

        private List<VertexSkinData> GetSkinData(XmlNode weightsDataNode, int[] counts, float[] weights)
        {
            var rawData = weightsDataNode["v"].InnerText.Split(' ');
            var SkinningData = new List<VertexSkinData>();
            var pointer = 0;
            for (var k = 0; k < counts.Length; k++)
            {
                var skinData = new VertexSkinData();
                for (var i = 0; i < counts[k]; i++)
                {
                    var jointId = int.Parse(rawData[pointer++]);
                    var weightId = int.Parse(rawData[pointer++]);
                    skinData.AddJointEffect(jointId, weights[weightId]);
                }

                skinData.LimitJointNumber(MaxWeights);
                SkinningData.Add(skinData);
            }

            return SkinningData;
        }
    }
}