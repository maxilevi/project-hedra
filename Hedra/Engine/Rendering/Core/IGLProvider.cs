using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering.Core
{
    public interface IGLProvider
    {
        ErrorSeverity Severity { get; set; }
        void ActiveTexture(TextureUnit Unit);
        void AttachShader(int S0, int S1);
        void BeginQuery(QueryTarget Target, int V0);
        void BindBuffer(BufferTarget Target, uint V0);
        void BindBufferBase(BufferRangeTarget Target, int V0, int V1);
        void BindFramebuffer(FramebufferTarget Target, uint Id);
        void BindTexture(TextureTarget Target, uint Id);
        void BindVertexArray(uint Id);
        void BlendEquation(BlendEquationMode Mode);
        void BlendFunc(BlendingFactor Src, BlendingFactor Dst);
        void BlitFramebuffer(int SrcX0, int SrcY0, int SrcX1, int SrcY1, int DstX0, int DstY0, int DstX1, int DstY1,
            ClearBufferMask Mask, BlitFramebufferFilter Filter);
        void BufferData(BufferTarget Target, IntPtr Size, IntPtr Ptr, BufferUsageHint Hint);
        void BufferData<T>(BufferTarget Target, IntPtr Size, T[] Data, BufferUsageHint Hint) where T : struct;
        void BufferSubData(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, IntPtr Ptr1);
        void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, ref T Data) where T : struct;
        void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, T[] Data) where T : struct;
        FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget Target);
        void Clear(ClearBufferMask Mask);
        void ClearColor(Vector4 DrawingColor);
        void ColorMask(bool B0, bool B1, bool B2, bool B3);
        void CompileShader(uint Program);
        int CreateProgram();
        int CreateShader(ShaderType Type);
        void CullFace(CullFaceMode Mode);
        void DebugMessageCallback(DebugProc Proc, IntPtr Ptr);
        void DeleteBuffers(int N, ref uint Id);
        void DeleteFramebuffers(int N, params uint[] Ids);
        void DeleteProgram(uint Program);
        void DeleteQuery(uint Query);
        void DeleteShader(uint Program);
        void DeleteTexture(uint Texture);
        void DeleteTextures(int N, params uint[] Ids);
        void DeleteVertexArrays(int N, ref uint Id);
        void DepthMask(bool Flag);
        void DetachShader(uint V0, uint V1);
        void Disable(EnableCap Cap);
        void DisableVertexAttribArray(uint N);
        void DrawArrays(PrimitiveType Type, int Offset, int Count);
        void DrawBuffer(DrawBufferMode Mode);
        void DrawBuffers(int N, DrawBuffersEnum[] Enums);
        void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices);
        void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices, int Instancecount);
        void Enable(EnableCap Cap);
        void EnableVertexAttribArray(uint Id);
        void EndQuery(QueryTarget Target);
        void FramebufferTexture(FramebufferTarget Framebuffer, FramebufferAttachment DepthAttachment, uint Id, int V0);
        void FramebufferTexture2D(FramebufferTarget Target, FramebufferAttachment Attachment, TextureTarget Textarget, uint Texture, int Level);
        void GenBuffers(int N, out uint V1);
        int GenFramebuffer();
        int GenQuery();
        uint GenTexture();
        void GenVertexArrays(int N, out uint V1);
        void GenerateMipmap(GenerateMipmapTarget Target);
        void GetActiveUniformBlock(uint V0, uint V1, ActiveUniformBlockParameter Parameter, out int V3);
        ErrorCode GetError();
        int GetInteger(GetPName PName);
        void GetInteger(GetPName PName, out int Value);
        void GetQueryObject(uint Program, GetQueryObjectParam Parameter, out int Value);
        void GetShader(uint Program, ShaderParameter Parameter, out int Value);
        string GetShaderInfoLog(int Id);
        string GetString(StringName Name);
        int GetUniformBlockIndex(uint V0, string Name);
        int GetUniformLocation(uint Program, string Name);
        void LinkProgram(uint Program);
        void MultiDrawElements(PrimitiveType Primitive, int[] Counts, DrawElementsType Type, IntPtr[] Offsets, int Count);
        void PointSize(float Size);
        void LineWidth(float Width);
        void PolygonMode(MaterialFace Face, PolygonMode Mode);
        void ReadBuffer(ReadBufferMode Mode);
        void ReadPixels(int V0, int V1, int V2, int V3, PixelFormat Format, PixelType Type, int[] Pixels);
        void ShaderSource(int V0, string Source);
        void TexImage2D(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3,
            PixelFormat Format, PixelType Type, IntPtr Ptr);
        void TexImage2DMultisample(TextureTargetMultisample Target, int Samples, PixelInternalFormat InternalFormat,
            int Width, int Height, bool FixedLocations);
        void TexImage3D<T>(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3,
            int V4, PixelFormat Format, PixelType Type, T[,,] Data) where T : struct;
        void TexParameter(TextureTarget Target, TextureParameterName Name, int Value);
        void Uniform1(int Location, int Uniform);
        void Uniform1(int Location, float Uniform);
        void Uniform1(int Location, double Uniform);
        void Uniform2(int Location, Vector2 Uniform);
        void Uniform3(int Location, Vector3 Uniform);
        void Uniform4(int Location, Vector4 Uniform);
        void UniformMatrix2(int Location, bool Transpose, ref Matrix2 Uniform);
        void UniformMatrix3(int Location, bool Transpose, ref Matrix3 Uniform);
        void UniformMatrix4(int Location, bool Transpose, ref Matrix4 Uniform);
        void UseProgram(uint Program);
        void VertexAttribDivisor(int V0, int V1);
        void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, IntPtr Ptr);
        void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, int V2);
        void Viewport(int V0, int V1, int V2, int V3);
        void GetProgram(int ShaderId, GetProgramParameterName ParameterName, out int Value);
        void GetProgramInfoLog(int ShaderId, out string Log);
        void UniformBlockBinding(int ShaderId, int Index, int BindingPoint);
    }
}