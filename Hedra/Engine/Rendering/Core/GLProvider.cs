using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Hedra.Engine.IO;
using OpenToolkit.Mathematics;
using Hedra.Engine.Core;
using EXT = OpenToolkit.Graphics.EXT;
using GL = OpenToolkit.Graphics.GL33.GL;
#region Typedefs
using GLBufferTarget = OpenToolkit.Graphics.GL33.BufferTargetARB;
using GLTextureUnit = OpenToolkit.Graphics.GL33.TextureUnit;
using GLQueryTarget = OpenToolkit.Graphics.GL33.QueryTarget;
using GLFramebufferTarget = OpenToolkit.Graphics.GL33.FramebufferTarget;
using GLTextureTarget = OpenToolkit.Graphics.GL33.TextureTarget;
using GLBlendEquationMode = OpenToolkit.Graphics.EXT.BlendEquationModeEXT;
using GLBufferRangeTarget = OpenToolkit.Graphics.EXT.BufferTargetARB;
using GLBlendingFactor = OpenToolkit.Graphics.GL33.BlendingFactor;
using GLBufferUsageHint = OpenToolkit.Graphics.GL33.BufferUsageARB;
using GLClearBufferMask = OpenToolkit.Graphics.GL33.ClearBufferMask;
using GLBlitFramebufferFilter = OpenToolkit.Graphics.EXT.BlitFramebufferFilter;
using GLFramebufferErrorCode = OpenToolkit.Graphics.EXT.FramebufferStatus;
using GLShaderType = OpenToolkit.Graphics.GL33.ShaderType;
using GLCullFaceMode = OpenToolkit.Graphics.GL33.CullFaceMode;
using GLDebugProc = OpenToolkit.Graphics.GL.DebugProc;
using GLEnableCap = OpenToolkit.Graphics.GL33.EnableCap;
using GLGetPName = OpenToolkit.Graphics.GL33.GetPName;
using GLVertexAttribPointerType = OpenToolkit.Graphics.GL33.VertexAttribPointerType;
using GLGetProgramParameterName = OpenToolkit.Graphics.GL33.ProgramPropertyARB;
using GLPixelInternalFormat = OpenToolkit.Graphics.GL33.InternalFormat;
using GLPixelInternalFormatEXT = OpenToolkit.Graphics.EXT.InternalFormat;
using GLTextureParameterName = OpenToolkit.Graphics.GL33.TextureParameterName;
using GLPixelType = OpenToolkit.Graphics.GL33.PixelType;
using GLPixelFormat = OpenToolkit.Graphics.GL33.PixelFormat;
using GLTextureTargetMultisample = OpenToolkit.Graphics.EXT.TextureTarget;
using GLMaterialFace = OpenToolkit.Graphics.GL33.MaterialFace;
using GLPolygonMode = OpenToolkit.Graphics.GL33.PolygonMode;
using GLPrimitiveType = OpenToolkit.Graphics.GL33.PrimitiveType;
using GLDrawElementsType = OpenToolkit.Graphics.GL33.DrawElementsType;
using GLDrawBuffersEnum = OpenToolkit.Graphics.GL33.DrawBufferMode;
using GLDrawBufferMode = OpenToolkit.Graphics.GL33.DrawBufferMode;
using GLFramebufferAttachment = OpenToolkit.Graphics.GL33.FramebufferAttachment;
using GLGenerateMipmapTarget = OpenToolkit.Graphics.EXT.TextureTarget;
using GLErrorCode = OpenToolkit.Graphics.GL33.ErrorCode;
using GLActiveUniformBlockParameter = OpenToolkit.Graphics.EXT.UniformBlockPName;
using GLReadBufferMode = OpenToolkit.Graphics.GL33.ReadBufferMode;
using GLStringName = OpenToolkit.Graphics.GL33.StringName;
using GLShaderParameter = OpenToolkit.Graphics.GL33.ShaderParameterName;
using GLGetQueryObjectParam = OpenToolkit.Graphics.GL33.QueryObjectParameterName;
using GLFramebufferTargetEXT = OpenToolkit.Graphics.EXT.FramebufferTarget;
using GLTextureTargetEXT = OpenToolkit.Graphics.EXT.TextureTarget;
using GLFramebufferAttachmentEXT = OpenToolkit.Graphics.EXT.FramebufferAttachment;
#endregion


namespace Hedra.Engine.Rendering.Core
{
    public class GLProvider : IGLProvider
    {
        public ErrorSeverity Severity { get; set; }
        
        public void ActiveTexture(TextureUnit Unit)
        {
            GL.ActiveTexture((GLTextureUnit)Unit);
            EnsureNoErrors();
        }

        public void AttachShader(int S0, int S1)
        {
            GL.AttachShader((uint)S0, (uint)S1);
            EnsureNoErrors();
        }

        public void BeginQuery(QueryTarget Target, int V0)
        {
            GL.BeginQuery((GLQueryTarget)Target, (uint)V0);
            EnsureNoErrors();
        }

        public void BindBuffer(BufferTarget Target, uint V0)
        {
            GL.BindBuffer((GLBufferTarget)Target, V0);
            EnsureNoErrors();
        }

        public void BindBufferBase(BufferRangeTarget Target, int V0, int V1)
        {
            EXT.GL.BindBufferBase((GLBufferRangeTarget)Target, (uint)V0, (uint)V1);
            EnsureNoErrors();
        }

        public void BindFramebuffer(FramebufferTarget Target, uint Id)
        {
            EXT.GL.BindFramebuffer((GLFramebufferTarget)Target, Id);
            EnsureNoErrors();
        }

        public void BindTexture(TextureTarget Target, uint Id)
        {
            GL.BindTexture((GLTextureTarget)Target, Id);
            EnsureNoErrors();
        }

        public void BindVertexArray(uint Id)
        {
            EXT.GL.BindVertexArray(Id);
            EnsureNoErrors();
        }

        public void BlendEquation(BlendEquationMode Mode)
        {
            EXT.GL.BlendEquation((GLBlendEquationMode)Mode);
            EnsureNoErrors();
        }

        public void BlendFunc(BlendingFactor Src, BlendingFactor Dst)
        {
            GL.BlendFunc((GLBlendingFactor)Src, (GLBlendingFactor)Dst);
            EnsureNoErrors();
        }

        public void BlitFramebuffer(int SrcX0, int SrcY0, int SrcX1, int SrcY1, int DstX0, int DstY0, int DstX1, int DstY1,
            ClearBufferMask Mask, BlitFramebufferFilter Filter)
        {
            EXT.GL.BlitFramebuffer(SrcX0, SrcY0, SrcX1, SrcY1, DstX0, DstY0, DstX1, DstY1, (uint)Mask, (GLBlitFramebufferFilter)Filter);
            EnsureNoErrors();
        }

        public unsafe void BufferData(BufferTarget Target, IntPtr Size, IntPtr Offset, BufferUsageHint Hint)
        {
            GL.BufferData((GLBufferTarget)Target, (UIntPtr)Size.ToPointer(), Offset.ToPointer(), (GLBufferUsageHint)Hint);
            EnsureNoErrors();
        }

        public unsafe void BufferData<T>(BufferTarget Target, IntPtr Size, T[] Data, BufferUsageHint Hint) where T : struct
        {
            GL.BufferData((GLBufferTarget)Target, (UIntPtr)Size.ToPointer(), Data, (GLBufferUsageHint)Hint);
            EnsureNoErrors();
        }

        public unsafe void BufferSubData(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, IntPtr Ptr1)
        {
            GL.BufferSubData((GLBufferTarget)Target, Ptr0, (UIntPtr)Offset.ToPointer(), Ptr1.ToPointer());
            EnsureNoErrors();
        }

        public unsafe void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, ref T Data) where T : struct
        {
            GL.BufferSubData((GLBufferTarget)Target, Ptr0, (UIntPtr)Offset.ToPointer(), ref Data);
            EnsureNoErrors();
        }

        public unsafe void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, T[] Data) where T : struct
        {
            GL.BufferSubData((GLBufferTarget)Target, Ptr0, (UIntPtr)Offset.ToPointer(), Data);
            EnsureNoErrors();
        }

        public FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget Target)
        {
            try
            {
                return (FramebufferErrorCode)EXT.GL.CheckFramebufferStatusEXT((GLFramebufferTarget)Target);
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

        public int CreateShader(ShaderType Type)
        {
            try
            {
                return (int)GL.CreateShader((GLShaderType)Type);
            }
            finally
            {
                EnsureNoErrors();
            }
        }

        public void CullFace(CullFaceMode Mode)
        {
            GL.CullFace((GLCullFaceMode)Mode);
            EnsureNoErrors();
        }

        public unsafe void DebugMessageCallback(GLDebugProc Proc, IntPtr Ptr)
        {
            EXT.GL.DebugMessageCallback(Proc, Ptr.ToPointer());
            EnsureNoErrors();
        }

        public void DeleteBuffers(int N, ref uint Id)
        {
            GL.DeleteBuffers((uint)N, ref Id);
            EnsureNoErrors();
        }

        public void DeleteFramebuffers(int N, params uint[] Ids)
        {
            EXT.GL.DeleteFramebuffers((uint)N, Ids);
            EnsureNoErrors();
        }

        public void DeleteProgram(uint Program)
        {
            GL.DeleteProgram(Program);
            EnsureNoErrors();
        }

        public void DeleteQuery(uint Query)
        {
            EXT.GL.DeleteQueriesEXT(1, ref Query);
            EnsureNoErrors();
        }

        public void DeleteShader(uint Program)
        {
            GL.DeleteShader(Program);
            EnsureNoErrors();
        }

        public void DeleteTexture(uint Texture)
        {
            EXT.GL.DeleteTexturesEXT(1, ref Texture);
            EnsureNoErrors();
        }

        public void DeleteTextures(int N, params uint[] Ids)
        {
            EXT.GL.DeleteTexturesEXT((uint)N, Ids);
            EnsureNoErrors();
        }

        public void DeleteVertexArrays(int N, params uint[] Ids)
        {
            EXT.GL.DeleteVertexArrays((uint)N, Ids);
            EnsureNoErrors();
        }

        public void DeleteTextures(int N, ref uint Id)
        {
            GL.DeleteTextures((uint)N, ref Id);
            EnsureNoErrors();
        }

        public void DeleteVertexArrays(int N, ref uint Id)
        {
            EXT.GL.DeleteVertexArrays((uint)N, ref Id);
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
            GL.Disable((GLEnableCap)Cap);
            EnsureNoErrors();
        }

        public void DisableVertexAttribArray(uint N)
        {
            GL.DisableVertexAttribArray(N);
            EnsureNoErrors();
        }

        public void DrawArrays(PrimitiveType Type, int Offset, int Count)
        {
            GL.DrawArrays((GLPrimitiveType)Type, Offset, (uint)Count);
            EnsureNoErrors();
        }

        public void DrawBuffer(DrawBufferMode Mode)
        {
            GL.DrawBuffer((GLDrawBufferMode)Mode);
            EnsureNoErrors();
        }

        public void DrawBuffers(int N, GLDrawBuffersEnum[] Enums)
        {
            GL.DrawBuffers((uint)N, Enums);
            EnsureNoErrors();
        }

        public unsafe void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices)
        {
            GL.DrawElements((GLPrimitiveType)Primitive, (uint)Count, (GLDrawElementsType)Type, Indices.ToPointer());
            EnsureNoErrors();
        }

        public unsafe void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices, int Instancecount)
        {
            GL.DrawElementsInstanced((GLPrimitiveType)Primitive, (uint)Count, (GLDrawElementsType)Type, Indices.ToPointer(), (uint)Instancecount);
            EnsureNoErrors();
        }

        public void Enable(EnableCap Cap)
        {
            GL.Enable((GLEnableCap)Cap);
            EnsureNoErrors();
        }

        public void EnableVertexAttribArray(uint Id)
        {
            GL.EnableVertexAttribArray(Id);
            EnsureNoErrors();
        }

        public void EndQuery(QueryTarget Target)
        {
            GL.EndQuery((GLQueryTarget)Target);
            EnsureNoErrors();
        }

        public void FramebufferTexture(FramebufferTarget Framebuffer, FramebufferAttachment DepthAttachment, uint Id, int V0)
        {
            GL.FramebufferTexture((GLFramebufferTarget)Framebuffer, (GLFramebufferAttachment)DepthAttachment, Id, V0);
            EnsureNoErrors();
        }

        public void FramebufferTexture2D(FramebufferTarget Target, FramebufferAttachment Attachment, TextureTarget TextureTarget, uint Texture, int Level)
        {
            EXT.GL.FramebufferTexture2D((GLFramebufferTargetEXT)Target, (GLFramebufferAttachmentEXT)Attachment, (GLTextureTargetEXT)TextureTarget, Texture, Level);
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
                EXT.GL.GenFramebuffers(1, ref id);
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
                GL.GenQueries(1, ref id);
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
                GL.GenTextures(1, ref id);
                return id;
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

        public void GenerateMipmap(GenerateMipmapTarget Target)
        {
            EXT.GL.GenerateMipmap((GLGenerateMipmapTarget)Target);
            EnsureNoErrors();
        }

        public void GetActiveUniformBlock(uint V0, uint V1, ActiveUniformBlockParameter Parameter, out int V3)
        {
            V3 = 0;
            EXT.GL.GetActiveUniformBlock(V0, V1, (GLActiveUniformBlockParameter)Parameter, ref V3);
            EnsureNoErrors();
        }

        public ErrorCode GetError()
        {
            try
            {
                return (ErrorCode)GL.GetError();
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
                var output = 0;
                GL.GetInteger((GLGetPName)PName, ref output);
                return output;
            }
            finally
            {
                EnsureNoErrors();
            }
        }
        
        public void GetInteger(GetPName PName, out int Value)
        {
            Value = 0;
            GL.GetInteger((GLGetPName)PName, ref Value);
            EnsureNoErrors();
        }

        public void GetQueryObject(uint Program, GetQueryObjectParam Parameter, out int Value)
        {
            Value = 0;
            GL.GetQueryObject(Program, (GLGetQueryObjectParam)Parameter, ref Value);
            EnsureNoErrors();
        }

        public void GetShader(uint Program, ShaderParameter Parameter, out int Value)
        {
            Value = 0;
            GL.GetShader(Program, (GLShaderParameter)Parameter, ref Value);
            EnsureNoErrors();
        }

        public string GetShaderInfoLog(int Id)
        {
            try
            {
                var str = string.Empty;
                GL.GetShaderInfoLog((uint)Id, str);
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
                return Marshal.PtrToStringAnsi((IntPtr)GL.GetString((GLStringName)Name));
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
                return (int)EXT.GL.GetUniformBlockIndex((uint)V0, Name);
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

        public void MultiDrawElements(PrimitiveType Primitive, uint[] Counts, DrawElementsType Type, IntPtr[] Offsets, int Count)
        {
            GL.MultiDrawElements((GLPrimitiveType)Primitive, Counts, (GLDrawElementsType)Type, Offsets, (uint)Count);
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
            GL.PolygonMode((GLMaterialFace)Face, (GLPolygonMode)Mode);
            EnsureNoErrors();
        }

        public void ReadBuffer(ReadBufferMode Mode)
        {
            GL.ReadBuffer((GLReadBufferMode)Mode);
            EnsureNoErrors();
        }

        public void ReadPixels(int V0, int V1, int V2, int V3, PixelFormat Format, PixelType Type, int[] Ptr)
        {
            GL.ReadPixels(V0, V1, (uint)V2, (uint)V3, (GLPixelFormat)Format, (GLPixelType)Type, Ptr);
            EnsureNoErrors();
        }

        public void ShaderSource(int V0, string Source)
        {
            GL.ShaderSource(V0, Source);
            EnsureNoErrors();
        }

        public unsafe void TexImage2D(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3,
            PixelFormat Format, PixelType Type, IntPtr Ptr)
        {
            GL.TexImage2D((GLTextureTarget)Target, V0, (int)InternalFormat, (uint)V1, (uint)V2, V3, (GLPixelFormat)Format, (GLPixelType)Type, Ptr.ToPointer());
            EnsureNoErrors();
        }

        public void TexImage2DMultisample(TextureTargetMultisample Target, int Samples, PixelInternalFormat InternalFormat,
            int Width, int Height, bool FixedLocations)
        {
            EXT.GL.TexImage2DMultisample((GLTextureTargetMultisample)Target, (uint)Samples, (GLPixelInternalFormatEXT)InternalFormat, (uint)Width, (uint)Height, FixedLocations);
            EnsureNoErrors();
        }

        public void TexImage3D<T>(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3, int V4,
            PixelFormat Format, PixelType Type, T[,,] Data) where T : struct
        {
            GL.TexImage3D((GLTextureTarget)Target, V0, (int)InternalFormat, V1, V2, V3, V4, Format, Type, Data);
            EnsureNoErrors();
        }

        public void TexParameter(TextureTarget Target, TextureParameterName Name, int Value)
        {
            GL.TexParameter((GLTextureTarget)Target, (GLTextureParameterName)Name, Value);
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
            GL.Uniform1(Location, (float)Uniform);
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

        public void VertexAttribDivisor(int V0, int V1)
        {
            GL.VertexAttribDivisor((uint)V0, (uint)V1);
            EnsureNoErrors();
        }

        public unsafe void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, IntPtr Ptr)
        {
            GL.VertexAttribPointer((uint)V0, V1, (GLVertexAttribPointerType)Type, Flag, (uint)Bytes, Ptr.ToPointer());
            EnsureNoErrors();
        }
        
        public void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, int V2)
        {
            GL.VertexAttribPointer((uint)V0, V1, (GLVertexAttribPointerType)Type, Flag, (uint)Bytes, V2);
            EnsureNoErrors();
        }

        public void Viewport(int V0, int V1, int V2, int V3)
        {
            GL.Viewport(V0, V1, (uint)V2, (uint)V3);
            EnsureNoErrors();
        }

        public void GetProgram(int ShaderId, GetProgramParameterName ParameterName, out int Value)
        {
            Value = 0;
            GL.GetProgram((uint)ShaderId, (GLGetProgramParameterName)ParameterName, ref Value);
            EnsureNoErrors();
        }

        public void GetProgramInfoLog(int ShaderId, out string Log)
        {
            GL.GetProgramInfoLog((uint)ShaderId, out Log);
            EnsureNoErrors();
        }

        public void UniformBlockBinding(int ShaderId, int Index, int BindingPoint)
        {
            EXT.GL.UniformBlockBinding((uint)ShaderId, (uint)Index, (uint)BindingPoint);
            EnsureNoErrors();
        }

        private void EnsureNoErrors()
        {
#if DEBUG
            if(System.Threading.Thread.CurrentThread.ManagedThreadId != Loader.Hedra.MainThreadId)
                throw new ArgumentException($"Invalid GL calls outside of the main thread.");
            var error = GL.GetError();
            if (error != 0 /*&& ErrorSeverity.Ignore != Severity*/)
            {
                var errorMsg = $"Unexpected OpenGL error: {error} {Environment.NewLine} Stack:{Environment.NewLine}{new StackTrace()}";
                Log.WriteResult(false, errorMsg);
                if (ErrorSeverity.High == Severity) throw new RenderException(errorMsg);
            }
#endif
        }
    }
}
