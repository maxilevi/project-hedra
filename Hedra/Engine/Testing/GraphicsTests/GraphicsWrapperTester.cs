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
    public class GraphicsWrapperTester : BaseTest
    {
        //[TestMethod]
        public void TestMatrixStack()
        {
            Renderer.MatrixMode(MatrixMode.Modelview);

            Renderer.LoadIdentity();

            Renderer.Translate(Vector3.One);

            Renderer.LoadIdentity();

            this.AssertEqual(Renderer.MvMatrix(), Matrix4.Identity, 
                "LoadIdentity returned an unexpected result. " + Environment.NewLine + Renderer.MvMatrix());

            Renderer.PushMatrix();
            Renderer.Translate(Vector3.One);
            Renderer.PopMatrix();

            this.AssertEqual(Renderer.MvMatrix(), Matrix4.Identity, 
                "PushMatrix and PopMatrix returned an unexpected result." + Environment.NewLine + Renderer.MvMatrix());
        }

        //[TestMethod]
        public void TestMatrixTransformationFunctionsWork()
        {
            Renderer.MatrixMode(MatrixMode.Modelview);

            Renderer.LoadIdentity();
            Renderer.Translate(Vector3.UnitY);
            this.AssertEqual(Renderer.MvMatrix(), Matrix4.CreateTranslation(Vector3.UnitY), 
                "Translate returned an unexpected result." + Environment.NewLine + Renderer.MvMatrix() + Environment.NewLine + Matrix4.CreateTranslation(Vector3.UnitY));

            Renderer.LoadIdentity();
            Renderer.Rotate(45f, Vector3.UnitY);
            var resultRot = Renderer.MvMatrix();
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
                "Rotate returned an unexpected result." + Environment.NewLine + Renderer.MvMatrix() + Environment.NewLine + Matrix4.CreateRotationY(45f * Mathf.Radian));

            Renderer.LoadIdentity();
            Renderer.Scale(Vector3.UnitY);
            this.AssertEqual(Renderer.MvMatrix(), Matrix4.CreateScale(Vector3.UnitY),
                "Scale returned an unexpected result." + Environment.NewLine + Renderer.MvMatrix() + Environment.NewLine + Matrix4.CreateScale(Vector3.UnitY));

        }
    }
}
