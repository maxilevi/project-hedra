using System;
using Silk.NET.OpenGL;

namespace Hedra.Engine.Windowing
{
    public enum BufferTarget
    {
        ArrayBuffer = GLEnum.ArrayBuffer,
        ElementArrayBuffer = GLEnum.ElementArrayBuffer,
        UniformBuffer = GLEnum.UniformBuffer
    }

    public enum TextureMinFilter
    {
        Nearest = GLEnum.Nearest,
        Linear = GLEnum.Linear,
        Repeat = GLEnum.Repeat
    }

    public enum TextureMagFilter
    {
        Nearest = GLEnum.Nearest,
        Linear = GLEnum.Linear,
        Repeat = GLEnum.Repeat
    }

    public enum TextureWrapMode
    {
        ClampToEdge = GLEnum.ClampToEdge,
        ClampToBorder = GLEnum.ClampToBorder,
        Repeat = GLEnum.Repeat
    }

    public enum BlendingFactor
    {
        SrcAlpha = GLEnum.SrcAlpha,
        OneMinusSrcAlpha = GLEnum.OneMinusSrcAlpha
    }
    
    public enum TextureUnit
    {
        Texture0 = GLEnum.Texture0,
        Texture1 = GLEnum.Texture1,
        Texture2 = GLEnum.Texture2,
        Texture3 = GLEnum.Texture3,
        Texture4 = GLEnum.Texture4,
        Texture5 = GLEnum.Texture5,
        Texture6 = GLEnum.Texture6,
    }
    
    public enum QueryTarget    
    {
        AnySamplesPassed = GLEnum.AnySamplesPassed
    }
    
    public enum FramebufferTarget    
    {
        Framebuffer = GLEnum.Framebuffer
    }
    
    public enum TextureTarget    
    {
        Texture2D = GLEnum.Texture2D,
        Texture3D = GLEnum.Texture3D,
        TextureCubeMap = GLEnum.TextureCubeMap,
        TextureCubeMapPositiveX = GLEnum.TextureCubeMapPositiveX,
        Texture2DMultisample = GLEnum.Texture2DMultisample,
    }
    
    public enum BlendEquationMode    
    {
        FuncAdd = GLEnum.FuncAdd
    }
    public enum BufferRangeTarget    
    {
        UniformBuffer = GLEnum.UniformBuffer
    }
    
    public enum LBlendingFactor    
    {
    }
    
    public enum BufferUsageHint    
    {
        StaticDraw = GLEnum.StaticDraw,
        DynamicDraw = GLEnum.DynamicDraw,
    }
    
    [Flags]
    public enum ClearBufferMask    
    {
        ColorBufferBit = GLEnum.ColorBufferBit,
        DepthBufferBit = GLEnum.DepthBufferBit,
        StencilBufferBit = GLEnum.StencilBufferBit
    }
    
    public enum BlitFramebufferFilter    
    {
    }
    
    public enum FramebufferErrorCode    
    {
        FramebufferComplete = GLEnum.FramebufferComplete,
    }
    
    public enum ShaderType
    {
        VertexShader = GLEnum.VertexShader,
        FragmentShader = GLEnum.FragmentShader,
        GeometryShader = GLEnum.GeometryShader
    }
    
    public enum CullFaceMode    
    {
        Front = GLEnum.Front,
        Back = GLEnum.Back
    }

    public enum EnableCap    
    {
        DepthTest = GLEnum.DepthTest,
        DepthClamp = GLEnum.DepthClamp,
        Blend = GLEnum.Blend,
        CullFace = GLEnum.CullFace,
        DebugOutput = GLEnum.DebugOutput
    }
    
    public enum GetPName    
    {
        MaxGeometryInputComponents = GLEnum.MaxGeometryInputComponents,
        MaxGeometryOutputVertices = GLEnum.MaxGeometryOutputVertices,
        MaxGeometryOutputComponents = GLEnum.MaxGeometryOutputComponents,
        MaxGeometryTotalOutputComponents = GLEnum.MaxGeometryTotalOutputComponents,
        CurrentProgram = GLEnum.CurrentProgram,
        MaxVertexUniformVectors = GLEnum.MaxVertexUniformVectors
    }
    
    public enum VertexAttribPointerType    
    {
        Float = GLEnum.Float,
        UnsignedInt = GLEnum.UnsignedInt,
        Int = GLEnum.Int,
        UnsignedShort = GLEnum.UnsignedShort,
    }
    
    public enum GetProgramParameterName    
    {
        LinkStatus = GLEnum.LinkStatus,
        InfoLogLength = GLEnum.InfoLogLength,
        CurrentProgram = GLEnum.CurrentProgram,
    }
    
    public enum PixelInternalFormat    
    {
        Rgba = GLEnum.Rgba,
        Rgba8 = GLEnum.Rgba8,
        Rgba32f = GLEnum.Rgba32f,
        Rgba16f = GLEnum.Rgba16f,
        DepthStencil = GLEnum.DepthStencil,
        Depth24Stencil8 = GLEnum.Depth24Stencil8,
        Rgb32f = GLEnum.Rgb32f,
        DepthComponent16 = GLEnum.DepthComponent16
        
        
    }
    
    public enum PixelInternalFormatEXT    
    {
    }

    public enum TextureCompareMode
    {
        CompareRefToTexture = GLEnum.CompareRefToTexture
    }
    
    public enum TextureParameterName
    {
        TextureMagFilter = GLEnum.TextureMagFilter,
        TextureMinFilter = GLEnum.TextureMinFilter,
        TextureWrapS = GLEnum.TextureWrapS,
        TextureWrapT = GLEnum.TextureWrapT,
        TextureWrapR = GLEnum.TextureWrapR,
        TextureCompareMode = GLEnum.TextureCompareMode
    }
    
    public enum PixelType    
    {
        UnsignedByte = GLEnum.UnsignedByte,
        Float = GLEnum.Float,
        Byte = GLEnum.Byte,
        UnsignedInt248 = GLEnum.UnsignedInt248
    }
    
    public enum PixelFormat    
    {
        Bgra = GLEnum.Bgra,
        DepthComponent = GLEnum.DepthComponent,
        Rgba = GLEnum.Rgba,
        DepthStencil = GLEnum.DepthStencil,
        Red = GLEnum.Red
    }
    
    public enum TextureTargetMultisample    
    {
        Texture2DMultisample = GLEnum.Texture2DMultisample,
    }
    
    public enum MaterialFace    
    {
        FrontAndBack = GLEnum.FrontAndBack,
        Front = GLEnum.Front,
        Back = GLEnum.Back
    }
    
    public enum PolygonMode    
    {
        Line = GLEnum.Line,
        Fill = GLEnum.Fill
    }
    
    public enum PrimitiveType    
    {
        Triangles = GLEnum.Triangles,
        Lines = GLEnum.Lines,
        TriangleStrip = GLEnum.TriangleStrip,
        LineStrip = GLEnum.LineStrip,
    }
    
    public enum DrawElementsType    
    {
        UnsignedInt = GLEnum.UnsignedInt,
        UnsignedShort = GLEnum.UnsignedShort,
        UnsignedByte = GLEnum.UnsignedByte
    }
    
    public enum DrawBuffersEnum    
    {
    }
    
    public enum DrawBufferMode    
    {
        None = GLEnum.None,
    }
    
    public enum FramebufferAttachment    
    {
        ColorAttachment0 = GLEnum.ColorAttachment0,
        ColorAttachment1 = GLEnum.ColorAttachment1,
        ColorAttachment2 = GLEnum.ColorAttachment2,
        DepthAttachment = GLEnum.DepthAttachment,
        StencilAttachment = GLEnum.StencilAttachment
    }
    
    public enum GenerateMipmapTarget    
    {
        Texture2D = GLEnum.Texture2D
    }
    
    public enum ErrorCode    
    {
        NoError = GLEnum.NoError,
        InvalidOperation = GLEnum.InvalidOperation
    }
    
    public enum ActiveUniformBlockParameter    
    {
    }
    
    public enum ReadBufferMode    
    {
        None = GLEnum.None,
    }
    
    public enum StringName    
    {
        Extensions = GLEnum.Extensions,
        Vendor =  GLEnum.Vendor,
        Renderer = GLEnum.Renderer,
        Version = GLEnum.Version,
        ShadingLanguageVersion = GLEnum.ShadingLanguageVersion
    }
    
    public enum ShaderParameter    
    {
        CompileStatus = GLEnum.CompileStatus,
        InfoLogLength = GLEnum.InfoLogLength
    }
    
    public enum GetQueryObjectParam
    {
        QueryResult = GLEnum.QueryResult,
        QueryResultAvailable = GLEnum.QueryResultAvailable
    }
}