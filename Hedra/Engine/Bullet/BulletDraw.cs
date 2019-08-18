using System.Drawing;
using BulletSharp;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using OpenTK;
using Vector3 = BulletSharp.Math.Vector3;

namespace Hedra.Engine.Bullet
{
    public class BulletDraw : DebugDraw
    {
        public override void Draw3DText(ref Vector3 location, string textString)
        {
            throw new System.NotImplementedException();
        }
        public override void DrawLine(ref Vector3 @from, ref Vector3 to, ref Vector3 color)
        {
            BasicGeometry.DrawLine(from.Compatible(), to.Compatible(), new Vector4(color.Compatible(), 1));
        }

        public override void ReportErrorWarning(string warningString)
        {
            Log.WriteLine(warningString);
        }

        public override DebugDrawModes DebugMode { get; set; }
    }
}