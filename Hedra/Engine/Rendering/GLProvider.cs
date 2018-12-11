using System;
using System.Diagnostics;
using Hedra.Core;
using Hedra.Engine.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public class GLProvider : IGLProvider
    {
        public ErrorSeverity Severity { get; set; }
        
        public void ActiveTexture(TextureUnit Unit)
        {
            GL.ActiveTexture(Unit);
            EnsureNoErrors();
        }

        public void AttachShader(int S0, int S1)
        {
            GL.AttachShader(S0, S1);
            EnsureNoErrors();
        }

        public void BeginQuery(QueryTarget Target, int V0)
        {
            GL.BeginQuery(Target, V0);
            EnsureNoErrors();
        }

        public void BindBuffer(BufferTarget Target, uint V0)
        {
            GL.BindBuffer(Target, V0);
            EnsureNoErrors();
        }

        public void BindBufferBase(BufferRangeTarget Target, int V0, int V1)
        {
            GL.BindBufferBase(Target, V0, V1);
            EnsureNoErrors();
        }

        public void BindFramebuffer(FramebufferTarget Target, uint Id)
        {
            GL.BindFramebuffer(Target, Id);
            EnsureNoErrors();
        }

        public void BindTexture(TextureTarget Target, uint Id)
        {
            GL.BindTexture(Target, Id);
            EnsureNoErrors();
        }

        public void BindVertexArray(uint Id)
        {
            GL.BindVertexArray(Id);
            EnsureNoErrors();
        }

        public void BlendEquation(BlendEquationMode Mode)
        {
            GL.BlendEquation(Mode);
            EnsureNoErrors();
        }

        public void BlendFunc(BlendingFactor Src, BlendingFactor Dst)
        {
            GL.BlendFunc(Src, Dst);
            EnsureNoErrors();
        }

        public void BlitFramebuffer(int SrcX0, int SrcY0, int SrcX1, int SrcY1, int DstX0, int DstY0, int DstX1, int DstY1,
            ClearBufferMask Mask, BlitFramebufferFilter Filter)
        {
            GL.BlitFramebuffer(SrcX0, SrcY0, SrcX1, SrcY1, DstX0, DstY0, DstX1, DstY1, Mask, Filter);
            EnsureNoErrors();
        }

        public void BufferData(BufferTarget Target, IntPtr Size, IntPtr Offset, BufferUsageHint Hint)
        {
            GL.BufferData(Target, Size, Offset, Hint);
            EnsureNoErrors();
        }

        public void BufferData<T>(BufferTarget Target, IntPtr Size, T[] Data, BufferUsageHint Hint) where T : struct
        {
            GL.BufferData(Target, Size, Data, Hint);
            EnsureNoErrors();
        }

        public void BufferSubData(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, IntPtr Ptr1)
        {
            GL.BufferSubData(Target, Ptr0, Offset, Ptr1);
            EnsureNoErrors();
        }

        public void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, ref T Data) where T : struct
        {
            GL.BufferSubData(Target, Ptr0, Offset, ref Data);
            EnsureNoErrors();
        }

        public void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, T[] Data) where T : struct
        {
            GL.BufferSubData(Target, Ptr0, Offset, Data);
            EnsureNoErrors();
        }

        public FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget Target)
        {
            try
            {
                return GL.CheckFramebufferStatus(Target);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void Clear(ClearBufferMask Mask)
        {
            GL.Clear(Mask);
            EnsureNoErrors();
        }

        public void ClearColor(Vector4 DrawingColor)
        {
            GL.ClearColor(DrawingColor.ToColor());
            EnsureNoErrors();
        }

        public void ColorMask(bool B0, bool B1, bool B2, bool B3)
        {
            GL.ColorMask(B0, B1, B2, B3);
            EnsureNoErrors();
        }

        public void CompileShader(uint Program)
        {
            GL.CompileShader(Program);
            EnsureNoErrors();
        }

        public int CreateProgram()
        {
            try
            {
                return GL.CreateProgram();
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public int CreateShader(ShaderType Type)
        {
            try
            {
                return GL.CreateShader(Type);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void CullFace(CullFaceMode Mode)
        {
            GL.CullFace(Mode);
            EnsureNoErrors();
        }

        public void DebugMessageCallback(DebugProc Proc, IntPtr Ptr)
        {
            GL.DebugMessageCallback(Proc, Ptr);
            EnsureNoErrors();
        }

        public void DeleteBuffers(int N, ref uint Id)
        {
            GL.DeleteBuffers(N, ref Id);
            EnsureNoErrors();
        }

        public void DeleteFramebuffers(int N, params uint[] Ids)
        {
            GL.DeleteFramebuffers(N, Ids);
            EnsureNoErrors();
        }

        public void DeleteProgram(uint Program)
        {
            GL.DeleteProgram(Program);
            EnsureNoErrors();
        }

        public void DeleteQuery(uint Query)
        {
            GL.DeleteQuery(Query);
            EnsureNoErrors();
        }

        public void DeleteShader(uint Program)
        {
            GL.DeleteShader(Program);
            EnsureNoErrors();
        }

        public void DeleteTexture(uint Texture)
        {
            GL.DeleteTexture(Texture);
            EnsureNoErrors();
        }

        public void DeleteTextures(int N, params uint[] Ids)
        {
            GL.DeleteTextures(N, Ids);
            EnsureNoErrors();
        }

        public void DeleteVertexArrays(int N, params uint[] Ids)
        {
            GL.DeleteVertexArrays(N, Ids);
            EnsureNoErrors();
        }

        public void DeleteTextures(int N, ref uint Id)
        {
            GL.DeleteTextures(N, ref Id);
            EnsureNoErrors();
        }

        public void DeleteVertexArrays(int N, ref uint Id)
        {
            GL.DeleteVertexArrays(N, ref Id);
            EnsureNoErrors();
        }

        public void DepthMask(bool Flag)
        {
            GL.DepthMask(Flag);
            EnsureNoErrors();
        }

        public void DetachShader(uint V0, uint V1)
        {
            GL.DetachShader(V0, V1);
            EnsureNoErrors();
        }

        public void Disable(EnableCap Cap)
        {
            GL.Disable(Cap);
            EnsureNoErrors();
        }

        public void DisableVertexAttribArray(int N)
        {
            GL.DisableVertexAttribArray(N);
            EnsureNoErrors();
        }

        public void DrawArrays(PrimitiveType Type, int Offset, int Count)
        {
            GL.DrawArrays(Type, Offset, Count);
            EnsureNoErrors();
        }

        public void DrawBuffer(DrawBufferMode Mode)
        {
            GL.DrawBuffer(Mode);
            EnsureNoErrors();
        }

        public void DrawBuffers(int N, DrawBuffersEnum[] Enums)
        {
            GL.DrawBuffers(N, Enums);
            EnsureNoErrors();
        }

        public void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices)
        {
            GL.DrawElements(Primitive, Count, Type, Indices);
            EnsureNoErrors();
        }

        public void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices, int Instancecount)
        {
            GL.DrawElementsInstanced(Primitive, Count, Type, Indices, Instancecount);
            EnsureNoErrors();
        }

        public void Enable(EnableCap Cap)
        {
            GL.Enable(Cap);
            EnsureNoErrors();
        }

        public void EnableVertexAttribArray(int Id)
        {
            GL.EnableVertexAttribArray(Id);
            EnsureNoErrors();
        }

        public void EndQuery(QueryTarget Target)
        {
            GL.EndQuery(Target);
            EnsureNoErrors();
        }

        public void FramebufferTexture(FramebufferTarget Framebuffer, FramebufferAttachment DepthAttachment, uint Id, int V0)
        {
            GL.FramebufferTexture(Framebuffer, DepthAttachment, Id, V0);
            EnsureNoErrors();
        }

        public void FramebufferTexture2D(FramebufferTarget Target, FramebufferAttachment Attachment, TextureTarget Textarget, uint Texture, int Level)
        {
            GL.FramebufferTexture2D(Target, Attachment, Textarget, Texture, Level);
            EnsureNoErrors();
        }

        public void GenBuffers(int N, out uint V1)
        {
            GL.GenBuffers(N, out V1);
            EnsureNoErrors();
        }

        public int GenFramebuffer()
        {
            try
            {
                return GL.GenFramebuffer();
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public int GenQuery()
        {
            try
            {
                return GL.GenQuery();
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public uint GenTexture()
        {
            try
            {
                return (uint) GL.GenTexture();
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void GenVertexArrays(int N, out uint V1)
        {
            GL.GenVertexArrays(N, out V1);
            EnsureNoErrors();
        }

        public void GenerateMipmap(GenerateMipmapTarget Target)
        {
            GL.GenerateMipmap(Target);
            EnsureNoErrors();
        }

        public void GetActiveUniformBlock(uint V0, uint V1, ActiveUniformBlockParameter Parameter, out int V3)
        {
            GL.GetActiveUniformBlock(V0, V1, Parameter, out V3);
            EnsureNoErrors();
        }

        public ErrorCode GetError()
        {
            try
            {
                return GL.GetError();
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public int GetInteger(GetPName PName)
        {
            try
            {
                return GL.GetInteger(PName);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void GetQueryObject(uint Program, GetQueryObjectParam Parameter, out int Value)
        {
            GL.GetQueryObject(Program, Parameter, out Value);
            EnsureNoErrors();
        }

        public void GetShader(uint Program, ShaderParameter Parameter, out int Value)
        {
            GL.GetShader(Program, Parameter, out Value);
            EnsureNoErrors();
        }

        public string GetShaderInfoLog(int Id)
        {
            try
            {
                return GL.GetShaderInfoLog(Id);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public string GetString(StringName Name)
        {
            try
            {
                return GL.GetString(Name);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public int GetUniformBlockIndex(uint V0, string Name)
        {
            try
            {
                return GL.GetUniformBlockIndex(V0, Name);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public int GetUniformLocation(uint Program, string Name)
        {
            try
            {
                return GL.GetUniformLocation(Program, Name);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void LinkProgram(uint Program)
        {
            GL.LinkProgram(Program);
            EnsureNoErrors();
        }

        public void MultiDrawElements(PrimitiveType Primitive, int[] Counts, DrawElementsType Type, IntPtr[] Offsets, int Count)
        {
            var error = GL.GetError();
            GL.MultiDrawElements(Primitive, Counts, Type, Offsets, Count);
            EnsureNoErrors();
        }

        public void PointSize(float Size)
        {
            GL.PointSize(Size);
            EnsureNoErrors();
        }

        public void PolygonMode(MaterialFace Face, PolygonMode Mode)
        {
            GL.PolygonMode(Face, Mode);
            EnsureNoErrors();
        }

        public void ReadBuffer(ReadBufferMode Mode)
        {
            GL.ReadBuffer(Mode);
            EnsureNoErrors();
        }

        public void ReadPixels(int V0, int V1, int V2, int V3, PixelFormat Format, PixelType Type, int[] Ptr)
        {
            GL.ReadPixels(V0, V1, V2, V3, Format, Type, Ptr);
            EnsureNoErrors();
        }

        public void ShaderSource(int V0, string Source)
        {
            GL.ShaderSource(V0, Source);
            EnsureNoErrors();
        }

        public void StencilFunc(StencilFunction Func, int V0, uint Id)
        {
            GL.StencilFunc(Func, V0, Id);
            EnsureNoErrors();
        }

        public void StencilMask(uint Mask)
        {
            GL.StencilMask(Mask);
            EnsureNoErrors();
        }

        public void StencilOp(StencilOp Fail, StencilOp ZFail, StencilOp ZPass)
        {
            GL.StencilOp(Fail, ZFail, ZPass);
            EnsureNoErrors();
        }

        public void TexImage2D(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3,
            PixelFormat Format, PixelType Type, IntPtr Ptr)
        {
            GL.TexImage2D(Target, V0, InternalFormat, V1, V2, V3, Format, Type, Ptr);
            EnsureNoErrors();
        }

        public void TexImage2DMultisample(TextureTargetMultisample Target, int Samples, PixelInternalFormat InternalFormat,
            int Width, int Height, bool FixedLocations)
        {
            GL.TexImage2DMultisample(Target, Samples, InternalFormat, Width, Height, FixedLocations);
            EnsureNoErrors();
        }

        public void TexImage3D<T>(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3, int V4,
            PixelFormat Format, PixelType Type, T[,,] Data) where T : struct
        {
            GL.TexImage3D(Target, V0, InternalFormat, V1, V2, V3, V4, Format, Type, Data);
            EnsureNoErrors();
        }

        public void TexParameter(TextureTarget Target, TextureParameterName Name, int Value)
        {
            GL.TexParameter(Target, Name, Value);
            EnsureNoErrors();
        }

        public void Uniform1(int Location, int Uniform)
        {
            GL.Uniform1(Location, Uniform);
            EnsureNoErrors();
        }
        
        public void Uniform1(int Location, float Uniform)
        {
            GL.Uniform1(Location, Uniform);
            EnsureNoErrors();
        }
        
        public void Uniform1(int Location, double Uniform)
        {
            GL.Uniform1(Location, Uniform);
            EnsureNoErrors();
        }       

        public void Uniform2(int Location, Vector2 Uniform)
        {
            GL.Uniform2(Location, Uniform);
            EnsureNoErrors();
        }

        public void Uniform3(int Location, Vector3 Uniform)
        {
            GL.Uniform3(Location, Uniform);
            EnsureNoErrors();
        }

        public void Uniform4(int Location, Vector4 Uniform)
        {
            GL.Uniform4(Location, Uniform);
            EnsureNoErrors();
        }

        public void UniformMatrix2(int Location, bool Transpose, ref Matrix2 Uniform)
        {
            GL.UniformMatrix2(Location, Transpose, ref Uniform);
            EnsureNoErrors();
        }

        public void UniformMatrix3(int Location, bool Transpose, ref Matrix3 Uniform)
        {
            GL.UniformMatrix3(Location, Transpose, ref Uniform);
            EnsureNoErrors();
        }

        public void UniformMatrix4(int Location, bool Transpose, ref Matrix4 Uniform)
        {
            GL.UniformMatrix4(Location, Transpose, ref Uniform);
            EnsureNoErrors();
        }

        public void UseProgram(uint Program)
        {
            GL.UseProgram(Program);
            EnsureNoErrors();
        }

        public void VertexAttribDivisor(int V0, int V1)
        {
            GL.VertexAttribDivisor(V0, V1);
            EnsureNoErrors();
        }

        public void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, IntPtr Ptr)
        {
            GL.VertexAttribPointer(V0, V1, Type, Flag, Bytes, Ptr);
            EnsureNoErrors();
        }
        
        public void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, int V2)
        {
            GL.VertexAttribPointer(V0, V1, Type, Flag, Bytes, V2);
            EnsureNoErrors();
        }

        public void Viewport(int V0, int V1, int V2, int V3)
        {
            GL.Viewport(V0, V1, V2, V3);
            EnsureNoErrors();
        }

        private void EnsureNoErrors()
        {
#if DEBUG
            if(System.Threading.Thread.CurrentThread.ManagedThreadId != Loader.Hedra.MainThreadId)
                throw new ArgumentException($"Invalid GL calls outside of the main thread.");
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                var errorMsg = $"Unexpected OpenGL error: {error}";
                Log.WriteResult(false, errorMsg);
                if (ErrorSeverity.High == Severity) throw new RenderException(errorMsg);
            }
#endif
        }
    }
}
