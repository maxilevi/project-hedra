using System;
using BulletSharp;
using BulletSharp.Math;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using Vector4 = System.Numerics.Vector4;

namespace Hedra.Engine.Bullet
{
    public class BulletDraw : DebugDraw
    {
        public override DebugDrawModes DebugMode { get; set; }

        public override void Draw3DText(ref Vector3 location, string textString)
        {
            throw new NotImplementedException();
        }

        public override void DrawLine(ref Vector3 from, ref Vector3 to, ref Vector3 color)
        {
            BasicGeometry.DrawLine(from.Compatible(), to.Compatible(), new Vector4(color.Compatible(), 1));
        }

        public override void ReportErrorWarning(string warningString)
        {
            Log.WriteLine(warningString);
        }
    }
}