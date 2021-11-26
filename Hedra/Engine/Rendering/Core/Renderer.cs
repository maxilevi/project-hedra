/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/11/2016
 * Time: 04:10 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using Hedra.Engine.Windowing;
using Hedra.Game;
using GLDebugProc = Silk.NET.OpenGL.DebugProc;
using GLDrawBuffersEnum = Silk.NET.OpenGL.GLEnum;


namespace Hedra.Engine.Rendering.Core
{
    public delegate void ShaderChangeEvent();

    public static class Renderer
    {
        static Renderer()
        {
            CapHandler = new CapHandler();
            TextureHandler = new TextureHandler();
            ShaderHandler = new ShaderHandler();
            VertexAttributeHandler = new VertexAttributeHandler();
            BufferHandler = new BufferHandler();
            FramebufferHandler = new FramebufferHandler();
        }

        public static IGLProvider Provider { get; set; }

        public static uint ShaderBound => ShaderHandler.Id;
        public static uint FBOBound => FramebufferHandler.Id;
        public static uint VAOBound => VertexAttributeHandler.Id;
        public static uint VBOBound => BufferHandler.Id;
        public static Matrix4x4 ModelViewProjectionMatrix { get; private set; }
        public static Matrix4x4 ModelViewMatrix { get; private set; }
        public static Matrix4x4 ViewMatrix { get; private set; }
        public static Matrix4x4 ProjectionMatrix { get; private set; }
        public static CapHandler CapHandler { get; }
        public static TextureHandler TextureHandler { get; }
        public static ShaderHandler ShaderHandler { get; }
        public static VertexAttributeHandler VertexAttributeHandler { get; }
        public static FramebufferHandler FramebufferHandler { get; }
        public static BufferHandler BufferHandler { get; }

        public static ErrorSeverity Severity
        {
            get => Provider.Severity;
            set => Provider.Severity = value;
        }

        public static event ShaderChangeEvent ShaderChanged;

        public static void LoadProvider()
        {
            Provider = new GLProvider();
        }

        public static void Load()
        {
            BlendEquation(BlendEquationMode.FuncAdd);
            BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            Shader.ShaderChanged += () => ShaderChanged?.Invoke();
        }

        public static void MultiDrawElements(PrimitiveType Type, uint[] Counts, DrawElementsType ElementsType,
            IntPtr[] Offsets, int Length)
        {
#if DEBUG
            DrawAsserter.AssertMultiDrawElement(Type, Counts, ElementsType, Offsets, Length);
#endif
            CompatibilityManager.MultiDrawElementsMethod(Type, Counts, ElementsType, Offsets, Length);
        }

        public static void DrawArrays(PrimitiveType Type, int Offset, int Count)
        {
#if DEBUG
            DrawAsserter.AssertDrawArrays(Type, Offset, Count);
#endif
            Provider.DrawArrays(Type, Offset, Count);
        }

        public static void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices)
        {
#if DEBUG
            DrawAsserter.AssertDrawElements(Primitive, Count, Type, Indices);
#endif
            Provider.DrawElements(Primitive, Count, Type, Indices);
        }

        public static void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type,
            IntPtr Indices, int InstanceCount)
        {
#if DEBUG
            DrawAsserter.AssertDrawElementsInstanced(Primitive, Count, Type, Indices, InstanceCount);
#endif
            Provider.DrawElementsInstanced(Primitive, Count, Type, Indices, InstanceCount);
        }

        public static void BindBuffer(BufferTarget Target, uint V0)
        {
            BufferHandler.Bind(Target, V0);
        }

        public static void BindTexture(TextureTarget Target, uint Id)
        {
            TextureHandler.Bind(Target, Id);
        }

        public static void BindVAO(uint Id)
        {
            VertexAttributeHandler.Bind(Id);
        }

        public static void BindFramebuffer(FramebufferTarget Target, uint Id)
        {
            FramebufferHandler.Bind(Target, Id);
        }

        public static void Enable(EnableCap Cap)
        {
            CapHandler.Enable(Cap);
        }

        public static void Disable(EnableCap Cap)
        {
            CapHandler.Disable(Cap);
        }

        public static void GetProgram(int ShaderId, GetProgramParameterName ParameterName, out int Value)
        {
            Provider.GetProgram(ShaderId, ParameterName, out Value);
        }

        public static void GetProgramInfoLog(int ShaderId, out string Log)
        {
            Provider.GetProgramInfoLog(ShaderId, out Log);
        }

        public static void UniformBlockBinding(int ShaderId, int Index, int BindingPoint)
        {
            Provider.UniformBlockBinding(ShaderId, Index, BindingPoint);
        }

        public static void EnableVertexAttribArray(uint Index)
        {
            VertexAttributeHandler.Enable(Index);
        }

        public static void DisableVertexAttribArray(uint Index)
        {
            VertexAttributeHandler.Disable(Index);
        }

        public static void BindShader(uint Id)
        {
            ShaderHandler.Use(Id);
        }

        public static void ActiveTexture(TextureUnit Unit)
        {
            TextureHandler.Active(Unit);
        }

        public static void LoadProjection(Matrix4x4 Projection)
        {
            ProjectionMatrix = Projection;
            RebuildMVP();
        }

        public static void LoadModelView(Matrix4x4 ModelView)
        {
            ModelViewMatrix = ModelView;
            ViewMatrix = GameManager.Player.View.ModelViewMatrix; //ModelView.ClearTranslation();
            RebuildMVP();
        }

        private static void RebuildMVP()
        {
            ModelViewProjectionMatrix = ModelViewMatrix * ProjectionMatrix;
        }

        public static void DrawBuffer(DrawBufferMode Mode)
        {
            Provider.DrawBuffer(Mode);
        }

        public static void DrawBuffers(int N, GLDrawBuffersEnum[] Enums)
        {
            Provider.DrawBuffers(N, Enums);
        }

        public static void AttachShader(int S0, int S1)
        {
            Provider.AttachShader(S0, S1);
        }

        public static void BeginQuery(QueryTarget Target, int V0)
        {
            Provider.BeginQuery(Target, V0);
        }

        public static void BindBufferBase(BufferRangeTarget Target, int V0, int V1)
        {
            Provider.BindBufferBase(Target, V0, V1);
        }

        public static void BlendEquation(BlendEquationMode Mode)
        {
            Provider.BlendEquation(Mode);
        }

        public static void BlendFunc(BlendingFactor Src, BlendingFactor Dst)
        {
            Provider.BlendFunc(Src, Dst);
        }

        public static void BufferData(BufferTarget Target, IntPtr Size, IntPtr Data, BufferUsageHint Hint)
        {
            Provider.BufferData(Target, Size, Data, Hint);
        }

        public static void BufferData<T>(BufferTarget Target, IntPtr Size, T[] Data, BufferUsageHint Hint)
            where T : unmanaged
        {
            Provider.BufferData(Target, Size, Data, Hint);
        }

        public static void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, ref T Data)
            where T : unmanaged
        {
            Provider.BufferSubData(Target, Ptr0, Offset, ref Data);
        }

        public static void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, T[] Data)
            where T : unmanaged
        {
            Provider.BufferSubData(Target, Ptr0, Offset, Data);
        }

        public static void BufferSubData(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, IntPtr Data)
        {
            Provider.BufferSubData(Target, Ptr0, Offset, Data);
        }

        public static FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget Target)
        {
            return Provider.CheckFramebufferStatus(Target);
        }

        public static void Clear(ClearBufferMask Mask)
        {
            Provider.Clear(Mask);
        }

        public static void ClearColor(Vector4 DrawingColor)
        {
            Provider.ClearColor(DrawingColor);
        }

        public static void ColorMask(bool B0, bool B1, bool B2, bool B3)
        {
            Provider.ColorMask(B0, B1, B2, B3);
        }

        public static void CompileShader(int Program)
        {
            Provider.CompileShader((uint)Program);
        }

        public static int CreateProgram()
        {
            return Provider.CreateProgram();
        }

        public static int CreateShader(ShaderType Type)
        {
            return Provider.CreateShader(Type);
        }

        public static void CullFace(CullFaceMode Mode)
        {
            Provider.CullFace(Mode);
        }

        public static void DebugMessageCallback(GLDebugProc Proc, IntPtr Ptr)
        {
            Provider.DebugMessageCallback(Proc, Ptr);
        }

        public static void DeleteBuffers(int N, ref uint Id)
        {
            Provider.DeleteBuffers(N, ref Id);
        }

        public static void DeleteFramebuffers(int N, params uint[] Ids)
        {
            Provider.DeleteFramebuffers(N, Ids);
        }

        public static void DeleteProgram(int Program)
        {
            Provider.DeleteProgram((uint)Program);
        }

        public static void DeleteQuery(int Query)
        {
            Provider.DeleteQuery((uint)Query);
        }

        public static void DeleteShader(int Program)
        {
            Provider.DeleteShader((uint)Program);
        }

        public static void DeleteVertexArrays(int Count, ref uint Id)
        {
            Provider.DeleteVertexArrays(Count, ref Id);
        }

        public static void DepthMask(bool Flag)
        {
            Provider.DepthMask(Flag);
        }

        public static void DetachShader(int V0, int V1)
        {
            Provider.DetachShader((uint)V0, (uint)V1);
        }

        public static void EndQuery(QueryTarget Target)
        {
            Provider.EndQuery(Target);
        }

        public static void FramebufferTexture(FramebufferTarget Framebuffer, FramebufferAttachment DepthAttachment,
            uint Id, int V0)
        {
            Provider.FramebufferTexture(Framebuffer, DepthAttachment, Id, V0);
        }

        public static void FramebufferTexture2D(FramebufferTarget Target, FramebufferAttachment Attachment,
            TextureTarget Textarget, uint Texture, int Level)
        {
            Provider.FramebufferTexture2D(Target, Attachment, Textarget, Texture, Level);
        }

        public static void GenBuffers(int N, out uint V1)
        {
            Provider.GenBuffers(N, out V1);
        }

        public static int GenFramebuffer()
        {
            return Provider.GenFramebuffer();
        }

        public static int GenQuery()
        {
            return Provider.GenQuery();
        }

        public static uint GenTexture()
        {
            return TextureHandler.Create();
        }

        public static void GenVertexArrays(int N, out uint V1)
        {
            Provider.GenVertexArrays(N, out V1);
        }

        public static void GenerateMipmap(GenerateMipmapTarget Target)
        {
            Provider.GenerateMipmap(Target);
        }

        public static void GetActiveUniformBlock(int V0, int V1, ActiveUniformBlockParameter Parameter, out int V3)
        {
            Provider.GetActiveUniformBlock((uint)V0, (uint)V1, Parameter, out V3);
        }

        public static ErrorCode GetError()
        {
            return Provider.GetError();
        }

        public static int GetInteger(GetPName PName)
        {
            return Provider.GetInteger(PName);
        }

        public static void GetQueryObject(int QueryObject, GetQueryObjectParam Parameter, out int Value)
        {
            Provider.GetQueryObject((uint)QueryObject, Parameter, out Value);
        }

        public static void GetShader(int Program, ShaderParameter Parameter, out int Value)
        {
            Provider.GetShader((uint)Program, Parameter, out Value);
        }

        public static string GetShaderInfoLog(int Id)
        {
            return Provider.GetShaderInfoLog(Id);
        }

        public static string GetString(StringName Name)
        {
            return Provider.GetString(Name);
        }

        public static int GetUniformBlockIndex(int V0, string Name)
        {
            return Provider.GetUniformBlockIndex((uint)V0, Name);
        }

        public static int GetUniformLocation(int Program, string Name)
        {
            return Provider.GetUniformLocation((uint)Program, Name);
        }

        public static void LinkProgram(int Program)
        {
            Provider.LinkProgram((uint)Program);
        }

        public static void PointSize(float Size)
        {
            Provider.PointSize(Size);
        }

        public static void LineWidth(float Width)
        {
            Provider.LineWidth(Width);
        }

        public static void PolygonMode(MaterialFace Face, PolygonMode Mode)
        {
            Provider.PolygonMode(Face, Mode);
        }

        public static void ReadBuffer(ReadBufferMode Mode)
        {
            Provider.ReadBuffer(Mode);
        }

        public static void ReadPixels(int V0, int V1, int V2, int V3, PixelFormat Format, PixelType Type, byte[] Pixels)
        {
            Provider.ReadPixels(V0, V1, V2, V3, Format, Type, Pixels);
        }

        public static void ShaderSource(int V0, string Source)
        {
            Provider.ShaderSource(V0, Source);
        }

        public static void TexImage2D(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2,
            int V3,
            PixelFormat Format, PixelType Type, IntPtr Ptr)
        {
            Provider.TexImage2D(Target, V0, InternalFormat, V1, V2, V3, Format, Type, Ptr);
        }

        public static void TexImage2DMultisample(TextureTargetMultisample Target, int Samples,
            PixelInternalFormat InternalFormat,
            int Width, int Height, bool FixedLocations)
        {
            Provider.TexImage2DMultisample(Target, Samples, InternalFormat, Width, Height, FixedLocations);
        }

        public static void TexImage3D<T>(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1,
            int V2, int V3,
            int V4, PixelFormat Format, PixelType Type, T[] Pixels) where T : unmanaged
        {
            Provider.TexImage3D(Target, V0, InternalFormat, V1, V2, V3, V4, Format, Type, Pixels);
        }

        public static void TexParameter(TextureTarget Target, TextureParameterName Name, int Value)
        {
            Provider.TexParameter(Target, Name, Value);
        }

        public static void Uniform1(int Location, int Uniform)
        {
            Provider.Uniform1(Location, Uniform);
        }

        public static void Uniform1(int Location, float Uniform)
        {
            Provider.Uniform1(Location, Uniform);
        }

        public static void Uniform1(int Location, double Uniform)
        {
            Provider.Uniform1(Location, Uniform);
        }

        public static void Uniform2(int Location, Vector2 Uniform)
        {
            Provider.Uniform2(Location, Uniform);
        }

        public static void Uniform3(int Location, Vector3 Uniform)
        {
            Provider.Uniform3(Location, Uniform);
        }

        public static void Uniform4(int Location, Vector4 Uniform)
        {
            Provider.Uniform4(Location, Uniform);
        }

        public static void UniformMatrix2(int Location, bool Transpose, ref Matrix4x4 Uniform)
        {
            Provider.UniformMatrix2(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix3(int Location, bool Transpose, ref Matrix4x4 Uniform)
        {
            Provider.UniformMatrix3(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix4x4(int Location, bool Transpose, ref Matrix4x4 Uniform)
        {
            Provider.UniformMatrix4x4(Location, Transpose, ref Uniform);
        }

        public static void VertexAttribDivisor(int V0, int V1)
        {
            Provider.VertexAttribDivisor(V0, V1);
        }

        public static void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag,
            int Bytes, IntPtr Ptr)
        {
            Provider.VertexAttribPointer(V0, V1, Type, Flag, Bytes, Ptr);
        }

        public static void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag,
            int Bytes)
        {
            Provider.VertexAttribPointer(V0, V1, Type, Flag, Bytes);
        }

        public static void Viewport(int V0, int V1, int V2, int V3)
        {
            Provider.Viewport(V0, V1, V2, V3);
        }
    }
}