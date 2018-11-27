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
using System.Linq;
using Hedra.Engine.Rendering;
using System.Collections.Generic;

namespace Hedra.Engine.PhysicsSystem
{
    /// <summary>
    /// Description of GJKCollision.
    /// </summary>
    public static class GJKCollision
    {
        private static Func<Simplex, bool>[] SimplexUpdateFuncs = new Func<Simplex, bool>[ (int) SimplexType.MaxCount ];
        private static Vector3 Direction;
        //private static Simplex Simplex = new Simplex();
        
        static GJKCollision(){
            SimplexUpdateFuncs[(int)SimplexType.Point] = delegate(Simplex Simplex){ return false; };
            SimplexUpdateFuncs[(int)SimplexType.Edge] = delegate(Simplex Simplex){ return SimplexEdgeUpdate(Simplex); };
            SimplexUpdateFuncs[(int)SimplexType.Face] = delegate(Simplex Simplex){ return SimplexFaceUpdate(Simplex); };
            SimplexUpdateFuncs[(int)SimplexType.Tetrahedron] = delegate(Simplex Simplex){ return SimplexTetrahedronUpdate(Simplex); };
        }
       
        private static void SupportPoint(Vector3 Direction, CollisionShape Shape1, CollisionShape Shape2, SimplexVertex Vertex){
            Vertex.SupportA = Shape1.Support(Direction);
            Vertex.SupportB = Shape2.Support(-Direction);
            Vertex.SupportPoint = Vertex.SupportA - Vertex.SupportB;
            
        }

        public static bool IsInsideBroadphase(ICollidable Shape1, ICollidable Shape2)
        {            
            var radii = Shape1.BroadphaseRadius + Shape2.BroadphaseRadius;
            return (Shape1.BroadphaseCenter - Shape2.BroadphaseCenter).LengthSquared < radii * radii;
        }
         
        public static bool Collides(CollisionShape Shape1, CollisionShape Shape2)
        {
            var simplex = Simplex.Cache;
            simplex.Lock();
            
            Direction = Vector3.One;
            //A it's our first supporting point.
            SupportPoint(Direction, Shape1, Shape2, simplex.A);
            simplex.Type = SimplexType.Point;
         
            Direction = -Direction;
         
            for (var i = 0; i < 20; ++i)
            {
                var newSimplexVertex = new SimplexVertex();
                SupportPoint(Direction, Shape1, Shape2, newSimplexVertex);
               
                if (newSimplexVertex.SupportPoint.Dot(Direction) <= 0) { simplex.Unlock(); return false; }
         
                //Add new point to create a new simplex.
                if (simplex.Type == SimplexType.Point) {
                    simplex.B = simplex.A;
                    simplex.A = newSimplexVertex;
                    simplex.Type = SimplexType.Edge;
                }
                
                else if (simplex.Type == SimplexType.Edge) {
                    simplex.C = simplex.B;
                    simplex.B = simplex.A;
                    simplex.A = newSimplexVertex;
                    simplex.Type = SimplexType.Face;
                }
                
                else if (simplex.Type == SimplexType.Face) {
                    simplex.D = simplex.C;
                    simplex.C = simplex.B;
                    simplex.B = simplex.A;
                    simplex.A = newSimplexVertex;
                    simplex.Type = SimplexType.Tetrahedron;
                }
         
                //Check if the simplex contains the origin.
                //Update simplex and direction.
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
            Direction = AB.Cross(AO).Cross(AB);
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
                Direction = AB.Cross(AO).Cross(AB); //Ab to Ao.
                return false;
            }
           
            if ( FaceNormal.Cross(AC).Dot(AO) > 0 ) {
                //A and B makes the edge.
                Simplex.B = Simplex.C;
                Simplex.Type = SimplexType.Edge;
                Direction = AC.Cross(AO).Cross(AC); //Ac to Ao.
                return false;
            }
         
            if ( FaceNormal.Dot(AO) > 0 ) {
                //Above face.
                Direction = FaceNormal;
                return false;
            }
           
            //Below face.
            //Do a swap
            SimplexVertex B0 = Simplex.B;
            Simplex.B = Simplex.C;
            Simplex.C = B0;
            
            Direction = -FaceNormal;
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
                Direction = AB.Cross(AO).Cross(AB);
                return false;
            }
           
            if ( FaceNormal.Cross(AC).Dot(AO) > 0 ) {
                //Test if origin it is in the voronoi region of the AC edge.
                //Change to edge.
                Simplex.B = Simplex.C;
                Simplex.Type = SimplexType.Edge;
                //Get the perpendicular direction of the edge towards the origin.
                   Direction = AC.Cross(AO).Cross(AC);
                return false;
            }
         
            Simplex.Type = SimplexType.Face;
            Direction = FaceNormal;
           
            return false;
        }
    }
}
