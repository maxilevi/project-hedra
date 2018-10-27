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

namespace Hedra.Engine.Rendering.Animation.ColladaParser
{
    /// <summary>
    /// Description of VertexSkinData.
    /// </summary>
    public class VertexSkinData
    {
        public readonly List<int> JointIds = new List<int>();
        public readonly List<float> Weights = new List<float>();
        
        public void AddJointEffect(int JointId, float Weight){
            for(int i=0; i<Weights.Count; i++){
                
                if(Weight > Weights[i]){
                    JointIds.Insert(i, JointId);
                    Weights.Insert(i, Weight);
                    return;
                }
            }
            JointIds.Add(JointId);
            Weights.Add(Weight);
        }
        
        public void LimitJointNumber(int max){
            
            if(JointIds.Count > max){
                
                float[] topWeights = new float[max];
                float total = SaveTopWeights(topWeights);
                this.RefillWeightList(topWeights, total);
                this.RemoveExcessJointIds(max);
                
            }else if(JointIds.Count < max){
                this.FillEmptyWeights(max);
            }
            
        }
    
        private void FillEmptyWeights(int max){
            while(JointIds.Count < max){
                JointIds.Add(0);
                Weights.Add(0f);
            }
        }
        
        private float SaveTopWeights(float[] TopWeightsArray){
            float Total = 0;
            for(int i=0; i<TopWeightsArray.Length; i++){
                TopWeightsArray[i] = Weights[i];
                Total += TopWeightsArray[i];
            }
            return Total;
        }
        
        private void RefillWeightList(float[] topWeights, float total){
            Weights.Clear();
            for(int i=0; i<topWeights.Length; i++){
                Weights.Add(Math.Min(topWeights[i]/total, 1));
            }
        }
        
        private void RemoveExcessJointIds(int max){
            while(JointIds.Count > max){
                JointIds.RemoveAt(JointIds.Count-1);
            }
        }
    }
}
