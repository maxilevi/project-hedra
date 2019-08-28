/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/11/2016
 * Time: 09:22 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.Rendering.Frustum
{

    public static class FrustumGJK
    {
        private static readonly Func<Simplex, bool>[] SimplexUpdateFuncs;
        private static Vector3 _direction;

        static FrustumGJK()
        {
            SimplexUpdateFuncs = new Func<Simplex, bool>[(int) SimplexType.MaxCount];
            SimplexUpdateFuncs[(int)SimplexType.Point] = Simplex => false;
            SimplexUpdateFuncs[(int)SimplexType.Edge] = SimplexEdgeUpdate;
            SimplexUpdateFuncs[(int)SimplexType.Face] = SimplexFaceUpdate;
            SimplexUpdateFuncs[(int)SimplexType.Tetrahedron] = SimplexTetrahedronUpdate;
        }
        
        private static Vector3 SupportPoint(Vector3[] Points, Vector3 Direction)
        {           
            var highest = float.MinValue;
            var support = Vector3.Zero;

            for (var i = 0; i < Points.Length; ++i)
            {
                var v = Points[i];
                var dot = Direction.X * v.X + Direction.Y * v.Y + Direction.Z * v.Z;

                if (!(dot > highest)) continue;
                highest = dot;
                support = v;
            }
        
            return support;
        }
       
        private static void SupportPoint(Vector3 Direction, Vector3[] Points1, Vector3[] Points2, SimplexVertex Vertex)
        {
            Vertex.SupportA = SupportPoint(Points1, Direction);
            Vertex.SupportB = SupportPoint(Points2, -Direction);
            Vertex.SupportPoint = Vertex.SupportA - Vertex.SupportB;
        }

        public static bool Collides(Vector3[] Points1, Vector3[] Points2)
        {
            var simplex = Simplex.Cache;
            simplex.Lock();
            
            _direction = Vector3.One;
            SupportPoint(_direction, Points1, Points2, simplex.A);
            simplex.Type = SimplexType.Point;
            _direction = -_direction;
         
            for (var i = 0; i < 20; ++i)
            {
                var newSimplexVertex = new SimplexVertex();
                SupportPoint(_direction, Points1, Points2, newSimplexVertex);

                if (newSimplexVertex.SupportPoint.Dot(_direction) <= 0)
                {
                    simplex.Unlock();
                    return false;
                }
                if (simplex.Type == SimplexType.Point)
                {
                    simplex.B = simplex.A;
                    simplex.A = newSimplexVertex;
                    simplex.Type = SimplexType.Edge;
                }
                
                else if (simplex.Type == SimplexType.Edge)
                {
                    simplex.C = simplex.B;
                    simplex.B = simplex.A;
                    simplex.A = newSimplexVertex;
                    simplex.Type = SimplexType.Face;
                }
                
                else if (simplex.Type == SimplexType.Face)
                {
                    simplex.D = simplex.C;
                    simplex.C = simplex.B;
                    simplex.B = simplex.A;
                    simplex.A = newSimplexVertex;
                    simplex.Type = SimplexType.Tetrahedron;
                }
                if (UpdateSimplex(simplex))
                {
                    simplex.Unlock();
                    return true;
                }
            }
            simplex.Unlock();
            return false;
        }
         
        private static bool UpdateSimplex(Simplex Simplex)
        {
            return SimplexUpdateFuncs[(int)Simplex.Type](Simplex);
        }
         
        private static bool SimplexEdgeUpdate(Simplex Simplex)
        {
            var AO = -Simplex.A.SupportPoint;
            var AB = Simplex.B.SupportPoint - Simplex.A.SupportPoint;
            _direction = AB.Cross(AO).Cross(AB);
            return false;
        }
         
        private static bool SimplexFaceUpdate(Simplex Simplex)
        {
            var AO = -Simplex.A.SupportPoint;
            var AB = Simplex.B.SupportPoint - Simplex.A.SupportPoint;
            var AC = Simplex.C.SupportPoint - Simplex.A.SupportPoint;
            var FaceNormal = AB.Cross(AC);
         
            if ( AB.Cross(FaceNormal).Dot(AO) > 0 ) {
                Simplex.Type = SimplexType.Edge;
                //A and B makes the edge.
                _direction = AB.Cross(AO).Cross(AB); //Ab to Ao.
                return false;
            }
           
            if ( FaceNormal.Cross(AC).Dot(AO) > 0 ) {
                //A and B makes the edge.
                Simplex.B = Simplex.C;
                Simplex.Type = SimplexType.Edge;
                _direction = AC.Cross(AO).Cross(AC); //Ac to Ao.
                return false;
            }
         
            if ( FaceNormal.Dot(AO) > 0 ) {
                //Above face.
                _direction = FaceNormal;
                return false;
            }
           
            //Below face.
            //Do a swap
            SimplexVertex B0 = Simplex.B;
            Simplex.B = Simplex.C;
            Simplex.C = B0;
            
            _direction = -FaceNormal;
            return false;
        }
         
        private static bool SimplexTetrahedronUpdate(Simplex Simplex) {
            Vector3 AO = -Simplex.A.SupportPoint;
         
            Vector3 AB = Simplex.B.SupportPoint - Simplex.A.SupportPoint;
            Vector3 AC = Simplex.C.SupportPoint - Simplex.A.SupportPoint;
            if ( AB.Cross(AC).Dot(AO) > 0 ) {
                return UpdateSimplexTetrahedronFace(AO, Simplex);
            }
         
            Vector3 AD = Simplex.D.SupportPoint - Simplex.A.SupportPoint;
            if ( AC.Cross(AD).Dot(AO) > 0 ) {
                Simplex.B = Simplex.C;
                Simplex.C = Simplex.D;
                return UpdateSimplexTetrahedronFace(AO, Simplex);
            }
           
            if ( AD.Cross(AB).Dot(AO) > 0 ) {
                SimplexVertex OldB = Simplex.B;
                Simplex.B = Simplex.D;
                Simplex.C = OldB;
                return UpdateSimplexTetrahedronFace(AO, Simplex);
            }
         
            return true;
        }
         
        private static bool UpdateSimplexTetrahedronFace(Vector3 AO, Simplex Simplex)
        {
            Vector3 AB = Simplex.B.SupportPoint - Simplex.A.SupportPoint;
            Vector3 AC = Simplex.C.SupportPoint - Simplex.A.SupportPoint;
            Vector3 FaceNormal = AB.Cross(AC);
         
            if (AB.Cross(FaceNormal).Dot(AO) > 0) {
                //Test if origin it is in the voronoi region of the AB edge.
                Simplex.Type = SimplexType.Edge;
                //Get the perpendicular direction of the edge towards the origin.
                _direction = AB.Cross(AO).Cross(AB);
                return false;
            }
           
            if ( FaceNormal.Cross(AC).Dot(AO) > 0 ) {
                //Test if origin it is in the voronoi region of the AC edge.
                //Change to edge.
                Simplex.B = Simplex.C;
                Simplex.Type = SimplexType.Edge;
                //Get the perpendicular direction of the edge towards the origin.
                   _direction = AC.Cross(AO).Cross(AC);
                return false;
            }
         
            Simplex.Type = SimplexType.Face;
            _direction = FaceNormal;
           
            return false;
        }
    }
}