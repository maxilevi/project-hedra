using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
    public class GLProvider : IGLProvider
    {
        public void ActiveTexture(TextureUnit Unit)
        {
            GL.ActiveTexture(Unit);
        }

        public void AttachShader(int S0, int S1)
        {
            GL.AttachShader(S0, S1);
        }

        public void Begin(PrimitiveType Type)
        {
            GL.Begin(Type);
        }

        public void BeginQuery(QueryTarget Target, int V0)
        {
            GL.BeginQuery(Target, V0);
        }

        public void BindBuffer(BufferTarget Target, uint V0)
        {
            GL.BindBuffer(Target, V0);
        }

        public void BindBufferBase(BufferRangeTarget Target, int V0, int V1)
        {
            GL.BindBufferBase(Target, V0, V1);
        }

        public void BindFramebuffer(FramebufferTarget Target, uint Id)
        {
            GL.BindFramebuffer(Target, Id);
        }

        public void BindTexture(TextureTarget Target, uint Id)
        {
            GL.BindTexture(Target, Id);
        }

        public void BindVertexArray(uint Id)
        {
            GL.BindVertexArray(Id);
        }

        public void BlendEquation(BlendEquationMode Mode)
        {
            GL.BlendEquation(Mode);
        }

        public void BlendFunc(BlendingFactor Src, BlendingFactor Dst)
        {
            GL.BlendFunc(Src, Dst);
        }

        public void BlitFramebuffer(int SrcX0, int SrcY0, int SrcX1, int SrcY1, int DstX0, int DstY0, int DstX1, int DstY1,
            ClearBufferMask Mask, BlitFramebufferFilter Filter)
        {
            GL.BlitFramebuffer(SrcX0, SrcY0, SrcX1, SrcY1, DstX0, DstY0, DstX1, DstY1, Mask, Filter);
        }

        public void BufferData(BufferTarget Target, IntPtr Size, IntPtr Offset, BufferUsageHint Hint)
        {
            GL.BufferData(Target, Size, Offset, Hint);
        }

        public void BufferData<T>(BufferTarget Target, IntPtr Size, T[] Data, BufferUsageHint Hint) where T : struct
        {
            GL.BufferData(Target, Size, Data, Hint);
        }

        public void BufferSubData(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, IntPtr Ptr1)
        {
            GL.BufferSubData(Target, Ptr0, Offset, Ptr1);
        }

        public void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, ref T Data) where T : struct
        {
            GL.BufferSubData(Target, Ptr0, Offset, ref Data);
        }

        public void BufferSubData<T>(BufferTarget Target, IntPtr Ptr0, IntPtr Offset, T[] Data) where T : struct
        {
            GL.BufferSubData(Target, Ptr0, Offset, Data);
        }

        public FramebufferErrorCode CheckFramebufferStatus(FramebufferTarget Target)
        {
           return  GL.CheckFramebufferStatus(Target);
        }

        public void Clear(ClearBufferMask Mask)
        {
            GL.Clear(Mask);
        }

        public void ClearColor(Vector4 DrawingColor)
        {
            GL.ClearColor(DrawingColor.ToColor());
        }

        public void Color3(Vector3 Color)
        {
            GL.Color3(Color);
        }

        public void ColorMask(bool B0, bool B1, bool B2, bool B3)
        {
            GL.ColorMask(B0, B1, B2, B3);
        }

        public void CompileShader(uint Program)
        {
            GL.CompileShader(Program);
        }

        public int CreateProgram()
        {
            return GL.CreateProgram();
        }

        public int CreateShader(ShaderType Type)
        {
            return GL.CreateShader(Type);
        }

        public void CreateTextures(TextureTarget Target, int N , out uint Id)
        {
            GL.CreateTextures(Target, N , out Id);
        }

        public void CullFace(CullFaceMode Mode)
        {
            GL.CullFace(Mode);
        }

        public void DebugMessageCallback(DebugProc Proc, IntPtr Ptr)
        {
            GL.DebugMessageCallback(Proc, Ptr);
        }

        public void DeleteBuffers(int N, ref uint Id)
        {
            GL.DeleteBuffers(N, ref Id);
        }

        public void DeleteFramebuffers(int N, params uint[] Ids)
        {
            GL.DeleteFramebuffers(N, Ids);
        }

        public void DeleteProgram(uint Program)
        {
            GL.DeleteProgram(Program);
        }

        public void DeleteQuery(uint Query)
        {
            GL.DeleteQuery(Query);
        }

        public void DeleteShader(uint Program)
        {
            GL.DeleteShader(Program);
        }

        public void DeleteTexture(uint Texture)
        {
            GL.DeleteTexture(Texture);
        }

        public void DeleteTextures(int N, params uint[] Ids)
        {
            GL.DeleteTextures(N, Ids);
        }

        public void DeleteVertexArrays(int N, params uint[] Ids)
        {
            GL.DeleteVertexArrays(N, Ids);
        }

        public void DeleteTextures(int N, ref uint Id)
        {
            GL.DeleteTextures(N, ref Id);
        }

        public void DeleteVertexArrays(int N, ref uint Id)
        {
            GL.DeleteVertexArrays(N, ref Id);
        }

        public void DepthMask(bool Flag)
        {
            GL.DepthMask(Flag);
        }

        public void DetachShader(uint V0, uint V1)
        {
            GL.DetachShader(V0, V1);
        }

        public void Disable(EnableCap Cap)
        {
            GL.Disable(Cap);
        }

        public void DisableVertexAttribArray(int N)
        {
            GL.DisableVertexAttribArray(N);
        }

        public void DrawArrays(PrimitiveType Type, int Offset, int Count)
        {
            GL.DrawArrays(Type, Offset, Count);
        }

        public void DrawBuffer(DrawBufferMode Mode)
        {
            GL.DrawBuffer(Mode);
        }

        public void DrawBuffers(int N, DrawBuffersEnum[] Enums)
        {
            GL.DrawBuffers(N, Enums);
        }

        public void DrawElements(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices)
        {
            GL.DrawElements(Primitive, Count, Type, Indices);
        }

        public void DrawElementsInstanced(PrimitiveType Primitive, int Count, DrawElementsType Type, IntPtr Indices, int Instancecount)
        {
            GL.DrawElementsInstanced(Primitive, Count, Type, Indices, Instancecount);
        }

        public void Enable(EnableCap Cap)
        {
            GL.Enable(Cap);
        }

        public void EnableVertexAttribArray(int Id)
        {
            GL.EnableVertexAttribArray(Id);
        }

        public void End()
        {
            GL.End();
        }

        public void EndQuery(QueryTarget Target)
        {
            GL.EndQuery(Target);
        }

        public void FramebufferTexture(FramebufferTarget Framebuffer, FramebufferAttachment DepthAttachment, uint Id, int V0)
        {
            GL.FramebufferTexture(Framebuffer, DepthAttachment, Id, V0);
        }

        public void FramebufferTexture2D(FramebufferTarget Target, FramebufferAttachment Attachment, TextureTarget Textarget, uint Texture, int Level)
        {
            GL.FramebufferTexture2D(Target, Attachment, Textarget, Texture, Level);
        }

        public void GenBuffers(int N, out uint V1)
        {
            GL.GenBuffers(N, out V1);
        }

        public int GenFramebuffer()
        {
            return GL.GenFramebuffer();
        }

        public int GenQuery()
        {
            return GL.GenQuery();
        }

        public int GenTexture()
        {
            return GL.GenTexture();
        }

        public void GenTextures(int Count, uint[] Textures)
        {
            GL.GenTextures(Count, Textures);
        }

        public void GenVertexArrays(int N, out uint V1)
        {
            GL.GenVertexArrays(N, out V1);
        }

        public void GenerateMipmap(GenerateMipmapTarget Target)
        {
            GL.GenerateMipmap(Target);
        }

        public void GetActiveUniformBlock(uint V0, uint V1, ActiveUniformBlockParameter Parameter, out int V3)
        {
            GL.GetActiveUniformBlock(V0, V1, Parameter, out V3);
        }

        public ErrorCode GetError()
        {
            return GL.GetError();
        }

        public int GetInteger(GetPName PName)
        {
            return GL.GetInteger(PName);
        }

        public void GetQueryObject(uint Program, GetQueryObjectParam Parameter, out int Value)
        {
            GL.GetQueryObject(Program, Parameter, out Value);
        }

        public void GetShader(uint Program, ShaderParameter Parameter, out int Value)
        {
            GL.GetShader(Program, Parameter, out Value);
        }

        public string GetShaderInfoLog(int Id)
        {
            return GL.GetShaderInfoLog(Id);
        }

        public string GetString(StringName Name)
        {
            return GL.GetString(Name);
        }

        public void GetTexParameter(TextureTarget Target, GetTextureParameter Parameter, out int V0)
        {
            GL.GetTexParameter(Target, Parameter, out V0);
        }

        public int GetUniformBlockIndex(uint V0, string Name)
        {
            return GL.GetUniformBlockIndex(V0, Name);
        }

        public int GetUniformLocation(uint Program, string Name)
        {
            return GL.GetUniformLocation(Program, Name);
        }

        public void LinkProgram(uint Program)
        {
            GL.LinkProgram(Program);
        }

        public void LoadMatrix(ref Matrix4 Matrix4)
        {
            GL.LoadMatrix(ref Matrix4);
        }

        public void MatrixMode(MatrixMode Mode)
        {
            GL.MatrixMode(Mode);
        }

        public void MultiDrawElements(PrimitiveType Primitive, int[] Counts, DrawElementsType Type, IntPtr[] Offsets, int Count)
        {
            GL.MultiDrawElements(Primitive, Counts, Type, Offsets, Count);
        }

        public void PointSize(float Size)
        {
            GL.PointSize(Size);
        }

        public void PolygonMode(MaterialFace Face, PolygonMode Mode)
        {
            GL.PolygonMode(Face, Mode);
        }

        public void PopMatrix()
        {
            GL.PopMatrix();
        }

        public void PushMatrix()
        {
            GL.PushMatrix();
        }

        public void ReadBuffer(ReadBufferMode Mode)
        {
            GL.ReadBuffer(Mode);
        }

        public void ReadPixels(int V0, int V1, int V2, int V3, PixelFormat Format, PixelType Type, int[] Ptr)
        {
            GL.ReadPixels(V0, V1, V2, V3, Format, Type, Ptr);
        }

        public void Rotate(float Angle, Vector3 Rotation)
        {
            GL.Rotate(Angle, Rotation);
        }

        public void Scale(Vector3 Scale)
        {
            GL.Scale(Scale);
        }

        public void ShaderSource(int V0, string Source)
        {
            GL.ShaderSource(V0, Source);
        }

        public void StencilFunc(StencilFunction Func, int V0, uint Id)
        {
            GL.StencilFunc(Func, V0, Id);
        }

        public void StencilMask(uint Mask)
        {
            GL.StencilMask(Mask);
        }

        public void StencilOp(StencilOp Fail, StencilOp ZFail, StencilOp ZPass)
        {
            GL.StencilOp(Fail, ZFail, ZPass);
        }

        public void TexImage2D(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3,
            PixelFormat Format, PixelType Type, IntPtr Ptr)
        {
            GL.TexImage2D(Target, V0, InternalFormat, V1, V2, V3, Format, Type, Ptr);
        }

        public void TexImage2DMultisample(TextureTargetMultisample Target, int Samples, PixelInternalFormat InternalFormat,
            int Width, int Height, bool FixedLocations)
        {
            GL.TexImage2DMultisample(Target, Samples, InternalFormat, Width, Height, FixedLocations);
        }

        public void TexImage3D<T>(TextureTarget Target, int V0, PixelInternalFormat InternalFormat, int V1, int V2, int V3, int V4,
            PixelFormat Format, PixelType Type, T[,,] Data) where T : struct
        {
            GL.TexImage3D(Target, V0, InternalFormat, V1, V2, V3, V4, Format, Type, Data);
        }

        public void TexParameter(TextureTarget Target, TextureParameterName Name, int Value)
        {
            GL.TexParameter(Target, Name, Value);
        }

        public void TexStorage3D(TextureTarget3d Target, int Levels, SizedInternalFormat Internalformat, int Width, int Height, int Depth)
        {
            GL.TexStorage3D(Target, Levels, Internalformat, Width, Height, Depth);
        }

        public void Translate(Vector3 Location)
        {
            GL.Translate(Location);
        }

        public void Uniform1(int Location, int Uniform)
        {
            GL.Uniform1(Location, Uniform);
        }
        
        public void Uniform1(int Location, float Uniform)
        {
            GL.Uniform1(Location, Uniform);
        }
        
        public void Uniform1(int Location, double Uniform)
        {
            GL.Uniform1(Location, Uniform);
        }       

        public void Uniform2(int Location, Vector2 Uniform)
        {
            GL.Uniform2(Location, Uniform);
        }

        public void Uniform3(int Location, Vector3 Uniform)
        {
            GL.Uniform3(Location, Uniform);
        }

        public void Uniform4(int Location, Vector4 Uniform)
        {
            GL.Uniform4(Location, Uniform);
        }

        public void UniformMatrix2(int Location, bool Transpose, ref Matrix2 Uniform)
        {
            GL.UniformMatrix2(Location, Transpose, ref Uniform);
        }

        public void UniformMatrix2x3(int Location, bool Transpose, ref Matrix2x3 Uniform)
        {
            GL.UniformMatrix2x3(Location, Transpose, ref Uniform);
        }

        public void UniformMatrix2x4(int Location, bool Transpose, ref Matrix2x4 Uniform)
        {
            GL.UniformMatrix2x4(Location, Transpose, ref Uniform);
        }

        public void UniformMatrix3(int Location, bool Transpose, ref Matrix3 Uniform)
        {
            GL.UniformMatrix3(Location, Transpose, ref Uniform);
        }

        public void UniformMatrix3x2(int Location, bool Transpose, ref Matrix3x2 Uniform)
        {
            GL.UniformMatrix3x2(Location, Transpose, ref Uniform);
        }

        public void UniformMatrix3x4(int Location, bool Transpose, ref Matrix3x4 Uniform)
        {
            GL.UniformMatrix3x4(Location, Transpose, ref Uniform);
        }

        public void UniformMatrix4(int Location, bool Transpose, ref Matrix4 Uniform)
        {
            GL.UniformMatrix4(Location, Transpose, ref Uniform);
        }

        public void UniformMatrix4x2(int Location, bool Transpose, ref Matrix4x2 Uniform)
        {
            GL.UniformMatrix4x2(Location, Transpose, ref Uniform);
        }

        public void UniformMatrix4x3(int Location, bool Transpose, ref Matrix4x3 Uniform)
        {
            GL.UniformMatrix4x3(Location, Transpose, ref Uniform);
        }

        public void UseProgram(uint Program)
        {
            GL.UseProgram(Program);
        }

        public void Vertex2(Vector2 Vertex)
        {
            GL.Vertex2(Vertex);
        }

        public void Vertex3(Vector3 Vertex)
        {
            GL.Vertex3(Vertex);
        }

        public void Vertex3(ref float Vertex)
        {
            GL.Vertex3(ref Vertex);
        }

        public void VertexAttribDivisor(int V0, int V1)
        {
            GL.VertexAttribDivisor(V0, V1);
        }

        public void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, IntPtr Ptr)
        {
            GL.VertexAttribPointer(V0, V1, Type, Flag, Bytes, Ptr);
        }
        
        public void VertexAttribPointer(int V0, int V1, VertexAttribPointerType Type, bool Flag, int Bytes, int V2)
        {
            GL.VertexAttribPointer(V0, V1, Type, Flag, Bytes, V2);
        }

        public void Viewport(int V0, int V1, int V2, int V3)
        {
            GL.Viewport(V0, V1, V2, V3);
        }
    }
}
