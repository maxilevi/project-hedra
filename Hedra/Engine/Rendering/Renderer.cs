/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/11/2016
 * Time: 04:10 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of OpenGLStateManager.
	/// </summary>
	public static class Renderer
	{
	    public static IGLProvider Provider { get; set; } = new GLProvider();
		public static int FBOBound { get; set; }
		public static int ShaderBound { get; set; }
	    private static readonly StateManager FboManager;
	    private static readonly StateManager ShaderManager;
	    private static readonly CapHandler CapHandler;
	    private static readonly VertexAttributeHandler VertexAttributeHandler;

        static Renderer()
	    {
	        CapHandler = new CapHandler();
	        VertexAttributeHandler = new VertexAttributeHandler();
            FboManager = new StateManager();
            FboManager.RegisterStateItem( () => FBOBound, O => FBOBound = (int) O);
            ShaderManager = new StateManager();
	        ShaderManager.RegisterStateItem(() => ShaderBound, O => ShaderBound = (int)O);
        }

	    public static void Enable(EnableCap Cap)
	    {
	        CapHandler.Enable(Cap);
        }

	    public static void Disable(EnableCap Cap)
	    {
	        CapHandler.Disable(Cap);
        }

	    public static void EnableVertexAttribArray(uint Index)
	    {
	        VertexAttributeHandler.Enable(Index);
        }

	    public static void DisableVertexAttribArray(uint Index)
	    {
	        VertexAttributeHandler.Disable(Index);
        }

        public static void PushFBO()
	    {
	        FboManager.CaptureState();
	    }

	    public static void PushShader()
	    {
	        ShaderManager.CaptureState();
	    }

	    public static int PopFBO()
	    {
	        FboManager.ReleaseState();
	        return FBOBound;
	    }

	    public static int PopShader()
	    {
	        ShaderManager.ReleaseState();
	        return ShaderBound;
	    }

	    public static void BindShader(int Id)
	    {
	        Provider.UseProgram((uint)Id);
	    }

	    public static void BindFramebuffer(FramebufferTarget Target, int Id)
	    {
	        Provider.BindFramebuffer(Target, (uint)Id);
	    }

        public static void MultiDrawElements(PrimitiveType Type, int[] Counts, DrawElementsType ElementsType, IntPtr[] Offsets, int Length)
	    {
	        CompatibilityManager.MultiDrawElementsMethod(Type, Counts, ElementsType, Offsets, Length);
        }
		
		public static void ActiveTexture(TextureUnit Unit)
        {
            Provider.ActiveTexture(Unit);
        }

        public static void AttachShader(int S0, int S1)
        {
            Provider.AttachShader(S0, S1);
        }

        public static void Begin(PrimitiveType Type)
        {
            Provider.Begin(Type);
        }

        public static void BeginQuery(QueryTarget Target, int V0)
        {
            Provider.BeginQuery(Target, V0);
        }

        public static void BindBuffer(BufferTarget Target, uint V0)
        {
            Provider.BindBuffer(Target, V0);
        }

        public static void BindBufferBase(BufferRangeTarget Target, int V0, int V1)
        {
            Provider.BindBufferBase(Target, V0, V1);
        }

        public static void BindFramebuffer(FramebufferTarget Target, uint Id)
        {
            Provider.BindFramebuffer(Target, Id);
        }

        public static void BindTexture(TextureTarget Target, uint Id)
        {
            Provider.BindTexture(Target, Id);
        }
	    
	    public static void BindTexture(TextureTarget Target, int Id)
	    {
	        Provider.BindTexture(Target, (uint) Id);
	    }

        public static void BindVertexArray(uint Id)
        {
            Provider.BindVertexArray(Id);
        }

        public static void BlendEquation(BlendEquationMode Mode)
        {
            Provider.BlendEquation(Mode);
        }

        public static void BlendFunc(BlendingFactor Src, BlendingFactor Dst)
        {
            Provider.BlendFunc(Src, Dst);
        }

        public static void BlitFramebuffer(int SrcX0, int SrcY0, int SrcX1, int SrcY1, int DstX0, int DstY0, int DstX1, int DstY1,
            ClearBufferMask Mask, BlitFramebufferFilter Filter)
        {
            Provider.BlitFramebuffer(SrcX0, SrcY0, SrcX1, SrcY1, DstX0, DstY0, DstX1, DstY1, Mask, Filter);
        }

        public static void BufferData(BufferTarget Target, IntPtr Size, IntPtr Data, BufferUsageHint Hint)
        {
            Provider.BufferData(Target, Size, Data, Hint);
        }

	    public static void BufferData<T>(BufferTarget Target, IntPtr Size, T[] Data, BufferUsageHint Hint) where T : struct
	    {
	        Provider.BufferData(Target, Size, Data, Hint);
	    }

        public static void BufferSubData(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, IntPtr Ptr1)
        {
            Provider.BufferSubData(Target, Ptr0, Offset, Ptr1);
        }

	    public static void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, ref T Data) where T : struct
	    {
	        Provider.BufferSubData(Target, Ptr0, Offset, ref Data);
        }

	    public static void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, T[] Data) where T : struct
	    {
	        Provider.BufferSubData(Target, Ptr0, Offset, Data);
	    }

        public static FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget Target)
        {
           return  Provider.CheckFramebufferStatus(Target);
        }

        public static void Clear(ClearBufferMask Mask)
        {
            Provider.Clear(Mask);
        }

        public static void ClearColor(Vector4 DrawingColor)
        {
            Provider.ClearColor(DrawingColor);
        }

        public static void Color3(Vector3 Color)
        {
            Provider.Color3(Color);
        }
	    
	    public static void Color3(Vector4 Color)
	    {
	        Provider.Color3(Color.Xyz);
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

        public static void DebugMessageCallback(DebugProc Proc, IntPtr Ptr)
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

        public static void DeleteTexture(uint Texture)
        {
            Provider.DeleteTexture(Texture);
        }

        public static void DeleteTextures(int Count, params uint[] Ids)
        {
            Provider.DeleteTextures(Count, Ids);
        }

	    public static void DeleteTextures(int Count, ref uint Id)
	    {
	        Provider.DeleteTextures(Count, ref Id);
	    }

        public static void DeleteVertexArrays(int Count, params uint[] Ids)
        {
            Provider.DeleteVertexArrays(Count, Ids);
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

        public static void DisableVertexAttribArray(int N)
        {
            Provider.DisableVertexAttribArray(N);
        }

        public static void DrawArrays(PrimitiveType Type, int Offset, int Count)
        {
            Provider.DrawArrays(Type, Offset, Count);
        }

        public static void DrawBuffer(DrawBufferMode Mode)
        {
            Provider.DrawBuffer(Mode);
        }

        public static void DrawBuffers(int N, DrawBuffersEnum[] Enums)
        {
            Provider.DrawBuffers(N, Enums);
        }

        public static void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices)
        {
            Provider.DrawElements(Primitive, Count, Type, Indices);
        }

        public static void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices, int Instancecount)
        {
            Provider.DrawElementsInstanced(Primitive, Count, Type, Indices, Instancecount);
        }

        public static void EnableVertexAttribArray(int Id)
        {
            Provider.EnableVertexAttribArray(Id);
        }

        public static void End()
        {
            Provider.End();
        }

        public static void EndQuery(QueryTarget Target)
        {
            Provider.EndQuery(Target);
        }

        public static void FramebufferTexture(FramebufferTarget Framebuffer, FramebufferAttachment DepthAttachment, uint Id, int V0)
        {
            Provider.FramebufferTexture(Framebuffer, DepthAttachment, Id, V0);
        }

        public static void FramebufferTexture2D(FramebufferTarget Target, FramebufferAttachment Attachment, TextureTarget Textarget, uint Texture, int Level)
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
            return Provider.GenTexture();
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
            Provider.GetActiveUniformBlock((uint) V0, (uint) V1, Parameter, out V3);
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

        public static void LoadMatrix(ref Matrix4 Matrix4)
        {
            Provider.LoadMatrix(ref Matrix4);
        }

        public static void MatrixMode(MatrixMode Mode)
        {
            Provider.MatrixMode(Mode);
        }

        public static void PointSize(float Size)
        {
            Provider.PointSize(Size);
        }

        public static void PolygonMode(MaterialFace Face, PolygonMode Mode)
        {
            Provider.PolygonMode(Face, Mode);
        }

        public static void PopMatrix()
        {
            Provider.PopMatrix();
        }

        public static void PushMatrix()
        {
            Provider.PushMatrix();
        }

        public static void ReadBuffer(ReadBufferMode Mode)
        {
            Provider.ReadBuffer(Mode);
        }

        public static void ReadPixels(int V0, int V1, int V2, int V3, PixelFormat Format, PixelType Type, int[] Pixels)
        {
            Provider.ReadPixels(V0, V1, V2, V3, Format, Type, Pixels);
        }

        public static void Rotate(float Angle, Vector3 Rotation)
        {
            Provider.Rotate(Angle, Rotation);
        }

        public static void Scale(Vector3 Scale)
        {
            Provider.Scale(Scale);
        }

        public static void ShaderSource(int V0, string Source)
        {
            Provider.ShaderSource(V0, Source);
        }

        public static void StencilFunc(StencilFunction Func, int V0, uint Id)
        {
            Provider.StencilFunc(Func, V0, Id);
        }

        public static void StencilMask(uint Mask)
        {
            Provider.StencilMask(Mask);
        }

        public static void StencilOp(StencilOp Fail, StencilOp ZFail, StencilOp ZPass)
        {
            Provider.StencilOp(Fail, ZFail, ZPass);
        }

        public static void TexImage2D(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3,
            PixelFormat Format, PixelType Type, IntPtr Ptr)
        {
            Provider.TexImage2D(Target, V0, InternalFormat, V1, V2, V3, Format, Type, Ptr);
        }

        public static void TexImage2DMultisample(TextureTargetMultisample Target, int Samples, PixelInternalFormat InternalFormat,
            int Width, int Height, bool FixedLocations)
        {
            Provider.TexImage2DMultisample(Target, Samples, InternalFormat, Width, Height, FixedLocations);
        }

        public static void TexImage3D<T>(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3,
            int V4, PixelFormat Format, PixelType Type, T[,,] Data) where T : struct
        {
            Provider.TexImage3D(Target, V0, InternalFormat, V1, V2, V3, V4, Format, Type, Data);
        }

        public static void TexParameter(TextureTarget Target, TextureParameterName Name, int Value)
        {
            Provider.TexParameter(Target, Name, Value);
        }

        public static void Translate(Vector3 Location)
        {
            Provider.Translate(Location);
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

        public static void UniformMatrix2(int Location, bool Transpose, ref Matrix2 Uniform)
        {
            Provider.UniformMatrix2(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix2x3(int Location, bool Transpose, ref Matrix2x3 Uniform)
        {
            Provider.UniformMatrix2x3(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix2x4(int Location, bool Transpose, ref Matrix2x4 Uniform)
        {
            Provider.UniformMatrix2x4(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix3(int Location, bool Transpose, ref Matrix3 Uniform)
        {
            Provider.UniformMatrix3(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix3x2(int Location, bool Transpose, ref Matrix3x2 Uniform)
        {
            Provider.UniformMatrix3x2(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix3x4(int Location, bool Transpose, ref Matrix3x4 Uniform)
        {
            Provider.UniformMatrix3x4(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix4(int Location, bool Transpose, ref Matrix4 Uniform)
        {
            Provider.UniformMatrix4(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix4x2(int Location, bool Transpose, ref Matrix4x2 Uniform)
        {
            Provider.UniformMatrix4x2(Location, Transpose, ref Uniform);
        }

        public static void UniformMatrix4x3(int Location, bool Transpose, ref Matrix4x3 Uniform)
        {
            Provider.UniformMatrix4x3(Location, Transpose, ref Uniform);
        }

        public static void UseProgram(int Program)
        {
            Provider.UseProgram((uint)Program);
        }

        public static void Vertex2(Vector2 Vertex)
        {
            Provider.Vertex2(Vertex);
        }

        public static void Vertex3(Vector3 Vertex)
        {
            Provider.Vertex3(Vertex);
        }

	    public static void Vertex3(ref float Vertex)
	    {
	        Provider.Vertex3(ref Vertex);
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
	        int Bytes, int V2)
	    {
	        Provider.VertexAttribPointer(V0, V1, Type, Flag, Bytes, V2);
	    }

        public static void Viewport(int V0, int V1, int V2, int V3)
        {
            Provider.Viewport(V0, V1, V2, V3);
        }
    }
}
