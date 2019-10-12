using System.IO;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Geometry;
using Hedra.Game;
using NUnit.Framework;
using OpenToolkit.Mathematics;

namespace HedraTests.Rendering
{
    [TestFixture]
    public class MeshAnalyzerTest
    {
        [Test]
        public void TestConnectedComponents()
        {
            var path = $"{AssetManager.AppPath}/test.ply";
            File.WriteAllText(path, PLYWithMultipleObjects);
            var assets = new CompressedAssetProvider();
            var model = assets.PLYLoader(path, Vector3.One, Vector3.Zero, Vector3.Zero);
            /* There are four cubes in the scene */
            Assert.AreEqual(4, MeshAnalyzer.GetConnectedComponents(model).Length);
        }

        private const string PLYWithMultipleObjects = @"ply
format ascii 1.0
comment Created by Blender 2.77 (sub 0) - www.blender.org, source file: ''
element vertex 108
property float x
property float y
property float z
property float nx
property float ny
property float nz
property uchar red
property uchar green
property uchar blue
element face 48
property list uchar uint vertex_indices
end_header
0.276231 -1.362038 -0.438347 0.616342 -0.490722 0.615886 255 79 3
-0.443217 0.380594 1.670118 0.616342 -0.490722 0.615886 255 79 3
-1.137459 -1.362038 0.976390 0.616342 -0.490722 0.615886 255 79 3
-1.675900 1.362038 0.438347 -0.616342 0.490722 -0.615886 255 79 3
-0.956451 -0.380594 -1.670119 -0.616342 0.490722 -0.615886 255 79 3
-2.370142 -0.380595 -0.255382 -0.616342 0.490722 -0.615886 255 79 3
-2.370142 -0.380595 -0.255382 -0.347121 -0.871316 -0.346864 255 79 3
0.276231 -1.362038 -0.438347 -0.347121 -0.871316 -0.346864 255 79 3
-1.137459 -1.362038 0.976390 -0.347121 -0.871316 -0.346864 255 79 3
-0.956451 -0.380594 -1.670119 0.706845 -0.000000 -0.707368 255 79 3
0.970474 0.380595 0.255382 0.706845 -0.000000 -0.707368 255 79 3
0.276231 -1.362038 -0.438347 0.706845 -0.000000 -0.707368 255 79 3
-0.262210 1.362038 -0.976389 0.347121 0.871316 0.346865 255 79 3
-0.443217 0.380594 1.670118 0.347121 0.871316 0.346865 255 79 3
0.970474 0.380595 0.255382 0.347121 0.871316 0.346865 255 79 3
-1.137459 -1.362038 0.976390 -0.706845 -0.000000 0.707368 255 79 3
-1.675900 1.362038 0.438347 -0.706845 -0.000000 0.707368 255 79 3
-2.370142 -0.380595 -0.255382 -0.706845 -0.000000 0.707368 255 79 3
2.115233 0.007012 1.430989 0.784600 0.602279 0.147182 255 79 3
0.437811 2.264863 1.133730 0.784600 0.602279 0.147182 255 79 3
1.020848 1.129108 2.673253 0.784600 0.602279 0.147182 255 79 3
-0.548352 -0.075450 2.378888 -0.784600 -0.602279 -0.147182 255 79 3
-0.037004 -0.061792 -0.402898 -0.784600 -0.602279 -0.147182 255 79 3
0.546033 -1.197547 1.136624 -0.784600 -0.602279 -0.147182 255 79 3
0.546033 -1.197547 1.136624 0.547193 -0.561048 -0.621132 255 79 3
1.532196 1.142766 -0.108534 0.547193 -0.561048 -0.621132 255 79 3
2.115233 0.007012 1.430989 0.547193 -0.561048 -0.621132 255 79 3
-0.037004 -0.061792 -0.402898 -0.291518 0.567877 -0.769761 255 79 3
0.437811 2.264863 1.133730 -0.291518 0.567877 -0.769761 255 79 3
1.532196 1.142766 -0.108534 -0.291518 0.567877 -0.769761 255 79 3
0.437811 2.264863 1.133730 -0.547193 0.561048 0.621132 255 79 3
-0.548352 -0.075450 2.378888 -0.547193 0.561048 0.621132 255 79 3
1.020848 1.129108 2.673253 -0.547193 0.561048 0.621132 255 79 3
2.115233 0.007012 1.430989 0.291518 -0.567877 0.769761 255 79 3
-0.548352 -0.075450 2.378888 0.291518 -0.567877 0.769761 255 79 3
0.546033 -1.197547 1.136624 0.291518 -0.567877 0.769761 255 79 3
2.336710 -0.882828 -1.243710 0.758559 -0.634765 0.147182 255 79 3
2.212124 -0.386701 1.538076 0.758559 -0.634765 0.147182 255 79 3
1.355016 -1.699007 0.295812 0.758559 -0.634765 0.147182 255 79 3
0.695006 0.882828 1.243711 -0.758559 0.634765 -0.147182 255 79 3
0.819593 0.386702 -1.538075 -0.758559 0.634765 -0.147182 255 79 3
-0.162101 -0.429478 0.001447 -0.758559 0.634765 -0.147182 255 79 3
-0.162101 -0.429478 0.001447 -0.428553 -0.656153 -0.621132 255 79 3
2.336710 -0.882828 -1.243710 -0.428553 -0.656153 -0.621132 255 79 3
1.355016 -1.699007 0.295812 -0.428553 -0.656153 -0.621132 255 79 3
2.336710 -0.882828 -1.243710 0.490847 0.408090 -0.769761 255 79 3
1.676700 1.699007 -0.295811 0.490847 0.408090 -0.769761 255 79 3
3.193817 0.429479 -0.001446 0.490847 0.408090 -0.769761 255 79 3
1.676700 1.699007 -0.295811 0.428553 0.656153 0.621132 255 79 3
2.212124 -0.386701 1.538076 0.428553 0.656153 0.621132 255 79 3
3.193817 0.429479 -0.001446 0.428553 0.656153 0.621132 255 79 3
1.355016 -1.699007 0.295812 -0.490847 -0.408089 0.769761 255 79 3
0.695006 0.882828 1.243711 -0.490847 -0.408089 0.769761 255 79 3
-0.162101 -0.429478 0.001447 -0.490847 -0.408089 0.769761 255 79 3
-1.000000 -1.000000 1.000000 0.000000 -1.000000 -0.000000 255 79 3
1.000000 -1.000000 -0.999999 0.000000 -1.000000 -0.000000 255 79 3
1.000000 -1.000000 1.000000 0.000000 -1.000000 -0.000000 255 79 3
-1.000001 1.000000 0.999999 -0.000000 1.000000 0.000000 255 79 3
1.000000 1.000000 -0.999999 -0.000000 1.000000 0.000000 255 79 3
-1.000000 1.000000 -1.000001 -0.000000 1.000000 0.000000 255 79 3
-1.000001 1.000000 0.999999 -1.000000 -0.000000 -0.000000 255 79 3
-1.000000 -1.000000 -1.000000 -1.000000 -0.000000 -0.000000 255 79 3
-1.000000 -1.000000 1.000000 -1.000000 -0.000000 -0.000000 255 79 3
-1.000000 1.000000 -1.000001 0.000000 -0.000000 -1.000000 255 79 3
1.000000 -1.000000 -0.999999 0.000000 -0.000000 -1.000000 255 79 3
-1.000000 -1.000000 -1.000000 0.000000 -0.000000 -1.000000 255 79 3
1.000000 -1.000000 -0.999999 1.000000 -0.000000 0.000000 255 79 3
1.000000 1.000000 1.000000 1.000000 -0.000000 0.000000 255 79 3
1.000000 -1.000000 1.000000 1.000000 -0.000000 0.000000 255 79 3
-1.000000 -1.000000 1.000000 -0.000000 0.000000 1.000000 255 79 3
1.000000 1.000000 1.000000 -0.000000 0.000000 1.000000 255 79 3
-1.000001 1.000000 0.999999 -0.000000 0.000000 1.000000 255 79 3
0.970474 0.380595 0.255382 0.616342 -0.490722 0.615886 255 79 3
-0.262210 1.362038 -0.976389 -0.616342 0.490722 -0.615886 255 79 3
-2.370142 -0.380595 -0.255382 -0.347121 -0.871316 -0.346865 255 79 3
-0.956451 -0.380594 -1.670119 -0.347121 -0.871316 -0.346865 255 79 3
0.276231 -1.362038 -0.438347 -0.347121 -0.871316 -0.346865 255 79 3
-0.956451 -0.380594 -1.670119 0.706845 0.000001 -0.707368 255 79 3
-0.262210 1.362038 -0.976389 0.706845 0.000001 -0.707368 255 79 3
0.970474 0.380595 0.255382 0.706845 0.000001 -0.707368 255 79 3
-1.675900 1.362038 0.438347 0.347121 0.871316 0.346865 255 79 3
-0.443217 0.380594 1.670118 -0.706845 -0.000000 0.707368 255 79 3
1.532196 1.142766 -0.108534 0.784600 0.602279 0.147182 255 79 3
-1.131389 1.060305 0.839366 -0.784600 -0.602279 -0.147182 255 79 3
0.546033 -1.197547 1.136624 0.547192 -0.561048 -0.621132 255 79 3
-0.037004 -0.061792 -0.402898 0.547192 -0.561048 -0.621132 255 79 3
1.532196 1.142766 -0.108534 0.547192 -0.561048 -0.621132 255 79 3
-0.037004 -0.061792 -0.402898 -0.291519 0.567877 -0.769761 255 79 3
-1.131389 1.060305 0.839366 -0.291519 0.567877 -0.769761 255 79 3
0.437811 2.264863 1.133730 -0.291519 0.567877 -0.769761 255 79 3
-1.131389 1.060305 0.839366 -0.547193 0.561048 0.621132 255 79 3
1.020848 1.129108 2.673253 0.291518 -0.567877 0.769761 255 79 3
3.193817 0.429479 -0.001446 0.758559 -0.634765 0.147182 255 79 3
1.676700 1.699007 -0.295811 -0.758559 0.634765 -0.147182 255 79 3
0.819593 0.386702 -1.538075 -0.428553 -0.656153 -0.621132 255 79 3
0.819593 0.386702 -1.538075 0.490847 0.408090 -0.769761 255 79 3
0.695006 0.882828 1.243711 0.428553 0.656153 0.621132 255 79 3
2.212124 -0.386701 1.538076 -0.490847 -0.408089 0.769761 255 79 3
-1.000000 -1.000000 -1.000000 0.000000 -1.000000 -0.000000 255 79 3
1.000000 1.000000 1.000000 -0.000000 1.000000 0.000000 255 79 3
-1.000001 1.000000 0.999999 -1.000000 0.000000 -0.000001 255 79 3
-1.000000 1.000000 -1.000001 -1.000000 0.000000 -0.000001 255 79 3
-1.000000 -1.000000 -1.000000 -1.000000 0.000000 -0.000001 255 79 3
-1.000000 1.000000 -1.000001 0.000001 0.000000 -1.000000 255 79 3
1.000000 1.000000 -0.999999 0.000001 0.000000 -1.000000 255 79 3
1.000000 -1.000000 -0.999999 0.000001 0.000000 -1.000000 255 79 3
1.000000 1.000000 -0.999999 1.000000 -0.000000 0.000000 255 79 3
1.000000 -1.000000 1.000000 -0.000000 0.000000 1.000000 255 79 3
3 0 1 2
3 3 4 5
3 6 7 8
3 9 10 11
3 12 13 14
3 15 16 17
3 18 19 20
3 21 22 23
3 24 25 26
3 27 28 29
3 30 31 32
3 33 34 35
3 36 37 38
3 39 40 41
3 42 43 44
3 45 46 47
3 48 49 50
3 51 52 53
3 54 55 56
3 57 58 59
3 60 61 62
3 63 64 65
3 66 67 68
3 69 70 71
3 0 72 1
3 3 73 4
3 74 75 76
3 77 78 79
3 12 80 13
3 15 81 16
3 18 82 19
3 21 83 22
3 84 85 86
3 87 88 89
3 30 90 31
3 33 91 34
3 36 92 37
3 39 93 40
3 42 94 43
3 45 95 46
3 48 96 49
3 51 97 52
3 54 98 55
3 57 99 58
3 100 101 102
3 103 104 105
3 66 106 67
3 69 107 70
";
    }
}