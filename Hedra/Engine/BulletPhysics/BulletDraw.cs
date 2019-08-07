using System.Drawing;
using BulletSharp;
using Hedra.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using Vector3 = BulletSharp.Vector3;

namespace Hedra.Engine.BulletPhysics
{
    public class BulletDraw : DebugDraw
    {
        public override void Draw3dText(ref Vector3 location, string textString)
        {
            throw new System.NotImplementedException();
        }

        public override void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, Color color)
        {
            throw new System.NotImplementedException();
        }

        public override void DrawLine(ref Vector3 @from, ref Vector3 to, Color color)
        {
            BasicGeometry.DrawLine(from.Compatible(), to.Compatible(), color.ToVector4());
        }

        public override void ReportErrorWarning(string warningString)
        {
            Log.WriteLine(warningString);
        }

        public override DebugDrawModes DebugMode { get; set; }
    }
}