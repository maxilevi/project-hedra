using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Testing.GraphicsTests
{
    internal class GraphicsWrapperTester : BaseTest
    {
        //[TestMethod]
        public void TestMatrixStack()
        {
            GraphicsLayer.MatrixMode(MatrixMode.Modelview);

            GraphicsLayer.LoadIdentity();

            GraphicsLayer.Translate(Vector3.One);

            GraphicsLayer.LoadIdentity();

            this.AssertEqual(GraphicsLayer.MvMatrix(), Matrix4.Identity, 
                "LoadIdentity returned an unexpected result. " + Environment.NewLine + GraphicsLayer.MvMatrix());

            GraphicsLayer.PushMatrix();
            GraphicsLayer.Translate(Vector3.One);
            GraphicsLayer.PopMatrix();

            this.AssertEqual(GraphicsLayer.MvMatrix(), Matrix4.Identity, 
                "PushMatrix and PopMatrix returned an unexpected result." + Environment.NewLine + GraphicsLayer.MvMatrix());
        }

        //[TestMethod]
        public void TestMatrixTransformationFunctionsWork()
        {
            GraphicsLayer.MatrixMode(MatrixMode.Modelview);

            GraphicsLayer.LoadIdentity();
            GraphicsLayer.Translate(Vector3.UnitY);
            this.AssertEqual(GraphicsLayer.MvMatrix(), Matrix4.CreateTranslation(Vector3.UnitY), 
                "Translate returned an unexpected result." + Environment.NewLine + GraphicsLayer.MvMatrix() + Environment.NewLine + Matrix4.CreateTranslation(Vector3.UnitY));

            GraphicsLayer.LoadIdentity();
            GraphicsLayer.Rotate(45f, Vector3.UnitY);
            var resultRot = GraphicsLayer.MvMatrix();
            var expectedRot = Matrix4.CreateRotationY(45f * Mathf.Radian);
            var areRotEqual = true;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (Math.Abs(resultRot[i,j] - expectedRot[i,j]) > 0.001f)
                        areRotEqual = false;
                }
            }

            this.AssertTrue(areRotEqual, 
                "Rotate returned an unexpected result." + Environment.NewLine + GraphicsLayer.MvMatrix() + Environment.NewLine + Matrix4.CreateRotationY(45f * Mathf.Radian));

            GraphicsLayer.LoadIdentity();
            GraphicsLayer.Scale(Vector3.UnitY);
            this.AssertEqual(GraphicsLayer.MvMatrix(), Matrix4.CreateScale(Vector3.UnitY),
                "Scale returned an unexpected result." + Environment.NewLine + GraphicsLayer.MvMatrix() + Environment.NewLine + Matrix4.CreateScale(Vector3.UnitY));

        }
    }
}
