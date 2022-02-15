using System;
using System.Collections.Generic;
using System.IO;
using Hedra.Engine.Management;
using Hedra.Numerics;
using Hedra.Rendering;
using SixLabors.ImageSharp;

namespace Hedra.Engine.WorldBuilding;

public static class LandformLoader
{
    private static readonly Dictionary<LandformType, string> LandformPaths = new Dictionary<LandformType, string>
    {
        { LandformType.Landform1, "Assets/Env/Landforms/landform1.png"},
        { LandformType.Landform2, "Assets/Env/Landforms/landform2.png"},
        { LandformType.Landform3, "Assets/Env/Landforms/landform3.png"},
        { LandformType.Landform4, "Assets/Env/Landforms/landform4.png"},
        { LandformType.Landform5, "Assets/Env/Landforms/landform5.png"},
        { LandformType.Landform6, "Assets/Env/Landforms/landform6.png"},
        { LandformType.Landform7, "Assets/Env/Landforms/landform7.png"},
        { LandformType.Landform8, "Assets/Env/Landforms/landform8.png"},
        { LandformType.Landform9, "Assets/Env/Landforms/landform9.png"},
        { LandformType.Landform10, "Assets/Env/Landforms/landform10.png"},
    };

    public static float[][] Load(LandformType LandformType)
    {
        var img = Graphics2D.LoadBitmapFromAssets(LandformPaths[LandformType]);
        if (!img.TryGetSinglePixelSpan(out var span))
            throw new ArgumentException();

        var scale = 2;
        var arr = new float[img.Width * scale][];
        for (var i = 0; i < arr.Length; ++i)
        {
            arr[i] = new float[img.Height * scale];
            for (var j = 0; j < arr[i].Length; ++j)
            {
                var x = i / scale;
                var y = j / scale;
                arr[i][j] = span[x + y * img.Width].R / 256f;
            }
        }

        return arr;
    }
}

public enum LandformType
{
    Landform1,
    Landform2,
    Landform3,
    Landform4,
    Landform5,
    Landform6,
    Landform7,
    Landform8,
    Landform9,
    Landform10,
    MaxLandforms,
}