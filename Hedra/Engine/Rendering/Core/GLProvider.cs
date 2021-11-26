//using Silk.NET.OpenGL;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Hedra.Engine.IO;
using Hedra.Engine.Windowing;
using Silk.NET.OpenGL;
using BlendingFactor = Hedra.Engine.Windowing.BlendingFactor;
using BlitFramebufferFilter = Hedra.Engine.Windowing.BlitFramebufferFilter;
using ClearBufferMask = Hedra.Engine.Windowing.ClearBufferMask;
using CullFaceMode = Hedra.Engine.Windowing.CullFaceMode;
using DrawBufferMode = Hedra.Engine.Windowing.DrawBufferMode;
using DrawElementsType = Hedra.Engine.Windowing.DrawElementsType;
using EnableCap = Hedra.Engine.Windowing.EnableCap;
using ErrorCode = Hedra.Engine.Windowing.ErrorCode;
using FramebufferAttachment = Hedra.Engine.Windowing.FramebufferAttachment;
using FramebufferTarget = Hedra.Engine.Windowing.FramebufferTarget;
using GetPName = Hedra.Engine.Windowing.GetPName;
using MaterialFace = Hedra.Engine.Windowing.MaterialFace;
using PixelFormat = Hedra.Engine.Windowing.PixelFormat;
using PixelType = Hedra.Engine.Windowing.PixelType;
using PolygonMode = Hedra.Engine.Windowing.PolygonMode;
using PrimitiveType = Hedra.Engine.Windowing.PrimitiveType;
using QueryTarget = Hedra.Engine.Windowing.QueryTarget;
using ReadBufferMode = Hedra.Engine.Windowing.ReadBufferMode;
using ShaderType = Hedra.Engine.Windowing.ShaderType;
using StringName = Hedra.Engine.Windowing.StringName;
using TextureParameterName = Hedra.Engine.Windowing.TextureParameterName;
using TextureTarget = Hedra.Engine.Windowing.TextureTarget;
using TextureUnit = Hedra.Engine.Windowing.TextureUnit;
using VertexAttribPointerType = Hedra.Engine.Windowing.VertexAttribPointerType;

#region Typedefs

using GLBufferTarget = Silk.NET.OpenGL.GLEnum;
using GLTextureUnit = Silk.NET.OpenGL.GLEnum;
using GLQueryTarget = Silk.NET.OpenGL.GLEnum;
using GLFramebufferTarget = Silk.NET.OpenGL.GLEnum;
using GLTextureTarget = Silk.NET.OpenGL.GLEnum;
using GLBlendEquationMode = Silk.NET.OpenGL.GLEnum;
using GLBufferRangeTarget = Silk.NET.OpenGL.GLEnum;
using GLBlendingFactor = Silk.NET.OpenGL.GLEnum;
using GLBufferUsageHint = Silk.NET.OpenGL.GLEnum;
using GLClearBufferMask = Silk.NET.OpenGL.GLEnum;
using GLBlitFramebufferFilter = Silk.NET.OpenGL.GLEnum;
using GLFramebufferErrorCode = Silk.NET.OpenGL.GLEnum;
using GLShaderType = Silk.NET.OpenGL.GLEnum;
using GLCullFaceMode = Silk.NET.OpenGL.GLEnum;
using GLDebugProc = Silk.NET.OpenGL.DebugProc;
using GLEnableCap = Silk.NET.OpenGL.GLEnum;
using GLGetPName = Silk.NET.OpenGL.GLEnum;
using GLVertexAttribPointerType = Silk.NET.OpenGL.GLEnum;
using GLGetProgramParameterName = Silk.NET.OpenGL.GLEnum;
using GLPixelInternalFormat = Silk.NET.OpenGL.GLEnum;
using GLPixelInternalFormatEXT = Silk.NET.OpenGL.GLEnum;
using GLTextureParameterName = Silk.NET.OpenGL.GLEnum;
using GLPixelType = Silk.NET.OpenGL.GLEnum;
using GLPixelFormat = Silk.NET.OpenGL.GLEnum;
using GLTextureTargetMultisample = Silk.NET.OpenGL.GLEnum;
using GLMaterialFace = Silk.NET.OpenGL.GLEnum;
using GLPolygonMode = Silk.NET.OpenGL.GLEnum;
using GLPrimitiveType = Silk.NET.OpenGL.GLEnum;
using GLDrawElementsType = Silk.NET.OpenGL.GLEnum;
using GLDrawBuffersEnum = Silk.NET.OpenGL.GLEnum;
using GLDrawBufferMode = Silk.NET.OpenGL.GLEnum;
using GLFramebufferAttachment = Silk.NET.OpenGL.GLEnum;
using GLGenerateMipmapTarget = Silk.NET.OpenGL.GLEnum;
using GLErrorCode = Silk.NET.OpenGL.GLEnum;
using GLActiveUniformBlockParameter = Silk.NET.OpenGL.GLEnum;
using GLReadBufferMode = Silk.NET.OpenGL.GLEnum;
using GLStringName = Silk.NET.OpenGL.GLEnum;
using GLShaderParameter = Silk.NET.OpenGL.GLEnum;
using GLGetQueryObjectParam = Silk.NET.OpenGL.GLEnum;
using GLFramebufferTargetEXT = Silk.NET.OpenGL.GLEnum;
using GLTextureTargetEXT = Silk.NET.OpenGL.GLEnum;
using GLFramebufferAttachmentEXT = Silk.NET.OpenGL.GLEnum;

#endregion


namespace Hedra.Engine.Rendering.Core
{
    public class GLProvider : IGLProvider
    {
        private readonly GL _gl;

        public GLProvider()
        {
            _gl = GL.GetApi(Program.GameWindow.Window);
        }

        public ErrorSeverity Severity { get; set; }

        public void ActiveTexture(TextureUnit Unit)
        {
            _gl.ActiveTexture((GLTextureUnit)Unit);
            EnsureNoErrors();
        }

        public void AttachShader(int S0, int S1)
        {
            _gl.AttachShader((uint)S0, (uint)S1);
            EnsureNoErrors();
        }

        public void BeginQuery(QueryTarget Target, int V0)
        {
            _gl.BeginQuery((GLQueryTarget)Target, (uint)V0);
            EnsureNoErrors();
        }

        public void BindBuffer(BufferTarget Target, uint V0)
        {
            _gl.BindBuffer((GLBufferTarget)Target, V0);
            EnsureNoErrors();
        }

        public void BindBufferBase(BufferRangeTarget Target, int V0, int V1)
        {
            _gl.BindBufferBase((GLBufferRangeTarget)Target, (uint)V0, (uint)V1);
            EnsureNoErrors();
        }

        public void BindFramebuffer(FramebufferTarget Target, uint Id)
        {
            _gl.BindFramebuffer((GLFramebufferTargetEXT)Target, Id);
            EnsureNoErrors();
        }

        public void BindTexture(TextureTarget Target, uint Id)
        {
            _gl.BindTexture((GLTextureTarget)Target, Id);
            EnsureNoErrors();
        }

        public void BindVertexArray(uint Id)
        {
            _gl.BindVertexArray(Id);
            EnsureNoErrors();
        }

        public void BlendEquation(BlendEquationMode Mode)
        {
            _gl.BlendEquation((GLBlendEquationMode)Mode);
            EnsureNoErrors();
        }

        public void BlendFunc(BlendingFactor Src, BlendingFactor Dst)
        {
            _gl.BlendFunc((GLBlendingFactor)Src, (GLBlendingFactor)Dst);
            EnsureNoErrors();
        }

        public void BlitFramebuffer(int SrcX0, int SrcY0, int SrcX1, int SrcY1, int DstX0, int DstY0, int DstX1,
            int DstY1,
            ClearBufferMask Mask, BlitFramebufferFilter Filter)
        {
            _gl.BlitFramebuffer(SrcX0, SrcY0, SrcX1, SrcY1, DstX0, DstY0, DstX1, DstY1, (uint)Mask,
                (GLBlitFramebufferFilter)Filter);
            EnsureNoErrors();
        }

        public unsafe void BufferData(BufferTarget Target, IntPtr Size, IntPtr Offset, BufferUsageHint Hint)
        {
            _gl.BufferData((GLBufferTarget)Target, (UIntPtr)Size.ToPointer(), Offset.ToPointer(),
                (GLBufferUsageHint)Hint);
            EnsureNoErrors();
        }

        public unsafe void BufferData<T>(BufferTarget Target, IntPtr Size, T[] Data, BufferUsageHint Hint)
            where T : unmanaged
        {
            fixed (void* ptr = Data)
            {
                _gl.BufferData((GLBufferTarget)Target, (UIntPtr)Size.ToPointer(), ptr, (GLBufferUsageHint)Hint);
            }

            EnsureNoErrors();
        }

        public unsafe void BufferSubData(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, IntPtr Ptr1)
        {
            _gl.BufferSubData((GLBufferTarget)Target, Ptr0, (UIntPtr)Offset.ToPointer(), Ptr1.ToPointer());
            EnsureNoErrors();
        }

        public unsafe void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, ref T Data)
            where T : unmanaged
        {
            fixed (void* ptr = &Data)
            {
                _gl.BufferSubData((GLBufferTarget)Target, Ptr0, (UIntPtr)Offset.ToPointer(), ptr);
            }

            EnsureNoErrors();
        }

        public unsafe void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, T[] Data)
            where T : unmanaged
        {
            fixed (void* ptr = Data)
            {
                _gl.BufferSubData((GLBufferTarget)Target, Ptr0, (UIntPtr)Offset.ToPointer(), ptr);
            }

            EnsureNoErrors();
        }

        public FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget Target)
        {
            try
            {
                return (FramebufferErrorCode)_gl.CheckFramebufferStatus((GLFramebufferTargetEXT)Target);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void Clear(ClearBufferMask Mask)
        {
            _gl.Clear((uint)Mask);
            EnsureNoErrors();
        }

        public void ClearColor(Vector4 DrawingColor)
        {
            _gl.ClearColor(DrawingColor.X, DrawingColor.Y, DrawingColor.Z, DrawingColor.W);
            EnsureNoErrors();
        }

        public void ColorMask(bool B0, bool B1, bool B2, bool B3)
        {
            _gl.ColorMask(B0, B1, B2, B3);
            EnsureNoErrors();
        }

        public void CompileShader(uint Program)
        {
            _gl.CompileShader(Program);
            EnsureNoErrors();
        }

        public int CreateProgram()
        {
            try
            {
                return (int)_gl.CreateProgram();
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
                return (int)_gl.CreateShader((GLShaderType)Type);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void CullFace(CullFaceMode Mode)
        {
            _gl.CullFace((GLCullFaceMode)Mode);
            EnsureNoErrors();
        }

        public unsafe void DebugMessageCallback(GLDebugProc Proc, IntPtr Ptr)
        {
            _gl.DebugMessageCallback(Proc, Ptr.ToPointer());
            EnsureNoErrors();
        }

        public unsafe void DeleteBuffers(int N, ref uint Id)
        {
            fixed (uint* ptr = &Id)
            {
                _gl.DeleteBuffers((uint)N, ptr);
            }

            EnsureNoErrors();
        }

        public void DeleteFramebuffers(int N, params uint[] Ids)
        {
            _gl.DeleteFramebuffers(new ReadOnlySpan<uint>(Ids, 0, N));
            EnsureNoErrors();
        }

        public void DeleteProgram(uint Program)
        {
            _gl.DeleteProgram(Program);
            EnsureNoErrors();
        }

        public void DeleteQuery(uint Query)
        {
            _gl.DeleteQueries(1, Query);
            EnsureNoErrors();
        }

        public void DeleteShader(uint Program)
        {
            _gl.DeleteShader(Program);
            EnsureNoErrors();
        }

        public void DeleteTexture(uint Texture)
        {
            _gl.DeleteTextures(1, Texture);
            EnsureNoErrors();
        }

        public void DeleteTextures(int N, params uint[] Ids)
        {
            _gl.DeleteTextures(new ReadOnlySpan<uint>(Ids, 0, N));
            EnsureNoErrors();
        }

        public void DeleteVertexArrays(int N, ref uint Id)
        {
            _gl.DeleteVertexArrays((uint)N, Id);
            EnsureNoErrors();
        }

        public void DepthMask(bool Flag)
        {
            _gl.DepthMask(Flag);
            EnsureNoErrors();
        }

        public void DetachShader(uint V0, uint V1)
        {
            _gl.DetachShader(V0, V1);
            EnsureNoErrors();
        }

        public void Disable(EnableCap Cap)
        {
            _gl.Disable((GLEnableCap)Cap);
            EnsureNoErrors();
        }

        public void DisableVertexAttribArray(uint N)
        {
            _gl.DisableVertexAttribArray(N);
            EnsureNoErrors();
        }

        public void DrawArrays(PrimitiveType Type, int Offset, int Count)
        {
            _gl.DrawArrays((GLPrimitiveType)Type, Offset, (uint)Count);
            EnsureNoErrors();
        }

        public void DrawBuffer(DrawBufferMode Mode)
        {
            _gl.DrawBuffer((GLDrawBufferMode)Mode);
            EnsureNoErrors();
        }

        public unsafe void DrawBuffers(int N, GLDrawBuffersEnum[] Enums)
        {
            fixed (GLDrawBuffersEnum* ptr = Enums)
            {
                _gl.DrawBuffers((uint)N, ptr);
            }

            EnsureNoErrors();
        }

        public unsafe void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices)
        {
            _gl.DrawElements((GLPrimitiveType)Primitive, (uint)Count, (GLDrawElementsType)Type, Indices.ToPointer());
            EnsureNoErrors();
        }

        public unsafe void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type,
            IntPtr Indices, int Instancecount)
        {
            _gl.DrawElementsInstanced((GLPrimitiveType)Primitive, (uint)Count, (GLDrawElementsType)Type,
                Indices.ToPointer(), (uint)Instancecount);
            EnsureNoErrors();
        }

        public void Enable(EnableCap Cap)
        {
            _gl.Enable((GLEnableCap)Cap);
            EnsureNoErrors();
        }

        public void EnableVertexAttribArray(uint Id)
        {
            _gl.EnableVertexAttribArray(Id);
            EnsureNoErrors();
        }

        public void EndQuery(QueryTarget Target)
        {
            _gl.EndQuery((GLQueryTarget)Target);
            EnsureNoErrors();
        }

        public void FramebufferTexture(FramebufferTarget Framebuffer, FramebufferAttachment DepthAttachment, uint Id,
            int V0)
        {
            _gl.FramebufferTexture((GLFramebufferTarget)Framebuffer, (GLFramebufferAttachment)DepthAttachment, Id, V0);
            EnsureNoErrors();
        }

        public void FramebufferTexture2D(FramebufferTarget Target, FramebufferAttachment Attachment,
            TextureTarget TextureTarget, uint Texture, int Level)
        {
            _gl.FramebufferTexture2D((GLFramebufferTargetEXT)Target, (GLFramebufferAttachmentEXT)Attachment,
                (GLTextureTargetEXT)TextureTarget, Texture, Level);
            EnsureNoErrors();
        }

        public void GenBuffers(int N, out uint V1)
        {
            _gl.GenBuffers((uint)N, out V1);
            EnsureNoErrors();
        }

        public int GenFramebuffer()
        {
            try
            {
                var id = 0u;
                _gl.GenFramebuffers(1, out id);
                return (int)id;
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
                var id = 0u;
                _gl.GenQueries(1, out id);
                return (int)id;
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
                var id = 0u;
                _gl.GenTextures(1, out id);
                return id;
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void GenVertexArrays(int N, out uint V1)
        {
            _gl.GenVertexArrays((uint)N, out V1);
            EnsureNoErrors();
        }

        public void GenerateMipmap(GenerateMipmapTarget Target)
        {
            _gl.GenerateMipmap((GLGenerateMipmapTarget)Target);
            EnsureNoErrors();
        }

        public void GetActiveUniformBlock(uint V0, uint V1, ActiveUniformBlockParameter Parameter, out int V3)
        {
            _gl.GetActiveUniformBlock(V0, V1, (GLActiveUniformBlockParameter)Parameter, out V3);
            EnsureNoErrors();
        }

        public ErrorCode GetError()
        {
            try
            {
                return (ErrorCode)_gl.GetError();
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public unsafe int GetInteger(GetPName PName)
        {
            try
            {
                var output = 0;
                _gl.GetInteger((GLGetPName)PName, &output);
                return output;
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void GetInteger(GetPName PName, out int Value)
        {
            _gl.GetInteger((GLGetPName)PName, out Value);
            EnsureNoErrors();
        }

        public void GetQueryObject(uint Program, GetQueryObjectParam Parameter, out int Value)
        {
            _gl.GetQueryObject(Program, (GLGetQueryObjectParam)Parameter, out Value);
            EnsureNoErrors();
        }

        public void GetShader(uint Program, ShaderParameter Parameter, out int Value)
        {
            _gl.GetShader(Program, (GLShaderParameter)Parameter, out Value);
            EnsureNoErrors();
        }

        public string GetShaderInfoLog(int Id)
        {
            var log = string.Empty;
            _gl.GetShaderInfoLog((uint)Id, out log);
            EnsureNoErrors();
            return log;
        }

        public unsafe string GetString(StringName Name)
        {
            try
            {
                return Marshal.PtrToStringAnsi((IntPtr)_gl.GetString((GLStringName)Name));
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
                return (int)_gl.GetUniformBlockIndex(V0, Name);
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
                return _gl.GetUniformLocation(Program, Name);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void LinkProgram(uint Program)
        {
            _gl.LinkProgram(Program);
            EnsureNoErrors();
        }

        public unsafe void MultiDrawElements(PrimitiveType Primitive, uint[] Counts, DrawElementsType Type,
            IntPtr[] Offsets, int Count)
        {
            var stackPtr = stackalloc byte[IntPtr.Size * Offsets.Length];
            var arr = (void**)stackPtr;
            for (var i = 0; i < Offsets.Length; ++i)
                arr[i] = Offsets[i].ToPointer();

            fixed (uint* ptr1 = Counts)
            {
                _gl.MultiDrawElements((GLPrimitiveType)Primitive, ptr1, (GLDrawElementsType)Type, arr, (uint)Count);
            }

            EnsureNoErrors();
        }

        public void PointSize(float Size)
        {
            _gl.PointSize(Size);
            EnsureNoErrors();
        }

        public void LineWidth(float Width)
        {
            _gl.LineWidth(Width);
            EnsureNoErrors();
        }

        public void PolygonMode(MaterialFace Face, PolygonMode Mode)
        {
            _gl.PolygonMode((GLMaterialFace)Face, (GLPolygonMode)Mode);
            EnsureNoErrors();
        }

        public void ReadBuffer(ReadBufferMode Mode)
        {
            _gl.ReadBuffer((GLReadBufferMode)Mode);
            EnsureNoErrors();
        }

        public unsafe void ReadPixels(int V0, int V1, int V2, int V3, PixelFormat Format, PixelType Type, byte[] Ptr)
        {
            fixed (void* ptr = Ptr)
            {
                _gl.ReadPixels(V0, V1, (uint)V2, (uint)V3, (GLPixelFormat)Format, (GLPixelType)Type, ptr);
            }

            EnsureNoErrors();
        }

        public void ShaderSource(int V0, string Source)
        {
            _gl.ShaderSource((uint)V0, Source);
            EnsureNoErrors();
        }

        public unsafe void TexImage2D(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2,
            int V3,
            PixelFormat Format, PixelType Type, IntPtr Ptr)
        {
            _gl.TexImage2D((GLTextureTarget)Target, V0, (int)InternalFormat, (uint)V1, (uint)V2, V3,
                (GLPixelFormat)Format, (GLPixelType)Type, Ptr.ToPointer());
            EnsureNoErrors();
        }

        public void TexImage2DMultisample(TextureTargetMultisample Target, int Samples,
            PixelInternalFormat InternalFormat,
            int Width, int Height, bool FixedLocations)
        {
            _gl.TexImage2DMultisample((GLTextureTargetMultisample)Target, (uint)Samples,
                (GLPixelInternalFormatEXT)InternalFormat, (uint)Width, (uint)Height, FixedLocations);
            EnsureNoErrors();
        }

        public unsafe void TexImage3D<T>(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1,
            int V2, int V3, int V4,
            PixelFormat Format, PixelType Type, T[] Pixels) where T : unmanaged
        {
            fixed (void* ptr = Pixels)
            {
                _gl.TexImage3D((GLTextureTarget)Target, V0, (int)InternalFormat, (uint)V1, (uint)V2, (uint)V3, V4,
                    (GLPixelFormat)Format, (GLPixelType)Type, ptr);
            }

            EnsureNoErrors();
        }

        public void TexParameter(TextureTarget Target, TextureParameterName Name, int Value)
        {
            _gl.TexParameter((GLTextureTarget)Target, (GLTextureParameterName)Name, Value);
            EnsureNoErrors();
        }

        public void Uniform1(int Location, int Uniform)
        {
            _gl.Uniform1(Location, Uniform);
            EnsureNoErrors();
        }

        public void Uniform1(int Location, float Uniform)
        {
            _gl.Uniform1(Location, Uniform);
            EnsureNoErrors();
        }

        public void Uniform1(int Location, double Uniform)
        {
            _gl.Uniform1(Location, Uniform);
            EnsureNoErrors();
        }

        public unsafe void Uniform2(int Location, Vector2 Uniform)
        {
            _gl.Uniform2(Location, 1, (float*)&Uniform);
            EnsureNoErrors();
        }

        public unsafe void Uniform3(int Location, Vector3 Uniform)
        {
            _gl.Uniform3(Location, 1, (float*)&Uniform);
            EnsureNoErrors();
        }

        public unsafe void Uniform4(int Location, Vector4 Uniform)
        {
            _gl.Uniform4(Location, 1, (float*)&Uniform);
            EnsureNoErrors();
        }

        public unsafe void UniformMatrix2(int Location, bool Transpose, ref Matrix4x4 Uniform)
        {
            var mem = new Vector4(Uniform.M11, Uniform.M12, Uniform.M21, Uniform.M22);
            _gl.UniformMatrix2(Location, 1, Transpose, &mem.X);
            EnsureNoErrors();
        }

        public unsafe void UniformMatrix3(int Location, bool Transpose, ref Matrix4x4 Uniform)
        {
            var mem = stackalloc float[9];
            mem[0] = Uniform.M11;
            mem[1] = Uniform.M12;
            mem[2] = Uniform.M13;
            mem[3] = Uniform.M21;
            mem[4] = Uniform.M22;
            mem[5] = Uniform.M23;
            mem[6] = Uniform.M31;
            mem[7] = Uniform.M32;
            mem[8] = Uniform.M33;
            _gl.UniformMatrix3(Location, 1, Transpose, mem);
            EnsureNoErrors();
        }

        public unsafe void UniformMatrix4x4(int Location, bool Transpose, ref Matrix4x4 Uniform)
        {
            fixed (float* ptr = &Uniform.M11)
            {
                _gl.UniformMatrix4(Location, 1, false, ptr);
            }

            EnsureNoErrors();
        }

        public void UseProgram(uint Program)
        {
            _gl.UseProgram(Program);
            EnsureNoErrors();
        }

        public void VertexAttribDivisor(int V0, int V1)
        {
            _gl.VertexAttribDivisor((uint)V0, (uint)V1);
            EnsureNoErrors();
        }

        public unsafe void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes,
            IntPtr Ptr)
        {
            _gl.VertexAttribPointer((uint)V0, V1, (GLVertexAttribPointerType)Type, Flag, (uint)Bytes, Ptr.ToPointer());
            EnsureNoErrors();
        }

        public unsafe void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes)
        {
            _gl.VertexAttribPointer((uint)V0, V1, (GLVertexAttribPointerType)Type, Flag, (uint)Bytes,
                IntPtr.Zero.ToPointer());
            EnsureNoErrors();
        }

        public void Viewport(int V0, int V1, int V2, int V3)
        {
            _gl.Viewport(V0, V1, (uint)V2, (uint)V3);
            EnsureNoErrors();
        }

        public void GetProgram(int ShaderId, GetProgramParameterName ParameterName, out int Value)
        {
            Value = 0;
            _gl.GetProgram((uint)ShaderId, (GLGetProgramParameterName)ParameterName, out Value);
            EnsureNoErrors();
        }

        public void GetProgramInfoLog(int ShaderId, out string Log)
        {
            _gl.GetProgramInfoLog((uint)ShaderId, out Log);
            EnsureNoErrors();
        }

        public void UniformBlockBinding(int ShaderId, int Index, int BindingPoint)
        {
            _gl.UniformBlockBinding((uint)ShaderId, (uint)Index, (uint)BindingPoint);
            EnsureNoErrors();
        }

        public void DeleteVertexArrays(int N, params uint[] Ids)
        {
            _gl.DeleteVertexArrays(new ReadOnlySpan<uint>(Ids, 0, N));
            EnsureNoErrors();
        }

        public void DeleteTextures(int N, ref uint Id)
        {
            _gl.DeleteTextures((uint)N, Id);
            EnsureNoErrors();
        }

        private void EnsureNoErrors()
        {
#if DEBUG
            if (Thread.CurrentThread.ManagedThreadId != Loader.Hedra.MainThreadId)
                throw new ArgumentException("Invalid GL calls outside of the main thread.");
            var error = _gl.GetError();
            if (error != GLEnum.NoError /*&& ErrorSeverity.Ignore != Severity*/)
            {
                var errorMsg =
                    $"Unexpected OpenGL error: {error} {Environment.NewLine} Stack:{Environment.NewLine}{new StackTrace()}";
                Log.WriteResult(false, errorMsg);
                if (ErrorSeverity.High == Severity) throw new RenderException(errorMsg);
            }
#endif
        }
    }
}