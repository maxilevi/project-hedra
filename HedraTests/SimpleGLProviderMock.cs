using System;
using Hedra.Engine.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace HedraTests
{
    public class SimpleGLProviderMock : IGLProvider
    {
        public virtual void ActiveTexture(TextureUnit Unit)
        {
        }

        public virtual void AttachShader(int S0, int S1)
        {
        }

        public virtual void Begin(PrimitiveType Type)
        {
        }

        public virtual void BeginQuery(QueryTarget Target, int V0)
        {
        }

        public virtual void BindBuffer(BufferTarget Target, uint V0)
        {
        }

        public virtual void BindBufferBase(BufferRangeTarget Target, int V0, int V1)
        {
        }

        public virtual void BindFramebuffer(FramebufferTarget Target, uint Id)
        {
        }

        public virtual void BindTexture(TextureTarget Target, uint Id)
        {
        }

        public virtual void BindVertexArray(uint Id)
        {
        }

        public virtual void BlendEquation(BlendEquationMode Mode)
        {
        }

        public virtual void BlendFunc(BlendingFactor Src, BlendingFactor Dst)
        {
        }

        public virtual void BlitFramebuffer(int SrcX0, int SrcY0, int SrcX1, int SrcY1, int DstX0, int DstY0, int DstX1, int DstY1,
            ClearBufferMask Mask, BlitFramebufferFilter Filter)
        {
        }

        public virtual void BufferData(BufferTarget Target, IntPtr Size, IntPtr Ptr, BufferUsageHint Hint)
        {
        }

        public virtual void BufferData<T>(BufferTarget Target, IntPtr Size, T[] Data, BufferUsageHint Hint) where T : struct
        {
        }

        public virtual void BufferSubData(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, IntPtr Ptr1)
        {
        }

        public virtual void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, ref T Data) where T : struct
        {
        }

        public virtual void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, T[] Data) where T : struct
        {
        }

        public virtual FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget Target)
        {
            return default(FramebufferErrorCode);
        }

        public virtual void Clear(ClearBufferMask Mask)
        {
        }

        public virtual void ClearColor(Vector4 DrawingColor)
        {
        }

        public virtual void Color3(Vector3 Color)
        {
        }

        public virtual void ColorMask(bool B0, bool B1, bool B2, bool B3)
        {
        }

        public virtual void CompileShader(uint Program)
        {
        }

        public virtual int CreateProgram()
        {
            return 0;
        }

        public virtual int CreateShader(ShaderType Type)
        {
            return 0;
        }

        public virtual void CreateTextures(TextureTarget Target, int N, out uint Id)
        {
            Id = 0;
        }

        public virtual void CullFace(CullFaceMode Mode)
        {
        }

        public virtual void DebugMessageCallback(DebugProc Proc, IntPtr Ptr)
        {
        }

        public virtual void DeleteBuffers(int N, ref uint Id)
        {
        }

        public virtual void DeleteFramebuffers(int N, params uint[] Ids)
        {
        }

        public virtual void DeleteProgram(uint Program)
        {
        }

        public virtual void DeleteQuery(uint Query)
        {
        }

        public virtual void DeleteShader(uint Program)
        {
        }

        public virtual void DeleteTexture(uint Texture)
        {
        }

        public virtual void DeleteTextures(int N, params uint[] Ids)
        {
        }

        public virtual void DeleteVertexArrays(int N, params uint[] Ids)
        {
        }

        public virtual void DeleteTextures(int N, ref uint Id)
        {
        }

        public virtual void DeleteVertexArrays(int N, ref uint Id)
        {
        }

        public virtual void DepthMask(bool Flag)
        {
        }

        public virtual void DetachShader(uint V0, uint V1)
        {
        }

        public virtual void Disable(EnableCap Cap)
        {
        }

        public virtual void DisableVertexAttribArray(int N)
        {
        }

        public virtual void DrawArrays(PrimitiveType Type, int Offset, int Count)
        {
        }

        public virtual void DrawBuffer(DrawBufferMode Mode)
        {
        }

        public virtual void DrawBuffers(int N, DrawBuffersEnum[] Enums)
        {
        }

        public virtual void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices)
        {
        }

        public virtual void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices,
            int Instancecount)
        {
        }

        public virtual void Enable(EnableCap Cap)
        {
        }

        public virtual void EnableVertexAttribArray(int Id)
        {
        }

        public virtual void End()
        {
        }

        public virtual void EndQuery(QueryTarget Target)
        {
        }

        public virtual void FramebufferTexture(FramebufferTarget Framebuffer, FramebufferAttachment DepthAttachment, uint Id, int V0)
        {
        }

        public virtual void FramebufferTexture2D(FramebufferTarget Target, FramebufferAttachment Attachment, TextureTarget Textarget,
            uint Texture, int Level)
        {
        }

        public virtual void GenBuffers(int N, out uint V1)
        {
            V1 = 0;
        }

        public virtual int GenFramebuffer()
        {
            return 0;
        }

        public virtual int GenQuery()
        {
            return 0;
        }

        public virtual uint GenTexture()
        {
            return 0;
        }

        public virtual void GenVertexArrays(int N, out uint V1)
        {
            V1 = 0;
        }

        public virtual void GenerateMipmap(GenerateMipmapTarget Target)
        {
        }

        public virtual void GetActiveUniformBlock(uint V0, uint V1, ActiveUniformBlockParameter Parameter, out int V3)
        {
            V3 = 0;
        }

        public virtual ErrorCode GetError()
        {
            return default(ErrorCode);
        }

        public virtual int GetInteger(GetPName PName)
        {
            return 0;
        }

        public virtual void GetQueryObject(uint Program, GetQueryObjectParam Parameter, out int Value)
        {
            Value = 0;
        }

        public virtual void GetShader(uint Program, ShaderParameter Parameter, out int Value)
        {
            Value = 0;
        }

        public virtual string GetShaderInfoLog(int Id)
        {
            return default(string);
        }

        public virtual string GetString(StringName Name)
        {
            return default(string);
        }

        public virtual void GetTexParameter(TextureTarget Target, GetTextureParameter Parameter, out int V0)
        {
            V0 = 0;
        }

        public virtual int GetUniformBlockIndex(uint V0, string Name)
        {
            return 0;
        }

        public virtual int GetUniformLocation(uint Program, string Name)
        {
            return 0;
        }

        public virtual void LinkProgram(uint Program)
        {
        }

        public virtual void LoadMatrix(ref Matrix4 Matrix4)
        {
        }

        public virtual void MatrixMode(MatrixMode Mode)
        {
        }

        public virtual void MultiDrawElements(PrimitiveType Primitive, int[] Counts, DrawElementsType Type, IntPtr[] Offsets, int Count)
        {
        }

        public virtual void PointSize(float Size)
        {
        }

        public virtual void PolygonMode(MaterialFace Face, PolygonMode Mode)
        {
        }

        public virtual void PopMatrix()
        {
        }

        public virtual void PushMatrix()
        {
        }

        public virtual void ReadBuffer(ReadBufferMode Mode)
        {
        }

        public virtual void ReadPixels(int V0, int V1, int V2, int V3, PixelFormat Format, PixelType Type, int[] Pixels)
        {
        }

        public virtual void Rotate(float Angle, Vector3 Rotation)
        {
        }

        public virtual void Scale(Vector3 Scale)
        {
        }

        public virtual void ShaderSource(int V0, string Source)
        {
        }

        public virtual void StencilFunc(StencilFunction Func, int V0, uint Id)
        {
        }

        public virtual void StencilMask(uint Mask)
        {
        }

        public virtual void StencilOp(StencilOp Fail, StencilOp ZFail, StencilOp ZPass)
        {
        }

        public virtual void TexImage2D(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3,
            PixelFormat Format, PixelType Type, IntPtr Ptr)
        {
        }

        public virtual void TexImage2DMultisample(TextureTargetMultisample Target, int Samples, PixelInternalFormat InternalFormat, int Width,
            int Height, bool FixedLocations)
        {
        }

        public virtual void TexImage3D<T>(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3, int V4,
            PixelFormat Format, PixelType Type, T[,,] Data) where T : struct
        {
        }

        public virtual void TexParameter(TextureTarget Target, TextureParameterName Name, int Value)
        {
        }

        public virtual void TexStorage3D(TextureTarget3d Target, int Levels, SizedInternalFormat Internalformat, int Width, int Height,
            int Depth)
        {
        }

        public virtual void Translate(Vector3 Location)
        {
        }

        public virtual void Uniform1(int Location, int Uniform)
        {
        }

        public virtual void Uniform1(int Location, float Uniform)
        {
        }

        public virtual void Uniform1(int Location, double Uniform)
        {
        }

        public virtual void Uniform2(int Location, Vector2 Uniform)
        {
        }

        public virtual void Uniform3(int Location, Vector3 Uniform)
        {
        }

        public virtual void Uniform4(int Location, Vector4 Uniform)
        {
        }

        public virtual void UniformMatrix2(int Location, bool Transpose, ref Matrix2 Uniform)
        {
        }

        public virtual void UniformMatrix2x3(int Location, bool Transpose, ref Matrix2x3 Uniform)
        {
        }

        public virtual void UniformMatrix2x4(int Location, bool Transpose, ref Matrix2x4 Uniform)
        {
        }

        public virtual void UniformMatrix3(int Location, bool Transpose, ref Matrix3 Uniform)
        {
        }

        public virtual void UniformMatrix3x2(int Location, bool Transpose, ref Matrix3x2 Uniform)
        {
        }

        public virtual void UniformMatrix3x4(int Location, bool Transpose, ref Matrix3x4 Uniform)
        {
        }

        public virtual void UniformMatrix4(int Location, bool Transpose, ref Matrix4 Uniform)
        {
        }

        public virtual void UniformMatrix4x2(int Location, bool Transpose, ref Matrix4x2 Uniform)
        {
        }

        public virtual void UniformMatrix4x3(int Location, bool Transpose, ref Matrix4x3 Uniform)
        {
        }

        public virtual void UseProgram(uint Program)
        {
        }

        public virtual void Vertex2(Vector2 Vertex)
        {
        }

        public virtual void Vertex3(Vector3 Vertex)
        {
        }

        public virtual void Vertex3(ref float Vertex)
        {
        }

        public virtual void VertexAttribDivisor(int V0, int V1)
        {
        }

        public virtual void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, IntPtr Ptr)
        {
        }

        public virtual void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, int V2)
        {
        }

        public virtual void Viewport(int V0, int V1, int V2, int V3)
        {
        }
    }
}