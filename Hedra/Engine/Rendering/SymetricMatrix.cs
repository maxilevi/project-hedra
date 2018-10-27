/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/05/2017
 * Time: 03:23 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of SymetricMatrix.
    /// </summary>
    public class SymetricMatrix {
        public float[] m = new float[10];
    
        public SymetricMatrix(float c=0){
            for(int i = 0; i < m.Length; i++)
                m[i]= c;
        }
        
        public SymetricMatrix(float m11, float m12, float m13, float m14,
                         float m22, float m23, float m24,
                                float m33, float m34,
                                       float m44){
            m[0] = m11; m[1]=m12; m[2] = m13; m[3]=m14;
                    m[4]=m12; m[5] = m23; m[6]=m24;
                                          m[7] = m23; m[8]=m34;
                                                      m[9]=m44;
        }
    
        public SymetricMatrix(float a,float b,float c,float d)
        {
            m[0] = a*a;  m[1] = a*b;  m[2] = a*c;  m[3] = a*d; 
                         m[4] = b*b;  m[5] = b*c;  m[6] = b*d; 
                                      m[7 ] =c*c; m[8 ] = c*d;
                                                   m[9 ] = d*d;
        }
    
        public float this[int i]
        {
                get { return m[i]; }
                set { m[i] = value; }
        }
    
        // Determinant
    
        public float det(int a11, int a12, int a13,
              int a21, int a22, int a23,
              int a31, int a32, int a33)
        {
            float det =  m[a11]*m[a22]*m[a33] + m[a13]*m[a21]*m[a32] + m[a12]*m[a23]*m[a31] 
                        - m[a13]*m[a22]*m[a31] - m[a11]*m[a23]*m[a32]- m[a12]*m[a21]*m[a33]; 
            return det;
        }
    
        public static SymetricMatrix operator+(SymetricMatrix n0, SymetricMatrix n)
        { 
            return new SymetricMatrix( n0.m[0]+n[0],   n0.m[1]+n[1],   n0.m[2]+n[2],   n0.m[3]+n[3], 
                                                n0.m[4]+n[4],   n0.m[5]+n[5],   n0.m[6]+n[6], 
                                                             n0.m[ 7]+n[ 7], n0.m[ 8]+n[8 ],
                                                                          n0.m[ 9]+n[9 ]);
        }
    }
}
