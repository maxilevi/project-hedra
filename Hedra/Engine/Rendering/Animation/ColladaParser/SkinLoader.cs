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
using System.Xml;
using System.Globalization;
using System.Linq;

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of SkinLoader.
    /// </summary>
    public class SkinLoader
    {
        private readonly XmlNode SkinningData;
        private readonly int MaxWeights;
    
        public SkinLoader(XmlNode ControllersNode, int MaxWeights)
        {
            SkinningData = ControllersNode?["controller"]?["skin"];
            if (SkinningData == null)
                throw new ArgumentOutOfRangeException("Provided model has no skinning data.");
            this.MaxWeights = MaxWeights;
        }
    
        public SkinningData ExtractSkinData() {
            List<string> jointsList = LoadJointsList();
            float[] Weights = LoadWeights();
            XmlNode WeightsDataNode = SkinningData["vertex_weights"];
            int[] EffectorJointCounts = GetEffectiveJointsCounts(WeightsDataNode);
            List<VertexSkinData> VertexWeights = GetSkinData(WeightsDataNode, EffectorJointCounts, Weights);
            return new SkinningData(jointsList, VertexWeights);
        }
    
        private List<string> LoadJointsList() {
            XmlNode inputNode = SkinningData["vertex_weights"];
            string jointDataId = inputNode.ChildWithAttribute("input", "semantic", "JOINT").GetAttribute("source").Value.Substring(1);
            XmlNode jointsNode = SkinningData.ChildWithAttribute("source", "id", jointDataId)["Name_array"];
            string[] Names = jointsNode.InnerText.Split(' ');
            List<String> jointsList = new List<String>();
            for (int i = 0; i < Names.Length; i++) {
                jointsList.Add(Names[i]);
            }
            return jointsList;
        }
    
        private float[] LoadWeights() {
            XmlNode inputNode = SkinningData["vertex_weights"];
            String weightsDataId = inputNode.ChildWithAttribute("input", "semantic", "WEIGHT").GetAttribute("source").Value.Substring(1);
            XmlNode weightsNode = SkinningData.ChildWithAttribute("source", "id", weightsDataId)["float_array"];
            string[] rawData = weightsNode.InnerText.Split(' ');
            float[] weights = new float[rawData.Length];
            for (int i = 0; i < weights.Length; i++) {
                weights[i] = float.Parse(rawData[i] ,NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            return weights;
        }
    
        private int[] GetEffectiveJointsCounts(XmlNode weightsDataNode) {
            String[] rawData = weightsDataNode["vcount"].InnerText.Split(' ');
            int[] counts = new int[rawData.Length-1];
            for (int i = 0; i < rawData.Length-1; i++) {
                counts[i] = int.Parse(rawData[i]);
            }
            return counts;
        }
    
        private List<VertexSkinData> GetSkinData(XmlNode weightsDataNode, int[] counts, float[] weights) {
            String[] rawData = weightsDataNode["v"].InnerText.Split(' ');
            List<VertexSkinData> SkinningData = new List<VertexSkinData>();
            int pointer = 0;
            for (int k = 0; k < counts.Length; k++) {
                VertexSkinData skinData = new VertexSkinData();
                for (int i = 0; i < counts[k]; i++) {
                    int jointId = int.Parse( rawData[pointer++] );
                    int weightId = int.Parse( rawData[pointer++] );
                    skinData.AddJointEffect(jointId, weights[weightId]);
                }
                skinData.LimitJointNumber(MaxWeights);
                SkinningData.Add(skinData);
            }
            return SkinningData;
        }
    }
}
