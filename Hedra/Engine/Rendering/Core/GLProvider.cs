using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Hedra.Engine.IO;
using OpenToolkit.Graphics.GL;
using OpenToolkit.Graphics.GL30;
using OpenToolkit.Mathematics;
using EXT = OpenToolkit.Graphics.EXT;

namespace Hedra.Engine.Rendering.Core
{
    public class GLProvider : IGLProvider
    {
        private const int ErrorCodeNoError = 0;
        
        public ErrorSeverity Severity { get; set; }
        
        public void ActiveTexture(TextureUnit Unit)
        {
            GL.ActiveTexture(Unit);
            EnsureNoErrors();
        }

        public void AttachShader(uint S0, uint S1)
        {
            GL.AttachShader(S0, S1);
            EnsureNoErrors();
        }

        public void BeginQuery(QueryTarget Target, uint V0)
        {
            GL.BeginQuery(Target, V0);
            EnsureNoErrors();
        }

        public void BindBuffer(BufferTargetARB Target, uint V0)
        {
            GL.BindBuffer(Target, V0);
            EnsureNoErrors();
        }

        public void BindBufferBase(EXT.BufferTargetARB Target, uint V0, uint V1)
        {
            EXT.GL.BindBufferBase(Target, V0, V1);
            EnsureNoErrors();
        }

        public void BindFramebuffer(EXT.FramebufferTarget Target, uint Id)
        {
            EXT.GL.BindFramebuffer(Target, Id);
            EnsureNoErrors();
        }

        public void BindTexture(TextureTarget Target, uint Id)
        {
            GL.BindTexture(Target, Id);
            EnsureNoErrors();
        }

        public void BindVertexArray(uint Id)
        {
            EXT.GL.BindVertexArray(Id);
            EnsureNoErrors();
        }

        public void BlendEquation(EXT.BlendEquationModeEXT Mode)
        {
            EXT.GL.BlendEquation(Mode);
            EnsureNoErrors();
        }

        public void BlendFunc(BlendingFactor Src, BlendingFactor Dst)
        {
            GL.BlendFunc(Src, Dst);
            EnsureNoErrors();
        }

        public void BlitFramebuffer(int SrcX0, int SrcY0, int SrcX1, int SrcY1, int DstX0, int DstY0, int DstX1, int DstY1,
            ClearBufferMask Mask, EXT.BlitFramebufferFilter Filter)
        {
            EXT.GL.BlitFramebuffer(SrcX0, SrcY0, SrcX1, SrcY1, DstX0, DstY0, DstX1, DstY1, (uint)Mask, Filter);
            EnsureNoErrors();
        }

        public unsafe void BufferData(BufferTargetARB Target, IntPtr Size, IntPtr Data, BufferUsageARB Hint)
        {
            GL.BufferData(Target, (UIntPtr)Size.ToInt32(), Data.ToPointer(), Hint);
            EnsureNoErrors();
        }

        public void BufferData<T>(BufferTargetARB Target, IntPtr Size, T[] Data, BufferUsageARB Hint) where T : unmanaged
        {
            GL.BufferData(Target, (UIntPtr)Size.ToInt32(), Data, Hint);
            EnsureNoErrors();
        }

        public unsafe void BufferSubData(BufferTargetARB Target, IntPtr Ptr0, UIntPtr Offset, IntPtr Ptr1)
        {
            GL.BufferSubData(Target, Ptr0, Offset, Ptr1.ToPointer());
            EnsureNoErrors();
        }

        public void BufferSubData<T>(BufferTargetARB Target, IntPtr Ptr0, UIntPtr Offset, ref T Data) where T : unmanaged
        {
            GL.BufferSubData(Target, Ptr0, Offset, ref Data);
            EnsureNoErrors();
        }

        public void BufferSubData<T>(BufferTargetARB Target, IntPtr Ptr0, UIntPtr Offset, T[] Data) where T : unmanaged
        {
            GL.BufferSubData(Target, Ptr0, Offset, Data);
            EnsureNoErrors();
        }
        
        public unsafe void BufferSubData<T>(BufferTargetARB Target, IntPtr Ptr0, UIntPtr Offset, IntPtr Data) where T : unmanaged
        {
            GL.BufferSubData(Target, Ptr0, Offset, Data.ToPointer());
            EnsureNoErrors();
        }

        public EXT.FramebufferStatus CheckFramebufferStatus(EXT.FramebufferTarget Target)
        {
            try
            {
                return EXT.GL.CheckFramebufferStatusEXT(Target);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void Clear(ClearBufferMask Mask)
        {
            GL.Clear((uint)Mask);
            EnsureNoErrors();
        }

        public void ClearColor(Vector4 DrawingColor)
        {
            GL.ClearColor(DrawingColor.X, DrawingColor.Y, DrawingColor.Z, DrawingColor.W);
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
                return (int)GL.CreateProgram();
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public uint CreateShader(ShaderType Type)
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

        public unsafe void DebugMessageCallback(DebugProc Proc, IntPtr Ptr)
        {
            EXT.GL.DebugMessageCallback(Proc, Ptr.ToPointer());
            EnsureNoErrors();
        }

        public void DeleteBuffers(uint N, ref uint Id)
        {
            GL.DeleteBuffers(N, ref Id);
            EnsureNoErrors();
        }

        public void DeleteFramebuffers(uint N, params uint[] Ids)
        {
            EXT.GL.DeleteFramebuffers(N, Ids);
            EnsureNoErrors();
        }

        public void DeleteProgram(uint Program)
        {
            GL.DeleteProgram(Program);
            EnsureNoErrors();
        }

        public void DeleteQuery(uint Query)
        {
            GL.DeleteQueries(1, ref Query);
            EnsureNoErrors();
        }

        public void DeleteShader(uint Program)
        {
            GL.DeleteShader(Program);
            EnsureNoErrors();
        }

        public void DeleteTextures(uint N, params uint[] Ids)
        {
            GL.DeleteTextures(N, Ids);
            EnsureNoErrors();
        }

        public void DeleteVertexArrays(uint N, params uint[] Ids)
        {
            EXT.GL.DeleteVertexArrays(N, Ids);
            EnsureNoErrors();
        }

        public void DeleteVertexArrays(uint N, ref uint Id)
        {
            EXT.GL.DeleteVertexArrays(N, ref Id);
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

        public void DisableVertexAttribArray(uint N)
        {
            GL.DisableVertexAttribArray(N);
            EnsureNoErrors();
        }

        public void DrawArrays(PrimitiveType Type, int Offset, uint Count)
        {
            GL.DrawArrays(Type, Offset, Count);
            EnsureNoErrors();
        }

        public void DrawBuffer(DrawBufferMode Mode)
        {
            GL.DrawBuffer(Mode);
            EnsureNoErrors();
        }

        public void DrawBuffers(int N, DrawBufferMode[] Enums)
        {
            GL.DrawBuffers((uint)N, Enums);
            EnsureNoErrors();
        }

        public unsafe void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices)
        {
            GL.DrawElements(Primitive, (uint)Count, Type, Indices.ToPointer());
            EnsureNoErrors();
        }

        public unsafe void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices, int InstanceCount)
        {
            GL.DrawElementsInstanced(Primitive, (uint)Count, Type, Indices.ToPointer(), (uint)InstanceCount);
            EnsureNoErrors();
        }

        public void Enable(EnableCap Cap)
        {
            GL.Enable(Cap);
            EnsureNoErrors();
        }

        public void EnableVertexAttribArray(uint Id)
        {
            GL.EnableVertexAttribArray(Id);
            EnsureNoErrors();
        }

        public void EndQuery(QueryTarget Target)
        {
            GL.EndQuery(Target);
            EnsureNoErrors();
        }

        public void FramebufferTexture(EXT.FramebufferTarget Framebuffer, EXT.FramebufferAttachment DepthAttachment, uint Id, int V0)
        {
            EXT.GL.FramebufferTextureEXT(Framebuffer, DepthAttachment, Id, V0);
            EnsureNoErrors();
        }

        public void FramebufferTexture2D(EXT.FramebufferTarget Target, EXT.FramebufferAttachment Attachment, EXT.TextureTarget TextureTarget, uint Texture, int Level)
        {
            EXT.GL.FramebufferTexture2D(Target, Attachment, TextureTarget, Texture, Level);
            EnsureNoErrors();
        }

        public void GenBuffers(int N, out uint V1)
        {
            V1 = 0;
            GL.GenBuffers((uint)N, ref V1);
            EnsureNoErrors();
        }

        public int GenFramebuffer()
        {
            try
            {
                var id = 0u;
                EXT.GL.GenFramebuffersEXT(1, ref id);
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
                var query = 0u;
                GL.GenQueries(1, ref query);
                return (int)query;
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
                var texture = 0u;
                GL.GenTextures(1, ref texture);
                return texture;
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void GenVertexArrays(int N, out uint V1)
        {
            V1 = 0;
            EXT.GL.GenVertexArrays((uint)N, ref V1);
            EnsureNoErrors();
        }

        public void GenerateMipmap(EXT.TextureTarget Target)
        {
            EXT.GL.GenerateMipmap(Target);
            EnsureNoErrors();
        }

        public void GetActiveUniformBlock(uint V0, uint V1, EXT.UniformBlockPName Parameter, out int V3)
        {
            V3 = 0;
            EXT.GL.GetActiveUniformBlock(V0, V1, Parameter, ref V3);
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
                var value = 0;
                GL.GetInteger(PName, ref value);
                return value;
            }
            finally
            {
                EnsureNoErrors();
            }
        }
        
        public void GetInteger(GetPName PName, out int Value)
        {
            Value = 0;
            GL.GetInteger(PName, ref Value);
            EnsureNoErrors();
        }

        public void GetQueryObject(uint Program, QueryObjectParameterName Parameter, out int Value)
        {
            Value = 0;
            GL.GetQueryObject(Program, Parameter, ref Value);
            EnsureNoErrors();
        }

        public void GetShader(uint Program, ShaderParameterName Parameter, out int Value)
        {
            Value = 0;
            GL.GetShader(Program, Parameter, ref Value);
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

        public unsafe string GetString(StringName Name)
        {
            try
            {
                return Marshal.PtrToStringAnsi((IntPtr)GL.GetString(Name));
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
                return (int) EXT.GL.GetUniformBlockIndex(V0, Name);
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

        public void MultiDrawElements(PrimitiveType Primitive, uint[] Counts, DrawElementsType Type, IntPtr[] Offsets, uint Count)
        {
            GL.MultiDrawElements(Primitive, Counts, Type, Offsets, Count);
            EnsureNoErrors();
        }

        public void PointSize(float Size)
        {
            GL.PointSize(Size);
            EnsureNoErrors();
        }
        
        public void LineWidth(float Width)
        {
            GL.LineWidth(Width);
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
            GL.ReadPixels(V0, V1, (uint)V2, (uint)V3, Format, Type, Ptr);
            EnsureNoErrors();
        }

        public void ShaderSource(int V0, string Source)
        {
            GL.ShaderSource(V0, Source);
            EnsureNoErrors();
        }

        public unsafe void TexImage2D(TextureTarget Target, int V0, InternalFormat InternalFormat, uint V1, uint V2, int V3, PixelFormat Format, PixelType Type, IntPtr Ptr)
        {
            GL.TexImage2D(Target, V0, (int)InternalFormat, V1, V2, V3, Format, Type, Ptr.ToPointer());
            EnsureNoErrors();
        }

        public void TexImage2DMultisample(EXT.TextureTarget Target, uint Samples, EXT.InternalFormat InternalFormat,
            uint Width, uint Height, bool FixedLocations)
        {
            EXT.GL.TexImage2DMultisample(Target, Samples, InternalFormat, Width, Height, FixedLocations);
            EnsureNoErrors();
        }

        public unsafe void TexImage3D<T>(TextureTarget Target, int V0, InternalFormat InternalFormat, int V1, int V2, int V3, int V4,
            PixelFormat Format, PixelType Type, IntPtr Data) where T : struct
        {
            GL.TexImage3D(Target, V0, (int)InternalFormat, V1, V2, V3, V4, Format, Type, Data.ToPointer());
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

        public void Uniform2(int Location, Vector2 Uniform)
        {
            GL.Uniform2(Location, 1, ref Uniform);
            EnsureNoErrors();
        }

        public void Uniform3(int Location, Vector3 Uniform)
        {
            GL.Uniform3(Location, 1, ref Uniform);
            EnsureNoErrors();
        }

        public void Uniform4(int Location, Vector4 Uniform)
        {
            GL.Uniform4(Location, 1, ref Uniform);
            EnsureNoErrors();
        }

        public void UniformMatrix2(int Location, bool Transpose, ref Matrix2 Uniform)
        {
            GL.UniformMatrix2(Location, 1, Transpose, ref Uniform);
            EnsureNoErrors();
        }

        public void UniformMatrix3(int Location, bool Transpose, ref Matrix3 Uniform)
        {
            GL.UniformMatrix3(Location, 1, Transpose, ref Uniform);
            EnsureNoErrors();
        }

        public void UniformMatrix4(int Location, bool Transpose, ref Matrix4 Uniform)
        {
            GL.UniformMatrix4(Location, 1, Transpose, ref Uniform);
            EnsureNoErrors();
        }

        public void UseProgram(uint Program)
        {
            GL.UseProgram(Program);
            EnsureNoErrors();
        }

        public void VertexAttribDivisor(uint V0, uint V1)
        {
            GL.VertexAttribDivisor(V0, V1);
            EnsureNoErrors();
        }

        public unsafe void VertexAttribPointer(uint V0, int V1, VertexAttribPointerType Type, bool Flag, uint Bytes, IntPtr Ptr)
        {
            GL.VertexAttribPointer(V0, V1, Type, Flag, Bytes, Ptr.ToPointer());
            EnsureNoErrors();
        }
        
        public void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, int V2)
        {
            GL.VertexAttribPointer((uint)V0, V1, Type, Flag, Bytes, V2);
            EnsureNoErrors();
        }

        public void Viewport(int V0, int V1, uint V2, uint V3)
        {
            GL.Viewport(V0, V1, V2, V3);
            EnsureNoErrors();
        }

        public void GetProgram(uint ShaderId, ProgramPropertyARB ParameterName, out int Value)
        {
            Value = 0;
            GL.GetProgram(ShaderId, ParameterName, ref Value);
            EnsureNoErrors();
        }

        public void GetProgramInfoLog(uint ShaderId, out string Log)
        {
            Log = string.Empty;
            GL.GetProgramInfoLog(ShaderId, Log);
            EnsureNoErrors();
        }

        public void UniformBlockBinding(uint ShaderId, uint Index, uint BindingPoint)
        {
            EXT.GL.UniformBlockBinding(ShaderId, Index, BindingPoint);
            EnsureNoErrors();
        }

        private void EnsureNoErrors()
        {
#if DEBUG
            if(System.Threading.Thread.CurrentThread.ManagedThreadId != Loader.Hedra.MainThreadId)
                throw new ArgumentException($"Invalid GL calls outside of the main thread.");
            var error = GL.GetError();
            if (error != ErrorCodeNoError /*&& ErrorSeverity.Ignore != Severity*/)
            {
                var errorMsg = $"Unexpected OpenGL error: {error} {Environment.NewLine} Stack:{Environment.NewLine}{new StackTrace()}";
                Log.WriteResult(false, errorMsg);
                if (ErrorSeverity.High == Severity) throw new RenderException(errorMsg);
            }
#endif
        }
    }
}
