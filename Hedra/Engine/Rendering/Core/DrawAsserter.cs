using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public static class DrawAsserter
    {
        public static void AssertMultiDrawElement(PrimitiveType Type, int[] Counts, DrawElementsType ElementsType, IntPtr[] Offsets, int Length)
        {
            if(Counts.Length != Offsets.Length)
                throw new ArgumentException($"Found difference in counts ('{Counts.Length}') and offsets ('{Offsets.Length}') arrays");
            
            BaseAssert();
            AssertElementBuffer();
        }

        public static void AssertDrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Offset)
        {
            BaseAssert();
            AssertElementBuffer();
        }

        public static void AssertDrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices, int InstanceCount)
        {
            BaseAssert();
            AssertElementBuffer();
        }

        private static void AssertElementBuffer()
        {
            if(Renderer.BufferHandler.Id == 0)
                throw new ArgumentException($"No indices VBO found when performing DrawElement call.");
            
            if(Renderer.BufferHandler.Target != BufferTarget.ElementArrayBuffer)
                throw new ArgumentException($"Bound VBO target is not 'ElementArrayBuffer'");
        }
        
        public static void AssertDrawArrays(PrimitiveType Type, int Offset, int Count)
        {
            if(Offset > Count) 
                throw new ArgumentException($"Draw offset '{Offset}' cannot be higher than the amount of elements '{Count}'");
            
            if(Renderer.BufferHandler.Id != 0)
                throw new ArgumentException($"Cannot have a bound VBO when using DrawArrays");
            
            BaseAssert();
        }
        
        private static void BaseAssert()
        {
            if(Renderer.ShaderBound == 0)
                throw new ArgumentException("A shader needs to be bound when performing a draw operation.");
            
            if(Renderer.VAOBound == 0)
                throw new ArgumentException("A VAO needs to be bound when performing a draw operation.");

            var shaderBound = Shader.GetById(Renderer.ShaderBound);
            AssertInputs(shaderBound, VAO.GetById(Renderer.VAOBound));
            AssertOutputs(shaderBound, FBO.GetById(Renderer.FBOBound));      
        }

        private static void AssertInputs(Shader Shader, VAO VAOBound)
        {
            var inputs = Shader.Inputs;
            if (Renderer.VertexAttributeHandler.Count != inputs.Length)
                throw new ArgumentException($"The amount of enabled inputs '({Renderer.VertexAttributeHandler.Count})' differs from the amount of shader inputs '({inputs.Length})'");
            
            for (var i = 0; i < inputs.Length; ++i)
            {
                if(!Renderer.VertexAttributeHandler.IsEnabled(inputs[i].Location))
                    throw new ArgumentException($"Shader '{Shader.Name}' expects input '{inputs[i].Name}' of type '{inputs[i].Type}' at location '{inputs[i].Location}' to be enabled but its disabled.");
            }

            var types = VAOBound.Types;
            /* We dont check for different sizes because the VAO might have some attributes disabled */
            for (var i = 0; i < Renderer.VertexAttributeHandler.Count; ++i)
            {
                if(inputs[i].Type != types[i])
                    throw new ArgumentException($"Shader '{Shader.Name}' has input '{inputs[i].Name}' of type '{inputs[i].Type}'' that differs from expected type '{types[i]}'");       
            }
        }

        private static void AssertOutputs(Shader ShaderBound, FBO FBOBound)
        {
            var outputs = ShaderBound.Outputs;
            if (FBOBound != null && FBOBound.Attachments.Length > outputs.Length || FBOBound == null && outputs.Length != 1)
                throw new ArgumentException($"The amount of enabled outputs '({outputs.Length})' from shader '{ShaderBound.Name}' differs from the amount of fbo inputs '({FBOBound?.Attachments.Length ?? 1})'");

            Type ParseType(FramebufferAttachment Attachment)
            {
                var attachmentId = (int)Attachment;
                if (attachmentId >= 36064 || attachmentId <= 36095) /* ColorAttachment */
                    return typeof(Vector4);
                if (attachmentId == 36096) /* DepthAttachment */
                    return typeof(Vector4);
                throw new ArgumentOutOfRangeException($"Unknown attachment type '{Attachment}'");
            }
            
            if (FBOBound != null)
            {
                for (var i = 0; i < FBOBound.Attachments.Length; ++i)
                {
                    var parsedType = ParseType(FBOBound.Attachments[i]);
                    if (outputs[i].Type != parsedType)
                        throw new ArgumentException($"FBO input with type '{parsedType}' differs from shader output type '{outputs[i].Type}'");
                }
            }
            else
            {
                if(outputs[0].Type != typeof(Vector4))
                    throw new ArgumentException($"Shader output type '{outputs[0].Type}' differs from the expected default fbo output '{typeof(Vector4)}'");
            }
        }
    }
}